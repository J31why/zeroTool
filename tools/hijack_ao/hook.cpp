#include "hook.h"

using namespace std;

namespace hook {
    string sjis2uni_addr_pattern = "41 54 41 55 41 57 48 81 EC 80 00 00 00";
    string check_encoding_addr_pattern = "48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 48 89 7C 24 20 41 54 41 56 41 57 0F B6 11";
    string load_mess_string_addr_pattern = "48 8B C4 48 89 58 10 48 89 70 18 48 89 78 20 55 41 54 41 55 41 56 41 57 48 8D 68 ?? 48 81 EC ?? ?? 00 00 0F 29 70 ?? 0F 29 78 ?? 48 8b 05 ?? ?? ?? 00 48 33 C4 48 ?? ?? ?? 48 8B F9 48";
    string get_mess_string_key_addr_pattern = "40 53 48 83 EC 30 45 33 c0 48 8b d9 81";
    string mess_string_jp_struct_offset_pattern = "90 33 d2 41 b8";
    string sjis2utf8_addr_pattern = "40 53 56 48 8B F2 48";
    string utf82sjis_addr_pattern = "48 89 74 24 18 41 56 49";
    string language_option_addr_pattern = "8B 0D ?? ?? ?? 00 85 c9 74";
    string loadNoteHelpKey_posMap_addr_pattern = "48 89 5C 24 10 48 89 74 24 18 48 89 7C 24 20 55 41 56 41 57 48 8D AC 24 ?? ?? FF FF 48 81 ec ?? ?? 00 00 48 8b 05 ?? ?? ?? ?? 48 33 C4 48 89 85 ?? ?? ?? ?? 48 8B F1 48";
    string WebMPlayerOpen_addr_pattern = "48 8b 16 48 8b cb ff 15";
    string TextWidthScalefactor_addr_pattern = "F3 44 0F 10 0D ?? ?? ?? ?? 41 0F 28 FB";
    string DialogBoxHeight_addr_pattern = "66 0F 6E 89 ?? ?? ?? ?? 0F 5B C9 F3 0F 58 C8 F3 0F 2C C1";
    // ==============
    //string SwitchFrameLimit_addr_pattern = "4C 8B C1 48 8B 49 10 49 8B 40 18";
    //string FrameLimit_addr_pattern = "EB ?? 66 0F 6E 0D";
    //string ScenaSleep_addr_pattern = "48 89 5C 24 10 44 8B 5A 08 4C 8B D1";
    //string DialogSleep_addr_pattern = "6B C7 64 BF 00 02 00 00";

    string main_sjis_byte_valid_addr_pattern = "80 F9 A0 72 ?? 33 C0";

    uintptr_t sjis2uni_addr = 0x1401369e0;
    uintptr_t check_encoding_addr = 0x1401360b0;
    uintptr_t load_mess_string_addr = 0x1401d3ff0;
    uintptr_t get_mess_string_key_addr = 0x140217cb0;
    uintptr_t mess_string_jp_offset_offset = 0x183e0;
    uintptr_t sjis2utf8_addr = 0x140136f00;
    uintptr_t utf82sjis_addr = 0x140137ba0;
    int32_t* language_option_addr = reinterpret_cast<int32_t*>(0x140807A24);
    uintptr_t loadNoteHelpKey_posMap_addr = 0x1401c5310;
    uintptr_t WebMPlayerOpen_addr = 0x1408a7c08;
    uintptr_t TextWidthScalefactor_addr = 0x1406ebfb8;
    uintptr_t DialogBoxHeight_addr[] =
    {
        0x1403bf071,    //dialog1
        0x1403bfa45     //dialog2
    };

    uintptr_t main_sjis_byte_valid_addr[] =
    {
        0x1402a7135,
        0x1402acf25,
    };

