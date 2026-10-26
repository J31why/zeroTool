#include "hook.h"

using namespace std;

namespace hook {


	void hook_uninstall() {
		MH_DisableHook(MH_ALL_HOOKS);
		MH_Uninitialize();
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
			int32_t utf8_len = static_cast<int32_t>(out_buffer.size() - out_len);
			utf8_len = min(utf8_len, static_cast<int32_t>(max_output - 1));
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
			int32_t gbk_len = static_cast<int32_t>(out_buffer.size() - out_len);
			gbk_len = min(gbk_len, static_cast<int32_t>(max_output - 1));
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

	static void write_mess_string(const char* ptr, const string& text) {
		const size_t len = text.size();
		const size_t capacity = len >= 16 ? len | 0xf : 0xf;

		char* mutable_ptr = const_cast<char*>(ptr);
		intptr_t* ptr_as_int = reinterpret_cast<intptr_t*>(mutable_ptr);

		if (len >= 16) {
			char* heap_mem = static_cast<char*>(malloc(capacity + 1));
			if (heap_mem == nullptr) {
				throw runtime_error("Memory allocation failed for mess string.");
			}
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

	static string trim_mess_string(string& str) {
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
		for (int32_t i = 1; i <= mess_string_max_index; i++)
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
		printf("[INFO]hooked_load_mess_string pStruct: 0x%llx\n", pStruct + mess_string_jp_struct_offset);
		load_mess_string_cn(pStruct + mess_string_jp_struct_offset + 0x8);
		return result;
	}

	static int32_t cp_mapping(int32_t cp) {
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
	  { "data_pc\\", "data_cn\\pc\\" },
	  { "data\\", "data_cn\\" },
	};

	string redirect_dir(string file) {
		string searchFile = file;
		std::replace(searchFile.begin(), searchFile.end(), '/', '\\');
		bool showInfo = is_debug;
		if (showInfo) {
			cout << "[DEBUG][CreateFileA]: " << file;
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
#if defined(ZERO)
		if (name.find("The Legend of Heroes: Trails from Zero") != string::npos) {
			lpWindowName = "英雄传说 零之轨迹：改";
		}
#elif defined(AO)
		if (name.find("The Legend of Heroes: Trails to Azure") != string::npos) {
			lpWindowName = "英雄传说 碧之轨迹：改";
		}
#endif
		return ori_CreateWindowExA(dwExStyle, lpClassName, lpWindowName,
			dwStyle, X, Y, nWidth, nHeight, hWndParent, hMenu,
			hInstance, lpParam);
	};
}