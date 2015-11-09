/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved. 
 *
 *
 * This file is part of Microsoft R Host.
 * 
 * Microsoft R Host is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 2 of the License, or
 * (at your option) any later version.
 *
 * Microsoft R Host is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Microsoft R Host.  If not, see <http://www.gnu.org/licenses/>.
 *
 * ***************************************************************************/

#include "stdafx.h"
#include "util.h"

namespace po = boost::program_options;

namespace rhost {
    namespace util {
        std::string to_utf8(const char* buf, size_t len) {
            auto& codecvt_wchar = std::use_facet<std::codecvt<wchar_t, char, std::mbstate_t>>(std::locale());
            std::wstring_convert<std::codecvt<wchar_t, char, std::mbstate_t>> convert(&codecvt_wchar);
            auto ws = convert.from_bytes(buf, buf + len);

            std::wstring_convert<std::codecvt_utf8<wchar_t>> codecvt_utf8;
            return codecvt_utf8.to_bytes(ws);
        }

        std::string from_utf8(const std::string& u8s) {
            std::wstring_convert<std::codecvt_utf8<wchar_t>> codecvt_utf8;
            auto ws = codecvt_utf8.from_bytes(u8s);

            auto& codecvt_wchar = std::use_facet<std::codecvt<wchar_t, char, std::mbstate_t>>(std::locale());
            std::wstring_convert<std::codecvt<wchar_t, char, std::mbstate_t>> convert(&codecvt_wchar);
            return convert.to_bytes(ws);
        }
    }
}

namespace boost {
    namespace asio {
        namespace ip {
            void validate(boost::any& v, const std::vector<std::string> values, boost::asio::ip::tcp::endpoint*, int) {
                po::validators::check_first_occurrence(v);
                auto& s = po::validators::get_single_string(values);

                std::string host;
                uint16_t port;
                size_t colon = s.find(':');
                if (colon == s.npos) {
                    host = s;
                    port = 5118;
                } else {
                    host = s.substr(0, colon);
                    auto port_str = s.substr(colon + 1);
                    try {
                        port = std::stoi(port_str);
                    } catch (std::invalid_argument&) {
                        throw po::validation_error(po::validation_error::invalid_option_value);
                    } catch (std::out_of_range&) {
                        throw po::validation_error(po::validation_error::invalid_option_value);
                    }
                }

                boost::asio::ip::address address;
                try {
                    address = boost::asio::ip::address::from_string(host);
                } catch (boost::system::system_error&) {
                    throw po::validation_error(po::validation_error::invalid_option_value);
                }

                v = boost::any(boost::asio::ip::tcp::endpoint(address, port));
            }
        }
    }
}

namespace websocketpp {
    void validate(boost::any& v, const std::vector<std::string> values, websocketpp::uri*, int) {
        po::validators::check_first_occurrence(v);
        auto& s = po::validators::get_single_string(values);

        websocketpp::uri uri(s);
        if (!uri.get_valid()) {
            throw po::validation_error(po::validation_error::invalid_option_value);
        }

        v = boost::any(uri);
    }
}
