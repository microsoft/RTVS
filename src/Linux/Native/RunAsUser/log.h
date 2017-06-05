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

#pragma once
#include "stdafx.h"

//TODO: Unify with log from RHost https://github.com/Microsoft/RTVS/issues/3590
namespace rau {
    namespace log {
        enum class log_verbosity {
            none,
            minimal,
            normal,
            traffic
        };

        enum class log_level {
            trace,
            information,
            warning,
            error
        };

        void init_log(const std::string& log_suffix, const fs::path& log_dir, log_verbosity log_level);

        void vlogf(log_verbosity level, log_level message_type, const char* format, va_list va);

        inline void logf(log_verbosity verbosity, log_level message_type, const char* format, ...) {
            va_list va;
            va_start(va, format);
            vlogf(verbosity, message_type, format, va);
            va_end(va);
        }

        inline void vlogf(log_verbosity verbosity, const char* format, va_list va) {
            vlogf(verbosity, log_level::trace, format, va);
        }

        inline void logf(log_verbosity verbosity, const char* format, ...) {
            va_list va;
            va_start(va, format);
            vlogf(verbosity, format, va);
            va_end(va);
        }

        void indent_log(int n);

        void flush_log();

        __attribute__((noreturn)) void terminate(const char* format, ...);

        __attribute__((noreturn)) void fatal_error(const char* format, ...);
    }
}