    CreateFileA_t ori_CreateFileA = nullptr;
    CreateWindowExA_t ori_CreateWindowExA = nullptr;
    sjis2uni_t ori_sjis2uni = nullptr;
    checkEncoding_t ori_check_encoding = nullptr;
    load_mess_string_t ori_load_mess_string = nullptr;
    get_mess_string_key_t get_mess_string_key = nullptr;
    sjis2utf8_t ori_sjis2utf8 = nullptr;
    utf82sjis_t ori_utf82sjis = nullptr;
    loadNoteHelpKey_posMap_t ori_loadNoteHelpKey_posMap = nullptr;
    WebMPlayerOpen_t ori_WebMPlayerOpen = nullptr;

    bool isMatchSuccessful = false;
    int matchedAddrCount = 0;
    int totalAddrCount = 15;
    bool is_debug = false;

    void search_all_addresses() {
        vector<uintptr_t> matchResults;
        if (SearchModuleMemory(sjis2uni_addr_pattern, matchResults, true) && matchResults.size() == 1) {
            sjis2uni_addr = matchResults[0];
            cout << "sjis2uni_addr : 0x" << hex << sjis2uni_addr << endl;
            matchedAddrCount++;
        }

        if (SearchModuleMemory(check_encoding_addr_pattern, matchResults) && matchResults.size() == 1) {
            check_encoding_addr = matchResults[0];
            cout << "check_encoding_addr : 0x" << hex << check_encoding_addr << endl;
            matchedAddrCount++;
        }

        if (SearchModuleMemory(load_mess_string_addr_pattern, matchResults) && matchResults.size() == 1) {
            load_mess_string_addr = matchResults[0];
            cout << "load_mess_string_addr : 0x" << hex << load_mess_string_addr << endl;
            matchedAddrCount++;
        }

        if (SearchModuleMemory(get_mess_string_key_addr_pattern, matchResults) && matchResults.size() == 1) {
            get_mess_string_key_addr = matchResults[0];
            cout << "get_mess_string_key_addr : 0x" << hex << get_mess_string_key_addr << endl;
            matchedAddrCount++;
        }

        if (SearchModuleMemory(mess_string_jp_struct_offset_pattern, matchResults) && matchResults.size() == 1) {
            mess_string_jp_offset_offset = matchResults[0] + 0x5;
            uint32_t* offset_ptr = reinterpret_cast<uint32_t*>(mess_string_jp_offset_offset);
            mess_string_jp_offset_offset = (uint64_t)*offset_ptr;
            cout << "mess_string_jp_struct_offset : 0x" << hex << mess_string_jp_offset_offset << endl;
            matchedAddrCount++;
        }

        if (SearchModuleMemory(sjis2utf8_addr_pattern, matchResults) && matchResults.size() == 1) {
            sjis2utf8_addr = matchResults[0];
            cout << "sjis2utf8_addr : 0x" << hex << sjis2utf8_addr << endl;
            matchedAddrCount++;
        }

        if (SearchModuleMemory(utf82sjis_addr_pattern, matchResults) && matchResults.size() == 1) {
            utf82sjis_addr = matchResults[0];
            cout << "utf82sjis_addr : 0x" << hex << utf82sjis_addr << endl;
            matchedAddrCount++;
        }

        if (SearchModuleMemory(language_option_addr_pattern, matchResults) && matchResults.size() == 1) {
            uintptr_t addr = matchResults[0] + 0x2;
            uintptr_t offset_ptr = *reinterpret_cast<uint32_t*>(addr);
            language_option_addr = reinterpret_cast<int32_t*>(offset_ptr + 0x4 + addr);
            cout << "language_option_addr : 0x" << hex << language_option_addr << endl;
            matchedAddrCount++;
        }

        if (SearchModuleMemory(loadNoteHelpKey_posMap_addr_pattern, matchResults) && matchResults.size() == 1) {
            loadNoteHelpKey_posMap_addr = matchResults[0];
            cout << "loadNoteHelpKey_posMap_addr : 0x" << hex << loadNoteHelpKey_posMap_addr << endl;
            matchedAddrCount++;
        }

        if (SearchModuleMemory(WebMPlayerOpen_addr_pattern, matchResults) && matchResults.size() == 1) {
            WebMPlayerOpen_addr = matchResults[0] + 0x8;
            uint32_t* offset_ptr = reinterpret_cast<uint32_t*>(WebMPlayerOpen_addr);
            WebMPlayerOpen_addr += (uint64_t)*offset_ptr + 0x4;
            cout << "WebMPlayerOpen_addr : 0x" << hex << WebMPlayerOpen_addr << endl;
            matchedAddrCount++;
        }

        if (SearchModuleMemory(TextWidthScalefactor_addr_pattern, matchResults) && matchResults.size() == 1) {
            TextWidthScalefactor_addr = matchResults[0] + 0x5;
            uint32_t* offset_ptr = reinterpret_cast<uint32_t*>(TextWidthScalefactor_addr);
            TextWidthScalefactor_addr += (uint64_t)*offset_ptr + 0x4;
            cout << "TextWidthScalefactor_addr : 0x" << hex << TextWidthScalefactor_addr << endl;
            matchedAddrCount++;
        }

        if (SearchModuleMemory(DialogBoxHeight_addr_pattern, matchResults) && matchResults.size() == 2) {
            for (int i = 0; i < matchResults.size(); i++) {
                DialogBoxHeight_addr[i] = matchResults[i];
                cout << "DialogBoxHeight_addr " << i << ": 0x" << hex << DialogBoxHeight_addr[i] << endl;
            }
            matchedAddrCount += matchResults.size();
        }

        cout << "==============================" << endl;

        if (SearchModuleMemory(main_sjis_byte_valid_addr_pattern, matchResults) && matchResults.size() == 2) {
            for (int i = 0; i < matchResults.size(); i++) {
                main_sjis_byte_valid_addr[i] = matchResults[i];
                cout << "main_sjis_byte_valid_addr " << i << ": 0x" << hex << main_sjis_byte_valid_addr[i] << endl;
            }
            matchedAddrCount += matchResults.size();
        }
    }

