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
#include "log.h"

using namespace std::literals;

namespace rau {
    namespace log {
        namespace {
            std::mutex log_mutex, terminate_mutex;
            fs::path log_filename;
            FILE* logfile;
            int indent;
            log::log_verbosity current_verbosity;

            void log_flush_thread() {
                for (;;) {
                    std::this_thread::sleep_for(1s);
                    flush_log();
                }
            }
        }
        void init_log(const std::string& log_suffix, const fs::path& log_dir, log::log_verbosity verbosity) {
            {
                current_verbosity = verbosity;

                std::string filename = "Microsoft.R.Host.RunAsUser_";
                if (!log_suffix.empty()) {
                    filename += log_suffix + "_";
                }

                time_t t;
                time(&t);

                tm tm;
                localtime_r(&t, &tm);

                size_t len = filename.size();
                filename.resize(len + 1 + PATH_MAX);
                auto it = filename.begin() + len;
                strftime(&*it, filename.end() - it, "%Y%m%d_%H%M%S", &tm);
                filename.resize(strlen(filename.c_str()));

                // Add PID to prevent conflicts in case two hosts with the same suffix
                // get started at the same time.
                filename += "_pid" + std::to_string(getpid());

                log_filename = log_dir / (filename + ".log");
            }

            logfile = fopen(log_filename.make_preferred().string().c_str(), "w");
            if (logfile) {
                // Logging happens often, so use a large buffer to avoid hitting the disk all the time.
                setvbuf(logfile, nullptr, _IOFBF, 0x100000);

                // Start a thread that will flush the buffer periodically.
                std::thread(log_flush_thread).detach();
            } else {
                std::string error = "Error creating logfile: " + log_filename.make_preferred().string() + "\r\n";
                fprintf(stderr, "Error: %d\r\n", errno);
                fputs(error.c_str(), stderr);
            }
        }

        void vlogf(log_verbosity verbosity, log_level message_type, const char* format, va_list va) {
            if (verbosity > current_verbosity) {
                return;
            }

            std::lock_guard<std::mutex> lock(log_mutex);

            va_list va2;
            va_copy(va2, va);

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

            // Don't log trace level messages to stderr by default.
            if (message_type != log_level::trace) {
                for (int i = 0; i < indent; ++i) {
                    fputc('\t', stderr);
                }
                vfprintf(stderr, format, va2);
            }

            va_end(va2);
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
            std::lock_guard<std::mutex> terminate_lock(terminate_mutex);

            char message[0xFFFF];
            vsprintf(message, format, va);

            if (unexpected) {
                logf(log_verbosity::minimal, "Fatal error: ");
            }
            logf(log_verbosity::minimal, unexpected ? log_level::error : log_level::information, "%s\n", message);
            flush_log();
            std::terminate();
        }

        void terminate(const char* format, ...) {
            va_list va;
            va_start(va, format);
            terminate(false, format, va);
            va_end(va);
        }

        void fatal_error(const char* format, ...) {
            va_list va;
            va_start(va, format);
            terminate(true, format, va);
            va_end(va);
        }
    }
}

