#pragma once
#pragma warning(disable: 4996)

#include <atomic>
#include <codecvt>
#include <csetjmp>
#include <cstdio>
#include <cstdlib>
#include <future>
#include <string>
#include <thread>
#include <tuple>
#include <utility>

#include "picojson.h"

#pragma warning(push, 0)
#define _WEBSOCKETPP_CPP11_STL_
#include "websocketpp/server.hpp"
#include "websocketpp/config/asio_no_tls.hpp"
#pragma warning(pop)

#include "windows.h"
