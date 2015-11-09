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
namespace rplots {
    class RPlotHost {
    public:
        static void Init(HWND handle);
        static void Terminate();

    private:
        RPlotHost(HWND wndPlotWindow);

        static LRESULT CALLBACK CBTProc(_In_ int nCode, _In_ WPARAM wParam, _In_ LPARAM lParam);

        static HWND m_hwndPlotWindow;
        static HHOOK m_hOldHook;
        static bool m_fProcessing;
        static std::unique_ptr<RPlotHost> m_pInstance;
    };
}