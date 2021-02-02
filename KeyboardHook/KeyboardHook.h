#pragma once
#include <Windows.h>
#ifndef DLLAPI
#define DLLAPI extern "C" __declspec(dllimport)
#endif

DLLAPI LRESULT CALLBACK MyHookProc(int, WPARAM, LPARAM);
DLLAPI int SetKeyboardHook(INT32 threadId, HWND winhandle, HWND trackhandle);
DLLAPI int ResetKeyboardHook();
DLLAPI void SetKeyboardEnable(bool gPlaying, bool gEnable);