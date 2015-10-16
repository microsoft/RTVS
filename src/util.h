#pragma once
#include "stdafx.h"

#define SCOPE_WARDEN(NAME, ...)                \
    auto xx##NAME##xx = [&]() { __VA_ARGS__ }; \
    ::rhost::util::scope_warden<decltype(xx##NAME##xx)> NAME(xx##NAME##xx)

namespace rhost {
    namespace util {
        template <typename F> class scope_warden {
        public:
            explicit __declspec(nothrow) scope_warden(F& f)
                : m_p(std::addressof(f)) {
            }

            void __declspec(nothrow) dismiss() {
                m_p = nullptr;
            }

            __declspec(nothrow) ~scope_warden() {
                if (m_p) {
                    try {
                        (*m_p)();
                    } catch (...) {
                        std::terminate();
                    }
                }
            }

        private:
            F * m_p;

            explicit scope_warden(F&&) = delete;
            scope_warden(const scope_warden&) = delete;
            scope_warden& operator=(const scope_warden&) = delete;
        };


        std::string to_utf8(const char* buf, size_t len) {
            auto& codecvt_wchar = std::use_facet<std::codecvt<wchar_t, char, std::mbstate_t>>(std::locale());
            std::wstring_convert<std::codecvt<wchar_t, char, std::mbstate_t>> convert(&codecvt_wchar);
            auto ws = convert.from_bytes(buf, buf + len);

            std::wstring_convert<std::codecvt_utf8<wchar_t>> codecvt_utf8;
            return codecvt_utf8.to_bytes(ws);
        }

        std::string to_utf8(const std::string& s) {
            return to_utf8(s.data(), s.size());
        }

        std::string from_utf8(const std::string& u8s) {
            std::wstring_convert<std::codecvt_utf8<wchar_t>> codecvt_utf8;
            auto ws = codecvt_utf8.from_bytes(u8s);

            auto& codecvt_wchar = std::use_facet<std::codecvt<wchar_t, char, std::mbstate_t>>(std::locale());
            std::wstring_convert<std::codecvt<wchar_t, char, std::mbstate_t>> convert(&codecvt_wchar);
            return convert.to_bytes(ws);
        }

        __declspec(noreturn) void fatal_error(const char* format, va_list va) {
            char message[0xFFFF];
            vsprintf_s(message, format, va);
            fprintf(stderr, "ERROR: %s\n", message);
            R_Suicide(message);
        }

        __declspec(noreturn) void fatal_error(const char* format, ...) {
            va_list va;
            va_start(va, format);
            fatal_error(format, va);
            va_end(format);
        }
    }
}