#pragma once
#include <windows.h>
#include <psapi.h> 
#include <vector>
#include <string>
#include <iostream>

struct PatchInfo {
    uintptr_t ori_addr = 0;
    std::vector<uint8_t> ori_data;
    uintptr_t ret_addr = 0;
};

struct PatternByte {
    int value;  // -1 = 通配符 ??，其他为 0x00-0xFF
};

std::vector<PatternByte> ParsePattern(const std::string& patternStr);

bool IsPageReadable(const MEMORY_BASIC_INFORMATION& mbi);

bool SearchModuleMemory(const std::string& patternStr, std::vector<uintptr_t>& results, bool outInfo = false);
void UnLockProtect(uintptr_t ptr);
void LockProtect(uintptr_t ptr);
void* BeginPatch(const uintptr_t ptr, size_t patchLength, size_t removeLength);
void WritePatchOriginalData(uint8_t*& patchedAddr);
void EndPatch(const uint8_t* patchedAddr);