    void fix_main_sjis_byte_valid(uintptr_t ptr) {
        UnLockProtect(ptr);
        uint8_t* p = reinterpret_cast<uint8_t*>(ptr + 2);
        *p = 0xff;
        LockProtect(ptr);
    }

    void fix_TextWidthScalefactor(uintptr_t ptr) {
        UnLockProtect(ptr);
        float* pScaleFactor = reinterpret_cast<float*>(ptr);
        *pScaleFactor = 0.88f;
        LockProtect(ptr);
    }

    void fix_DialogBoxHeight(uintptr_t ptr) {
        uint8_t* jmpAddr = static_cast<uint8_t*>(BeginPatch(ptr, 19, 0));
        WritePatchOriginalData(jmpAddr);
        //add eax, 03
        *(jmpAddr++) = 0x83;
        *(jmpAddr++) = 0xc0;
        *(jmpAddr++) = 0x03;
        EndPatch(jmpAddr);
    }

    void hook_install() {
        try
        {

            cout << endl;
#if HIJACK
            search_all_addresses();
            cout << "匹配到地址数量：" << dec << matchedAddrCount << "/" << totalAddrCount << hex << endl;
            if (matchedAddrCount == totalAddrCount) {
                cout << endl << "[INFO]匹配成功" << endl << endl;
                isMatchSuccessful = true;
            }
            else {
                //throw runtime_error("部分地址未匹配成功，停止hook以防止崩溃");
            }
#else
            cout << "[INFO]跳过地址匹配" << endl;
            isMatchSuccessful = true;
#endif // NOHIJACK
            cout << "==============================" << endl;
            MH_STATUS status = MH_Initialize();
            if (status != MH_OK) {
                throw runtime_error("MinHook initialize failed!");
            }
            cout << "[INFO]hook CreateFileA" << endl;
            status = MH_CreateHook(&CreateFileA, &hooked_CreateFileA, reinterpret_cast<LPVOID*>(&ori_CreateFileA));
            if (status != MH_OK) {
                throw runtime_error("MinHook create CreateFileA hook failed!");
            }
            cout << "[INFO]hook CreateWindowExW" << endl;
            status = MH_CreateHook(&CreateWindowExA, &hooked_CreateWindowExA, reinterpret_cast<LPVOID*>(&ori_CreateWindowExA));
            if (status != MH_OK) {
                throw runtime_error("MinHook create CreateWindowExW hook failed!");
            }

            cout << "[INFO]hook sjis2uni" << endl;
            status = MH_CreateHook((LPVOID)sjis2uni_addr, &hooked_sjis2uni, reinterpret_cast<LPVOID*>(&ori_sjis2uni));
            if (status != MH_OK) {
                throw runtime_error("MinHook create sjis2uni hook failed!");
            }
            cout << "[INFO]hook check_encoding" << endl;
            status = MH_CreateHook((LPVOID)check_encoding_addr, &hooked_check_encoding, reinterpret_cast<LPVOID*>(&ori_check_encoding));
            if (status != MH_OK) {
                throw runtime_error("MinHook create check_encoding hook failed!");
            }

            cout << "[INFO]hook load_mess_string" << endl;
            status = MH_CreateHook((LPVOID)load_mess_string_addr, &hooked_load_mess_string, reinterpret_cast<LPVOID*>(&ori_load_mess_string));
            if (status != MH_OK) {
                throw runtime_error("MinHook create load_mess_string hook failed!");
            }

            cout << "[INFO]hook sjis2utf8" << endl;
            status = MH_CreateHook((LPVOID)sjis2utf8_addr, &hooked_sjis2utf8, reinterpret_cast<LPVOID*>(&ori_sjis2utf8));
            if (status != MH_OK) {
                throw runtime_error("MinHook create sjis2utf8 hook failed!");
            }

            cout << "[INFO]hook utf82sjis" << endl;
            status = MH_CreateHook((LPVOID)utf82sjis_addr, &hooked_utf82sjis, reinterpret_cast<LPVOID*>(&ori_utf82sjis));
            if (status != MH_OK) {
                throw runtime_error("MinHook create utf82sjis hook failed!");
            }

            cout << "[INFO]hook loadNoteHelpKeyPos" << endl;
            status = MH_CreateHook((LPVOID)loadNoteHelpKey_posMap_addr, &hooked_loadNoteHelpKey_posMap, reinterpret_cast<LPVOID*>(&ori_loadNoteHelpKey_posMap));
            if (status != MH_OK) {
                throw runtime_error("MinHook create loadNoteHelpKeyPos hook failed!");
            }

            cout << "[INFO]hook WebMPlayerOpen" << endl;
            WebMPlayerOpen_addr = *(uint64_t*)WebMPlayerOpen_addr;
            status = MH_CreateHook((LPVOID)WebMPlayerOpen_addr, &hooked_WebMPlayerOpen, reinterpret_cast<LPVOID*>(&ori_WebMPlayerOpen));
            if (status != MH_OK) {
                throw runtime_error("MinHook create WebMPlayerOpen hook failed!");
            }

            cout << "[INFO]fix text length scale factor" << endl;
            fix_TextWidthScalefactor(TextWidthScalefactor_addr);

            cout << "[INFO]fix text height scale factor" << endl;
            for (size_t i = 0; i < size(DialogBoxHeight_addr); i++)
                fix_DialogBoxHeight(DialogBoxHeight_addr[i]);

            cout << "[INFO]fix main_sjis_byte_valid" << endl;
            for (size_t i = 0; i < size(main_sjis_byte_valid_addr); i++)
                fix_main_sjis_byte_valid(main_sjis_byte_valid_addr[i]);

            status = MH_EnableHook(MH_ALL_HOOKS);
            if (status != MH_OK) {
                throw runtime_error("MinHook enable hook failed!");
            }
            cout << endl << "[Info]Hook成功" << endl << endl;
        }
        catch (const std::exception& e)
        {
            MH_Uninitialize();
            cerr << "[Error]" << e.what() << endl;
            cerr << "[Error]Hook失败" << endl << endl;
        }
    }


