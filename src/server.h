#pragma once
#include "stdafx.h"
#include "Rapi.h"

namespace rhost {
    namespace server {
        std::future<void> wait_for_client(const boost::asio::ip::tcp::endpoint& endpoint);
        std::future<void> connect_to_server(const websocketpp::uri& uri);
        void register_callbacks(structRstart& rp);
        void plot_xaml(const std::string& xaml);
    }
}
