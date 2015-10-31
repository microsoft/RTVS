#include "stdafx.h"
#include "log.h"
#include "server.h"
#include "Rapi.h"
#include "vsgd.h"
#include "r_util.h"
#include "RPlotHost.h"

using namespace rhost::log;
namespace po = boost::program_options;

const unsigned PORT = 5118;
const char* plot_window_option_name = "plot_window";

void parse_arguments(int argc, char** argv, po::variables_map* vm);
void run_R(int argc, char** argv, HWND plot_window_container_handle);
void cleanup_rplot_hook();

po::variables_map vm;

int main(int argc, char** argv) {
    setlocale(LC_NUMERIC, "C");
    init_log();
    atexit(cleanup_rplot_hook);

    HWND plot_window_container_handle = NULL;

    parse_arguments(argc, argv, &vm);
    if (vm.count(plot_window_option_name)) {
        plot_window_container_handle = (HWND)vm[plot_window_option_name].as<INT64>();
    }

    run_R(argc, argv, plot_window_container_handle);
}

void run_R(int argc, char** argv, HWND plot_window_container_handle) {
    __try {

        rplots::RPlotHost::Init(plot_window_container_handle);

        logf("Waiting for connection on port %u ...\n", PORT);
        rhost::server::wait_for_client(PORT);

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
        R_set_command_line_arguments(argc, argv);

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
    } __finally {
        flush_log();
    }
}

void parse_arguments(int argc, char** argv, po::variables_map* pvm) {
    po::options_description desc;
    try {
        desc.add_options()
            ("plot_window", po::value<INT64>()->default_value(0), "R Plot window container handle (resides in VS tool window)");

        po::store(po::parse_command_line(argc, argv, desc), *pvm); // can throw 
    } catch (po::error& e) {
        std::cerr << "ERROR: " << e.what() << std::endl << std::endl;
        std::cerr << desc << std::endl;
    }
}

void cleanup_rplot_hook() {
    rplots::RPlotHost::Terminate();
}