    void hook_uninstall() {
        MH_DisableHook(MH_ALL_HOOKS);
        MH_Uninitialize();
    }


    static void fix_noteHelpKey_pos(uintptr_t ptr) {
        uintptr_t pDic = ptr;
        pDic = (uintptr_t) * (uint64_t*)(pDic + 0x18);
        pDic = (uintptr_t) * (uint64_t*)(pDic + 0x48);
        pDic = (uintptr_t) * (uint64_t*)(pDic + 0x30);
        //导力器1
        uintptr_t pDic_key = (uintptr_t) * (uint64_t*)(pDic + 0x60);
        pDic_key = ((uintptr_t) * (uint64_t*)(pDic_key + 0x30)) + 0xc;
        *(float_t*)pDic_key += 2.0f;
        *(float_t*)(pDic_key + 4) += 3.0f;
        *(float_t*)(pDic_key + 0x1c) += 2.0f;
        //战技2
        pDic_key = (uintptr_t) * (uint64_t*)(pDic + 0x78);
        pDic_key = ((uintptr_t) * (uint64_t*)(pDic_key + 0x30)) + 0xc;
        *(float_t*)pDic_key = 443.0f;
        *(float_t*)(pDic_key + 4) = 153.0f;
        *(float_t*)(pDic_key + 8) = 40.0f;
        *(float_t*)(pDic_key + 0xC) = 40.0f;
        //快捷操作
        pDic_key = (uintptr_t) * (uint64_t*)(pDic + 0x8);
        pDic_key = ((uintptr_t) * (uint64_t*)(pDic_key + 0x30)) + 0xc;
        *(float_t*)pDic_key = 191.5f;
        *(float_t*)(pDic_key + 4) = 107.0f;
        *(float_t*)(pDic_key + 8) = 40.0f;
        *(float_t*)(pDic_key + 0xC) = 40.0f;
        *(float_t*)(pDic_key + 0x8c) = 85.0f;
        *(float_t*)(pDic_key + 0x90) += 0.5f;
        //爆裂猛攻
        pDic_key = (uintptr_t) * (uint64_t*)(pDic + 0x40);
        pDic_key = ((uintptr_t) * (uint64_t*)(pDic_key + 0x30));
        *(float_t*)(pDic_key + 0xc) = 514.0f;
        *(float_t*)(pDic_key + 0x7C) = 232.0f;
        *(float_t*)(pDic_key + 0x80) = 352.0f;
        *(float_t*)(pDic_key + 0x84) = 30.0f;
        *(float_t*)(pDic_key + 0x88) = 30.0f;
        *(float_t*)(pDic_key + 0x98) = 232.0f;
        *(float_t*)(pDic_key + 0x9C) = 416.0f;
        *(float_t*)(pDic_key + 0xA0) = 30.0f;
        *(float_t*)(pDic_key + 0xA4) = 30.0f;
        *(float_t*)(pDic_key + 0xB4) = 202.0f;
        *(float_t*)(pDic_key + 0xB8) = 275.5f;
        *(float_t*)(pDic_key + 0xBC) = 28.0f;
        *(float_t*)(pDic_key + 0xC0) = 28.0f;
        cout << "[INFO]Fixed note help key position" << endl;
    }

