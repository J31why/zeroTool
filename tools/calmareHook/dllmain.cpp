// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include "pch.h"
#include "MinHook.h"
#include "iconv.h"
#include <cstdint>
#include <iostream>
#include <string>
#include <vector>

using namespace std;

struct decodeResult {
    uint64_t pStrLength;
    char* pStr;
    uint64_t strLength;
};

typedef decodeResult* (__fastcall* cp932_decode_t)(
    decodeResult* retstr,       // Result<String, usize>*
    const uint8_t* bytes
    );


uintptr_t baseAddr = (uintptr_t)GetModuleHandleA("calmare.exe");
uintptr_t cp932_decode_addr = 0x1fcdd0 + baseAddr;

cp932_decode_t ori_cp932_decode = nullptr;
iconv_t hIconv = nullptr;

decodeResult* __fastcall hooked_cp932_decode(
    decodeResult* retstr,
    const uint8_t* bytes
) {
    //uint64_t* result = ori_cp932_decode(retstr, bytes);
    uint8_t* pCheck = const_cast<uint8_t*>(bytes);
    char* pStream = (char*)(bytes);
    uint64_t len = 0;
    bool isMenu = *(pCheck - 8) == 0x5E;
    while (true) {
        uint8_t b = *pCheck++;
        if (b > 0x1f || (isMenu && b==1)) {
            len++;
        }
        else break;
    }
    
    size_t utf8_len = len * 2 + 2;
    std::vector<char> utf8_buffer(utf8_len);
    size_t out_left = utf8_len;
    char* out_ptr = utf8_buffer.data();
    //cout << "文本:" << string(pStream) << endl;
    size_t ret = iconv(hIconv, &pStream, &len, &out_ptr, &out_left);
    if (ret == (size_t)-1) {
        cout << "iconv error" << endl;
        cout << "ret: 0x" << hex << retstr << endl;
        cout << "bytes: 0x" << hex << (uint8_t*)bytes << endl;
        cout << "size: 0x" << len << endl;
        cout << "out_left: 0x" << out_left << endl;
        system("pause");
        throw new exception("error");
    }
	size_t utf8_size = utf8_len - out_left;
    char* pUtf8 = new char[utf8_size + 1];
	_memccpy(pUtf8, utf8_buffer.data(), 0, utf8_size + 1);

    retstr->pStrLength = utf8_size;
    retstr->pStr = pUtf8;
    retstr->strLength = utf8_size;
  
    //cout << "ret: 0x" << hex << retstr << endl;
    //cout << "size: 0x" << utf8_buffer.size() << endl;
    //cout << "out_left: 0x" << out_left << endl;
	//system("pause");
    return retstr;
}

void hook() {

    hIconv = iconv_open("UTF-8", "CP936");

    std::cout <<std::hex<< cp932_decode_addr << std::endl;

    MH_STATUS status = MH_Initialize();
    std::cout << "初始化" << std::endl;
     status = MH_CreateHook((LPVOID)cp932_decode_addr, &hooked_cp932_decode, reinterpret_cast<LPVOID*>(&ori_cp932_decode));
    if (status != MH_OK)
        std::cout << "MH_CreateHook error" << std::endl;
    std::cout << "MH_CreateHook" << std::endl;
     status = MH_EnableHook(MH_ALL_HOOKS);
    if (status != MH_OK)
        std::cout << "MH_EnableHook error" << std::endl;
    std::cout << "MH_EnableHook" << std::endl;

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

