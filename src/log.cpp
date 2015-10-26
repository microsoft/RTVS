#pragma once
#include "log.h"
#include "Rapi.h"

using namespace std::literals;


namespace rhost {
    namespace log {
        namespace {
            std::mutex log_mutex;
            FILE* logfile;
            int indent;

            void log_flush_thread() {
                for (;;) {
                    std::this_thread::sleep_for(1s);
                    flush_log();
                }
            }
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
            if (logfile) {
                // Logging happens often, so use a large buffer to avoid hitting the disk all the time.
                setvbuf(logfile, nullptr, _IOFBF, 0x100000);

                // Start a thread that will flush the buffer periodically.
                std::thread(log_flush_thread).detach();
            } else {
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

#ifndef NDEBUG
                // In Debug builds, flush on every write so that log is always up-to-date.
                // In Release builds, we rely on flush_log being called on process shutdown.
                fflush(logfile);
#endif
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

        void flush_log() {
            std::lock_guard<std::mutex> lock(log_mutex);
            if (logfile) {
                fflush(logfile);
            }
        }


        void terminate(bool unexpected, const char* format, va_list va) {
            char message[0xFFFF];
            vsprintf_s(message, format, va);

            if (unexpected) {
                logf("Fatal error: ");
            }
            logf("%s\n", message);
            flush_log();

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