    int64_t __fastcall hooked_loadNoteHelpKey_posMap(uintptr_t ctx) {
        int64_t result = ori_loadNoteHelpKey_posMap(ctx);
        printf("[INFO]hooked_loadNoteHelpKey_posMap ctx: 0x%llx\n", ctx);
        fix_noteHelpKey_pos(ctx);
        return result;
    }

    int64_t __fastcall hooked_sjis2utf8(char* output, uint8_t* input, int64_t max_output, int32_t* _pTable, int64_t* usedLen) {
        if (*language_option_addr == 0) {
            return ori_sjis2utf8(output, input, max_output, _pTable, usedLen);
        }
        try {
            if (max_output == 0)
                return 0;
            iconv_t _hiconv = iconv_open("UTF-8", "CP936");
            if (_hiconv == (iconv_t)-1) {
                throw EncodingError("encoding error: gbk to utf8 iconv initialize failed.");
            }

            char* pdata = const_cast<char*>(reinterpret_cast<char*>(input));
            size_t in_len = encoding::get_input_length(pdata);
            size_t out_len = in_len * 3;
            vector<char> out_buffer(out_len, 0);
            char* out_ptr = out_buffer.data();

            size_t ret = iconv(_hiconv, &pdata, &in_len, &out_ptr, &out_len);
            if (ret == (size_t)-1) {
                throw EncodingError("encoding error: gbk to utf8 iconv convert failed.");
            }
            int32_t utf8_len = out_buffer.size() - out_len;
            utf8_len = min(utf8_len, max_output - 1);
            memcpy(output, out_buffer.data(), utf8_len);
            output[utf8_len] = 0;
            iconv_close(_hiconv);
            return utf8_len;
        }
        catch (const std::exception& e)
        {
            cout << e.what() << endl;
            throw;
        }
    }

