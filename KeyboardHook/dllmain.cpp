// dllmain.cpp : DLL アプリケーションのエントリ ポイントを定義します。
#include "pch.h"
#ifndef DLLAPI
#define DLLAPI extern "C" __declspec(dllexport)
#endif
#include "KeyboardHook.h"

#pragma comment(linker, "/section:shared,rws")
#pragma data_seg("shared")
HHOOK hMyHook = 0;
bool playing = true;
bool enable = false;
HWND hwnd = NULL;
HWND trackhwnd = NULL;
#pragma data_seg()

HINSTANCE hInst;

BOOL WINAPI DllMain(HINSTANCE hInstDLL, DWORD dwReason, LPVOID lpReserved)
{
    switch (dwReason) {
    case DLL_PROCESS_ATTACH:
        hInst = hInstDLL;
        break;
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

DLLAPI int SetKeyboardHook(INT32 threadId, HWND winhandle, HWND trackhandle)
{
    if (hMyHook != NULL) ResetKeyboardHook();

    hwnd = winhandle;
    trackhwnd = trackhandle;
    hMyHook = SetWindowsHookEx(WH_KEYBOARD, MyHookProc, hInst, threadId);
    if (hMyHook == NULL)MessageBox(NULL, L"Failed to hook keyboard", L"Error", MB_OK);
    return 0;
}

DLLAPI int ResetKeyboardHook()
{
    if (hMyHook != 0) {
        if (UnhookWindowsHookEx(hMyHook) != 0) {
            hMyHook = 0;
        }
    }
    return 0;
}

DLLAPI void SetKeyboardEnable(bool gPlaying, bool gEnable) {
    playing = gPlaying;
    enable = gEnable;
}

DLLAPI LRESULT CALLBACK MyHookProc(int nCode, WPARAM wp, LPARAM lp)
{

    if (nCode < 0 || !enable)
        return CallNextHookEx(hMyHook, nCode, wp, lp);

    if (wp == VK_CONTROL && (((UINT32)lp >> 30) == 0)) {
        if (IsWindowVisible(hwnd)) {
            ShowWindow(hwnd, SW_HIDE);
            ShowWindow(trackhwnd, SW_HIDE);
        }
        else {
            ShowWindow(hwnd, SW_SHOWNA);
            if(!playing)
                ShowWindow(trackhwnd, SW_SHOW);
        }
    } 

    return CallNextHookEx(hMyHook, nCode, wp, lp);
}


