#pragma once
#include "iconv.h"


class EncodingError : public std::runtime_error {
public:
	using std::runtime_error::runtime_error;
};

namespace encoding {

	enum class EncodingType {
		EMPTY = 0,    // ø’ ‰»Î
		ASCII = 1,    // ASCII ±‡¬Î
		SJIS_GBK = 2, // Shift-JIS ªÚ GBK ±‡¬Î
		UTF8 = 3      // UTF-8 ±‡¬Î
	};

	extern iconv_t hIconv ;

	void iconv_initialize();
	void iconv_close();
	std::vector<char> textPtr_to_unicode(const char* src_ptr, size_t src_len, size_t* out_len);
	std::vector<int32_t> chars_to_unicode(const char* src_ptr, size_t src_len);
	bool is_printable_ascii(uint8_t byte);
	size_t check_utf8_sequence(const char* ptr, size_t remaining_len);
	size_t check_sjis_sequence(const char* ptr, size_t remaining_len);
	size_t check_gbk_sequence(const char* ptr, size_t remaining_len);
	int64_t check_encoding(const char* data);
	size_t get_input_length(const char* input);
}

