
#include "memo.h"

using namespace std;

DWORD old_protect;

PatchInfo info;

void UnLockProtect(uintptr_t ptr) {
    VirtualProtect((LPVOID)ptr, 0x100, PAGE_EXECUTE_READWRITE, &old_protect);
}
void LockProtect(uintptr_t ptr) {
    VirtualProtect((LPVOID)ptr, 0x100, old_protect, &old_protect);
    FlushInstructionCache(GetCurrentProcess(), (LPVOID)ptr, 0x100);
    old_protect = 0;
}

void* BeginPatch(const uintptr_t ptr, size_t patchLength, size_t removeLength) {
    UnLockProtect(ptr);
    info.ori_addr = ptr;
    size_t backupSize = (patchLength > removeLength) ? (patchLength - removeLength) : 0;
    info.ret_addr = ptr + patchLength;
    info.ori_data.resize(backupSize);
    memcpy(info.ori_data.data(), (void*)(ptr + removeLength), backupSize);
    memset((void*)ptr, 0x90, patchLength);
    void* jmpAddr = malloc(0x100);
    memset(jmpAddr, 0x0, 0x100);
    DWORD old;
    VirtualProtect(jmpAddr, 0x100, PAGE_EXECUTE_READWRITE, &old);
    uint8_t* p = (uint8_t*)ptr;
    p[0] = 0xFF;
    p[1] = 0x25;
    *(uint32_t*)(p + 2) = 0;
    *(uintptr_t*)(p + 6) = (uintptr_t)jmpAddr;
    return jmpAddr;
}

void WritePatchOriginalData(uint8_t*& patchedAddr) {
    memcpy((PVOID)patchedAddr, info.ori_data.data(), info.ori_data.size());
    patchedAddr += info.ori_data.size();
}

void EndPatch(const uint8_t* patchedAddr) {
    uint8_t* p = const_cast<uint8_t*>(patchedAddr);
    p[0] = 0xFF;
    p[1] = 0x25;
    *(uint32_t*)(p + 2) = 0;
    *(uintptr_t*)(p + 6) = info.ret_addr;
    LockProtect(info.ori_addr);
    std::vector<uint8_t>().swap(info.ori_data);
    info.ori_addr = 0;
    info.ret_addr = 0;
}

vector<PatternByte> ParsePattern(const string& patternStr) {
    vector<PatternByte> pattern;
    string token;

    for (char c : patternStr) {
        if (c == ' ') {
            if (!token.empty()) {
                PatternByte pb;
                if (token == "??") pb.value = -1;
                else {
                    pb.value = stoi(token, nullptr, 16);
                    if (pb.value < 0 || pb.value > 0xFF)
                        throw invalid_argument("无效字节：" + token);
                }
                pattern.push_back(pb);
                token.clear();
            }
        }
        else token += toupper(c);
    }

    if (!token.empty()) {
        PatternByte pb;
        if (token == "??") pb.value = -1;
        else {
            pb.value = stoi(token, nullptr, 16);
            if (pb.value < 0 || pb.value > 0xFF)
                throw invalid_argument("无效字节：" + token);
        }
        pattern.push_back(pb);
    }

    if (pattern.empty()) throw invalid_argument("模式为空");
    return pattern;
}

bool IsPageReadable(const MEMORY_BASIC_INFORMATION& mbi) {
    return (mbi.Protect & (PAGE_READONLY | PAGE_READWRITE | PAGE_WRITECOPY |
        PAGE_EXECUTE_READ | PAGE_EXECUTE_READWRITE | PAGE_EXECUTE_WRITECOPY)) != 0;
}

bool SearchModuleMemory(const string& patternStr, vector<uintptr_t>& results, bool outInfo) {
    results.clear();
    // 获取当前模块（主EXE）信息
    HMODULE hModule = GetModuleHandleA(NULL);
    if (!hModule) {
        cerr << "错误：获取模块句柄失败，错误码：" << GetLastError() << endl;
        return false;
    }

    MODULEINFO moduleInfo = { 0 };
    if (!GetModuleInformation(GetCurrentProcess(), hModule, &moduleInfo, sizeof(MODULEINFO))) {
        cerr << "错误：获取模块信息失败，错误码：" << GetLastError() << endl;
        return false;
    }

    const uintptr_t moduleBase = reinterpret_cast<uintptr_t>(moduleInfo.lpBaseOfDll);
    const uintptr_t moduleEnd = moduleBase + moduleInfo.SizeOfImage;
    const size_t moduleSize = moduleInfo.SizeOfImage;

    if (outInfo) {
        cout << "=== 模块信息 ===" << endl;
        cout << "主模块基址: 0x" << hex << moduleBase << endl;
        cout << "模块大小: " << dec << moduleSize << " 字节（0x" << hex << moduleSize << "）" << endl;
        cout << "模块结束地址: 0x" << hex << moduleEnd << endl;
        cout << "=================" << endl;
    }

    // 解析搜索模式
    vector<PatternByte> pattern;
    try {
        pattern = ParsePattern(patternStr);
    }
    catch (const runtime_error& e) {
        cerr << "错误：模式解析失败 - " << e.what() << endl;
        return false;
    }

    const size_t patternLen = pattern.size();
    if (patternLen > moduleSize) {
        cerr << "错误：模式长度（" << patternLen << " 字节）超过模块大小" << endl;
        return false;
    }

    // 遍历模块内存（按内存页遍历，提升效率）
    MEMORY_BASIC_INFORMATION mbi = { 0 };
    uintptr_t currentAddr = moduleBase;

    while (currentAddr < moduleEnd) {
        // 查询当前内存页信息
        const SIZE_T queryResult = VirtualQuery(reinterpret_cast<LPCVOID>(currentAddr), &mbi, sizeof(mbi));
        if (queryResult == 0) {
            currentAddr += 0x1000;  // 跳过无效内存页（默认页大小4KB）
            continue;
        }

        const uintptr_t pageBase = reinterpret_cast<uintptr_t>(mbi.BaseAddress);
        uintptr_t pageEnd = pageBase + mbi.RegionSize;
        // 裁剪到模块范围（避免超出模块边界）
        if (pageBase >= moduleEnd) break;
        if (pageEnd > moduleEnd) pageEnd = moduleEnd;

        // 跳过不可读的内存页（避免Access Violation）
        if (!IsPageReadable(mbi)) {
            currentAddr = pageEnd;
            continue;
        }

        // 4. 在当前可读页中搜索模式
        const size_t pageSearchSize = pageEnd - pageBase;
        const size_t maxI = pageSearchSize - patternLen;
        const uint8_t* pageData = reinterpret_cast<const uint8_t*>(pageBase);

        // 遍历页内所有可能的起始位置（留出模式长度的空间）
        for (size_t i = 0; i <= pageSearchSize - patternLen; ++i) {
            bool isMatch = true;
            for (size_t j = 0; j < patternLen; ++j) {
                const PatternByte& pb = pattern[j];
                // 非通配符时，必须精确匹配字节
                if (pb.value != -1 && pageData[i + j] != static_cast<uint8_t>(pb.value)) {
                    isMatch = false;
                    break;
                }
            }

            // 找到匹配，记录地址
            if (isMatch) {
                const uintptr_t matchAddr = pageBase + i;
                results.push_back(matchAddr);
            }
        }

        // 移动到下一个内存页
        currentAddr = pageEnd;
    }

    return true;
}