#pragma once
#include <windows.h>
#include <psapi.h> 
#include <vector>
#include <string>
#include <iostream>



struct PatternByte {
    int value;  // -1 = 通配符 ??，其他为 0x00-0xFF
};
std::vector<PatternByte> ParsePattern(const std::string& patternStr);

bool IsPageReadable(const MEMORY_BASIC_INFORMATION& mbi);

bool SearchModuleMemory(const std::string& patternStr, std::vector<uintptr_t>& results, bool outInfo = false);