#include "hook.h"

using namespace std;

namespace hook {
    string sjis2uni_addr_pattern = "48 89 54 24 10 55 41 54 41 57 48";
    string check_encoding_addr_pattern = "5F 5B 5D C3 CC CC CC CC CC CC CC CC CC 48 89 5C 24 08"; // result+0xd
    string load_mess_string_addr_pattern = "FF C3 CC CC CC CC CC 48 89 5C 24 18 48 89 4C 24 08"; //result+0x7
    string get_mess_string_key_addr_pattern = "40 53 48 83 EC 20 48 8B D9 81 FA ?? ?? 00 00 0f";
    string mess_string_jp_struct_addr_pattern = "89 5d ?? 48 8d 15 ?? ?? ?? 00"; //result+0x10+0x38af17
    string sjis2utf8_addr_pattern = "40 56 48 83 EC 10";
    string utf82sjis_addr_pattern = "48 89 5C 24 10 56 49 8B D9";
    string language_option_addr_pattern = "8B 0D ?? ?? ?? 00 85 c9 74";
    string noteHelpKey_posMap_addr_pattern = "F3 0F 10 05 ?? ?? ?? 00 F3 0F 11 05 ?? ?? ?? 00 4c 8b c0 41";
    string loadNoteHelpKey_posMap_addr_pattern = "48 89 5C 24 ?? 48 89 4C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 8D AC 24 ?? ?? FF FF 48 81 EC ?? ?? 00 00 48 8d 05 ?? ?? ?? 00";
    string WebMPlayerOpen_addr_pattern = "72 03 48 8B 16 48 8B CB FF 15";
    string TextWidthScalefactor_addr_pattern = "F3 0F 59 D1 F3 41 0F 59 D0 F3 0F 58 C2 F3 41 0F 59 C0";
    string DialogBoxHeight_addr_pattern = "66 0F 6E C8 0F 5B C9 F3 0F 58 C8 F3 0F 2C C1 89 83 8C 01 00 00 48 8B 47 40";

    string main_sjis_byte_valid_addr_pattern = "80 ?? 7F 76";
    string ui_sjis_byte_valid_addr_pattern = "3C 3F 77";
    string talk_sjis_byte_valid_addr_pattern = "3C 1F 76 ?? 80";
    string menu_sjis_byte_valid_addr_pattern = "8D 41 60 3C 3F 76 ?? 44";
	string escape_sjis_byte_valid_addr_pattern = "41 8D 48 60 80 F9 3F 77";
	string story_sjis_byte_valid_addr_pattern = "80 F9 7F 0F ?? ?? ?? 00 00 ?? ?? 60";
	string add_al_60_r15_sjis_byte_valid_addr_pattern = "04 60 3c 3f 0f";

    uintptr_t sjis2uni_addr = 0x140075A60;
    uintptr_t check_encoding_addr = 0x140075300;
    uintptr_t load_mess_string_addr = 0x140110950;
    uintptr_t get_mess_string_key_addr = 0x1400C6EF0;
    uintptr_t mess_string_jp_struct_addr = 0x14051FFF8;
    uintptr_t sjis2utf8_addr = 0x1400755D0;
    uintptr_t utf82sjis_addr = 0x1400757B0;
    int32_t* language_option_addr = reinterpret_cast<int32_t*>(0x140538B74);
    uintptr_t noteHelpKey_posMap_addr = 0x14053a160;
    uintptr_t loadNoteHelpKey_posMap_addr = 0x1400beb80;
    uintptr_t WebMPlayerOpen_addr = 0x14041AEC0;
    uintptr_t TextWidthScalefactor_addr = 0x14046b260;
    uintptr_t DialogBoxHeight_addr = 0x140232A58;

