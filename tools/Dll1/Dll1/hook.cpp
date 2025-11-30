#include "pch.h"
#include "hook.h"

using namespace std;


namespace hook {
    string sjis2uni_addr_pattern = "48 89 54 24 10 55 41 54 41 57 48";
    string check_encoding_addr_pattern = "5F 5B 5D C3 CC CC CC CC CC CC CC CC CC 48 89 5C 24 08"; // result+0xd
    string load_mess_string_addr_pattern = "FF C3 CC CC CC CC CC 48 89 5C 24 18"; //result+0x7
    string get_mess_string_key_addr_pattern = "40 53 48 83 EC 20 48 8B D9 81 FA 1B";
    string sjis2utf8_addr_pattern = "40 56 48 83 EC 10";
    string utf82sjis_addr_pattern = "48 89 5C 24 10 56 49 8B D9";

    string memo_sjis_byte_valid_addr_pattern = "41 0f b6 0f 80 f9 7f 76 ?? 8d 41 60 3c 3f 76 ??"; //result+0x7
    string memo_2_sjis_byte_valid_addr_pattern = "41 0F B6 0F 80 F9 7F 0F"; //result+0x4
    string desc_sjis_byte_valid_addr_pattern = "00 0F B6 0F 80 F9 7F 76 ?? 8d";  //result+0x4
    string unk_r10_sjis_byte_valid_addr_pattern = "49 FF C6 80 F9 7F 76"; //result+3
    string add_al_60_r15_sjis_byte_valid_addr_pattern = "3C 7F 0F 86 ?? ?? 00 00 04 60";
    string menu_rsi_sjis_byte_valid_addr_pattern = "8D 41 60 3C 3F 76 ?? 44";
    /* mess_string_jp_struct_addr_pattern
    140101A17 - 89 5D F7              - mov [rbp-09],ebx
    140101A1A - 48 8D 15 17AF3800     - lea rdx,[14048C938] { ("MESS_UNDEFINED") }
    140101A21 - 48 8D 4D C7           - lea rcx,[rbp-39]
    140101A25 - E8 56E5FFFF           - call 1400FFF80
    */
    string mess_string_jp_struct_addr_pattern = "89 5d ?? 48 8d 15 ?? ?? ?? 00"; //result+0x10+0x38af17

    uintptr_t sjis2uni_addr =                           0x140078FB0;
    uintptr_t check_encoding_addr =                     0x140078850;
    uintptr_t load_mess_string_addr =                   0x1401015C0;
    uintptr_t get_mess_string_key_addr =                0x1400C72B0;
	uintptr_t mess_string_jp_struct_addr =              0x14048C938;
    uintptr_t sjis2utf8_addr =                          0x140078B20;
    uintptr_t utf82sjis_addr =                          0x140078D00;

    uintptr_t memo_sjis_byte_valid_addr =               0x140289c33;                   //手册Messstring
    uintptr_t memo_2_sjis_byte_valid_addr =             0x14028B905;                   //手册 2
    uintptr_t desc_sjis_byte_valid_addr =               0x1401CE400;                   //物品描述
    uintptr_t unk_r10_sjis_byte_valid_addr =            0x140210677;                   //unknown
    uintptr_t menu_rsi_sjis_byte_valid_addr =           0x140332CC7;                   //menu
    uintptr_t add_al_60_r15_sjis_byte_valid_addr[2] = { 0x140211D65 ,0x140214998 };    //0: 对话printText2; 1: unk


    sjis2uni_t ori_sjis2uni = nullptr;
    checkEncoding_t ori_check_encoding = nullptr;
    load_mess_string_t ori_load_mess_string = nullptr;
	sjis2utf8_t ori_sjis2utf8 = nullptr;
	utf82sjis_t ori_utf82sjis = nullptr;
    get_mess_string_key_t get_mess_string_key = reinterpret_cast<get_mess_string_key_t>(get_mess_string_key_addr);


