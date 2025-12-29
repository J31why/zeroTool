#include <windows.h>
#include "NsHiJack.h"
#include <iostream>
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
    std::cout << "[INFO]零之轨迹NISA版本GBK读取DLL已载入" << std::endl;
}

void CloseConsole() {
    fclose(stdout);
    fclose(stderr);
    fclose(stdin);
    FreeConsole();
}

bool isHook = false;
bool consoleOpened = false;

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                      )
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		{
			if (!NsInitDll())
				return false;
            LPCSTR cmdLine = GetCommandLineA();
            if (std::string(cmdLine).find(" -debug") != -1) {
                OpenConsole();
                consoleOpened = true;
            }
            if (std::string(cmdLine).find(" -nohook") == -1) {
                encoding::iconv_initialize();
                hook::hook_install();
                isHook = true;
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