#include <windows.h>
#include "NsHiJack.h"
#include <iostream>
#include "hook.h"

HANDLE hConsole = nullptr;
bool isHook = false;
bool consoleOpened = false;

static bool HasCmdArg(const char* arg) {
	return std::string(GetCommandLineA()).find(arg) != std::string::npos;
}

static void OpenConsole() {
	AllocConsole();
	FILE* fp = nullptr;
	freopen_s(&fp, "CONOUT$", "w", stdout);
	freopen_s(&fp, "CONOUT$", "w", stderr);
	freopen_s(&fp, "CONIN$", "r", stdin);
	hConsole = GetStdHandle(STD_OUTPUT_HANDLE);
	SetConsoleOutputCP(936);
	SetConsoleTitleA("DLL Debug Console");
	printf("[INFO]ED7 NISA GBK DLL已载入\n");
}

static void CloseConsole() {
	fclose(stdout);
	fclose(stderr);
	fclose(stdin);
	FreeConsole();
}

static void Start() {
	if (HasCmdArg("-debug")) {
		OpenConsole();
		consoleOpened = true;
		hook::is_debug = true;
	}
	if (!HasCmdArg("-nohook")) {
		encoding::iconv_initialize();
		if (!hook::hook_install() || !hook::isMatchSuccessful)
		{
#if defined(ZERO)
			MessageBoxW(NULL,
				L"Hook失败。\n补丁基于《零之轨迹》v1.5.7 制作，请确认游戏版本。",
				L"汉化补丁 错误", 0);
#elif defined(AO)
			MessageBoxW(NULL,
				L"Hook失败。\n补丁基于《碧之轨迹》v1.2.7 制作，请确认游戏版本。",
				L"汉化补丁 错误", 0);
#endif
		}
		else {
			isHook = true;
		}
	}
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
	(void)lpReserved;
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	{
		DisableThreadLibraryCalls(hModule);
#if defined(HIJACK)
		if (!NsInitDll()) {
			MessageBoxW(NULL, L"NsInitDll失败", L"错误", 0);
			return FALSE;
		}
#endif
		Start();
		break;
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