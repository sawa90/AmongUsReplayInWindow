#pragma once
#include <Windows.h>
#ifndef DLLAPI
#define DLLAPI extern "C" __declspec(dllimport)
#endif

DLLAPI LRESULT CALLBACK MyHookProc(int, WPARAM, LPARAM);
DLLAPI LRESULT CALLBACK MyWndProc(int nCode, WPARAM wp, LPARAM lp);
DLLAPI BOOL SetKeyboardHook(INT32 threadId, HWND winhandle, HWND trackhandle, HWND OwnerWndhandle);
DLLAPI BOOL ResetKeyboardHook();
DLLAPI void SetKeyboardEnable(BOOL gPlaying, BOOL gEnable);
DLLAPI void SetZorder();