    uintptr_t main_sjis_byte_valid_addr[] = 
    { 
        0x14022C3C7,
        0x140231AC0,
        0x140231B0D,
        0x1402AEE7E,
        0x1402AFF20
    };
    uintptr_t talk_sjis_byte_valid_addr[] =
    {
        0x14011400F,
        0x14022B692,
        0x14023125E,
        0x14036D88E
    };
    uintptr_t ui_sjis_byte_valid_addr[] =
    {
        0x140200669,
        0x14023161D,
        0x1402F9138
    };
    uintptr_t story_sjis_byte_valid_addr[] = 
    {
        0x1402309E8,
        0x1402B0734
    };
    uintptr_t menu_sjis_byte_valid_addr = 0x14037A60B;
	uintptr_t escape_sjis_byte_valid_addr = 0x1402AC70D;

	uintptr_t add_al_60_r15_sjis_byte_valid_addr = 0x14022DC7F;

    CreateFileA_t ori_CreateFileA = nullptr;
    CreateWindowExA_t ori_CreateWindowExA = nullptr;
    sjis2uni_t ori_sjis2uni = nullptr;
    sjis2uni_t ori_check_encoding = nullptr;
    load_mess_string_t ori_load_mess_string = nullptr;
    get_mess_string_key_t get_mess_string_key = nullptr;
    sjis2utf8_t ori_sjis2utf8 = nullptr;
    utf82sjis_t ori_utf82sjis = nullptr;
    loadNoteHelpKey_posMap_t ori_loadNoteHelpKey_posMap = nullptr;
    WebMPlayerOpen_t ori_WebMPlayerOpen = nullptr;

    bool isMatchSuccessful = false;
    int matchedAddrCount = 0;
    int totalAddrCount = 30;

