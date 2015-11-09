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
#include "Rapi.h"
#include "msvcrt.h"
#include "util.h"

using namespace rhost::util;

namespace rhost {
    namespace r_util {
        class memory_connection {
        public:
            const SEXP connection_sexp;

            static memory_connection* create(int max_size = R_NaInt, int expected_size = R_NaInt) {
                Rconnection conn;
                auto conn_sexp = R_new_custom_connection("", "w", "memory_connection", &conn);
                return new memory_connection(conn, conn_sexp, max_size, expected_size);
            }

            static memory_connection* create(SEXP max_size, SEXP expected_size) {
                return create(Rf_asInteger(max_size), Rf_asInteger(expected_size));
            }

            static memory_connection* of_connection_sexp(SEXP conn_sexp) {
                auto it = _instances.find(conn_sexp);
                if (it == _instances.end()) {
                    throw std::exception("Connection is not a memory_connection");
                }
                return it->second;
            }

            Rboolean open() {
                return R_TRUE;
            }

            void close() {
                _data.clear();
            }

            int vfprintf(const char* format, va_list va) {
                int count;

                // Try with a reasonably large stack allocated buffer first.
                // Use vsnprintf from msvcrt, since that is what R normally uses, and there are differences.
                va_list va2;
                va_copy(va2, va);
                char buf[0x1000], *pbuf = buf;
                count = msvcrt::vsnprintf(buf, sizeof buf, format, va2);
                va_end(va2);

                std::unique_ptr<char[]> buf_deleter;
                if (count < 0) { // error
                    return count;
                } else if (count >= sizeof buf) {
                    // It didn't fit in the stack buffer, so heap-allocate a buffer of just the right size.
                    buf_deleter.reset(pbuf = new char[count + 1]);
                    va_copy(va2, va);
                    count = msvcrt::vsnprintf(pbuf, count + 1, format, va2);
                    va_end(va2);
                    if (count < 0) { // error
                        return count;
                    }
                }

                if (!_eof_marker.empty()) {
                    if (char* eof = strstr(pbuf, _eof_marker.c_str())) {
                        *eof = '\0';
                        _seen_eof = true;
                    }
                }

                _data.append(pbuf);
                if (_max_size != R_NaInt && _data.size() > _max_size) {
                    _data.resize(_max_size - _overflow_suffix.size());
                    _data += _overflow_suffix;
                    _overflown = true;
                    throw std::exception("Connection size limit exceeded");
                }

                if (_seen_eof) {
                    throw std::exception("EOF marker encountered");
                }

                return count;
            }

            const std::string& overflow_suffix() const {
                return _overflow_suffix;
            }

            const std::string& overflow_suffix(const std::string& value) {
                if (value.size() > _max_size) {
                    throw std::invalid_argument("max_size is not large enough to fit overflow_suffix");
                }
                return _overflow_suffix = value;
            }

            const std::string& overflow_suffix(SEXP value) {
                if (Rf_isNull(value)) {
                    return overflow_suffix("");
                }

                unique_sexp value_char(Rf_asChar(value));
                return overflow_suffix(R_CHAR(value_char.get()));
            }

            const std::string& eof_marker() const {
                return _eof_marker;
            }

            const std::string& eof_marker(const std::string& value) {
                return _eof_marker = value;
            }

            const std::string& eof_marker(SEXP value) {
                if (Rf_isNull(value)) {
                    return eof_marker("");
                }

                unique_sexp value_char(Rf_asChar(value));
                return eof_marker(R_CHAR(value_char.get()));
            }

            const std::string& data() const {
                return _data;
            }

            SEXP data_sexp() const {
                return Rf_mkString(_data.c_str());
            }

            bool overflown() const {
                return _overflown;
            }

            SEXP overflown_sexp() const {
                return _overflown ? R_TrueValue : R_FalseValue;
            }

        private:
            static std::unordered_map<SEXP, memory_connection*> _instances;

            Rconnection _conn;
            int _max_size;
            std::string _data, _overflow_suffix, _eof_marker;
            bool _overflown, _seen_eof;

            memory_connection(Rconnection conn, SEXP conn_sexp, int max_size, int expected_size):
                connection_sexp(conn_sexp),
                _conn(conn),
                _max_size(max_size),
                _overflown(false),
                _seen_eof(false)
            {
                if (expected_size > 0 && expected_size != R_NaInt) {
                    _data.reserve(expected_size);
                }

                _conn->private_ = this;
                _conn->isopen = R_TRUE;
                _conn->canwrite = R_TRUE;

                _conn->destroy = [](Rconnection conn) {
                    delete reinterpret_cast<memory_connection*>(conn->private_);
                };

                _conn->open = [](Rconnection conn) {
                    return exceptions_to_errors([&] {
                        return reinterpret_cast<memory_connection*>(conn->private_)->open();
                    });
                };

                _conn->close = [](Rconnection conn) {
                    return exceptions_to_errors([&] {
                        return reinterpret_cast<memory_connection*>(conn->private_)->close();
                    });
                };

                _conn->vfprintf = [](Rconnection conn, const char* format, va_list va) {
                    return exceptions_to_errors([&] {
                        return reinterpret_cast<memory_connection*>(conn->private_)->vfprintf(format, va);
                    });
                };

                _instances[conn_sexp] = this;
            }

            ~memory_connection() {
                _instances.erase(connection_sexp);
                _conn->private_ = nullptr;
            }
        };

        std::unordered_map<SEXP, memory_connection*> memory_connection::_instances;


        extern "C" SEXP unevaluated_promise(SEXP name, SEXP env) {
            if (!Rf_isEnvironment(env)) {
                Rf_error("env is not an environment");
            }
            if (!Rf_isString(name) || Rf_length(name) != 1) {
                Rf_error("name is not a single string");
            }

            SEXP value = Rf_findVar(Rf_installChar(STRING_ELT(name, 0)), env);
            if (TYPEOF(value) != PROMSXP || PRVALUE(value) != R_UnboundValue) {
                return R_NilValue;
            }

            return PRCODE(value);
        }

        extern "C" SEXP memory_connection_new(SEXP max_size, SEXP expected_size, SEXP overflow_suffix, SEXP eof_marker) {
            return exceptions_to_errors([&] {
                auto btc = memory_connection::create(max_size, expected_size);
                btc->overflow_suffix(overflow_suffix);
                btc->eof_marker(eof_marker);
                return btc->connection_sexp;
            });
        }

        extern "C" SEXP memory_connection_tochar(SEXP conn_sexp) {
            return exceptions_to_errors([&] {
                return memory_connection::of_connection_sexp(conn_sexp)->data_sexp();
            });
        }

        extern "C" SEXP memory_connection_overflown(SEXP conn_sexp) {
            return exceptions_to_errors([&] {
                return memory_connection::of_connection_sexp(conn_sexp)->overflown_sexp();
            });
        }

        R_CallMethodDef call_methods[] = {
            { "rtvs::Call.unevaluated_promise", (DL_FUNC)unevaluated_promise, 2 },
            { "rtvs::Call.memory_connection", (DL_FUNC)memory_connection_new, 4 },
            { "rtvs::Call.memory_connection_tochar", (DL_FUNC)memory_connection_tochar, 1 },
            { "rtvs::Call.memory_connection_overflown", (DL_FUNC)memory_connection_overflown, 1 },
            { }
        };

        void init(DllInfo *dll) {
            R_registerRoutines(dll, nullptr, call_methods, nullptr, nullptr);
        }
    }
}

