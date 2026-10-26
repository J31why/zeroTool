
#include "encoding.h"


namespace encoding {
	iconv_t hIconv = nullptr;

	void iconv_initialize() {
		hIconv = iconv_open("UTF-16LE", "CP936");
		if (hIconv == (iconv_t)-1) {
			printf("iconv_open error");
			throw EncodingError("iconv_open error");
		}
	}
	void iconv_close() {
		::iconv_close(hIconv);
	}
	size_t get_input_length(const char* input) {
		if (!input) {
			throw EncodingError("input string is NULL");
		}
		size_t max_len = 1024 * 1024;
		size_t len = 0;
		while (len < max_len && input[len] != 0) {
			len++;
		}
		if (len >= max_len) {
			throw EncodingError("input string too long");
		}
		return len;;
	}


	std::vector<char> textPtr_to_unicode(const char* src_ptr, size_t src_len, size_t* out_len, int64_t* usedByteLen) {

		size_t unicode_len = src_len * 2 + 2;
		std::vector<char> unicode_buffer(unicode_len);
		char* out_ptr = unicode_buffer.data();
		size_t out_left = unicode_len;
		char* in_ptr = const_cast<char*>(src_ptr);
		size_t in_left = src_len;

		// convert
		size_t ret = iconv(hIconv, &in_ptr, &in_left, &out_ptr, &out_left);
		if (ret == (size_t)-1) {
			throw EncodingError("error: iconv convert failed.");
		}
		*out_len = unicode_len - out_left;
		if (usedByteLen)
			*usedByteLen = src_len - in_left - 1;

		return unicode_buffer;
	}

	std::vector<int32_t> chars_to_unicode(const char* src_ptr, size_t src_len, int64_t* usedByteLen) {

		size_t out_len = 0;
		auto unicode_buffer = textPtr_to_unicode(src_ptr, src_len, &out_len, usedByteLen);
		const int16_t* unicode_ptr = reinterpret_cast<int16_t*>(unicode_buffer.data());

		std::vector<int32_t> result;
		result.reserve(out_len);
		for (size_t i = 0; i < out_len; i++)
		{
			int32_t code = static_cast<uint16_t>(unicode_ptr[i]);
			if (code == 0)
			{
				result.push_back(-1);
				break;
			}
			result.push_back(code);
		}
		return result;
	}


	bool is_printable_ascii(uint8_t byte) {
		return byte >= 0x20 && byte <= 0x7E;
	}

	size_t check_utf8_sequence(const char* ptr, size_t remaining_len) {
		if (ptr == nullptr || remaining_len == 0) return 0;

		uint8_t first = *ptr;
		// UTF-8 2字节序列：110xxxxx 10xxxxxx
		//if ((first & 0xE0) == 0xC0) {
		//    if (remaining_len < 2) return 0;
		//    uint8_t second = ptr[1];
		//    return (second & 0xC0) == 0x80 ? 2 : 0;
		//}
		// UTF-8 3字节序列：1110xxxx 10xxxxxx 10xxxxxx
		if ((first & 0xF0) == 0xE0) {
			if (remaining_len < 3) return 0;
			uint8_t second = ptr[1], third = ptr[2];
			return ((second & 0xC0) == 0x80 && (third & 0xC0) == 0x80) ? 3 : 0;
		}
		// UTF-8 4字节序列：11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
		else if ((first & 0xF8) == 0xF0) {
			if (remaining_len < 4) return 0;
			uint8_t second = ptr[1], third = ptr[2], fourth = ptr[3];
			return ((second & 0xC0) == 0x80 && (third & 0xC0) == 0x80 && (fourth & 0xC0) == 0x80) ? 4 : 0;
		}
		return 0;
	}

