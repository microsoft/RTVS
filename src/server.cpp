#include "server.h"
#include "log.h"
#include "crtutils.h"
#include "eval.h"
#include "util.h"

using namespace std::literals;
using namespace rhost::log;
using namespace rhost::util;
using namespace rhost::eval;

namespace rhost {
    namespace server {
        inline namespace _impl {
            typedef websocketpp::server<websocketpp::config::asio> ws_server_type;
            typedef ws_server_type::connection_type ws_connection_type;

            struct message {
                std::string id;
                std::string request_id; // not a response if empty
                std::string name;
                picojson::array args;
            };

            class eval_cancel_error : std::exception {
            };

            DWORD main_thread_id;
            std::unique_ptr<std::thread> server_thread;
            std::promise<ws_server_type::connection_ptr> ws_conn_promise;
            ws_server_type::connection_ptr ws_conn;
            std::atomic<bool> is_connection_closed = false;
            long long next_message_id = 0;
            bool allow_callbacks = true, allow_intr_in_CallBack = true;

            message incoming;
            enum { INCOMING_UNEXPECTED, INCOMING_EXPECTED, INCOMING_RECEIVED } incoming_state;
            std::mutex incoming_mutex;

            struct eval_info {
                const std::string id;
                bool is_cancelable;

                eval_info(const std::string& id, bool is_cancelable)
                    : id(id), is_cancelable(is_cancelable) {
                }
            };

            std::vector<eval_info> eval_stack({ eval_info("", true) });
            bool canceling_eval;
            std::string eval_cancel_target;
            std::mutex eval_mutex;

            void terminate_if_closed() {
                // terminate invokes R_Suicide, which may invoke WriteConsole and/or ShowMessage, which will
                // then call terminate again, so we need to prevent infinite recursion here.
                static bool is_terminating;
                if (is_terminating) {
                    return;
                }

                if (is_connection_closed) {
                    is_terminating = true;
                    terminate("Lost connection to client.");
                }
            }

            std::error_code send_json(ws_connection_type& conn, const picojson::value& value) {
                std::string json = value.serialize();

#ifdef TRACE_JSON
                logf("<== %s\n\n", json.c_str());
#endif

                if (!is_connection_closed) {
                    auto err = conn.send(json, websocketpp::frame::opcode::text);
                    if (err) {
                        fatal_error("Send failed: [%d] %s", err.value(), err.message().c_str());
                    }
                    return err;
                } else {
                    return std::error_code();
                }
            }

            template<class... Args>
            std::string make_message_json(picojson::array& array, const char* name, Args&&... args) {
                char id[0x20];
                sprintf_s(id, "#%lld#", next_message_id);
                next_message_id += 2;

                append(array, id, name, args...);
                return id;
            }

            template<class... Args>
            std::string send_message(ws_connection_type& conn, const char* name, Args&&... args) {
                picojson::value value(picojson::array_type, false);
                auto& array = value.get<picojson::array>();
                auto id = make_message_json(array, name, args...);
                send_json(conn, value);
                return id;
            }

            template<class... Args>
            std::string respond_to_message(ws_connection_type& conn, const message& request, Args... args) {
                picojson::value value(picojson::array_type, false);
                auto& array = value.get<picojson::array>();
                auto id = make_message_json(array, ":", request.id, request.name.c_str(), args...);
                send_json(conn, value);
                return id;
            }

            bool query_interrupt() {
                std::lock_guard<std::mutex> lock(eval_mutex);
                if (!canceling_eval) {
                    return false;
                }

                // If there is a non-cancellable eval on the stack, do not allow to interrupt it or anything nested.
                auto it = std::find_if(eval_stack.begin(), eval_stack.end(), [](auto ei) { return !ei.is_cancelable; });
                return it == eval_stack.end();
            }

            extern "C" void CallBack() {
                // Called periodically by R_ProcessEvents and Rf_eval. This is where we check for various
                // cancellation requests and issue an interrupt (Rf_onintr) if one is applicable in the
                // current context.

                // Rf_onintr may end up calling CallBack before it returns. We don't want to recursively
                // call it again, so do nothing and let the next eligible callback handle things.
                if (!allow_intr_in_CallBack) {
                    return;
                }

                if (query_interrupt()) {
                    allow_intr_in_CallBack = false;
                    interrupt_eval();
                    // Note that allow_intr_in_CallBack is not reset to false here. This is because Rf_onintr
                    // does not return (it unwinds via longjmp), and therefore any code here wouldn't run.
                    // Instead, we reset the flag where the control will end up after unwinding - either
                    // immediately after r_try_eval returns, or else (if we unwound R's own REPL eval) at
                    // the beginning of the next ReadConsole.
                    assert(!"Rf_onintr should never return.");
                }
            }

            void handle_eval(ws_connection_type& conn, const message& msg) {
                assert(msg.name.size() > 0 && msg.name[0] == '=');
                if (msg.args.size() != 1 || !msg.args[0].is<std::string>()) {
                    fatal_error("Evaluation request must have form [id, '=<flags>', expr].");
                }

                SCOPE_WARDEN_RESTORE(allow_callbacks);
                allow_callbacks = false;

                const auto& expr = from_utf8(msg.args[0].get<std::string>());
                SEXP env = nullptr;
                bool is_cancelable = false, json_result = false;

                for (auto it = msg.name.begin() + 1; it != msg.name.end(); ++it) {
                    switch (char c = *it) {
                    case 'B':
                    case 'E':
                        if (env != nullptr) {
                            fatal_error("'%s': multiple environment flags specified.", msg.name.c_str());
                        }
                        env = (c == 'B') ? R_BaseEnv : R_EmptyEnv;
                        break;
                    case 'j':
                        json_result = true;
                        break;
                    case '@':
                        allow_callbacks = true;
                        break;
                    case '/':
                        is_cancelable = true;
                        break;
                    default:
                        fatal_error("'%s': unrecognized flag '%c'.", msg.name.c_str(), c);
                    }
                }

                if (!env) {
                    env = R_GlobalEnv;
                }

                r_eval_result<std::string> result;
                ParseStatus ps;
                {
                    // We must not register this eval as a potential cancellation target before it gets a chance to establish
                    // the restart context; otherwise, there is a possibility that a cancellation request will arrive during
                    // that interval, and abort the outer eval instead. Similarly, we must remove this eval from the eval stack
                    // before the restart context is torn down, so that untimely cancellation request for the outer eval doesn't
                    // cancel his one.

                    auto before = [&] {
                        std::lock_guard<std::mutex> lock(eval_mutex);
                        eval_stack.push_back(eval_info(msg.id, is_cancelable));
                    };

                    bool was_after_invoked = false;
                    auto after = [&] {
                        std::lock_guard<std::mutex> lock(eval_mutex);

                        assert(!eval_stack.empty());
                        assert(eval_stack.end()[-1].id == msg.id);

                        if (canceling_eval && msg.id == eval_cancel_target) {
                            // If we were unwinding the stack for cancellation purposes, and this eval was the target
                            // of the cancellation, then we're done and should stop unwinding. Otherwise, we should 
                            // continue unwinding after reporting the result of the evaluation, which we'll do at the
                            // end of handle_eval if this flag is still set.
                            canceling_eval = false;
                        }

                        eval_stack.pop_back();
                        was_after_invoked = true;
                    };

                    result = r_try_eval_str(expr, env, ps, before, after);

                    // If eval was canceled, the "after" block was never executed (since it is normally run within the eval
                    // context, and so cancelation unwinds it along with everything else in that context), so we need to run
                    // it manually afterwards. Note that there's no potential race with newly arriving cancellation requests
                    // in this case, since we're already servicing one for this eval (or some parent eval).
                    if (!was_after_invoked) {
                        after();
                    }

                    allow_intr_in_CallBack = true;
                }

                picojson::value parse_status;
                switch (ps) {
                case PARSE_NULL:
                    parse_status = picojson::value("NULL");
                    break;
                case PARSE_OK:
                    parse_status = picojson::value("OK");
                    break;
                case PARSE_INCOMPLETE:
                    parse_status = picojson::value("INCOMPLETE");
                    break;
                case PARSE_ERROR:
                    parse_status = picojson::value("ERROR");
                    break;
                case PARSE_EOF:
                    parse_status = picojson::value("EOF");
                    break;
                default:
                    parse_status = picojson::value(double(ps));
                    break;
                }

                picojson::value error, value;
                if (result.has_error) {
                    error = picojson::value(to_utf8(result.error));
                }
                if (result.has_value) {
                    if (json_result) {
                        auto err = picojson::parse(value, to_utf8(result.value));
                        if (!err.empty()) {
                            fatal_error(
                                "'%s': evaluation result couldn't be parsed as JSON: %s\n\n%s",
                                msg.name.c_str(), err.c_str(), result.value.c_str());
                        }
                    } else {
                        value = picojson::value(to_utf8(result.value));
                    }
                }

                {
                    std::lock_guard<std::mutex> lock(incoming_mutex);
                    incoming_state = INCOMING_EXPECTED;
                }

#ifdef TRACE_JSON
                indent_log(+1);
#endif
                if (result.is_canceled) {
                    respond_to_message(conn, msg, picojson::value());
                } else {
                    respond_to_message(conn, msg, parse_status, error, value);
                }
#ifdef TRACE_JSON
                indent_log(-1);
#endif

                // If cancellation hasn't finished yet, continue unwinding the context stack. We don't want to call
                // Rf_onintr here, because this would skip over all the local object destructors in this function,
                // as well as the callback that invoked it. Instead, throw an exception and let C++ do unwinding the
                // normal way, and callback will then catch it at the very end, and invoke Rf_onintr just before it 
                // would've normally returned to R; see with_cancellation.
                if (query_interrupt()) {
                    throw eval_cancel_error();
                }
            }

            template<class... Args>
            message send_message_and_get_response(ws_connection_type& conn, const char* name, Args... args) {
                {
                    std::lock_guard<std::mutex> lock(incoming_mutex);
                    incoming_state = INCOMING_EXPECTED;
                }

                bool need_to_interrupt = false;
                {
                    auto id = send_message(conn, name, args...);
                    terminate_if_closed();

                    indent_log(+1);
                    SCOPE_WARDEN(dedent_log, { indent_log(-1); });

                    for (;;) {
                        message msg;
                        for (;;) {
                            {
                                std::lock_guard<std::mutex> lock(incoming_mutex);
                                assert(incoming_state != INCOMING_UNEXPECTED);
                                if (incoming_state == INCOMING_RECEIVED) {
                                    msg = incoming;
                                    incoming_state = INCOMING_UNEXPECTED;
                                    break;
                                }
                            }

                            // R_ProcessEvents may invoke CallBack. If there is a pending cancellation request, we do
                            // not want CallBack to call Rf_onintr as it normally does, since it would unwind the stack
                            // using longjmp, which will skip destructors for all our local variables. Instead, make
                            // CallBack a no-op until event processing is done, and then do a manual cancellation check.
                            allow_intr_in_CallBack = false;
                            R_ToplevelExec([](void*) {
                                // Errors can happen during event processing (from GUI windows such as graphs), and
                                // we don't want them to bubble up here, so run these in a fresh execution context.
                                R_WaitEvent();
                                R_ProcessEvents();
                            }, nullptr);
                            allow_intr_in_CallBack = true;

                            terminate_if_closed();

                            if (query_interrupt()) {
                                throw eval_cancel_error();
                            }
                        }

                        if (!msg.request_id.empty()) {
                            if (msg.request_id != id) {
                                fatal_error("Received response ['%s','%s'], while awaiting response for ['%s','%s'].",
                                    msg.request_id.c_str(), msg.name.c_str(), id.c_str(), name);
                            } else if (msg.name != name) {
                                fatal_error("Response to ['%s','%s'] has mismatching name '%s'.",
                                    id.c_str(), name, msg.name.c_str());
                            }
                            return msg;
                        }

                        if (msg.name.size() > 0 && msg.name[0] == '=') {
                            handle_eval(*ws_conn, msg);
                        } else {
                            fatal_error("Unrecognized incoming message name '%s'.", msg.name.c_str());
                        }
                    }
                }
            }

            template <class F>
            auto with_cancellation(F body) -> decltype(body()) {
                terminate_if_closed();
                try {
                    return body();
                } catch (const eval_cancel_error&) {
                    // Prevent CallBack from doing anything if it's called from within Rf_onintr again.
                    allow_intr_in_CallBack = false;

                    interrupt_eval();

                    assert(!"Rf_onintr should never return.");
                    throw;
                }
            }

            void unblock_message_loop() {
                // Unblock any pending with_response call that is waiting in a message loop.
                PostThreadMessage(main_thread_id, WM_NULL, 0, 0);
            }

            picojson::array get_context() {
                picojson::array context;
                for (RCNTXT* ctxt = R_GlobalContext; ctxt != nullptr; ctxt = ctxt->nextcontext) {
                    context.push_back(picojson::value(double(ctxt->callflag)));
                }
                return context;
            }

            extern "C" int ReadConsole(const char* prompt, char* buf, int len, int addToHistory) {
                return with_cancellation([&] {
                    if (!allow_intr_in_CallBack) {
                        // If we got here, this means that we've just processed a cancellation request that had
                        // unwound the context stack all the way to the bottom, cancelling all the active evals;
                        // otherwise, handle_eval would have allow_intr_in_CallBack set to true immediately after
                        // the targeted eval had returned. Mark everything cancellation-related as done.
                        assert(eval_stack.size() == 1);
                        canceling_eval = false;
                        allow_intr_in_CallBack = true;

                        // Notify client that cancellation has completed. When a specific eval is being canceled,
                        // there will be a corresponding (error) response to the original '=' message indicating
                        // completion, but for top-level canellation we need a special message.
                        send_message(*ws_conn, "\\");
                    }

                    if (!allow_callbacks) {
                        Rf_error("ReadConsole: blocking callback not allowed during evaluation.");
                    }

                    for (std::string retry_reason;;) {
                        auto msg = send_message_and_get_response(
                            *ws_conn, ">", get_context(), double(len), addToHistory != 0,
                            retry_reason.empty() ? picojson::value() : picojson::value(retry_reason),
                            to_utf8_json(prompt));

                        if (msg.args.size() != 1) {
                            fatal_error("ReadConsole: response must have a single argument.");
                        }

                        const auto& arg = msg.args[0];
                        if (arg.is<picojson::null>()) {
                            return 0;
                        }

                        if (!arg.is<std::string>()) {
                            fatal_error("ReadConsole: response argument must be string or null.");
                        }

                        auto s = from_utf8(arg.get<std::string>());
                        if (s.size() >= len) {
                            retry_reason = "BUFFER_OVERFLOW";
                            continue;
                        }

                        strcpy_s(buf, len, s.c_str());
                        return 1;
                    }
                });
            }

            extern "C" void WriteConsoleEx(const char* buf, int len, int otype) {
                with_cancellation([&] {
                    send_message(*ws_conn, (otype ? "!!" : "!"), to_utf8_json(buf));
                });
            }

            extern "C" void ShowMessage(const char* s) {
                with_cancellation([&] {
                    send_message(*ws_conn, "![]", to_utf8_json(s));
                });
            }

            extern "C" int YesNoCancel(const char* s) {
                return with_cancellation([&] {
                    if (!allow_callbacks) {
                        Rf_error("YesNoCancel: blocking callback not allowed during evaluation.");
                    }

                    auto msg = send_message_and_get_response(*ws_conn, "?", get_context(), to_utf8_json(s));
                    if (msg.args.size() != 1 || !msg.args[0].is<std::string>()) {
                        fatal_error("YesNoCancel: response argument must be a string.");
                    }

                    auto& r = msg.args[0].get<std::string>();
                    if (r == "N") {
                        return -1;
                    } else if (r == "C") {
                        return 0;
                    } else if (r == "Y") {
                        return 1;
                    } else {
                        fatal_error("YesNoCancel: response argument must be 'Y', 'N' or 'C'.");
                    }
                });
            }

            extern "C" void Busy(int which) {
                with_cancellation([&] {
                    send_message(*ws_conn, which ? "~+" : "~-");
                });
            }

            extern "C" void atexit_handler() {
                with_cancellation([&] {
                    send_json(*ws_conn, picojson::value());
                });
            }

            void on_ws_message(websocketpp::connection_hdl hdl, ws_server_type::message_ptr msg) {
                const auto& json = msg->get_payload();
#ifdef TRACE_JSON
                logf("==> %s\n\n", json.c_str());
#endif

                std::lock_guard<std::mutex> lock(incoming_mutex);

                picojson::value value;
                auto err = picojson::parse(value, json);
                if (!err.empty()) {
                    fatal_error("Malformed incoming message: %s", err.c_str());
                }

                if (value.is<picojson::null>()) {
                    terminate("Shutdown request received.");
                }

                if (!value.is<picojson::array>()) {
                    fatal_error("Message must be an array.");
                }

                auto& array = value.get<picojson::array>();
                if (array.size() < 2 || !array[0].is<std::string>() || !array[1].is<std::string>()) {
                    fatal_error("Message must be of the form [id, name, ...].");
                }

                incoming.id = array[0].get<std::string>();
                incoming.name = array[1].get<std::string>();
                auto args = array.begin() + 2;

                if (incoming.name == "/") {
                    if (array.size() != 3) {
                        fatal_error("Evaluation cancellation request must be of the form [id, '/', eval_id].");
                    }

                    std::string eval_id;
                    if (!array[2].is<picojson::null>()) {
                        if (!array[2].is<std::string>()) {
                            fatal_error("Evaluation cancellation request eval_id must be string or null.");
                        }
                        eval_id = array[2].get<std::string>();
                    }

                    std::lock_guard<std::mutex> lock(eval_mutex);

                    for (auto eval_info : eval_stack) {
                        auto& id = eval_info.id;

                        if (canceling_eval && id == eval_cancel_target) {
                            // If we're already in the process of cancelling some eval, and that one is below the
                            // one that we're been asked to cancel in the stack, then we don't need to do anything.
                            break;
                        }

                        if (id == eval_id) {
                            canceling_eval = true;
                            eval_cancel_target = id;
                            break;
                        }
                    }

                    if (canceling_eval) {
                        // Spin the loop in send_message_and_get_response so that it gets a chance to run cancel checks.
                        unblock_message_loop();
                    } else {
                        // If we didn't find the target eval in the stack, it must have completed already, and we've
                        // got a belated cancelation request for it, which we can simply ignore.
                    }

                    return;
                } else if (incoming.name == ":") {
                    if (array.size() < 4 || !array[2].is<std::string>() || !array[3].is<std::string>()) {
                        fatal_error("Response message must be of the form [id, ':', request_id, name, ...].");
                    }

                    incoming.request_id = array[2].get<std::string>();
                    incoming.name = array[3].get<std::string>();
                    args += 2;
                } else {
                    incoming.request_id.clear();
                }

                incoming.args.assign(args, array.end());

                assert(incoming_state != INCOMING_RECEIVED);
                if (incoming_state == INCOMING_UNEXPECTED) {
                    fatal_error("Unexpected incoming client message.");
                }

                incoming_state = INCOMING_RECEIVED;
                unblock_message_loop();
            }

            void connection_close_handler(websocketpp::connection_hdl h) {
                is_connection_closed = true;
                unblock_message_loop();
            }

            void server_worker(unsigned port) {
                ws_server_type server;

#ifndef TRACE_WEBSOCKET
                server.set_access_channels(websocketpp::log::alevel::none);
                server.set_error_channels(websocketpp::log::elevel::none);
#endif

                server.set_open_handler([&](websocketpp::connection_hdl hdl) {
                    ws_conn_promise.set_value(server.get_con_from_hdl(hdl));
                });
                server.set_message_handler(on_ws_message);
                server.set_close_handler(connection_close_handler);

                server.init_asio();

                boost::asio::ip::tcp::endpoint ep(boost::asio::ip::address_v4({ 127, 0, 0, 1 }), port);
                server.listen(ep);

                auto conn(server.get_connection());
                server.async_accept(conn, [&](auto error_code) {
                    if (error_code) {
                        conn->terminate(error_code);
                        fatal_error("Could not establish connection to client.");
                    } else {
                        conn->start();
                        // R itself is built with MinGW, and links to msvcrt.dll, so it uses the latter's exit() to terminate the main loop.
                        // To ensure that our code runs during shutdown, we need to use the corresponding atexit().
                        rhost::crt::atexit(atexit_handler);
                    }
                });

                server.run();
            }

            void server_thread_func(unsigned port) {
                __try {
                    server_worker(port);
                } __finally {
                    flush_log();
                }
            }
        }

        void wait_for_client(unsigned port) {
            main_thread_id = GetCurrentThreadId();
            server_thread.reset(new std::thread(server_thread_func, port));
            ws_conn = ws_conn_promise.get_future().get();

            send_message(*ws_conn, "Microsoft.R.Host", 1.0, getDLLVersion());
        }

        void register_callbacks(structRstart& rp) {
            rp.ReadConsole = ReadConsole;
            rp.WriteConsoleEx = WriteConsoleEx;
            rp.CallBack = CallBack;
            rp.ShowMessage = ShowMessage;
            rp.YesNoCancel = YesNoCancel;
            rp.Busy = Busy;
        }

        void plot_xaml(const std::string& filepath) {
            with_cancellation([&] {
                send_message(*ws_conn, "PlotXaml", to_utf8(filepath));
            });
        }
    }
}
