// injector.cpp - 支持创建并挂起进程的 DLL 注入器

#define _WIN32_WINNT 0x0500  // Windows 2000 或更高

#include <windows.h>          // 基础 Win32 API
#include <tlhelp32.h>         // 进程快照相关：CreateToolhelp32Snapshot 等
#include <stdio.h>
#include <string>

// 将 DLL 注入指定 PID 的进程
bool InjectDLL(DWORD targetPid, const wchar_t* dllPath) {
    HANDLE hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, targetPid);
    if (!hProcess) {
        printf("[-] 无法打开目标进程 (PID: %u)\n", targetPid);
        return false;
    }

    // 分配内存
    size_t dllSize = (wcslen(dllPath) + 1) * sizeof(wchar_t);
    LPVOID pRemoteMem = VirtualAllocEx(hProcess, nullptr, dllSize, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
    if (!pRemoteMem) {
        printf("[-] VirtualAllocEx 失败\n");
        CloseHandle(hProcess);
        return false;
    }

    // 写入 DLL 路径
    if (!WriteProcessMemory(hProcess, pRemoteMem, dllPath, dllSize, nullptr)) {
        printf("[-] WriteProcessMemory 失败\n");
        VirtualFreeEx(hProcess, pRemoteMem, 0, MEM_RELEASE);
        CloseHandle(hProcess);
        return false;
    }

    // 获取 LoadLibraryW 地址
    HMODULE hKernel32 = GetModuleHandleW(L"kernel32");
    LPTHREAD_START_ROUTINE pLoadLibrary = (LPTHREAD_START_ROUTINE)GetProcAddress(hKernel32, "LoadLibraryW");
    if (!pLoadLibrary) {
        printf("[-] 获取 LoadLibraryW 地址失败\n");
        VirtualFreeEx(hProcess, pRemoteMem, 0, MEM_RELEASE);
        CloseHandle(hProcess);
        return false;
    }

    // 创建远程线程加载 DLL
    HANDLE hRemoteThread = CreateRemoteThread(hProcess, nullptr, 0, pLoadLibrary, pRemoteMem, 0, nullptr);
    if (!hRemoteThread) {
        printf("[-] CreateRemoteThread 失败\n");
        VirtualFreeEx(hProcess, pRemoteMem, 0, MEM_RELEASE);
        CloseHandle(hProcess);
        return false;
    }

    // 等待 DLL 加载完成（可选）
    WaitForSingleObject(hRemoteThread, 5000);

    // 清理
    CloseHandle(hRemoteThread);
    VirtualFreeEx(hProcess, pRemoteMem, 0, MEM_RELEASE);
    CloseHandle(hProcess);

    printf("[+] DLL 注入成功！PID: %u\n", targetPid);
    return true;
}

// 根据进程名查找 PID
DWORD GetProcessIdByName(const wchar_t* processName) {
    DWORD pid = 0;
    HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (hSnapshot != INVALID_HANDLE_VALUE) {
        PROCESSENTRY32W pe32;
        pe32.dwSize = sizeof(PROCESSENTRY32W);

        if (Process32FirstW(hSnapshot, &pe32)) {
            do {
                if (_wcsicmp(pe32.szExeFile, processName) == 0) {
                    pid = pe32.th32ProcessID;
                    break;
                }
            } while (Process32NextW(hSnapshot, &pe32));
        }
        CloseHandle(hSnapshot);
    }
    return pid;
}

// 获取可执行文件名（从完整路径中提取文件名）
const wchar_t* GetFileName(const wchar_t* path) {
    const wchar_t* lastBackslash = wcsrchr(path, L'\\');
    const wchar_t* lastSlash = wcsrchr(path, L'/');
    const wchar_t* filename = path;
    if (lastBackslash && lastBackslash > filename)
        filename = lastBackslash + 1;
    if (lastSlash && lastSlash > filename)
        filename = lastSlash + 1;
    return filename;
}