    void search_all_addresses() {
        vector<uintptr_t> matchResults;

        if (SearchModuleMemory(sjis2uni_addr_pattern, matchResults, true) && matchResults.size() == 1) {
            sjis2uni_addr = matchResults[0];
            cout << "sjis2uni_addr : 0x" << hex << sjis2uni_addr << endl;
        }
        else throw runtime_error("sjis2uni_addr pattern not found or multiple results.");

        if (SearchModuleMemory(check_encoding_addr_pattern, matchResults) && matchResults.size() == 1) {
            check_encoding_addr = matchResults[0] + 0xd;
            cout << "check_encoding_addr : 0x" << hex << check_encoding_addr << endl;
        }
        else throw runtime_error("check_encoding_addr pattern not found or multiple results.");

        if (SearchModuleMemory(load_mess_string_addr_pattern, matchResults) && matchResults.size() == 1) {
            load_mess_string_addr = matchResults[0] + 0x7;
            cout << "load_mess_string_addr : 0x" << hex << load_mess_string_addr << endl;
        }
        else throw runtime_error("load_mess_string_addr pattern not found or multiple results.");

        if (SearchModuleMemory(get_mess_string_key_addr_pattern, matchResults) && matchResults.size() == 1) {
            get_mess_string_key_addr = matchResults[0] ;
            cout << "get_mess_string_key_addr : 0x" << hex << get_mess_string_key_addr << endl;
        }
        else throw runtime_error("get_mess_string_key_addr pattern not found or multiple results.");

        if (SearchModuleMemory(mess_string_jp_struct_addr_pattern, matchResults) && matchResults.size() == 1) {
            mess_string_jp_struct_addr = matchResults[0] + 0x6;
			uint32_t* offset_ptr = reinterpret_cast<uint32_t*>(mess_string_jp_struct_addr);
            mess_string_jp_struct_addr += (uint64_t)*offset_ptr + 0x4;
            cout << "mess_string_jp_struct_addr : 0x" << hex << mess_string_jp_struct_addr << endl;
        }

        if (SearchModuleMemory(sjis2utf8_addr_pattern, matchResults) && matchResults.size() == 1) {
            sjis2utf8_addr = matchResults[0];
            cout << "sjis2utf8_addr : 0x" << hex << sjis2utf8_addr << endl;
        }

        if (SearchModuleMemory(utf82sjis_addr_pattern, matchResults) && matchResults.size() == 1) {
            utf82sjis_addr = matchResults[0];
            cout << "utf82sjis_addr : 0x" << hex << utf82sjis_addr << endl;
        }

        cout << "==============================" << endl;

        if (SearchModuleMemory(memo_sjis_byte_valid_addr_pattern, matchResults) && matchResults.size() == 1) {
            memo_sjis_byte_valid_addr = matchResults[0] + 0x4;
            cout << "memo_sjis_byte_valid_addr : 0x" << hex << memo_sjis_byte_valid_addr << endl;
        }

        if (SearchModuleMemory(memo_2_sjis_byte_valid_addr_pattern, matchResults) && matchResults.size() == 1) {
            memo_2_sjis_byte_valid_addr = matchResults[0] + 0x4;
            cout << "memo_2_sjis_byte_valid_addr : 0x" << hex << memo_2_sjis_byte_valid_addr << endl;
        }

        if (SearchModuleMemory(desc_sjis_byte_valid_addr_pattern, matchResults) && matchResults.size() == 1) {
            desc_sjis_byte_valid_addr = matchResults[0] + 0x4;
            cout << "desc_sjis_byte_valid_addr : 0x" << hex << desc_sjis_byte_valid_addr << endl;
        }

        if (SearchModuleMemory(unk_r10_sjis_byte_valid_addr_pattern, matchResults) && matchResults.size() == 1) {
            unk_r10_sjis_byte_valid_addr = matchResults[0] + 0x3;
            cout << "unk_r10_sjis_byte_valid_addr : 0x" << hex << unk_r10_sjis_byte_valid_addr << endl;
        }
        if (SearchModuleMemory(menu_rsi_sjis_byte_valid_addr_pattern, matchResults) && matchResults.size() == 1) {
            menu_rsi_sjis_byte_valid_addr = matchResults[0] ;
            cout << "menu_rsi_sjis_byte_valid_addr : 0x" << hex << menu_rsi_sjis_byte_valid_addr << endl;
        }
        if (SearchModuleMemory(add_al_60_r15_sjis_byte_valid_addr_pattern, matchResults) && matchResults.size() == 2) {
            for (size_t i = 0; i < 2; i++)
            {
                add_al_60_r15_sjis_byte_valid_addr[i] = matchResults[i];
                cout << "add_al_60_r15_sjis_byte_valid_addr " << dec << i << " : 0x" << hex << add_al_60_r15_sjis_byte_valid_addr[i] << endl;
            }
        }


        else throw runtime_error("mess_string_jp_struct_addr pattern not found or multiple results.");

    }

