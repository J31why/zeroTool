// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include "pch.h"
#include "hook.h"

HANDLE hConsole;

void OpenConsole() {

    AllocConsole();
    freopen_s((FILE**)stdout, "CONOUT$", "w", stdout);
    freopen_s((FILE**)stderr, "CONOUT$", "w", stderr);
    freopen_s((FILE**)stdin, "CONIN$", "r", stdin);
    hConsole = GetStdHandle(STD_OUTPUT_HANDLE);
    SetConsoleTitleA("DLL Debug Console");
    SetConsoleOutputCP(936);
    cout << "[INFO]零之轨迹NISA版本GBK读取DLL已载入" << endl ;
}

void CloseConsole() {
    fclose(stdout);
    fclose(stderr);
    fclose(stdin);
    FreeConsole();
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved) {
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    {
        LPCSTR cmdLine = GetCommandLineA();
        if (string(cmdLine).find("debug") !=-1) {
            OpenConsole();
        }
        encoding::iconv_initialize();
        hook::hook_install();
        break;
    }
    case DLL_THREAD_ATTACH:
        break;
    case DLL_THREAD_DETACH:
        break;
    case DLL_PROCESS_DETACH:
    {
        hook::hook_uninstall();
        encoding::iconv_close();
        CloseConsole();
    }
    break;
    }
    return TRUE;
}

