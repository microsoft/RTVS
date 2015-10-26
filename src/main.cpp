#include "stdafx.h"
#include "log.h"
#include "server.h"
#include "Rapi.h"
#include "vsgd.h"
#include "r_util.h"

using namespace rhost::log;

const unsigned PORT = 5118;

int main(int argc, char** argv) {
    setlocale(LC_NUMERIC, "C");
    init_log();

    __try {
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
