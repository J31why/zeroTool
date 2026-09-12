#pragma once
#include "iconv.h"
#include <iostream>
#include <vector>
#include <string>
#include <stdexcept>

class EncodingError : public std::runtime_error {
public:
	using std::runtime_error::runtime_error;
};

namespace encoding {

	enum class EncodingType {
		EMPTY = 0,    // 空输入
		ASCII = 1,    // ASCII 编码
		SJIS_GBK = 2, // Shift-JIS 或 GBK 编码
		UTF8 = 3      // UTF-8 编码
	};

	extern iconv_t hIconv;

	void iconv_initialize();
	void iconv_close();
	std::vector<char> textPtr_to_unicode(const char* src_ptr, size_t src_len, size_t* out_len, int64_t* usedByteLen);
	std::vector<int32_t> chars_to_unicode(const char* src_ptr, size_t src_len, int64_t* usedByteLen);
	bool is_printable_ascii(uint8_t byte);
	size_t check_utf8_sequence(const char* ptr, size_t remaining_len);
	size_t check_sjis_sequence(const char* ptr, size_t remaining_len);
	size_t check_gbk_sequence(const char* ptr, size_t remaining_len);
	int check_utf8_strict(const uint8_t* ptr, size_t remaining);
	int64_t check_encoding(const char* data);
	size_t get_input_length(const char* input);
}

