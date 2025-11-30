#pragma once

#include "MinHook.h"
#include "encoding.h"
#include <cstdio>
#include <sstream>
#include <unordered_map>
#include "memo.h"


namespace hook {
	typedef int64_t(__fastcall* sjis2uni_t)(int64_t ctx, int32_t* output, char* input, int64_t max_output);
	typedef int64_t(__fastcall* checkEncoding_t) (char* input_str);
	typedef int64_t(__fastcall* get_mess_string_key_t) (int64_t output_str, int32_t index);
	typedef int32_t(__fastcall* load_mess_string_t) ();
	typedef int64_t(__fastcall* sjis2utf8_t)(char* output, uint8_t* input, int64_t max_output, int32_t* _pTable);
	typedef int64_t(__fastcall* utf82sjis_t)(char* output, uint8_t* input, int64_t max_output);

	void hook_install();
	void hook_uninstall();
	int64_t __fastcall hooked_sjis2uni(int64_t ctx, int32_t* output, char* input, int64_t max_output);
	int64_t __fastcall hooked_check_encoding(char* input_str);
	int32_t __fastcall hooked_load_mess_string();
	int64_t __fastcall hooked_sjis2utf8(char* output, uint8_t* input, int64_t max_output, int32_t* _pTable);
	int64_t __fastcall hooked_utf82sjis(char* output, uint8_t* input, int64_t max_output);
	bool load_mess_string_cn();
	unordered_map<string, string> build_mess_string_map(const char* data, const size_t len);
	void search_all_addresses();

}

