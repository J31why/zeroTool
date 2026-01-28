// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include "pch.h"
#include "MinHook.h"
#include "iconv.h"
#include <cstdint>
#include <iostream>
#include <string>
#include <vector>

using namespace std;

struct Result {
    uint64_t srcLength;
    char* pStr;
    uint64_t outLen;
};

typedef Result* (__fastcall* cp932_decode_t)(
    Result* retstr,       // Result<String, usize>*
    const uint8_t* bytes,
    size_t len
    );

typedef Result* (__fastcall* cp932_encode_t)(
    Result* retstr,       // Result<String, usize>*
    const uint8_t* text,
    size_t len
    );


uintptr_t baseAddr = (uintptr_t)GetModuleHandleA("calmare.exe");
uintptr_t cp932_decode_addr = 0x1fcdd0 + baseAddr;
uintptr_t cp932_encode_addr = 0x7FF73577D170;

cp932_decode_t ori_cp932_decode = nullptr;
cp932_encode_t ori_cp932_encode = nullptr;
iconv_t hIconv_gbk2utf8 = nullptr;
iconv_t hIconv_utf82gbk = nullptr;

Result* __fastcall hooked_cp932_encode(
    Result* retstr,
    const uint8_t* text,
    size_t len
) {
    if (len == 0) {
        return ori_cp932_decode(retstr, text, len);
    }
    char* pStream = const_cast<char*>(reinterpret_cast<const char*>(text));
    size_t gbk_len = len + 2;
    std::vector<char> gbk_buffer(gbk_len);
    size_t out_left = gbk_len;
    char* out_ptr = gbk_buffer.data();
    size_t ret = iconv(hIconv_utf82gbk, &pStream, &len, &out_ptr, &out_left);
    if (ret == (size_t)-1) {
        cout << "iconv error" << endl;
        cout << "ret: 0x" << hex << retstr << endl;
        cout << "text: 0x" << hex << (uint64_t)text << endl;
        cout << "size: 0x" << len << endl;
        cout << "out_left: 0x" << out_left << endl;
        system("pause");
        throw new exception("error");
    }
    size_t gbk_size = gbk_len - out_left;
    char* pGbk = new char[gbk_size + 1];
    _memccpy(pGbk, gbk_buffer.data(), 0, gbk_size + 1);
    retstr->srcLength = len;
    retstr->pStr = pGbk;
    retstr->outLen = gbk_size;
    return retstr;
}

Result* __fastcall hooked_cp932_decode(
    Result* retstr,
    const uint8_t* bytes,
    size_t len
) {
    if (len == 0) {
		return ori_cp932_decode(retstr, bytes, len);
    }
    char* pStream = const_cast<char*>(reinterpret_cast<const char*>(bytes));
    size_t utf8_len = len * 2 + 2;
    std::vector<char> utf8_buffer(utf8_len);
    size_t out_left = utf8_len;
    char* out_ptr = utf8_buffer.data();
    //cout << "文本:" << string(pStream) << endl;
    size_t ret = iconv(hIconv_gbk2utf8, &pStream, &len, &out_ptr, &out_left);
    if (ret == (size_t)-1) {
        cout << "iconv error" << endl;
        cout << "ret: 0x" << hex << retstr << endl;
        cout << "bytes: 0x" << hex << (uint64_t)bytes << endl;
        cout << "size: 0x" << len << endl;
        cout << "out_left: 0x" << out_left << endl;
        system("pause");
        throw new exception("error");
    }
	size_t utf8_size = utf8_len - out_left;
    char* pUtf8 = new char[utf8_size + 1];
	_memccpy(pUtf8, utf8_buffer.data(), 0, utf8_size + 1);

    retstr->srcLength = len;
    retstr->pStr = pUtf8;
    retstr->outLen = utf8_size;
  
    //cout << "ret: 0x" << hex << retstr << endl;
    //cout << "size: 0x" << utf8_buffer.size() << endl;
    //cout << "out_left: 0x" << out_left << endl;
	//system("pause");
    return retstr;
}

void hook() {

    hIconv_gbk2utf8 = iconv_open("UTF-8", "CP936");
    hIconv_utf82gbk = iconv_open("CP936", "UTF-8");

    MH_STATUS status = MH_Initialize();
    status = MH_CreateHook((LPVOID)cp932_decode_addr, &hooked_cp932_decode, reinterpret_cast<LPVOID*>(&ori_cp932_decode));
    if (status != MH_OK)
        std::cout << "MH_CreateHook decode error" << std::endl;
    status = MH_CreateHook((LPVOID)cp932_encode_addr, &hooked_cp932_encode, reinterpret_cast<LPVOID*>(&ori_cp932_encode));
    if (status != MH_OK)
        std::cout << "MH_CreateHook encode error" << std::endl;
     status = MH_EnableHook(MH_ALL_HOOKS);
    if (status != MH_OK)
        std::cout << "MH_EnableHook error" << std::endl;
}


BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        hook();
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

