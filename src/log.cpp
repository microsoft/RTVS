#pragma once
#include "log.h"
#include "Rapi.h"

// For failure to init logging we always want to assert, even in Release builds.
#pragma push_macro("NDEBUG")
#undef NDEBUG
#include <cassert>
#pragma pop_macro("NDEBUG")


namespace rhost {
    namespace log {
        namespace {
            FILE* logfile;
            std::atomic<int> indent;
        }

        void init_log() {
            char filename[MAX_PATH + 1 + MAX_PATH] = {};
            GetTempPathA(sizeof filename, filename);

            time_t t;
            time(&t);

            tm tm;
            localtime_s(&tm, &t);

            size_t len = strlen(filename);
            strftime(filename + len, sizeof filename - len, "/Microsoft.R.Host_%Y%m%d_%H%M%S.log", &tm);

            logfile = fopen(filename, "wc");
            if (!logfile) {
                std::string error = "Error creating logfile: " + std::string(filename) + "\r\n";
                fputs(error.c_str(), stderr);
                MessageBoxA(HWND_DESKTOP, error.c_str(), "Microsoft R Host", MB_OK | MB_ICONWARNING);
            }
        }

        void vlogf(const char* format, va_list va) {
#ifndef NDEBUG
            va_list va2;
            va_copy(va2, va);
#endif

            if (logfile) {
                for (int i = 0; i < indent; ++i) {
                    fputc('\t', logfile);
                }
                vfprintf(logfile, format, va);
                fflush(logfile);
            }

#ifndef NDEBUG
            for (int i = 0; i < indent; ++i) {
                fputc('\t', stderr);
            }
            vfprintf(stderr, format, va2);
            va_end(va2);
#endif
        }

        void logf(const char* format, ...) {
            va_list va;
            va_start(va, format);
            vlogf(format, va);
            va_end(format);
        }

        void indent_log(int n) {
            indent += n;
            if (indent < 0) {
                indent = 0;
            }
        }
    }
}
