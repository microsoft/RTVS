#pragma once
#include "stdafx.h"
#include "Rapi.h"

#define SCOPE_WARDEN(NAME, ...)                \
    auto xx##NAME##xx = [&]() { __VA_ARGS__ }; \
    ::rhost::util::scope_warden<decltype(xx##NAME##xx)> NAME(xx##NAME##xx)

namespace rhost {
    namespace util {
        template<typename F>
        class scope_warden {
        public:
            explicit __declspec(nothrow) scope_warden(F& f)
                : _p(std::addressof(f)) {
            }

            void __declspec(nothrow) dismiss() {
                _p = nullptr;
            }

            __declspec(nothrow) ~scope_warden() {
                if (_p) {
                    try {
                        (*_p)();
                    } catch (...) {
                        std::terminate();
                    }
                }
            }

        private:
            F* _p;

            explicit scope_warden(F&&) = delete;
            scope_warden(const scope_warden&) = delete;
            scope_warden& operator=(const scope_warden&) = delete;
        };


        struct SEXP_delete {
            typedef SEXP pointer;

            void operator() (SEXP sexp) {
                if (sexp) {
                    R_ReleaseObject(sexp);
                }
            }
        };

        typedef std::unique_ptr<SEXP, SEXP_delete> unique_sexp;


        __declspec(noreturn) void fatal_error(const char* format, va_list va);

        __declspec(noreturn) void fatal_error(const char* format, ...);

        std::string to_utf8(const char* buf, size_t len);

        inline std::string to_utf8(const std::string& s) {
            return to_utf8(s.data(), s.size());
        }

        std::string from_utf8(const std::string& u8s);
    }
}