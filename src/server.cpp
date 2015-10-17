#include "server.h"
#include "crtutils.h"
#include "util.h"
#include "Rapi.h"

using namespace rhost::util;
using namespace std::literals;

namespace rhost {
    namespace server {
        namespace {
            typedef websocketpp::server<websocketpp::config::asio> ws_server_type;

            DWORD main_thread_id;
            std::unique_ptr<std::thread> server_thread;
            std::promise<ws_server_type::connection_ptr> ws_conn_promise;
            ws_server_type::connection_ptr ws_conn;
            std::unique_ptr<std::promise<picojson::value>> response_promise;
            bool allow_callbacks = true;
            std::atomic<bool> is_connection_closed = false;
            std::atomic<int> nesting_level = 0;

            std::error_code send_json(ws_server_type::connection_type& conn, const picojson::value& value) {
                std::string json = value.serialize();
#ifdef TRACE_JSON
                for (int i = 0; i < nesting_level; ++i) fputc('\t', stderr);
                fprintf(stderr, "<<< %s\n\n", json.c_str());
#endif
                return conn.send(json, websocketpp::frame::opcode::text);
            }

            std::error_code send_json(ws_server_type::connection_type& conn, const picojson::object& obj) {
                return send_json(conn, picojson::value(obj));
            }

            template<class F>
            picojson::value with_response(F&& f) {
                response_promise = std::make_unique<std::promise<picojson::value>>();
                f();

                ++nesting_level;

                picojson::value r;
                do {
                    std::future<picojson::value> response_future = response_promise->get_future();
                    while (response_future.wait_for(0ms) != std::future_status::ready) {
                        R_WaitEvent();
                        R_ProcessEvents();
                        if (is_connection_closed) {
                            fatal_error("Lost connection to client.");
                        }
                    }

                    r = response_future.get();
                    response_promise = nullptr;

                    if (r.is<picojson::object>()) {
                        const auto& obj = r.get<picojson::object>();
                        auto it = obj.find("command");
                        if (it != obj.end() && it->second.is<std::string>() && it->second.get<std::string>() == "eval") {
                            bool old_allow_callbacks = allow_callbacks;
                            SCOPE_WARDEN(restore_allow_callbacks, {
                                allow_callbacks = old_allow_callbacks;
                            });

                            std::string expr;
                            it = obj.find("expr");
                            if (it != obj.end() && it->second.is<std::string>()) {
                                expr = it->second.get<std::string>();
                            } else {
                                fatal_error("'eval': 'expr' must be present, and must be a string.");
                            }

                            SEXP env = R_GlobalEnv;
                            it = obj.find("env");
                            if (it != obj.end()) {
                                if (!it->second.is<std::string>()) {
                                    fatal_error("'eval': 'env' must be a string.");
                                }

                                std::string env_name = it->second.get<std::string>();
                                if (env_name == "global") {
                                    env = R_GlobalEnv;
                                } else if (env_name == "base") {
                                    env = R_BaseEnv;
                                } else if (env_name == "empty") {
                                    env = R_EmptyEnv;
                                } else {
                                    fatal_error("'eval': 'env' must be one of: 'global', 'base', 'empty'.");
                                }
                            }

                            allow_callbacks = true;
                            it = obj.find("allow_callbacks");
                            if (it != obj.end()) {
                                if (!it->second.is<bool>()) {
                                    fatal_error("'eval': 'allow_callbacks' must be a boolean.");
                                }

                                allow_callbacks = it->second.get<bool>();
                            }

                            ParseStatus parse_status;
                            auto result = r_eval_str(expr, env, parse_status);

                            picojson::object result_obj;
                            result_obj["event"] = picojson::value("eval");
                            result_obj["ParseStatus"] = picojson::value(static_cast<double>(parse_status));
                            if (result.has_error) {
                                result_obj["error"] = picojson::value(result.error);
                            }
                            if (result.has_value) {
                                result_obj["result"] = picojson::value(result.value);
                            }

                            response_promise = std::make_unique<std::promise<picojson::value>>();
                            send_json(*ws_conn, result_obj);
                        }
                    }
                } while (response_promise);

                --nesting_level;
                return r;
            }

            void unblock_message_loop() {
                // Unblock any pending with_response call that is waiting in a message loop.
                PostThreadMessage(main_thread_id, WM_NULL, 0, 0);
            }

            void add_context(picojson::object& obj) {
                picojson::array ctxs;
                for (RCNTXT* ctxt = R_GlobalContext; ctxt != nullptr; ctxt = ctxt->nextcontext) {
                    picojson::object ctx;
                    ctx["callflag"] = picojson::value(static_cast<double>(ctxt->callflag));
                    ctxs.push_back(picojson::value(ctx));
                }
                obj["contexts"] = picojson::value(ctxs);
            }

