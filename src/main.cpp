#include "stdafx.h"
#include <cstdlib>
#include <cstdio>
#include "Rapi.h"

namespace {
    const unsigned PORT = 5118;

    typedef websocketpp::server<websocketpp::config::asio> ws_server_type;

    std::unique_ptr<std::thread> server_thread;
    std::promise<ws_server_type::connection_ptr> ws_conn_promise;
    ws_server_type::connection_ptr ws_conn;
    std::unique_ptr<std::promise<picojson::value>> response_promise;

    std::string to_utf8(const char* buf, size_t len) {
        auto& codecvt_wchar = std::use_facet<std::codecvt<wchar_t, char, std::mbstate_t>>(std::locale());
        std::wstring_convert<std::codecvt<wchar_t, char, std::mbstate_t>> convert(&codecvt_wchar);
        auto ws = convert.from_bytes(buf, buf + len);

        std::wstring_convert<std::codecvt_utf8<wchar_t>> codecvt_utf8;
        return codecvt_utf8.to_bytes(ws);
    }

    std::string to_utf8(const std::string& s) {
        return to_utf8(s.data(), s.size());
    }

    std::string from_utf8(const std::string& u8s) {
        std::wstring_convert<std::codecvt_utf8<wchar_t>> codecvt_utf8;
        auto ws = codecvt_utf8.from_bytes(u8s);

        auto& codecvt_wchar = std::use_facet<std::codecvt<wchar_t, char, std::mbstate_t>>(std::locale());
        std::wstring_convert<std::codecvt<wchar_t, char, std::mbstate_t>> convert(&codecvt_wchar);
        return convert.to_bytes(ws);
    }

    template<class F>
    picojson::value with_response(F&& f) {
        response_promise = std::make_unique<std::promise<picojson::value>>();
        f();
        picojson::value r = response_promise->get_future().get();
        response_promise = nullptr;
        return r;
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
        picojson::object obj;
        add_context(obj);

        obj["event"] = picojson::value("ReadConsole");
        obj["prompt"] = picojson::value(prompt);
        obj["len"] = picojson::value(static_cast<double>(len));
        obj["addToHistory"] = picojson::value(addToHistory ? true : false);
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
        strcpy_s(buf, len, s.c_str());
        return 1;
    }

    extern "C" void WriteConsoleEx(const char* buf, int len, int otype) {
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
        picojson::object obj;
        add_context(obj);

        obj["event"] = picojson::value("ShowMessage");
        obj["s"] = picojson::value(to_utf8(s));

        std::string json = picojson::value(obj).serialize();
        ws_conn->send(json, websocketpp::frame::opcode::text);
    }

    extern "C" int YesNoCancel(const char* s) {
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
        picojson::object obj;
        add_context(obj);

        obj["event"] = picojson::value("Busy");
        obj["which"] = picojson::value(which ? true : false);

        std::string json = picojson::value(obj).serialize();
        ws_conn->send(json, websocketpp::frame::opcode::text);
    }

    void on_ws_message(websocketpp::connection_hdl hdl, ws_server_type::message_ptr msg) {
        if (!response_promise) {
            fprintf(stderr, "!!! UNEXPECTED MESSAGE: %s\n", msg->get_payload().c_str());
            return;
        }

        picojson::value v;
        picojson::parse(v, msg->get_payload());
        response_promise->set_value(v);
    }

    void server_thread_func(unsigned port) {
        ws_server_type server;

        server.set_open_handler([&](websocketpp::connection_hdl hdl) {
            ws_conn_promise.set_value(server.get_con_from_hdl(hdl));
        });

        server.set_message_handler(on_ws_message);

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
            }
        });

        server.run();
    }

    extern "C" void finalizer() {
        picojson::object obj;
        obj["event"] = picojson::value("exit");
        std::string json = picojson::value(obj).serialize();
        ws_conn->send(json, websocketpp::frame::opcode::text);
        server_thread->join();
    }
}


int main(int argc, char** argv) {
    // R itself is built with MinGW, and links to msvcrt.dll, so it uses the latter's exit() to terminate the main loop.
    // To ensure that our code runs during shutdown, we need to use the corresponding atexit().
    auto msvcrt_atexit = reinterpret_cast<int(*)(void(*)())>(GetProcAddress(LoadLibrary(L"msvcrt.dll"), "atexit"));

    server_thread.reset(new std::thread(server_thread_func, PORT));

    fprintf(stderr, "Waiting for connection on port %u ...\n", PORT);
    ws_conn = ws_conn_promise.get_future().get();

    picojson::object obj;
    obj["protocol_version"] = picojson::value(1.0);
    obj["R_version"] = picojson::value(getDLLVersion());
    std::string json = picojson::value(obj).serialize();
    ws_conn->send(json, websocketpp::frame::opcode::text);

    R_setStartTime();
    structRstart rp = {};
    R_DefParams(&rp);

    rp.rhome = get_R_HOME();
    rp.home = getRUser();
    rp.CharacterMode = LinkDLL;
    rp.R_Quiet = R_TRUE;
    rp.R_Interactive = R_TRUE;
    rp.RestoreAction = SA_RESTORE;
    rp.SaveAction = SA_NOSAVE;

    rp.ReadConsole = ReadConsole;
    rp.WriteConsoleEx = WriteConsoleEx;
    rp.CallBack = CallBack;
    rp.ShowMessage = ShowMessage;
    rp.YesNoCancel = YesNoCancel;
    rp.Busy = Busy;

    R_SetParams(&rp);
    R_set_command_line_arguments(argc, argv);

    GA_initapp(0, 0);
    readconsolecfg();
    setup_Rmainloop();

    msvcrt_atexit(finalizer);

    run_Rmainloop();

    Rf_unprotect(1);
    Rf_endEmbeddedR(0);
}