#include "pch.h"
#include "encoding.h"

namespace encoding{
    iconv_t hIconv=nullptr;

    void iconv_initialize() {
        hIconv = iconv_open("UTF-16LE", "CP936");
        if (hIconv == (iconv_t)-1) {
            cout << "iconv_open error" << endl;
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


    std::vector<char> textPtr_to_unicode(const char* src_ptr, size_t src_len, size_t* out_len) {

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
        return unicode_buffer;
    }

    std::vector<int32_t> chars_to_unicode(const char* src_ptr, size_t src_len) {

        size_t out_len = 0;
        auto unicode_buffer = textPtr_to_unicode(src_ptr, src_len, &out_len);
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


    inline static bool is_printable_ascii(uint8_t byte) {
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

    int64_t check_encoding(const char* data) {
        if (data == nullptr || *data == 0) {
            return static_cast<int64_t>(EncodingType::EMPTY);
        }
        if (data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF) {
            return static_cast<int64_t>(EncodingType::EMPTY);
        }
        size_t count_ascii = 0;       // 可打印 ASCII 字符数
        size_t count_utf8 = 0;        // 合法 UTF-8 多字节序列数
        size_t count_sjis = 0;        // 合法 SJIS 多字节序列数
        size_t count_gbk = 0;         // 合法 GBK 多字节序列数
        size_t count_utf8_special = 0;// 特定 UTF-8 序列（0xE3 开头，日文相关）
        const char* ptr = data;

        while (*ptr != 0) {
            char current = *ptr;
            if (is_printable_ascii(current)) {
                count_ascii++;
                ptr++;
                continue;
            }
            // utf8
            size_t utf8_len = check_utf8_sequence(ptr, std::distance(ptr, std::find(ptr, ptr + 4, 0)));
            if (utf8_len > 0) {
                count_utf8++;
                // 统计特定 UTF-8 序列（0xE3 开头的 3 字节序列，日文平假名范围）
                if (utf8_len == 3 && current == 0xE3) {
                    uint8_t second = ptr[1];
                    if (second >= 0x80 && second <= 0x82) {
                        count_utf8_special++;
                    }
                }
                ptr += utf8_len;
                continue;
            }
            // gbk
            size_t gbk_len = check_gbk_sequence(ptr, std::distance(ptr, std::find(ptr, ptr + 2, 0)));
            if (gbk_len > 0) {
                count_gbk++;
                ptr += gbk_len;
                continue;
            }
			// sjis
            size_t sjis_len = check_sjis_sequence(ptr, std::distance(ptr, std::find(ptr, ptr + 2, 0)));
            if (sjis_len > 0) {
                count_sjis++;
                ptr += sjis_len;
                continue;
            }

            ptr++;
        }

        if (count_utf8 > 0 || count_utf8_special > 0) {
            return static_cast<int64_t>(EncodingType::UTF8);
        }
        else if (count_sjis > 0 || count_gbk > 0) {
            return static_cast<int64_t>(EncodingType::SJIS_GBK);
        }
        else if (count_ascii > 0){
            return static_cast<int64_t>(EncodingType::ASCII);
        }
        return static_cast<int64_t>(EncodingType::EMPTY);
    }
}
