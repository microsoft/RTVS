#pragma once
#pragma warning(disable: 4996)

#include <atomic>
#include <codecvt>
#include <csetjmp>
#include <cstdio>
#include <cstdlib>
#include <fstream>
#include <future>
#include <mutex>
#include <string>
#include <thread>
#include <tuple>
#include <utility>

#include "boost/algorithm/string.hpp"
#include "boost/program_options/cmdline.hpp"
#include "boost/program_options/options_description.hpp"
#include "boost/program_options/value_semantic.hpp"
#include "boost/program_options/variables_map.hpp"
#include "boost/program_options/parsers.hpp"
#include "boost/optional.hpp"

#include "picojson.h"

#pragma warning(push, 0)
#define _WEBSOCKETPP_CPP11_STL_
#include "websocketpp/client.hpp"
#include "websocketpp/server.hpp"
#include "websocketpp/config/asio_no_tls.hpp"
#pragma warning(pop)

#include "windows.h"
