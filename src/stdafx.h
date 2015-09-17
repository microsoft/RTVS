#pragma once

#define _WIN32_WINNT 0x0601

#include <codecvt>
#include <future>
#include <string>
#include <thread>

#include "picojson.h"

#define _WEBSOCKETPP_CPP11_STL_
#include "websocketpp/server.hpp"
#include "websocketpp/config/asio_no_tls.hpp"