    static void hook_memo_2_sjis_byte_valid(uintptr_t ptr) {

        hook_sjis* hook = new hook_sjis(ptr, 20, 20);
        hook->calcSingleByteAddr(5, 4);
        hook->start();
        hook->cmp_cl(0x7f);
        hook->jna_singleByte();
        hook->cmp_cl(0x81);
        hook->jb_singleByte();
        hook->cmp_cl(0xFE);
        hook->ja_singleByte();
        hook->cmp_byte_r15(1, 0x40);
        hook->jb_singleByte();
        hook->cmp_byte_r15(1, 0xFE);
        hook->ja_singleByte();
        hook->end();
        delete hook;
        hook = nullptr;
        /* original r15
            14028B901 - 41 0FB6 0F            - movzx ecx,byte ptr [r15]
            14028B905 - 80 F9 7F              - cmp cl,7F { 127 }           <--input ptr
            14028B908 - 0F86 E2060000         - jbe 14028BFF0
            14028B90E - 8D 41 60              - lea eax,[rcx+60]
            14028B911 - 3C 3F                 - cmp al,3F { 63 }
            14028B913 - 0F86 D7060000         - jbe 14028BFF0
            14028B919 - 41 8B D0              - mov edx,r8d
            14028B91C - 65 48 8B 04 25 58000000  - mov rax,gs:[00000058]
            14028B925 - 48 8B 08              - mov rcx,[rax]               <--return 
        */
    }

    static void hook_menu_rsi_sjis_byte_valid(uintptr_t ptr) {
        hook_sjis* hook = new hook_sjis(ptr, 7, 14);
        hook->calcSingleByteAddr(6, 1);
        hook->start();
        hook->cmp_cl(0x7f);
        hook->jna_singleByte();
        hook->cmp_cl(0x81);
        hook->jb_singleByte();
        hook->cmp_cl(0xFE);
        hook->ja_singleByte();
        hook->cmp_byte_rsi(0, 0x40);
        hook->jb_singleByte();
        hook->cmp_byte_rsi(0, 0xFE);
        hook->ja_singleByte();
        hook->end();
        delete hook;
        hook = nullptr;
        /* original r15
            140332CC7 - 8D 41 60              - lea eax,[rcx+60]
            140332CCA - 3C 3F                 - cmp al,3F { 63 }
            140332CCC - 76 AD                 - jna 140332C7B 
            140332CCE - 44 38 A5 79100000     - cmp [rbp+00001079],r12l
            140332CD5 - 75 0C                 - jne 140332CE3
            140332CD7 - 88 0B                 - mov [rbx],cl
            140332CD9 - 0FB6 06               - movzx eax,byte ptr [rsi]
        */
    }

