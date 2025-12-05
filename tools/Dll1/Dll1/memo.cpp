#include "pch.h"
#include "memo.h"

 hook_sjis::hook_sjis(uintptr_t ptr, int32_t bytes_len, int32_t return_offset) {
    if(return_offset<14)
		throw runtime_error("hook_sjis return_offset too small");
    hookptr = ptr;
    return_addr = hookptr + return_offset;

    if (return_offset <= bytes_len) {
        return;
    }
    uintptr_t extra_start = hookptr + bytes_len;
    size_t extra_size = return_addr - extra_start;
    if (extra_start >= return_addr || extra_size == 0) {
        return;
    }
    read_extra_bytes(extra_start, extra_size);
}

void hook_sjis::read_extra_bytes(uintptr_t start, size_t size) {
    DWORD old_protect = 0;
    if (!VirtualProtect((LPVOID)start, size, PAGE_EXECUTE_READWRITE, &old_protect)) {
        return;
    }
    extra_bytes.resize(size);
    memcpy(extra_bytes.data(), (const void*)start, size);
    if (!VirtualProtect((LPVOID)start, size, old_protect, &old_protect)) {
        return;
    }
}

void hook_sjis::calcSingleByteAddr(int32_t offset, int32_t len) {
	uintptr_t addr = hookptr + offset;
    if (len == 1) {
        int32_t jmp_len = *(int8_t*)addr;
		singleByte_addr = addr + 1 + jmp_len;
    }
    else if  (len == 4) {
        int32_t jmp_len = *(int32_t*)addr;
        singleByte_addr = addr + 4 + jmp_len;
    }
}
void hook_sjis::start() {
    size_t mem_size = 0x100;
	DWORD old_protect = 0;
	alloc_addr = (uintptr_t)malloc(mem_size);
    if (!VirtualProtect((LPVOID)alloc_addr, mem_size, PAGE_EXECUTE_READWRITE, &old_protect)) {
		throw runtime_error("无法更改内存保护");
    }
    memset((PVOID)alloc_addr, 0, mem_size);
	uint8_t* p = (uint8_t*)hookptr;
    if (!VirtualProtect((LPVOID)hookptr, 0x20, PAGE_EXECUTE_READWRITE, &old_protect)) {
        throw runtime_error("无法更改内存保护");
    }
    memset(p, 0x90, return_addr - hookptr);
	*p++ = 0xff; 
    *p++ = 0x25;
    *p++ = 0x0;
    *p++ = 0x0;
    *p++ = 0x0;
    *p++ = 0x0;
    *(uint64_t*)p = alloc_addr;
    if (!VirtualProtect((LPVOID)hookptr, 0x20, old_protect, &old_protect)) {
        throw runtime_error("无法更改内存保护");
    }
    hookcode_bytes.clear();
}
void hook_sjis::end() {
    uint8_t* p = (uint8_t*)alloc_addr;
    for (size_t i = 0; i < hookcode_bytes.size(); i++){
        *p++ = hookcode_bytes[i];
    }
    for (size_t i = 0; i < extra_bytes.size(); i++){
        *p++ = extra_bytes[i];
    }
    *p++ = 0xff;
    *p++ = 0x25;
    *p++ = 0x0;
    *p++ = 0x0;
    *p++ = 0x0;
    *p++ = 0x0;
    *(uint64_t*)p = return_addr;
    p = (uint8_t*)alloc_addr + 0x50;
    *p++ = 0xff;
    *p++ = 0x25;
    *p++ = 0x0;
    *p++ = 0x0;
    *p++ = 0x0;
    *p++ = 0x0;
    *(uint64_t*)p = singleByte_addr;
}

void hook_sjis::cmp_al(uint8_t num) {
    hookcode_bytes.push_back(0x3c);
    hookcode_bytes.push_back(num);
}

void hook_sjis::cmp_cl(uint8_t num) {
    hookcode_bytes.push_back(0x80);
    hookcode_bytes.push_back(0xf9);
    hookcode_bytes.push_back(num);
}

void hook_sjis::cmp_byte_r15(uint8_t offset, uint8_t num) {
    hookcode_bytes.push_back(0x41);
    hookcode_bytes.push_back(0x80);
    hookcode_bytes.push_back(0x7f);
    hookcode_bytes.push_back(offset);
    hookcode_bytes.push_back(num);
}

void hook_sjis::cmp_byte_r14(uint8_t offset, uint8_t num) {
    hookcode_bytes.push_back(0x41);
    hookcode_bytes.push_back(0x80);
    hookcode_bytes.push_back(0x7E);
    hookcode_bytes.push_back(offset);
    hookcode_bytes.push_back(num);
}

void hook_sjis::cmp_byte_r10(uint8_t offset, uint8_t num) {
    hookcode_bytes.push_back(0x41);
    hookcode_bytes.push_back(0x80);
    hookcode_bytes.push_back(0x7A);
    hookcode_bytes.push_back(offset);
    hookcode_bytes.push_back(num);
}

void hook_sjis::cmp_byte_rdi(uint8_t offset, uint8_t num) {
    hookcode_bytes.push_back(0x80);
    hookcode_bytes.push_back(0x7f);
    hookcode_bytes.push_back(offset);
    hookcode_bytes.push_back(num);
}

void hook_sjis::cmp_byte_rbx(uint8_t offset, uint8_t num) {
    hookcode_bytes.push_back(0x80);
    hookcode_bytes.push_back(0x7b);
    hookcode_bytes.push_back(offset);
    hookcode_bytes.push_back(num);
}

void hook_sjis::cmp_byte_rsi(uint8_t offset, uint8_t num) {
    hookcode_bytes.push_back(0x80);
    hookcode_bytes.push_back(0x7E);
    hookcode_bytes.push_back(offset);
    hookcode_bytes.push_back(num);
}

void hook_sjis::jna_singleByte() {
    hookcode_bytes.push_back(0x76);
    uint8_t sjmp = 0x50 - hookcode_bytes.size() - 1;
    hookcode_bytes.push_back(sjmp);
}

void hook_sjis::ja_singleByte() {
    hookcode_bytes.push_back(0x77);
    uint8_t sjmp = 0x50 - hookcode_bytes.size() - 1;
    hookcode_bytes.push_back(sjmp);
}

void hook_sjis::jb_singleByte() {
    hookcode_bytes.push_back(0x72);
    uint8_t sjmp = 0x50 - hookcode_bytes.size() - 1;
    hookcode_bytes.push_back(sjmp);
}

hook_sjis::~hook_sjis() {
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

bool SearchModuleMemory(const string& patternStr, vector<uintptr_t>& results , bool outInfo) {
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
        cout <<   "=== 模块信息 ===" << endl;
        cout << "主模块基址: 0x" << hex  << moduleBase << endl;
        cout << "模块大小: " << dec << moduleSize << " 字节（0x" << hex << moduleSize << "）" << endl;
        cout << "模块结束地址: 0x" << hex << moduleEnd << endl;
        cout << "=================" << endl ;
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