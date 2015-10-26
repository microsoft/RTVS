#pragma once
#include "stdafx.h"

namespace rhost {
    namespace log {
        void init_log();

        void vlogf(const char* format, va_list va);

        void logf(const char* format, ...);

        void indent_log(int n);

        void flush_log();

        __declspec(noreturn) void terminate(const char* format, ...);

        __declspec(noreturn) void fatal_error(const char* format, ...);
    }
}
