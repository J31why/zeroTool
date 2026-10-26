#include "hook.h"

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

	string main_sjis_byte_valid_addr_pattern = "80 F9 A0 72 ?? 33 C0";
	string log_sjis_byte_valid_addr_pattern = "80 F9 80 ?? ?? ?? ?? ?? ?? 8d 41 60";

	uintptr_t sjis2uni_addr = 0x140136eb0;
	uintptr_t check_encoding_addr = 0x140136580;
	uintptr_t load_mess_string_addr = 0x1401d44c0;
	uintptr_t get_mess_string_key_addr = 0x140218180;
	uintptr_t mess_string_jp_struct_offset = 0x183e0;
	uintptr_t sjis2utf8_addr = 0x1401373d0;
	uintptr_t utf82sjis_addr = 0x140138070;

	int32_t* language_option_addr = reinterpret_cast<int32_t*>(0x140807A24);

	uintptr_t loadNoteHelpKey_posMap_addr = 0x1401c57e0;
	uintptr_t WebMPlayerOpen_addr = 0x1408a7c08;
	uintptr_t TextWidthScalefactor_addr = 0x1406ec178;

	uintptr_t DialogBoxHeight_addr[] =
	{
		0x1403bf601,    // dialog1
		0x1403bffd5     // dialog2
	};

	uintptr_t main_sjis_byte_valid_addr[] =
	{
		0x1402a7605,
		0x1402ad3f5,
	};
	uintptr_t log_sjis_byte_valid_addr = 0x14023c8fa;

	// ==============
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

	int matchedAddrCount = 0;
	int totalAddrCount = 16;
	bool isMatchSuccessful = false;
	bool is_debug = false;
	int mess_string_max_index = 0xC1D;


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
			mess_string_jp_struct_offset = matchResults[0] + 0x5;
			uint32_t* offset_ptr = reinterpret_cast<uint32_t*>(mess_string_jp_struct_offset);
			mess_string_jp_struct_offset = (uint64_t)*offset_ptr;
			cout << "mess_string_jp_struct_offset : 0x" << hex << mess_string_jp_struct_offset << endl;
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
			matchedAddrCount += static_cast<int32_t>(matchResults.size());
		}

		cout << "==============================" << endl;

		if (SearchModuleMemory(main_sjis_byte_valid_addr_pattern, matchResults) && matchResults.size() == 2) {
			for (int i = 0; i < matchResults.size(); i++) {
				main_sjis_byte_valid_addr[i] = matchResults[i];
				cout << "main_sjis_byte_valid_addr " << i << ": 0x" << hex << main_sjis_byte_valid_addr[i] << endl;
			}
			matchedAddrCount += static_cast<int32_t>(matchResults.size());
		}
		if (SearchModuleMemory(log_sjis_byte_valid_addr_pattern, matchResults) && matchResults.size() == 1) {
			log_sjis_byte_valid_addr = matchResults[0] + 9;
			cout << "log_sjis_byte_valid_addr : 0x" << hex << log_sjis_byte_valid_addr << endl;
			matchedAddrCount++;
		}
	}

	static void fix_main_sjis_byte_valid(uintptr_t ptr) {
		UnLockProtect(ptr);
		uint8_t* p = reinterpret_cast<uint8_t*>(ptr + 2);
		*p = 0xff;
		LockProtect(ptr);
	}

	static void fix_log_sjis_byte_valid(uintptr_t ptr) {
		UnLockProtect(ptr);
		uint8_t* p = reinterpret_cast<uint8_t*>(ptr);
		memset(p, 0x90, 11);
		LockProtect(ptr);
	}

	static void fix_TextWidthScalefactor(uintptr_t ptr) {
		UnLockProtect(ptr);
		float* pScaleFactor = reinterpret_cast<float*>(ptr);
		*pScaleFactor = 0.88f;
		LockProtect(ptr);
	}

	static void fix_DialogBoxHeight(uintptr_t ptr) {
		uint8_t* jmpAddr = static_cast<uint8_t*>(BeginPatch(ptr, 19, 0));
		WritePatchOriginalData(jmpAddr);
		//add eax, 03
		*(jmpAddr++) = 0x83;
		*(jmpAddr++) = 0xc0;
		*(jmpAddr++) = 0x03;
		EndPatch(jmpAddr);
	}

	bool hook_install() {
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
			for (size_t i = 0; i < size(DialogBoxHeight_addr); i++)
				fix_DialogBoxHeight(DialogBoxHeight_addr[i]);

			cout << "[INFO]fix main_sjis_byte_valid" << endl;
			for (size_t i = 0; i < size(main_sjis_byte_valid_addr); i++)
				fix_main_sjis_byte_valid(main_sjis_byte_valid_addr[i]);

			cout << "[INFO]fix log_sjis_byte_valid_addr" << endl;
			fix_log_sjis_byte_valid(log_sjis_byte_valid_addr);

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
			return false;
		}
		return true;
	}


	void fix_noteHelpKey_pos(uintptr_t ptr) {
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


}