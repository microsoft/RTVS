#pragma once
#include "stdafx.h"

namespace rhost {
    namespace msvcrt {
        extern void* (*malloc)(size_t size);
        extern void (*free)(void *memblock);
        extern int (*atexit)(void(*func)(void));
        extern int (*vsnprintf)(char*, size_t, const char*, va_list);
    }
}
