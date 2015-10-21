#pragma once
#include "Rapi.h"

namespace rhost {
    namespace server {
        void wait_for_client(unsigned port);
        void register_callbacks(structRstart& rp);
        void plot_xaml(const std::string& xaml);
    }
}