    static void hook_unk_r10_sjis_byte_valid(uintptr_t ptr) {
        hook_sjis* hook = new hook_sjis(ptr, 14, 14);
        hook->calcSingleByteAddr(4, 1);
        hook->start();
        hook->cmp_cl(0x7f);
        hook->jna_singleByte();
        hook->cmp_cl(0x81);
        hook->jb_singleByte();
        hook->cmp_cl(0xFE);
        hook->ja_singleByte();
        hook->cmp_byte_r10(1, 0x40);
        hook->jb_singleByte();
        hook->cmp_byte_r10(1, 0xFE);
        hook->ja_singleByte();
        hook->end();
        delete hook;
        hook = nullptr;
        /* original r15
            140210674 - 49 FF C6              - inc r14
            140210677 - 80 F9 7F              - cmp cl,7F { 127 }                <--input ptr
            14021067A - 76 2D                 - jna 1402106A9                    <--singleByte
            14021067C - 8D 41 60              - lea eax,[rcx+60]
            14021067F - 3C 3F                 - cmp al,3F { 63 }
            140210681 - 76 26                 - jna 1402106A9
            140210683 - 88 0A                 - mov [rdx],cl
            140210685 - F3 45 0F10 98 80000000  - movss xmm11,[r8+00000080]      <--return
            14021068E - 49 FF C2              - inc r10
        */
    }

    static void hook_desc_sjis_byte_valid(uintptr_t ptr) {
        hook_sjis* hook = new hook_sjis(ptr, 12, 16);
        hook->calcSingleByteAddr(4, 1);
		hook->start();
        hook->cmp_cl(0x7f);
        hook->jna_singleByte();
        hook->cmp_cl(0x81);
        hook->jb_singleByte();
        hook->cmp_cl(0xFE);
        hook->ja_singleByte();
        hook->cmp_byte_rdi(1,0x40);
        hook->jb_singleByte();
        hook->cmp_byte_rdi(1, 0xFE);
        hook->ja_singleByte();
		hook->end();
        delete hook;
        hook = nullptr;
        /* original 
		zero.exe+1CE400 - 80 F9 7F              - cmp cl,7F { 127 }             <--input ptr
		zero.exe+1CE403 - 76 23                 - jna zero.exe+1CE428           <--singleByte
        zero.exe+1CE405 - 8D 41 60              - lea eax,[rcx+60]
        zero.exe+1CE408 - 3C 3F                 - cmp al,3F { 63 }
        zero.exe+1CE40A - 76 1C                 - jna zero.exe+1CE428
        zero.exe+1CE40C - 0FB6 47 01            - movzx eax,byte ptr [rdi+01]
        zero.exe+1CE410 - 48 83 C7 02           - add rdi,02 { 2 }              <--return
        */
    }
    static void hook_add_al_60_r15_sjis_byte_valid(uintptr_t ptr) {
        hook_sjis* hook = new hook_sjis(ptr, 18, 18);
        hook->calcSingleByteAddr(4, 4);
        hook->start();
        hook->cmp_al(0x7f);
        hook->jna_singleByte();
        hook->cmp_al(0x81);
        hook->jb_singleByte();
        hook->cmp_al(0xFE);
        hook->ja_singleByte();
        hook->cmp_byte_r15(1, 0x40);
        hook->jb_singleByte();
        hook->cmp_byte_r15(1, 0xFE);
        hook->ja_singleByte();
        hook->end();
        delete hook;
        hook = nullptr;
        /*  original code:
        140211D65 - 3C 7F                 - cmp al,7F { 127 }       <--input ptr
        140211D67 - 0F86 2A070000         - jbe 140212497
        140211D6D - 04 60                 - add al,60 { 96 } 
        140211D6F - 3C 3F                 - cmp al,3F { 63 }
        140211D71 - 0F86 20070000         - jbe 140212497
        140211D77 - 41 B8 02000000        - mov r8d,00000002 { 2 }  <--return
        */
    }

