#pragma once
#include "stdafx.h"
#include "Rapi.h"

#define SCOPE_WARDEN(NAME, ...)                \
    auto xx##NAME##xx = [&]() { __VA_ARGS__ }; \
    ::rhost::util::scope_warden<decltype(xx##NAME##xx)> NAME(xx##NAME##xx)

#define SCOPE_WARDEN_RESTORE(NAME) \
    auto NAME##_old_value = (NAME); \
    SCOPE_WARDEN(restore_##NAME, (NAME) = NAME##_old_value;)

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

            void __declspec(nothrow) run() {
                if (_p) {
                    (*_p)();
                }
                dismiss();
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


        std::string to_utf8(const char* buf, size_t len);

        inline std::string to_utf8(const std::string& s) {
            return to_utf8(s.data(), s.size());
        }

        inline picojson::value to_utf8_json(const char* buf) {
            return buf ? picojson::value(to_utf8(buf)) : picojson::value();
        }

        std::string from_utf8(const std::string& u8s);


        template<class Arg>
        inline void append(picojson::array& msg, Arg&& arg) {
            msg.push_back(picojson::value(std::forward<Arg>(arg)));
        }

        template<class Arg, class... Args>
        inline void append(picojson::array& msg, Arg&& arg, Args&&... args) {
            msg.push_back(picojson::value(std::forward<Arg>(arg)));
            append(msg, std::forward<Args>(args)...);
        }
    }
}

namespace boost {
    namespace asio {
        namespace ip {
            // Enable boost::asio::ip::tcp::endpoint to be used with boost::program_options.
            void validate(boost::any& v, const std::vector<std::string> values, boost::asio::ip::tcp::endpoint*, int);
        }
    }
}

namespace websocketpp {
    // Enable websocketpp::uri to be used with boost::program_options.
    void validate(boost::any& v, const std::vector<std::string> values, websocketpp::uri*, int);
}