	/**
	 * 辅助函数：校验 SJIS 多字节序列（从 ptr 开始）
	 * 返回值：合法序列长度（0=非法，2=2字节）
	 */
	size_t check_sjis_sequence(const char* ptr, size_t remaining_len) {
		if (ptr == nullptr || remaining_len < 2) return 0;

		uint8_t first = *ptr, second = ptr[1];
		// SJIS 规则：首字节 0x81~0x9F 或 0xE0~0xFC；第二字节 0x40~0xFC（排除0x7F）
		bool valid_first = (first >= 0x81 && first <= 0x9F) || (first >= 0xE0 && first <= 0xFC);
		bool valid_second = (second >= 0x40 && second <= 0xFC) && (second != 0x7F);
		return (valid_first && valid_second) ? 2 : 0;
	}

	/**
	 * 辅助函数：校验 GBK 多字节序列（从 ptr 开始）
	 * 返回值：合法序列长度（0=非法，2=2字节）
	 */
	size_t check_gbk_sequence(const char* ptr, size_t remaining_len) {
		if (ptr == nullptr || remaining_len < 2) return 0;

		uint8_t first = *ptr, second = ptr[1];
		// GBK 规则：
		// 首字节：0x81~0xFE（兼容 GB2312 的 0xA1~0xF7，扩展到 0x81~0xFE）
		// 第二字节：0x40~0x7E 或 0x80~0xFE（两段范围，无 0x7F 限制）
		bool valid_first = first >= 0x81 && first <= 0xFE;
		bool valid_second = (second >= 0x40 && second <= 0x7E) || (second >= 0x80 && second <= 0xFE);
		return (valid_first && valid_second) ? 2 : 0;
	}

	int check_utf8_strict(const uint8_t* ptr, size_t remaining) {
		uint8_t first = *ptr;

		// 1字节 (0xxxxxxx) - ASCII
		//if (first < 0x80) return 1;
		//
		//// 2字节 (110xxxxx 10xxxxxx)
		//if ((first & 0xE0) == 0xC0 && first >= 0xC2) {
		//    if (remaining < 2) return -1;
		//    if ((ptr[1] & 0xC0) != 0x80) return -1;
		//    return 2;
		//}

		// 3字节 (1110xxxx 10xxxxxx 10xxxxxx)
		if ((first & 0xF0) == 0xE0) {
			if (remaining < 3) return -1;
			if ((ptr[1] & 0xC0) != 0x80 || (ptr[2] & 0xC0) != 0x80) return -1;
			return 3;
		}

		// 4字节 (11110xxx 10xxxxxx 10xxxxxx 10xxxxxx)
		if ((first & 0xF8) == 0xF0 && first <= 0xF4) {
			if (remaining < 4) return -1;
			if ((ptr[1] & 0xC0) != 0x80 || (ptr[2] & 0xC0) != 0x80 || (ptr[3] & 0xC0) != 0x80) return -1;
			return 4;
		}

		return -1; // 违反 UTF-8 规则
	}

	int64_t check_encoding(const char* data) {
		if (!data || *data == 0) return (int64_t)EncodingType::EMPTY;

		const uint8_t* ptr = reinterpret_cast<const uint8_t*>(data);
		bool has_multi_byte = false;
		bool maybe_utf8 = true;
		size_t utf8_sequences = 0;

		// 计算长度（防止越界）
		size_t len = 0;
		while (data[len] != 0 && len < 1024 * 1024) len++;

		size_t i = 0;
		while (i < len) {
			if (ptr[i] < 0x80) { // ASCII
				i++;
				continue;
			}

			has_multi_byte = true;
			int res = check_utf8_strict(ptr + i, len - i);

			if (res > 1) {
				utf8_sequences++;
				i += res;
			}
			else {
				// 只要遇到一个不符合 UTF-8 规范的高位字节，就判定不是 UTF-8
				maybe_utf8 = false;
				break;
			}
		}

		if (!has_multi_byte) return (int64_t)EncodingType::ASCII;

		// 如果符合 UTF-8 结构，且有实际的多字节序列
		if (maybe_utf8 && utf8_sequences > 0) {
			return (int64_t)EncodingType::UTF8;
		}

		// 默认回退到 GBK/SJIS
		return (int64_t)EncodingType::SJIS_GBK;
	}
}
