#include <windows.h>
#include "NsHiJack.h"
#include <iostream>
#include "hook.h"

HANDLE hConsole;
bool isHook = false;
bool consoleOpened = false;

void OpenConsole() {

    AllocConsole();
    freopen_s((FILE**)stdout, "CONOUT$", "w", stdout);
    freopen_s((FILE**)stderr, "CONOUT$", "w", stderr);
    freopen_s((FILE**)stdin, "CONIN$", "r", stdin);
    hConsole = GetStdHandle(STD_OUTPUT_HANDLE);
    SetConsoleTitleA("DLL Debug Console");
    SetConsoleOutputCP(936);
    std::cout << "[INFO]碧之轨迹NISA版本GBK读取DLL已载入" << std::endl;
}

void CloseConsole() {
    fclose(stdout);
    fclose(stderr);
    fclose(stdin);
    FreeConsole();
}


BOOL APIENTRY DllMain(HMODULE hModule,
    DWORD  ul_reason_for_call,
    LPVOID lpReserved
)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    {
#if HIJACK
        if (!NsInitDll()) {
            MessageBoxA(NULL, "NsInitDll失败", "错误", 0);
            return false;
        }
#endif // HIJACK
        std::string cmdLine = std::string(GetCommandLineA());
        if (cmdLine.find("-debug") != std::string::npos) {
            OpenConsole();
            consoleOpened = true;
			hook::is_debug = true;
        }
        if (cmdLine.find("-nohook") == std::string::npos) {
            encoding::iconv_initialize();
            hook::hook_install();
            isHook = true;
            if (!hook::isMatchSuccessful) {
                MessageBoxA(NULL, "地址匹配失败，Hook失败。", "错误", 0);
            }
        }
    }
    case DLL_THREAD_ATTACH:
        break;
    case DLL_THREAD_DETACH:
        break;
    case DLL_PROCESS_DETACH:
        if (isHook)
        {
            hook::hook_uninstall();
            encoding::iconv_close();
            isHook = false;
        }
        if (consoleOpened)
        {
            CloseConsole();
            consoleOpened = false;
        }
        break;
    }
    return TRUE;
}