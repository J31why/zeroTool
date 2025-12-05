#pragma once
#include <psapi.h> 

struct PatternByte {
    int value;  // -1 = 通配符 ??，其他为 0x00-0xFF
};
class hook_sjis {
private:
    uintptr_t hookptr;
    uintptr_t singleByte_addr;
    uintptr_t return_addr;
    uintptr_t alloc_addr;
    vector<uint8_t> extra_bytes;
    vector<uint8_t> hookcode_bytes;

public:
    hook_sjis(uintptr_t ptr, int32_t bytes_len, int32_t return_offset);
    void read_extra_bytes(uintptr_t start, size_t size);
    void calcSingleByteAddr(int32_t offset, int32_t len);

    void start();
    void end();
    void cmp_cl(uint8_t num);
    void cmp_al(uint8_t num);
    void cmp_byte_r10(uint8_t offset, uint8_t num);
    void cmp_byte_r14(uint8_t offset, uint8_t num);
    void cmp_byte_r15(uint8_t offset, uint8_t num);
    void cmp_byte_rdi(uint8_t offset, uint8_t num);
    void cmp_byte_rbx(uint8_t offset, uint8_t num);
    void cmp_byte_rsi(uint8_t offset, uint8_t num);
    void jna_singleByte();
    void ja_singleByte();
    void jb_singleByte();

    ~hook_sjis();
};
std::vector<PatternByte> ParsePattern(const std::string& patternStr);

bool IsPageReadable(const MEMORY_BASIC_INFORMATION& mbi);

bool SearchModuleMemory(const std::string& patternStr, std::vector<uintptr_t>& results,bool outInfo=false);