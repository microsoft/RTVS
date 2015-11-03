#include "stdafx.h"
#include "crtutils.h"
#include "log.h"
#include "RPlotHost.h"
#include "r_util.h"
#include "server.h"
#include "Rapi.h"
#include "util.h"
#include "vsgd.h"

using namespace rhost::log;
namespace po = boost::program_options;

namespace rhost {
    struct command_line_args {
        boost::optional<boost::asio::ip::tcp::endpoint> listen_endpoint;
        boost::optional<websocketpp::uri> connect_uri;
        boost::optional<HWND> plot_window_parent;

        std::vector<std::string> unrecognized;
        int argc;
        std::vector<char*> argv;
    };

    command_line_args parse_command_line(int argc, char** argv) {
        command_line_args args;

        po::option_description
            help("rhost-help", new po::untyped_value(true), "Produce help message."),
            listen("rhost-listen", po::value<boost::asio::ip::tcp::endpoint>(), "Listen for incoming connections on the specified IP address and port."),
            connect("rhost-connect", po::value<websocketpp::uri>(), "Connect to a websocket server at the specified URI."),
            reparent_plot_windows("rhost-reparent-plot-windows", po::value<int64_t>(), "Reparent R plot windows to the specified HWND.");

        po::options_description desc;
        for (auto&& opt : { help, listen, connect, reparent_plot_windows }) {
            boost::shared_ptr<po::option_description> popt(new po::option_description(opt));
            desc.add(popt);
        }

        po::variables_map vm;
        try {
            auto parsed = po::command_line_parser(argc, argv).options(desc).allow_unregistered().run();
            po::store(parsed, vm);
            po::notify(vm);
            args.unrecognized = po::collect_unrecognized(parsed.options, po::include_positional);
        } catch (po::error& e) {
            std::cerr << "ERROR: " << e.what() << std::endl << std::endl;
            std::cerr << desc << std::endl;
            std::exit(EXIT_FAILURE);
        }

        if (vm.count(help.long_name())) {
            std::cerr << desc << std::endl;
            std::exit(EXIT_SUCCESS);
        }

        auto listen_arg = vm.find(listen.long_name());
        if (listen_arg != vm.end()) {
            args.listen_endpoint = listen_arg->second.as<boost::asio::ip::tcp::endpoint>();
        }

        auto connect_arg = vm.find(connect.long_name());
        if (connect_arg != vm.end()) {
            args.connect_uri = connect_arg->second.as<websocketpp::uri>();
        }

        if (!args.listen_endpoint && !args.connect_uri) {
            std::cerr << "Either " << listen.format_name() << " or " << connect.format_name() << " must be specified." << std::endl;
            std::cerr << desc << std::endl;
            std::exit(EXIT_FAILURE);
        } else if (args.listen_endpoint && args.connect_uri) {
            std::cerr << "Both " << listen.format_name() << " and " << connect.format_name() << " cannot be specified at the same time." << std::endl;
            std::cerr << desc << std::endl;
            std::exit(EXIT_FAILURE);
        }

        auto reparent_plot_windows_arg = vm.find(reparent_plot_windows.long_name());
        if (reparent_plot_windows_arg != vm.end()) {
            args.plot_window_parent = reinterpret_cast<HWND>(reparent_plot_windows_arg->second.as<int64_t>());
        }

        args.argv.push_back(argv[0]);
        for (auto& s : args.unrecognized) {
            args.argv.push_back(&s[0]);
        }
        args.argc = int(args.argv.size());
        args.argv.push_back(nullptr);

        return args;
    }

    int run(command_line_args& args) {
        if (args.plot_window_parent) {
            rplots::RPlotHost::Init(*args.plot_window_parent);
            rhost::crt::atexit([] { rplots::RPlotHost::Terminate(); });
        }
        
        if (args.listen_endpoint) {
            rhost::server::wait_for_client(*args.listen_endpoint).get();
        } else if (args.connect_uri) {
            rhost::server::connect_to_server(*args.connect_uri).get();
        } else {
            return EXIT_FAILURE;
        }

        R_setStartTime();
        structRstart rp = {};
        R_DefParams(&rp);

        rp.rhome = get_R_HOME();
        rp.home = getRUser();
        rp.CharacterMode = RGui;
        rp.R_Quiet = R_TRUE;
        rp.R_Interactive = R_TRUE;
        rp.RestoreAction = SA_RESTORE;
        rp.SaveAction = SA_NOSAVE;

        rhost::server::register_callbacks(rp);

        R_SetParams(&rp);
        R_set_command_line_arguments(args.argc, args.argv.data());

        GA_initapp(0, 0);
        readconsolecfg();

        DllInfo *dll = R_getEmbeddingDllInfo();
        R_init_vsgd(dll);
        R_init_util(dll);

        CharacterMode = LinkDLL;
        setup_Rmainloop();
        CharacterMode = RGui;

        run_Rmainloop();

        Rf_endEmbeddedR(0);
        return EXIT_SUCCESS;
    }

    int run(int argc, char** argv) {
        return rhost::run(rhost::parse_command_line(argc, argv));
    }
}

int main(int argc, char** argv) {
    setlocale(LC_NUMERIC, "C");
    init_log();
    __try {
        return rhost::run(argc, argv);
    } __finally {
        flush_log();
    }
}
