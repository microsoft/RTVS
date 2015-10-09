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
            std::atomic<bool> is_connection_closed = false;

            picojson::object r_eval(const std::string& expr) {
                picojson::object obj;
                obj["event"] = picojson::value("eval");

                SEXP sexp_expr = Rf_protect(Rf_allocVector3(STRSXP, 1, nullptr));
                SET_STRING_ELT(sexp_expr, 0, Rf_mkChar(expr.c_str()));

                ParseStatus ps;
                SEXP sexp_parsed = Rf_protect(R_ParseVector(sexp_expr, -1, &ps, R_NilValue));
                obj["ParseStatus"] = picojson::value(static_cast<double>(ps));

                if (ps == PARSE_OK) {
                    picojson::array results(Rf_length(sexp_parsed));
                    for (int i = 0; i < results.size(); ++i) {
                        picojson::object res;

                        int has_error;
                        SEXP sexp_result = R_tryEvalSilent(VECTOR_ELT(sexp_parsed, i), R_GlobalEnv, &has_error);
                        if (has_error) {
                            obj["error"] = picojson::value(R_curErrorBuf());
                        }

                        if (sexp_result) {
                            sexp_result = Rf_protect(Rf_asChar(sexp_result));
                            obj["result"] = picojson::value(R_CHAR(sexp_result));
                            Rf_unprotect(1);
                        }
                    }
                }

                Rf_unprotect(2);
                return obj;
            }

            template<class F>
            picojson::value with_response(F&& f) {
                response_promise = std::make_unique<std::promise<picojson::value>>();
                f();

                picojson::value r;
                do {
                    std::future<picojson::value> response_future = response_promise->get_future();
                    while (response_future.wait_for(0ms) != std::future_status::ready) {
                        R_WaitEvent();
                        R_ProcessEvents();
                        if (is_connection_closed) {
                            R_Suicide("Lost connection to client.");
                            assert(false); // not reachable
                        }
                    }

                    r = response_future.get();
                    response_promise = nullptr;

                    if (r.is<picojson::object>()) {
                        const auto& obj = r.get<picojson::object>();
                        auto it = obj.find("command");
                        if (it != obj.end() && it->second.is<std::string>() && it->second.get<std::string>() == "eval") {
                            std::string expr;
                            it = obj.find("expr");
                            if (it != obj.end() && it->second.is<std::string>()) {
                                expr = it->second.get<std::string>();
                            }

                            picojson::object result = r_eval(expr);
                            std::string json = picojson::value(result).serialize();

                            response_promise = std::make_unique<std::promise<picojson::value>>();
                            ws_conn->send(json, websocketpp::frame::opcode::text);
                        }
                    }
                } while (response_promise);

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

                picojson::object obj;
                add_context(obj);

                obj["event"] = picojson::value("ReadConsole");
                obj["prompt"] = picojson::value(prompt);
                obj["len"] = picojson::value(static_cast<double>(len));
                obj["addToHistory"] = picojson::value(addToHistory ? true : false);

                for (;;) {
                    std::string json = picojson::value(obj).serialize();

                    picojson::value resp = with_response([&] {
                        ws_conn->send(json, websocketpp::frame::opcode::text);
                    });

                    if (resp.is<picojson::null>()) {
                        return 0;
                    }
                    if (!resp.is<std::string>()) {
                        fprintf(stderr, "!!! ERROR: expected string, got %s\n", resp.to_str().c_str());
                        return 0;
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

                std::string json = picojson::value(obj).serialize();
                ws_conn->send(json, websocketpp::frame::opcode::text);
            }

            extern "C" void CallBack() {
#if false
                picojson::object obj;
                add_context(obj);

                obj["event"] = picojson::value("CallBack");

                std::string json = picojson::value(obj).serialize();
                ws_conn->send(json, websocketpp::frame::opcode::text);
#endif
            }

            extern "C" void ShowMessage(const char* s) {
                if (is_connection_closed) {
                    return;
                }

                picojson::object obj;
                add_context(obj);

                obj["event"] = picojson::value("ShowMessage");
                obj["s"] = picojson::value(to_utf8(s));

                std::string json = picojson::value(obj).serialize();
                ws_conn->send(json, websocketpp::frame::opcode::text);
            }

            extern "C" int YesNoCancel(const char* s) {
                if (is_connection_closed) {
                    return 0;
                }

                picojson::object obj;
                obj["event"] = picojson::value("YesNoCancel");
                obj["s"] = picojson::value(to_utf8(s));
                std::string json = picojson::value(obj).serialize();

                picojson::value resp = with_response([&] {
                    ws_conn->send(json, websocketpp::frame::opcode::text);
                });

                if (resp.is<picojson::null>()) {
                    return 0;
                }
                if (!resp.is<double>()) {
                    fprintf(stderr, "!!! ERROR: expected double, got %s\n", resp.to_str().c_str());
                    return 0;
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

                std::string json = picojson::value(obj).serialize();
                ws_conn->send(json, websocketpp::frame::opcode::text);
            }

            void on_ws_message(websocketpp::connection_hdl hdl, ws_server_type::message_ptr msg) {
                if (!response_promise) {
                    fprintf(stderr, "Message received when no messages are expected: %s\n", msg->get_payload().c_str());
                    return;
                }

                picojson::value v;
                std::string err = picojson::parse(v, msg->get_payload());
                if (!err.empty()) {
                    fprintf(stderr, "Couldn't parse message: %s\n%s", msg->get_payload().c_str(), err.c_str());
                    return;
                }

                response_promise->set_value(v);
                unblock_message_loop();
            }

            extern "C" void on_exit() {
                if (!is_connection_closed) {
                    picojson::object obj;
                    obj["event"] = picojson::value("exit");
                    std::string json = picojson::value(obj).serialize();
                    ws_conn->send(json, websocketpp::frame::opcode::text);
                    server_thread->join();
                }
            }

            void connection_close_handler(websocketpp::connection_hdl h)
            {
                is_connection_closed = true;
                unblock_message_loop();
            }

            void server_thread_func(unsigned port) {
                ws_server_type server;

#ifdef NDEBUG
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
            std::string json = picojson::value(obj).serialize();
            ws_conn->send(json, websocketpp::frame::opcode::text);
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
            std::string json = picojson::value(obj).serialize();
            ws_conn->send(json, websocketpp::frame::opcode::text);
        }
    }
}
