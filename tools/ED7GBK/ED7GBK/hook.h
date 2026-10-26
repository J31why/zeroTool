
#pragma once

#include "MinHook.h"
#include "encoding.h"
#include <cstdio>
#include <sstream>
#include <unordered_map>
#include <filesystem>
#include <string>
#include "memo.h"
#include <cstdint> 

using std::string;
using std::cout;
using std::dec;
using std::hex;
using std::endl;
using std::cerr;
using std::vector;
using std::runtime_error;
using std::size;
using std::uintptr_t;

namespace hook {

	typedef int64_t	(__fastcall*	sjis2uni_t)					(int64_t ctx, int32_t* output, char* input, int64_t max_output, int64_t* usedLen);
	typedef int64_t	(__fastcall*	checkEncoding_t)			(char* input_str);
	typedef int64_t	(__fastcall*	get_mess_string_key_t)		(int64_t output_str, int32_t index);
	typedef int64_t	(__fastcall*	load_mess_string_t)			(int64_t pStruct);
	typedef int64_t	(__fastcall*	sjis2utf8_t)				(char* output, uint8_t* input, int64_t max_output, int32_t* _pTable, int64_t* usedLen);
	typedef int64_t	(__fastcall*	utf82sjis_t)				(char* output, uint8_t* input, int64_t max_output, int64_t* usedLen);
	typedef int64_t	(__fastcall*	loadNoteHelpKey_posMap_t)	(uintptr_t ctx);
	typedef bool	(__fastcall*	WebMPlayerOpen_t)			(int64_t h, char* file);
	typedef HANDLE	(WINAPI*		CreateFileA_t)				(LPCSTR, DWORD, DWORD, LPSECURITY_ATTRIBUTES, DWORD, DWORD, HANDLE);
	typedef HANDLE	(WINAPI*		CreateWindowExA_t)			(DWORD, LPCSTR, LPCSTR, DWORD, int, int, int, int, HWND, HMENU, HINSTANCE, LPVOID);

	int64_t __fastcall	hooked_sjis2uni(int64_t ctx, int32_t* output, char* input, int64_t max_output, int64_t* len);
	int64_t __fastcall	hooked_check_encoding(char* input_str);
	int64_t __fastcall	hooked_load_mess_string(int64_t pStruct);
	int64_t __fastcall	hooked_sjis2utf8(char* output, uint8_t* input, int64_t max_output, int32_t* _pTable, int64_t* usedLen);
	int64_t __fastcall	hooked_utf82sjis(char* output, uint8_t* input, int64_t max_output, int64_t* usedLen);
	bool	__fastcall	hooked_WebMPlayerOpen(int64_t h, char* file);
	int64_t __fastcall	hooked_loadNoteHelpKey_posMap(uintptr_t ctx);
	HANDLE	WINAPI		hooked_CreateFileA(LPCSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile);
	HANDLE	WINAPI		hooked_CreateWindowExA(DWORD dwExStyle, LPCSTR lpClassName, LPCSTR lpWindowName, DWORD dwStyle,int X,int Y,int nWidth, int nHeight, HWND hWndParent, HMENU hMenu, HINSTANCE hInstance, LPVOID lpParam);

	bool			hook_install();
	void			hook_uninstall();
	string		redirect_dir(string file);
	bool			load_mess_string_cn(int64_t jpStructAddr);
	void			search_all_addresses();
	std::unordered_map<string, string>
					build_mess_string_map(const char* data, const size_t len);
	void			fix_noteHelpKey_pos(uintptr_t ctx);

	extern CreateFileA_t            ori_CreateFileA;
	extern CreateWindowExA_t        ori_CreateWindowExA;
	extern sjis2uni_t               ori_sjis2uni;
	extern checkEncoding_t          ori_check_encoding;
	extern load_mess_string_t       ori_load_mess_string;
	extern get_mess_string_key_t    get_mess_string_key;
	extern sjis2utf8_t              ori_sjis2utf8;
	extern utf82sjis_t              ori_utf82sjis;
	extern loadNoteHelpKey_posMap_t ori_loadNoteHelpKey_posMap;
	extern WebMPlayerOpen_t         ori_WebMPlayerOpen;

	extern int  matchedAddrCount;
	extern int  totalAddrCount;
	extern bool isMatchSuccessful;
	extern bool is_debug;

	extern uintptr_t	mess_string_jp_struct_offset;
	extern uintptr_t	get_mess_string_key_addr;

	extern int32_t*			language_option_addr;
	extern int				mess_string_max_index;
}