    int64_t __fastcall hooked_utf82sjis(char* output, uint8_t* input, int64_t max_output, int64_t* usedLen) {
        if (*language_option_addr == 0) {
            return ori_utf82sjis(output, input, max_output, usedLen);
        }
        try {

            if (max_output == 0)
                return 0;
            iconv_t _hiconv = iconv_open("CP936", "UTF-8");
            if (_hiconv == (iconv_t)-1) {
                throw EncodingError("encoding error: utf8 to gbk iconv initialize failed.");
            }
            char* pdata = const_cast<char*>(reinterpret_cast<char*>(input));
            size_t in_len = encoding::get_input_length(pdata);
            size_t out_len = in_len * 3;
            vector<char> out_buffer(out_len, 0);
            char* out_ptr = out_buffer.data();
            size_t ret = iconv(_hiconv, &pdata, &in_len, &out_ptr, &out_len);
            if (ret == (size_t)-1) {
                throw EncodingError("encoding error: utf8 to gbk iconv convert failed.");
            }
            int32_t gbk_len = (int32_t)out_buffer.size() - out_len;
            gbk_len = min(gbk_len, max_output - 1);
            memcpy(output, out_buffer.data(), gbk_len);
            output[gbk_len] = 0;

            iconv_close(_hiconv);
            return gbk_len;

        }
        catch (const std::exception& e)
        {
            cout << e.what() << endl;
            throw;
        }
    }

    string trim_mess_string(string& str) {
        size_t pos = 0;
        str.erase(0, str.find_first_not_of(" \t\""));
        str.erase(str.find_last_not_of(" \t\"") + 1); // 去除头尾空白
        while ((pos = str.find("\\n", pos)) != string::npos) {
            str.replace(pos, 2, "\n");
            pos += 1;
        }
        return str;
    }

    unordered_map<string, string> build_mess_string_map(const char* data, const size_t len) {
        unordered_map<string, string> mess_map;
        size_t pos = 0;
        while (pos < len) {
            // 查找下一个换行符
            size_t line_end = pos;
            while (line_end < len && data[line_end] != '\n' && data[line_end] != '\r') {
                line_end++;
            }
            size_t line_len = line_end - pos; // 一行的长度
            if (line_len > 0) {
                // 查找冒号分隔符
                size_t colon_pos = pos;
                while (colon_pos < line_end && data[colon_pos] != ':') {
                    colon_pos++;
                }
                if (colon_pos < line_end) {
                    string key(data + pos, colon_pos - pos);
                    string value(data + colon_pos + 1, line_end - colon_pos - 1);

                    trim_mess_string(value);
                    mess_map[key] = value;
                    //cout << "Loaded key: \"" << key <<"\"" << " value: \"" << value << "\"" << endl;
                }
            }
            pos = line_end + 1;
        }
        return mess_map;
    }

    void write_mess_string(const char* ptr, const string& text) {
        const size_t len = text.size();
        const size_t capacity = len >= 16 ? len | 0xf : 0xf;

        char* mutable_ptr = const_cast<char*>(ptr);
        intptr_t* ptr_as_int = reinterpret_cast<intptr_t*>(mutable_ptr);

        if (len >= 16) {
            char* heap_mem = static_cast<char*>(malloc(capacity));
            if (heap_mem == nullptr) {
                throw runtime_error("Memory allocation failed for mess string.");
            }
            // +1 终止符
            memcpy(heap_mem, text.data(), len + 1);
            ptr_as_int[0] = reinterpret_cast<intptr_t>(heap_mem);
        }
        else if (len > 0) {
            memcpy(mutable_ptr, text.data(), len + 1);
        }
        else {
            ptr_as_int[0] = 0;
        }

        ptr_as_int[2] = static_cast<intptr_t>(len);     // mutable_ptr + 0x10
        ptr_as_int[3] = static_cast<intptr_t>(capacity); // mutable_ptr + 0x18
    }

