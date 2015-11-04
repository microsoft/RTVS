#include "stdafx.h"
#include "msvcrt.h"

#define MSVCRT_EXPORT(name) \
    decltype(name) name = reinterpret_cast<decltype(name)>(GetProcAddress(get_msvcrt(), #name))

namespace rhost {
    namespace msvcrt {
        namespace {
            HMODULE get_msvcrt() {
                static HMODULE msvcrt = LoadLibrary(L"msvcrt.dll");
                return msvcrt;
            }
        }

        MSVCRT_EXPORT(malloc);
        MSVCRT_EXPORT(free);
        MSVCRT_EXPORT(atexit);
        MSVCRT_EXPORT(vsnprintf);
    }
}
