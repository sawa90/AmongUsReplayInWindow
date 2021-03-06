// dllmain.cpp : DLL アプリケーションのエントリ ポイントを定義します。
#include "pch.h"
#ifndef DLLAPI
#define DLLAPI extern "C" __declspec(dllexport)
#endif
#include "KeyboardHook.h"

#pragma comment(linker, "/section:shared,rws")
#pragma data_seg("shared")
HHOOK hMyKeyboardHook = 0;
HHOOK hMyWndHook = 0;
bool playing = true;
bool enable = false;
HWND hwnd = NULL;
HWND trackhwnd = NULL;
HWND hOwnerWnd = NULL;
UINT32 keycode = VK_CONTROL;
#pragma data_seg()

HINSTANCE hInst;

BOOL WINAPI DllMain(HINSTANCE hInstDLL, DWORD dwReason, LPVOID lpReserved)
{
    switch (dwReason) {
    case DLL_PROCESS_ATTACH:
        hInst = hInstDLL;
        break;
    case DLL_PROCESS_DETACH:
        ResetKeyboardHook();
        if(IsWindow(hwnd))
            PostMessage(hwnd, WM_CLOSE, 0, 0);
        break;
    }
    return TRUE;
}


DLLAPI BOOL SetKeyboardHook(INT32 threadId, HWND winhandle, HWND trackhandle, HWND OwnerWndhandle)
{
    if (hMyKeyboardHook != NULL) ResetKeyboardHook();

    hwnd = winhandle;
    trackhwnd = trackhandle;
    hOwnerWnd = OwnerWndhandle;
    if(threadId == 0)return false;
    hMyKeyboardHook = SetWindowsHookEx(WH_KEYBOARD, MyHookProc, hInst, threadId);
    if (hMyKeyboardHook == NULL) {
        MessageBox(NULL, L"Failed to hook keyboard", L"Error", MB_OK); 
        return false;
    }
    hMyWndHook = SetWindowsHookEx(WH_CALLWNDPROC, (HOOKPROC)MyWndProc, hInst, threadId);
    if (hMyWndHook == NULL) {
        MessageBox(NULL, L"Failed to hook window procedure", L"Error", MB_OK);
        ResetKeyboardHook();
        return false;
    }
    SetZorder();
    return true;
}

DLLAPI BOOL ResetKeyboardHook()
{
    bool result = true;
    if (hMyKeyboardHook != 0) {
        if (UnhookWindowsHookEx(hMyKeyboardHook) != 0) {
            hMyKeyboardHook = 0;
        }
        else result = false;
    }
    if (hMyWndHook != 0) {
        if (UnhookWindowsHookEx(hMyWndHook) != 0) {
            hMyWndHook = 0;
        }
        else result = false;
    }
    return result;
}

DLLAPI void SetKeyboardEnable(BOOL gPlaying, BOOL gEnable) {
    playing = gPlaying;
    enable = gEnable;
}

DLLAPI void SetHotKey(UINT32 key) {
    keycode = key;
}

DLLAPI LRESULT CALLBACK MyHookProc(int nCode, WPARAM wp, LPARAM lp)
{

    if (nCode < 0 || !enable)
        return CallNextHookEx(hMyKeyboardHook, nCode, wp, lp);

    if (wp == keycode && (((UINT32)lp >> 30) == 0)) {
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

    return CallNextHookEx(hMyKeyboardHook, nCode, wp, lp);
}


DLLAPI LRESULT CALLBACK MyWndProc(int nCode, WPARAM wp, LPARAM lp) {
    CWPSTRUCT* pcwp;
    unsigned int SWP_Z = SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE | SWP_ASYNCWINDOWPOS;
    if (nCode < 0)
        return CallNextHookEx(hMyWndHook, nCode, wp, lp);
    if (nCode == HC_ACTION) {
        pcwp = (CWPSTRUCT*)lp;  
        if (pcwp->message == WM_ACTIVATE || (pcwp->message == WM_SIZE && pcwp->wParam == SIZE_MAXSHOW)) {          
            if ((pcwp->wParam & 0xFFFF) == WA_INACTIVE) {
                bool r1 = SetWindowPos(hwnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_Z);
                bool r2 = SetWindowPos(trackhwnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_Z);
                HWND hWndPrev = GetWindow(hOwnerWnd, GW_HWNDPREV);
                SetWindowPos(trackhwnd, hWndPrev, 0, 0, 0, 0, SWP_Z);
                SetWindowPos(hwnd, trackhwnd, 0, 0, 0, 0, SWP_Z);
            }
            else {
                bool r1 = SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_Z);
                bool r2 = SetWindowPos(trackhwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_Z);
                bool r3 = SetWindowPos(hwnd, trackhwnd, 0, 0, 0, 0, SWP_Z);
            }
        }
        int x, y;
        switch (pcwp->message) {
        case WM_SIZE:
                x = pcwp->lParam & 0xFFFF;
                y = (pcwp->lParam >> 16) & 0xFFFF;
                if (pcwp->wParam == SIZE_MINIMIZED) {
                    x = 0;
                    y = 0;
                }
                SetWindowPos(hwnd, 0, 0, 0, x, y, SWP_NOZORDER | SWP_NOMOVE | SWP_NOACTIVATE | SWP_ASYNCWINDOWPOS);
                break;
        case WM_MOVE:
                x = pcwp->lParam & 0xFFFF;
                y = (pcwp->lParam >> 16) & 0xFFFF;
                SetWindowPos(hwnd, 0, x, y, 0, 0, SWP_NOZORDER | SWP_NOSIZE | SWP_NOACTIVATE | SWP_ASYNCWINDOWPOS);
                break;
        case WM_CLOSE:
        case WM_DESTROY:
                PostMessage(hwnd, WM_CLOSE, 0, 0);
                break;
        default:
            break;
        }
            
    }
    return CallNextHookEx(hMyWndHook, nCode, wp, lp);
}

DLLAPI void SetZorder() {
    if (hwnd == 0 || trackhwnd == 0) return;
    bool active = GetForegroundWindow() == hOwnerWnd;
    unsigned int SWP_Z = SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE | SWP_ASYNCWINDOWPOS;
    if (!active) {
        bool r1 = SetWindowPos(hwnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_Z);
        bool r2 = SetWindowPos(trackhwnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_Z);
        HWND hWndPrev = GetWindow(hOwnerWnd, GW_HWNDPREV);
        SetWindowPos(trackhwnd, hWndPrev, 0, 0, 0, 0, SWP_Z);
        SetWindowPos(hwnd, trackhwnd, 0, 0, 0, 0, SWP_Z);
    }
    else {
        bool r1 = SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_Z);
        bool r2 = SetWindowPos(trackhwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_Z);
        bool r3 = SetWindowPos(hwnd, trackhwnd, 0, 0, 0, 0, SWP_Z);
    }
}


