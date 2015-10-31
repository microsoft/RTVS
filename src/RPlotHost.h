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