// 主函数
int wmain(int argc, wchar_t* argv[]) {
    // 使用说明：
    // injector.exe <exe_path> <dll_path>
    // 示例: injector.exe "C:\Windows\System32\notepad.exe" "C:\path\to\hook.dll"

    if (argc < 3) {
        printf("Usage: %ls <full_path_to_exe> <full_path_to_dll>\n", argv[0]);
        printf("Example: injector.exe \"C:\\Windows\\System32\\notepad.exe\" \"C:\\tools\\hook.dll\"\n");
        system("pause");
        return 1;
    }

    const wchar_t* exePath = argv[1];  // 可执行文件完整路径
    const wchar_t* dllPath = argv[2];  // DLL 完整路径

    // 检查文件是否存在
    if (GetFileAttributesW(exePath) == INVALID_FILE_ATTRIBUTES) {
        printf("[-] 可执行文件不存在: %ls\n", exePath);
        system("pause");
        return 1;
    }
    if (GetFileAttributesW(dllPath) == INVALID_FILE_ATTRIBUTES) {
        printf("[-] DLL 文件不存在: %ls\n", dllPath);
        system("pause");
        return 1;
    }

    const wchar_t* processName = GetFileName(exePath);

    printf("[*] 目标进程名: %ls\n", processName);
    printf("[*] 可执行文件: %ls\n", exePath);
    printf("[*] 注入 DLL: %ls\n", dllPath);

    // 步骤1：尝试查找已存在的进程
    DWORD pid = GetProcessIdByName(processName);

    if (pid != 0) {
        printf("[*] 发现正在运行的进程，PID = %u\n", pid);
        printf("[*] 正在注入 DLL...\n");
        if (InjectDLL(pid, dllPath)) {
            printf("[+] 注入成功！\n");
        }
        else {
            printf("[-] 注入失败！\n");
        }
        system("pause");
        return 0;
    }

    // 步骤2：未找到进程，创建挂起的进程
    printf("[*] 未找到进程，正在创建并挂起...\n");

    STARTUPINFOW si = { sizeof(si) };
    PROCESS_INFORMATION pi = {};
    wchar_t cmdLine[4096] = { 0 };
    swprintf_s(cmdLine, L"\"%ls\"", exePath);
    for (int i = 3; i < argc; i++) {
        swprintf_s(cmdLine + wcslen(cmdLine), sizeof(cmdLine) / sizeof(wchar_t) - wcslen(cmdLine),
            L" \"%ls\"", argv[i]);  
    }
    printf("[*] 目标进程命令行: %ls\n", cmdLine);
    BOOL success = CreateProcessW(
        nullptr,              // 可执行文件路径
        cmdLine,              // 命令行参数（使用 exePath 时为 nullptr）
        nullptr,              // 进程安全属性
        nullptr,              // 线程安全属性
        FALSE,                // 不继承句柄
        CREATE_SUSPENDED,     // 创建后挂起
        nullptr,              // 环境变量（使用父进程）
        nullptr,              // 当前目录（使用父进程）
        &si,                  // 启动信息
        &pi                   // 进程信息（输出）
    );

    if (!success) {
        printf("[-] CreateProcessW 失败！错误码: %u\n", GetLastError());
        system("pause");
        return 1;
    }

    printf("[+] 进程创建成功，PID = %u，主线程处于挂起状态\n", pi.dwProcessId);

    // 步骤3：注入 DLL
    printf("[*] 正在注入 DLL...\n");
    if (!InjectDLL(pi.dwProcessId, dllPath)) {
        printf("[-] 注入失败！终止挂起的进程...\n");
        TerminateProcess(pi.hProcess, 1);
        CloseHandle(pi.hProcess);
        CloseHandle(pi.hThread);
        return 1;
    }

    // 步骤4：恢复主线程运行
    printf("[*] 注入成功，正在恢复进程运行...\n");
    DWORD suspendCount;
    do {
        suspendCount = ResumeThread(pi.hThread);
        if (suspendCount == (DWORD)-1) {
            printf("[-] ResumeThread 失败！错误码: %u\n", GetLastError());
            break;
        }
    } while (suspendCount > 0); // 确保完全恢复

    printf("[+] 进程已恢复运行！注入完成。\n");

    // 关闭句柄（可选）
    CloseHandle(pi.hThread); // 可以关闭，除非后续还需操作主线程
    CloseHandle(pi.hProcess); // 通常保留以便监控进程
    
    return 0;
}