    bool load_mess_string_cn(int64_t jpStructAddr) {
        char path[] = "data_cn\\localization\\mess_strings_cn.txt";
        FILE* file = nullptr;

        if (fopen_s(&file, path, "rb") != 0) {
            return false;
        }
        fseek(file, 0, SEEK_END);
        long fileSize = ftell(file);
        std::vector<char> buffer(fileSize);
        fseek(file, 0, 0);
        char* pbuffer = buffer.data();
        size_t readBytes = fread(pbuffer, 1, fileSize, file);
        fclose(file);
        file = nullptr;
        if (fileSize != readBytes)
            return false;
        unordered_map<string, string> mess_map = build_mess_string_map(pbuffer, fileSize);
        string FileName[2];
        get_mess_string_key = reinterpret_cast<get_mess_string_key_t>(get_mess_string_key_addr);
        for (int32_t i = 1; i < 0xC1E; i++)
        {
            auto key = get_mess_string_key(reinterpret_cast<__int64>(FileName), i);

            if (!FileName[0].empty() && mess_map.count(FileName[0])) {
                write_mess_string((char*)(jpStructAddr + (int64_t)i * 0x20), mess_map[FileName[0]]);
            }
            else {
                cerr << "[INFO]未找到 mess string : " << FileName[0] << endl;
            }
        }
        memset(pbuffer, 0, buffer.size());
        cout << "[INFO]已载入mess_strings_cn" << endl;
        return true;
    }

    int64_t __fastcall hooked_check_encoding(char* input_str) {
        return encoding::check_encoding(input_str);
    }

    int64_t __fastcall hooked_load_mess_string(int64_t pStruct) {
        int64_t result = ori_load_mess_string(pStruct);
        printf("[INFO]hooked_load_mess_string pStruct: 0x%llx\n", pStruct + mess_string_jp_offset_offset);
        load_mess_string_cn(pStruct + mess_string_jp_offset_offset + 0x8);
        return result;
    }

    int32_t cp_mapping(int32_t cp) {
        switch (cp)
        {
        case 0x4E04:
            return 0x30FB;  // 丄 → ・
        case 0x4E05:
            return 0x266A;  // 丅 → ♪
        case 0x4E06:
            return 0x246A;  // 丆 → ⑪
        default:
            return cp;
        }
    }

    static int32_t FindUnicodeInTable(int32_t cp, uint64_t* fontIndexTable, size_t totalCount) {
        int32_t nodeIndex = 1;
        while (true)
        {
            int32_t arrayIndex = nodeIndex - 1;
            if (arrayIndex >= totalCount)
                return -1;
            int32_t currentUnicode = *(int32_t*)(fontIndexTable + arrayIndex);
            if (currentUnicode == cp) {
                return arrayIndex;
            }
            nodeIndex *= 2;
            if (cp >= currentUnicode)
                nodeIndex |= 1;
        }
        return -1;
    }


