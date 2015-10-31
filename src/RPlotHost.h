#pragma once
class RPlotHost {
public:
    static void Init(HWND handle);
    static void Terminate();

private:
    RPlotHost(HWND wndPlotWindow);
    ~RPlotHost();

    static LRESULT CALLBACK CBTProc(_In_ int nCode, _In_ WPARAM wParam, _In_ LPARAM lParam);

    static HWND m_hwndPlotWindow;
    static HHOOK m_hOldHook;
    static bool m_fProcessing;
    static RPlotHost* m_pInstance;
};