    void search_all_addresses() {
        vector<uintptr_t> matchResults;
        if (SearchModuleMemory(sjis2uni_addr_pattern, matchResults, true) && matchResults.size() == 1) {
            sjis2uni_addr = matchResults[0];
            cout << "sjis2uni_addr : 0x" << hex << sjis2uni_addr << endl;
            matchedAddrCount++;
        }

        if (SearchModuleMemory(check_encoding_addr_pattern, matchResults) && matchResults.size() == 1) {
            check_encoding_addr = matchResults[0] + 0xd;
            cout << "check_encoding_addr : 0x" << hex << check_encoding_addr << endl;
            matchedAddrCount++;
        }

        if (SearchModuleMemory(load_mess_string_addr_pattern, matchResults) && matchResults.size() == 1) {
            load_mess_string_addr = matchResults[0] + 0x7;
            cout << "load_mess_string_addr : 0x" << hex << load_mess_string_addr << endl;
            matchedAddrCount++;
        }

        if (SearchModuleMemory(get_mess_string_key_addr_pattern, matchResults) && matchResults.size() == 1) {
            get_mess_string_key_addr = matchResults[0];
            cout << "get_mess_string_key_addr : 0x" << hex << get_mess_string_key_addr << endl;
            matchedAddrCount++;
        }

        if (SearchModuleMemory(mess_string_jp_struct_addr_pattern, matchResults) && matchResults.size() == 1) {
            mess_string_jp_struct_addr = matchResults[0] + 0x6;
            uint32_t* offset_ptr = reinterpret_cast<uint32_t*>(mess_string_jp_struct_addr);
            mess_string_jp_struct_addr += (uint64_t)*offset_ptr + 0x4;
            cout << "mess_string_jp_struct_addr : 0x" << hex << mess_string_jp_struct_addr << endl;
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

        if (SearchModuleMemory(noteHelpKey_posMap_addr_pattern, matchResults) && matchResults.size() == 1) {
            noteHelpKey_posMap_addr = matchResults[0] + 0xc;
            uint32_t* offset_ptr = reinterpret_cast<uint32_t*>(noteHelpKey_posMap_addr);
            noteHelpKey_posMap_addr += (uint64_t)*offset_ptr + 0x4;
            cout << "noteHelpKey_posMap_addr : 0x" << hex << noteHelpKey_posMap_addr << endl;
            matchedAddrCount++;
        }

        if (SearchModuleMemory(loadNoteHelpKey_posMap_addr_pattern, matchResults) && matchResults.size() == 1) {
            loadNoteHelpKey_posMap_addr = matchResults[0];
            cout << "loadNoteHelpKey_posMap_addr : 0x" << hex << loadNoteHelpKey_posMap_addr << endl;
            matchedAddrCount++;
        }

        if (SearchModuleMemory(WebMPlayerOpen_addr_pattern, matchResults) && matchResults.size() == 1) {
            WebMPlayerOpen_addr = matchResults[0] + 0xa;
            uint32_t* offset_ptr = reinterpret_cast<uint32_t*>(WebMPlayerOpen_addr);
            WebMPlayerOpen_addr += (uint64_t)*offset_ptr + 0x4;
      
            cout << "WebMPlayerOpen_addr : 0x" << hex << WebMPlayerOpen_addr << endl;
            matchedAddrCount++;
        }

        if (SearchModuleMemory(TextWidthScalefactor_addr_pattern, matchResults) && matchResults.size() == 1) {
            TextWidthScalefactor_addr = matchResults[0] + 0x16;
            uint32_t* offset_ptr = reinterpret_cast<uint32_t*>(TextWidthScalefactor_addr);
            TextWidthScalefactor_addr += (uint64_t)*offset_ptr + 0x4;
            cout << "TextWidthScalefactor_addr : 0x" << hex << TextWidthScalefactor_addr << endl;
            matchedAddrCount++;
        }

        if (SearchModuleMemory(DialogBoxHeight_addr_pattern, matchResults) && matchResults.size() == 1) {
            DialogBoxHeight_addr = matchResults[0];
            cout << "DialogBoxHeight_addr : 0x" << hex << DialogBoxHeight_addr << endl;
            matchedAddrCount++;
        }

        cout << "==============================" << endl;

        if (SearchModuleMemory(main_sjis_byte_valid_addr_pattern, matchResults) && matchResults.size() == 5) {
            for (size_t i = 0; i < matchResults.size(); i++)
            {
                main_sjis_byte_valid_addr[i] = matchResults[i];
                cout << "main_sjis_byte_valid_addr " << i << " : 0x" << hex << main_sjis_byte_valid_addr[i] << endl;
            }
            matchedAddrCount += matchResults.size();
        }

        if (SearchModuleMemory(talk_sjis_byte_valid_addr_pattern, matchResults) && matchResults.size() == 4) {
            for (size_t i = 0; i < matchResults.size(); i++)
            {
                talk_sjis_byte_valid_addr[i] = matchResults[i];
                cout << "talk_sjis_byte_valid_addr " << i << " : 0x" << hex << talk_sjis_byte_valid_addr[i] << endl;
            }
            matchedAddrCount += matchResults.size();
        }

        if (SearchModuleMemory(ui_sjis_byte_valid_addr_pattern, matchResults) && matchResults.size() == 3) {
            for (size_t i = 0; i < matchResults.size(); i++)
            {
                ui_sjis_byte_valid_addr[i] = matchResults[i];
                cout << "ui_sjis_byte_valid_addr " << i << " : 0x" << hex << ui_sjis_byte_valid_addr[i] << endl;
            }
            matchedAddrCount += matchResults.size();
        }

        if (SearchModuleMemory(story_sjis_byte_valid_addr_pattern, matchResults) && matchResults.size() == 2) {
            for (size_t i = 0; i < matchResults.size(); i++)
            {
                story_sjis_byte_valid_addr[i] = matchResults[i];
                cout << "story_sjis_byte_valid_addr " << i << " : 0x" << hex << story_sjis_byte_valid_addr[i] << endl;
            }
            matchedAddrCount += matchResults.size();
        }

        if (SearchModuleMemory(menu_sjis_byte_valid_addr_pattern, matchResults) && matchResults.size() == 1) {
            menu_sjis_byte_valid_addr = matchResults[0];
            cout << "menu_sjis_byte_valid_addr : 0x" << hex << menu_sjis_byte_valid_addr << endl;
            matchedAddrCount++;
        }

        if (SearchModuleMemory(escape_sjis_byte_valid_addr_pattern, matchResults) && matchResults.size() == 1) {
            escape_sjis_byte_valid_addr = matchResults[0];
            cout << "escape_sjis_byte_valid_addr : 0x" << hex << escape_sjis_byte_valid_addr << endl;
            matchedAddrCount++;
        }

        if (SearchModuleMemory(add_al_60_r15_sjis_byte_valid_addr_pattern, matchResults) && matchResults.size() == 1) {
            add_al_60_r15_sjis_byte_valid_addr = matchResults[0];
            cout << "add_al_60_r15_sjis_byte_valid_addr : 0x" << hex << add_al_60_r15_sjis_byte_valid_addr << endl;
            matchedAddrCount++;
        }
    }

    void fix_main_sjis_byte_valid(uintptr_t ptr) {
        UnLockProtect(ptr);
        uint8_t* p = reinterpret_cast<uint8_t*>(ptr + 5);
        for (size_t i = 1; i < 0x10; i++)
        {
            if (*(p) == 0x76)
                break;
            p++;
        }
        memset((PVOID)p, 0x90, 2);
        LockProtect(ptr);
    }

    void fix_talk_sjis_byte_valid(uintptr_t ptr) {
        UnLockProtect(ptr);
        uint8_t* p = reinterpret_cast<uint8_t*>(ptr);
        for (size_t i = 1; i < 0x10; i++)
        {
            if (*(p - i) == 0x80)
            {
                *(p - i) = 0;
                break;
            }
        }
        *(p + 1) = 0x80;
        *(p + 2) = 0x73;
        *(p + 7) = 0xEB;
        LockProtect(ptr);
    }

    void fix_menu_sjis_byte_valid(uintptr_t ptr) {
        UnLockProtect(ptr);
        memset((PVOID)(ptr+5), 0x90,2);
        LockProtect(ptr);
    }

    void fix_escape_sjis_byte_valid(uintptr_t ptr) {
        UnLockProtect(ptr);
        uint8_t* p = reinterpret_cast<uint8_t*>(ptr);
        *(p + 7) = 0xeb;
        LockProtect(ptr);
    }

    void fix_story_sjis_byte_valid(uintptr_t ptr) {
        UnLockProtect(ptr);
        int count = 0;
        uint8_t* p = reinterpret_cast<uint8_t*>(ptr);
        for (size_t i = 1; i < 0x20; i++)
        {
            if (*(p + i) == 0xF) {
                count++;
            }
            if (count == 2) {
                memset((PVOID)(p + i), 0x90, 6);
                break;
            }
        }
        LockProtect(ptr);
    }

    void fix_ui_sjis_byte_valid(uintptr_t ptr) {
        UnLockProtect(ptr);
        uint8_t* p = reinterpret_cast<uint8_t*>(ptr);
        *(p - 1) = 0;
        *(p + 1) = 0x7f;
        LockProtect(ptr);
    }

    void fix_add_al_60_r15_sjis_byte_valid(uintptr_t ptr) {
        UnLockProtect(ptr);
        memset((PVOID)(ptr + 4), 0x90, 6);
        LockProtect(ptr);
    }

    void fix_TextWidthScalefactor(uintptr_t ptr) {
        UnLockProtect(ptr);
        float* pScaleFactor = reinterpret_cast<float*>(ptr);
        *pScaleFactor = 0.86f;
        LockProtect(ptr);
    }

    void fix_DialogBoxHeight(uintptr_t ptr) {
        uint8_t* jmpAddr = static_cast<uint8_t*>(BeginPatch(ptr, 0xf, 0));
        WritePatchOriginalData(jmpAddr);
        *(jmpAddr++) = 0x83;
        *(jmpAddr++) = 0xc0;
        *(jmpAddr++) = 0x03;
        //add eax, 03
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
                throw runtime_error("部分地址未匹配成功，停止hook以防止崩溃");
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
            fix_DialogBoxHeight(DialogBoxHeight_addr);

            cout << "[INFO]fix main_sjis_byte_valid" << endl;
            for (size_t i = 0; i < size(main_sjis_byte_valid_addr); i++)
                fix_main_sjis_byte_valid(main_sjis_byte_valid_addr[i]);

            cout << "[INFO]fix talk_sjis_byte_valid" << endl;
            for (size_t i = 0; i < size(talk_sjis_byte_valid_addr); i++)
                fix_talk_sjis_byte_valid(talk_sjis_byte_valid_addr[i]);

            cout << "[INFO]fix story_sjis_byte_valid" << endl;
            for (size_t i = 0; i < size(story_sjis_byte_valid_addr); i++)
                fix_story_sjis_byte_valid(story_sjis_byte_valid_addr[i]);

            cout << "[INFO]fix ui_sjis_byte_valid" << endl;
            for (size_t i = 0; i < size(ui_sjis_byte_valid_addr); i++)
                fix_ui_sjis_byte_valid(ui_sjis_byte_valid_addr[i]);

            cout << "[INFO]fix menu_sjis_byte_valid" << endl;
			fix_menu_sjis_byte_valid(menu_sjis_byte_valid_addr);

			cout << "[INFO]fix escape_sjis_byte_valid" << endl;
			fix_escape_sjis_byte_valid(escape_sjis_byte_valid_addr);

			cout << "[INFO]fix add_al_60_r15_sjis_byte_valid" << endl;
			fix_add_al_60_r15_sjis_byte_valid(add_al_60_r15_sjis_byte_valid_addr);
 
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

    int32_t __fastcall hooked_loadNoteHelpKey_posMap() {
        int32_t result = ori_loadNoteHelpKey_posMap();
        fix_noteHelpKey_pos(noteHelpKey_posMap_addr);
        return result;
    }

    int64_t __fastcall hooked_sjis2utf8(char* output, uint8_t* input, int64_t max_output, int32_t* _pTable) {
    
        try {
            if (*language_option_addr == 0) {
                return ori_sjis2utf8(output, input, max_output, _pTable);
            }
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
            if (*language_option_addr == 0) {
                return ori_utf82sjis(output, input, max_output);
            }
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

    bool load_mess_string_cn() {
        char path[] = "data_cn\\localization\\mess_strings_cn.txt";
        FILE* file = nullptr;

        if (fopen_s(&file, path, "rb") != 0) {
            cout<< "[ERROR]无法载入mess_strings_cn" << endl;
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
        for (int32_t i = 1; i < 0xC08; i++)
        {
            auto key = get_mess_string_key(reinterpret_cast<__int64>(FileName), i);

            if (!FileName[0].empty() && mess_map.count(FileName[0])) {
                write_mess_string((char*)(mess_string_jp_struct_addr + (int64_t)i * 0x20), mess_map[FileName[0]]);
            }
            else {
                //cerr << "[INFO]未找到 mess string : " << FileName[0] << endl;
            }
        }
        memset(pbuffer, 0, buffer.size());
        cout << "[INFO]已载入mess_strings_cn" << endl;
        return true;
    }

    int64_t __fastcall hooked_check_encoding(char* input_str) {
        return encoding::check_encoding(input_str);
    }

    int32_t __fastcall hooked_load_mess_string() {
        int32_t result = ori_load_mess_string();
        load_mess_string_cn();
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

    int64_t __fastcall hooked_sjis2uni(int64_t ctx, int32_t* output_addr, char* input_str, int64_t max_output)
    {
        if (*language_option_addr == 0) {
            return ori_sjis2uni(ctx, output_addr, input_str, max_output);
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
            vector<int32_t> unicodes = encoding::chars_to_unicode(input_str, max_output);
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

        int64_t original_count = ori_sjis2uni(ctx, output_addr, input_str, max_output);
        return original_count;
    }

    static const vector<pair<string, string>> patterns = {
      {"data_pc/", "data_cn/pc/"},
      {"data/", "data_cn/"},
    };

    string redirect_dir(string file) {
        for (size_t i = 0; i < patterns.size(); i++)
        {
            size_t pos = file.find(patterns[i].first);
            if (pos < 5) {
                string cnFile = file;
                cnFile.replace(pos, patterns[i].first.length(), patterns[i].second);
                if (std::filesystem::exists(cnFile))
                {
                    cout << "[INFO]重定向：" << cnFile << endl;
                    return cnFile;
                }
                break;
            }
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