    int64_t __fastcall hooked_sjis2uni(int64_t ctx, int32_t* output_addr, char* input_str, int64_t max_output, int64_t* usedLen)
    {

        if (*language_option_addr == 0) {
            return ori_sjis2uni(ctx, output_addr, input_str, max_output, usedLen);
        }
        try
        {
            if (!max_output)
                return 0;
            // font table
            uintptr_t fontAddr = *(uintptr_t*)(ctx + 0x10);
            if (!fontAddr || !input_str || max_output == 1) {
                *output_addr = -1;
                return 0;
            }
            uint64_t* fontEntryAddr = (uint64_t*)(fontAddr + 0x50);
            uint64_t* fontIndexTable = *(uint64_t**)(fontAddr + 0x58);

            if (!*fontEntryAddr || encoding::check_encoding(input_str) == 0) {
                *output_addr = -1;
                return 0;
            }
            int32_t totalCharCount = *(int32_t*)(*fontEntryAddr + 0x8);
            int32_t unfound_symbol_index = -1;
            // search
            vector<int32_t> unicodes = encoding::chars_to_unicode(input_str, max_output, usedLen);

            size_t uni_len = unicodes.size();

            for (uint32_t i = 0; i < uni_len; i++)
            {
                int32_t* output = (int32_t*)(output_addr + i);
                int32_t cp = unicodes[i];

                if (cp == -1) {
                    *output = -1;
                    break;
                }
                cp = cp_mapping(cp);

                int32_t index = FindUnicodeInTable(cp, fontIndexTable, totalCharCount);

                if (index == -1)// not found
                {
                    if (unfound_symbol_index == -1) {
                        unfound_symbol_index = FindUnicodeInTable(9632, fontIndexTable, totalCharCount);
                    }
                    *output = unfound_symbol_index;
                }
                else
                {
                    *output = index;
                }
            }
            return uni_len - 1;
        }
        catch (EncodingError e)
        {
            stringstream ss; // 已初始化
            ss << e.what() << endl;
            ss << "input_str_ptr: " << hex << (intptr_t)(input_str) << endl;
            ss << "size: " << dec << encoding::get_input_length(input_str) << endl;
            ss << "max_output: " << dec << max_output << endl;
            cerr << ss.str() << endl;
        }

        int64_t original_count = ori_sjis2uni(ctx, output_addr, input_str, max_output, usedLen);
        return original_count;
    }

    static const vector<pair<string, string>> patterns = {
      {"data_pc\\", "data_cn\\pc\\"},
      {"data\\", "data_cn\\"},
    };

    string redirect_dir(string file) {
        string searchFile = file;
        std::replace(searchFile.begin(), searchFile.end(), '/', '\\');
        bool showInfo = is_debug;
        if (showInfo) {
            cout << "[DEBUG][CreateFileA]：" << file;
        }
        for (const auto& pattern : patterns) {
            size_t pos = searchFile.find(pattern.first);
            if (pos != string::npos) {
                string redirected = searchFile;
                redirected.replace(pos, pattern.first.length(), pattern.second);
                if (std::filesystem::exists(redirected)) {
                    if (showInfo) {
                        cout << "  重定向=>  " << redirected << endl;
                    }
                    return redirected;
                }
            }
        }
        if (showInfo) {
            cout << endl;
        }
        return file;
    }

    bool __fastcall hooked_WebMPlayerOpen(int64_t h, char* file) {
        string fileName = redirect_dir(string(file));
        char buf[256] = { 0 };
        strncpy_s(buf, fileName.c_str(), sizeof(buf) - 1);
        return ori_WebMPlayerOpen(h, buf);
    }

    HANDLE WINAPI hooked_CreateFileA(LPCSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode,
        LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition,
        DWORD dwFlagsAndAttributes, HANDLE hTemplateFile) {

        string fileName = redirect_dir(string(lpFileName));

        return ori_CreateFileA(fileName.c_str(), dwDesiredAccess, dwShareMode,
            lpSecurityAttributes, dwCreationDisposition,
            dwFlagsAndAttributes, hTemplateFile);
    };

    HANDLE WINAPI hooked_CreateWindowExA(DWORD dwExStyle, LPCSTR lpClassName, LPCSTR lpWindowName,
        DWORD dwStyle, int X, int Y, int nWidth, int nHeight, HWND hWndParent, HMENU hMenu,
        HINSTANCE hInstance, LPVOID lpParam) {
        string name = lpWindowName ? string(lpWindowName) : "";
        if (name.find("The Legend of Heroes: Trails to Azure") != string::npos) {
            lpWindowName = "英雄传说 碧之轨迹：改";
        }
        return ori_CreateWindowExA(dwExStyle, lpClassName, lpWindowName,
            dwStyle, X, Y, nWidth, nHeight, hWndParent, hMenu,
            hInstance, lpParam);
    };

}