    static void hook_memo_sjis_byte_valid(uintptr_t ptr) {
        hook_sjis* hook = new hook_sjis(ptr, 12, 17);
        hook->calcSingleByteAddr(4, 1);
        hook->start();
        hook->cmp_cl(0x7f);
        hook->jna_singleByte();
        hook->cmp_cl(0x81);
        hook->jb_singleByte();
        hook->cmp_cl(0xFE);
        hook->ja_singleByte();
        hook->cmp_byte_r15(1, 0x40);
        hook->jb_singleByte();
        hook->cmp_byte_r15(1, 0xFE);
        hook->ja_singleByte();
        hook->end();
        delete hook;
        hook = nullptr;
		/* original code:
            140289C33 - 80 F9 7F              - cmp cl,7F { 127 }
            140289C36 - 76 36                 - jna 140289C6E
            140289C38 - 8D 41 60              - lea eax,[rcx+60]
            140289C3B - 3C 3F                 - cmp al,3F { 63 }
            140289C3D - 76 2F                 - jna 140289C6E
            140289C3F - 41 0FB6 47 01         - movzx eax,byte ptr [r15+01]
            140289C44 - 83 C7 02              - add edi,02 { 2 }
            140289C47 - 49 83 C7 02           - add r15,02 { 2 }
        */
    }

	void hook_install() {
    
        try
        {
            cout << endl;
            search_all_addresses();
            cout << "==============================" << endl;
            MH_STATUS status = MH_Initialize();
            if (status != MH_OK) {
				throw runtime_error("MinHook initialize failed!");
            }
            cout << "[INFO]hooking sjis2uni" << endl;
            status = MH_CreateHook((LPVOID)sjis2uni_addr, &hooked_sjis2uni, reinterpret_cast<LPVOID*>(&ori_sjis2uni));
            if (status != MH_OK) {
				throw runtime_error("MinHook create sjis2uni hook failed!");
            }
            cout << "[INFO]hooking check_encoding" << endl;
            status = MH_CreateHook((LPVOID)check_encoding_addr, &hooked_check_encoding, reinterpret_cast<LPVOID*>(&ori_check_encoding));
            if (status != MH_OK) {
				throw runtime_error("MinHook create check_encoding hook failed!");
            }
            cout << "[INFO]hooking load_mess_string" << endl;
            status = MH_CreateHook((LPVOID)load_mess_string_addr, &hooked_load_mess_string, reinterpret_cast<LPVOID*>(&ori_load_mess_string));
            if (status != MH_OK) {
				throw runtime_error("MinHook create load_mess_string hook failed!");
            }
            cout << "[INFO]hooking sjis2utf8" << endl;
            status = MH_CreateHook((LPVOID)sjis2utf8_addr, &hooked_sjis2utf8, reinterpret_cast<LPVOID*>(&ori_sjis2utf8));
            if (status != MH_OK) {
                throw runtime_error("MinHook create sjis2utf8 hook failed!");
            }
            cout << "[INFO]hooking utf82sjis" << endl;
            status = MH_CreateHook((LPVOID)utf82sjis_addr, &hooked_utf82sjis, reinterpret_cast<LPVOID*>(&ori_utf82sjis));
            if (status != MH_OK) {
                throw runtime_error("MinHook create utf82sjis hook failed!");
            }

            cout << "[INFO]hooking memo_sjis_byte_valid" << endl;
            hook_memo_sjis_byte_valid(memo_sjis_byte_valid_addr);

            cout << "[INFO]hooking memo_2_sjis_byte_valid" << endl;
            hook_memo_2_sjis_byte_valid(memo_2_sjis_byte_valid_addr);

            cout << "[INFO]hooking desc_sjis_byte_valid" << endl;
            hook_desc_sjis_byte_valid(desc_sjis_byte_valid_addr);

            cout << "[INFO]hooking unk_r10_sjis_byte_valid" << endl;
            hook_unk_r10_sjis_byte_valid(unk_r10_sjis_byte_valid_addr);
            
            cout << "[INFO]hooking menu_rsi_sjis_byte_valid" << endl;
            hook_menu_rsi_sjis_byte_valid(menu_rsi_sjis_byte_valid_addr);


            for (size_t i = 0; i < 2; i++){
                cout << "[INFO]hooking add_al_60_r15_sjis_byte_valid " << dec << i << endl;
                hook_add_al_60_r15_sjis_byte_valid(add_al_60_r15_sjis_byte_valid_addr[i]);
            }

            status = MH_EnableHook(MH_ALL_HOOKS);
            if (status != MH_OK) {
				throw runtime_error("MinHook enable hook failed!");
            }
            cout << endl << "[INFO]Hook成功" << endl << endl;
        }
        catch (const std::exception& e)
        {
            MH_Uninitialize();
            cerr << "[Error]" << e.what() << endl ;
            cerr << "[Error]Hook失败" << endl << endl;
        }
	}

