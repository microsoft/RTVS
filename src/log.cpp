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
            std::mutex log_mutex;
            FILE* logfile;
            int indent;
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

            logfile = _fsopen(filename, "wc", _SH_DENYWR);
            if (!logfile) {
                std::string error = "Error creating logfile: " + std::string(filename) + "\r\n";
                fputs(error.c_str(), stderr);
                MessageBoxA(HWND_DESKTOP, error.c_str(), "Microsoft R Host", MB_OK | MB_ICONWARNING);
            }
        }

        void vlogf(const char* format, va_list va) {
            std::lock_guard<std::mutex> lock(log_mutex);

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


        void terminate(bool unexpected, const char* format, va_list va) {
            char message[0xFFFF];
            vsprintf_s(message, format, va);

            if (unexpected) {
                logf("Fatal error: ");
            }
            logf("%s\n", message);

            if (unexpected) {
                std::string msgbox_text;
                for (int i = 0; i < strlen(message); ++i) {
                    char c = message[i];
                    if (c == '\n') {
                        msgbox_text += '\r';
                    } 
                    msgbox_text += c;
                }
                
                assert(false);
                MessageBoxA(HWND_DESKTOP, msgbox_text.c_str(), "Microsoft R Host Process fatal error", MB_OK | MB_ICONERROR);
            }
            
            R_Suicide(message);
        }

        void terminate(const char* format, ...) {
            va_list va;
            va_start(va, format);
            terminate(false, format, va);
            va_end(format);
        }

        void fatal_error(const char* format, ...) {
            va_list va;
            va_start(va, format);
            terminate(true, format, va);
            va_end(format);
        }
    }
}
