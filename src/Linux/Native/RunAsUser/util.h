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

#define SCOPE_WARDEN(NAME, ...) \
    auto xx##NAME##xx = [&](){ __VA_ARGS__ }; \

template<typename F> class scope_warden {
public:
    scope_warden(F& f) noexcept
        : _p(std::addressof(f)) {}

    void dismiss() noexcept {
        _p = nullptr;
    }

    void run() noexcept {
        if (_p) {
            (*_p)();
        }
        dismiss();
    }

    ~scope_warden() noexcept {
        if (_p) {
            try {
                (*_p)();
            }
            catch (...) {
                std::terminate();
            }
        }
    }

private:
    F* _p;

    explicit scope_warden(F&&) = delete;
    scope_warden(const scope_warden&) = delete;
    scope_warden& operator=(const scope_warden&) = delete;
};


inline void append_json(picojson::array& msg) {
}

template<class Arg>
inline void append_json(picojson::array& msg, Arg&& arg) {
    msg.push_back(picojson::value(std::forward<Arg>(arg)));
}

template<class Arg, class... Args>
inline void append_json(picojson::array& msg, Arg&& arg, Args&&... args) {
    msg.push_back(picojson::value(std::forward<Arg>(arg)));
    append_json(msg, std::forward<Args>(args)...);
}