	void hook_uninstall() {
        MH_DisableHook(MH_ALL_HOOKS);
        MH_Uninitialize();
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

    bool load_mess_string_cn() {
        char path[] = "data\\localization\\mess_strings_cn.txt";
        FILE* file = nullptr;

        if (fopen_s(&file, path, "rb") != 0) {
            return false;
        }
        fseek(file, 0, SEEK_END);
        long fileSize = ftell(file);
        std::vector<char> buffer(fileSize);
        fseek(file, 0, 0);
        size_t readBytes = fread(buffer.data(), 1, fileSize, file);
        fclose(file);
        file = nullptr;
        if (fileSize != readBytes)
            return false;
        unordered_map<string, string> mess_map = build_mess_string_map(buffer.data(), fileSize);
        string FileName[2];
        
        for (int32_t i = 1; i < 0xA1C; i++)
        {
            auto key = get_mess_string_key(reinterpret_cast<__int64>(FileName), i);

            if (!FileName[0].empty() && mess_map.count(FileName[0])) {
                write_mess_string((char*)(mess_string_jp_struct_addr + (int64_t)i * 0x20), mess_map[FileName[0]]);
            }
            else {
                cerr << "[INFO]未找到 mess string : " << FileName[0] << endl;
            }
        }
        mess_map.clear();
        return true;
    }

    int64_t __fastcall hooked_sjis2utf8(char* output, uint8_t* input, int64_t max_output, int32_t* _pTable) {
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

    int64_t __fastcall hooked_utf82sjis(char* output, uint8_t* input, int64_t max_output) {
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

    int32_t __fastcall hooked_load_mess_string () {
        int32_t result = ori_load_mess_string();
        load_mess_string_cn();
        return result;
    }
  
    int64_t __fastcall hooked_check_encoding(char* input_str) {
        return encoding::check_encoding(input_str);
    }

    int64_t __fastcall hooked_sjis2uni(int64_t ctx, int32_t* output_addr, char* input_str, int64_t max_output)
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

        if (!*fontEntryAddr || encoding::check_encoding(input_str) ==0) {
            *output_addr = -1;
            return 0;
        }

        int32_t totalCharCount = *(int32_t*)(*fontEntryAddr + 0x8);
        // search
        try
        {
            vector<int32_t> unicodes = encoding::chars_to_unicode(input_str, max_output);
            size_t uni_len = unicodes.size();
            int32_t unfound_symbol_index = -1;

            for (uint32_t i = 0; i < uni_len; i++)
            {
                int32_t* output = (int32_t*)(output_addr + i);
                int32_t cp = unicodes[i];
                if (cp == -1) {
                    *output = -1;
                    break;
                }
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

        int64_t original_count = ori_sjis2uni(ctx, output_addr, input_str, max_output);
        return original_count;
    }
}