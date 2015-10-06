#include "stdafx.h"
#include "crtutils.h"

namespace rhost {
    namespace crt {
        static ::HMODULE load_msvcrt() {
            return ::LoadLibrary(L"msvcrt.dll");
        }

        static auto msvcrt_malloc = reinterpret_cast<void*(*)(size_t)>(GetProcAddress(load_msvcrt(), "malloc"));
        static auto msvcrt_free = reinterpret_cast<void(*)(void*)>(GetProcAddress(load_msvcrt(), "free"));
        static auto msvcrt_atexit = reinterpret_cast<int(*)(void(*)())>(GetProcAddress(load_msvcrt(), "atexit"));

        void * malloc(size_t size) {
            return msvcrt_malloc(size);
        }

        void free(void *memblock) {
            msvcrt_free(memblock);
        }

        int atexit(void(__cdecl *func)(void)) {
            return msvcrt_atexit(func);
        }
    }
}
