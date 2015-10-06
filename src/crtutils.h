#pragma once

namespace rhost {
    namespace crt {
        void * malloc(size_t size);
        void free(void *memblock);
        int atexit(void(__cdecl *func)(void));
    }
}