            extern "C" int ReadConsole(const char* prompt, char* buf, int len, int addToHistory) {
                if (is_connection_closed) {
                    return 1;
                }
                if (!allow_callbacks) {
                    Rf_error("ReadConsole: blocking callback not allowed during eval.");
                }

                picojson::object obj;
                add_context(obj);

                obj["event"] = picojson::value("ReadConsole");
                obj["prompt"] = picojson::value(prompt);
                obj["len"] = picojson::value(static_cast<double>(len));
                obj["addToHistory"] = picojson::value(addToHistory ? true : false);

                for (;;) {
                    picojson::value resp = with_response([&] {
                        send_json(*ws_conn, obj);
                    });

                    if (resp.is<picojson::null>()) {
                        return 0;
                    }
                    if (!resp.is<std::string>()) {
                        fatal_error("ReadConsole: expected string, got %s\n", resp.to_str().c_str());
                    }

                    std::string s = from_utf8(resp.get<std::string>());
                    if (s.size() >= len) {
                        obj["retryReason"] = picojson::value("BUFFER_OVERFLOW");
                        continue;
                    }

                    strcpy_s(buf, len, s.c_str());
                    return 1;
                }
            }

            extern "C" void WriteConsoleEx(const char* buf, int len, int otype) {
                if (is_connection_closed) {
                    return;
                }

                picojson::object obj;
                add_context(obj);

                obj["event"] = picojson::value("WriteConsoleEx");
                obj["buf"] = picojson::value(to_utf8(buf, len));
                obj["otype"] = picojson::value(static_cast<double>(otype));

                send_json(*ws_conn, obj);
            }

            extern "C" void CallBack() {
            }

            extern "C" void ShowMessage(const char* s) {
                if (is_connection_closed) {
                    return;
                }

                picojson::object obj;
                add_context(obj);

                obj["event"] = picojson::value("ShowMessage");
                obj["s"] = picojson::value(to_utf8(s));

                send_json(*ws_conn, obj);
            }

            extern "C" int YesNoCancel(const char* s) {
                if (is_connection_closed) {
                    return 0;
                }
                if (!allow_callbacks) {
                    Rf_error("YesNoCancel: blocking callback not allowed during eval.");
                }

                picojson::object obj;
                obj["event"] = picojson::value("YesNoCancel");
                obj["s"] = picojson::value(to_utf8(s));

                picojson::value resp = with_response([&] {
                    send_json(*ws_conn, obj);
                });

                if (resp.is<picojson::null>()) {
                    return 0;
                }
                if (!resp.is<double>()) {
                    fatal_error("YesNoCancel: expected number, got %s\n", resp.to_str().c_str());
                }

                return static_cast<int>(resp.get<double>());
            }

            extern "C" void Busy(int which) {
                if (is_connection_closed) {
                    return;
                }

                picojson::object obj;
                add_context(obj);

                obj["event"] = picojson::value("Busy");
                obj["which"] = picojson::value(which ? true : false);

                send_json(*ws_conn, obj);
            }

            void on_ws_message(websocketpp::connection_hdl hdl, ws_server_type::message_ptr msg) {
                if (!response_promise) {
                    fatal_error("Message received when no messages are expected: %s\n", msg->get_payload().c_str());
                }

                const std::string& json = msg->get_payload();

#ifdef TRACE_JSON
                for (int i = 0; i < nesting_level; ++i) fputc('\t', stderr);
                fprintf(stderr, ">>> %s\n\n", json.c_str());
#endif

                picojson::value v;
                std::string err = picojson::parse(v, json);
                if (!err.empty()) {
                    fatal_error("Couldn't parse message: %s\n%s", json.c_str(), err.c_str());
                }

                response_promise->set_value(v);
                unblock_message_loop();
            }

            extern "C" void on_exit() {
                if (!is_connection_closed) {
                    picojson::object obj;
                    obj["event"] = picojson::value("exit");
                    send_json(*ws_conn, obj);
                    server_thread->join();
                }
            }

            void connection_close_handler(websocketpp::connection_hdl h) {
                is_connection_closed = true;
                unblock_message_loop();
            }

            void server_thread_func(unsigned port) {
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
                server.listen(port);
                //server.start_accept();
                //server.run();

                ws_server_type::connection_ptr conn(server.get_connection());
                server.async_accept(conn, [&](const websocketpp::lib::error_code& ec) {
                    if (ec) {
                        conn->terminate(ec);
                        std::exit(1);
                    } else {
                        conn->start();
                        // R itself is built with MinGW, and links to msvcrt.dll, so it uses the latter's exit() to terminate the main loop.
                        // To ensure that our code runs during shutdown, we need to use the corresponding atexit().
                        rhost::crt::atexit(on_exit);
                    }
                });

                server.run();
            }
        }

        void wait_for_client(unsigned port) {
            main_thread_id = GetCurrentThreadId();
            server_thread.reset(new std::thread(server_thread_func, port));
            ws_conn = ws_conn_promise.get_future().get();

            picojson::object obj;
            obj["protocol_version"] = picojson::value(1.0);
            obj["R_version"] = picojson::value(getDLLVersion());
            send_json(*ws_conn, obj);
        }

        void register_callbacks(structRstart& rp) {
            rp.ReadConsole = ReadConsole;
            rp.WriteConsoleEx = WriteConsoleEx;
            rp.CallBack = CallBack;
            rp.ShowMessage = ShowMessage;
            rp.YesNoCancel = YesNoCancel;
            rp.Busy = Busy;
        }

        void plot_xaml(std::string& filepath) {
            picojson::object obj;
            add_context(obj);

            obj["event"] = picojson::value("PlotXaml");
            obj["filepath"] = picojson::value(to_utf8(filepath));
            send_json(*ws_conn, obj);
        }
    }
}
