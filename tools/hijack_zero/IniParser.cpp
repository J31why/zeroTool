#include "IniParser.h"
#include <fstream>
#include <sstream>
#include <algorithm>
#include <cctype>
#include <stdexcept>

// 辅助函数：将字符串转换为小写
static std::string toLower(const std::string& str) {
    std::string result = str;
    std::transform(result.begin(), result.end(), result.begin(), ::tolower);
    return result;
}

// 辅助函数：检查字符串是否以指定前缀开头
static bool startsWith(const std::string& str, const std::string& prefix) {
    return str.size() >= prefix.size() &&
        str.compare(0, prefix.size(), prefix) == 0;
}

// 辅助函数：检查字符串是否以指定后缀结尾
static bool endsWith(const std::string& str, const std::string& suffix) {
    return str.size() >= suffix.size() &&
        str.compare(str.size() - suffix.size(), suffix.size(), suffix) == 0;
}

std::string IniParser::trim(const std::string& str) const {
    size_t first = str.find_first_not_of(" \t\r\n");
    if (first == std::string::npos) return "";
    size_t last = str.find_last_not_of(" \t\r\n");
    return str.substr(first, last - first + 1);
}

bool IniParser::parseKeyValue(const std::string& line, std::string& key, std::string& value) const {
    size_t pos = line.find('=');
    if (pos == std::string::npos) return false;

    key = trim(line.substr(0, pos));
    value = trim(line.substr(pos + 1));

    // 键不能为空
    if (key.empty()) return false;

    // 移除值的引号
    value = removeQuotes(value);

    return true;
}

bool IniParser::parseSection(const std::string& line, std::string& section) const {
    if (line.empty() || line[0] != '[' || !endsWith(line, "]")) return false;

    section = trim(line.substr(1, line.length() - 2));
    return !section.empty();
}

std::string IniParser::removeQuotes(const std::string& value) const {
    std::string result = value;

    // 移除开头和结尾的引号
    if (result.size() >= 2) {
        if ((result.front() == '"' && result.back() == '"') ||
            (result.front() == '\'' && result.back() == '\'')) {
            result = result.substr(1, result.size() - 2);
        }
    }

    return result;
}

bool IniParser::load(const std::string& filePath) {
    std::ifstream file(filePath);
    if (!file.is_open()) {
        m_lastError = "无法打开文件: " + filePath;
        return false;
    }

    std::stringstream buffer;
    buffer << file.rdbuf();
    file.close();

    return loadFromString(buffer.str());
}

bool IniParser::loadFromString(const std::string& content) {
    clear();

    std::istringstream stream(content);
    std::string line;
    std::string currentSection;
    int lineNumber = 0;

    while (std::getline(stream, line)) {
        lineNumber++;

        // 去除首尾空白
        line = trim(line);

        // 跳过空行和注释行（; 或 # 开头的行）
        if (line.empty() || line[0] == ';' || line[0] == '#') {
            continue;
        }

        // 尝试解析节
        std::string section;
        if (parseSection(line, section)) {
            currentSection = section;
            continue;
        }

        // 如果没有当前节，则跳过该键值对
        if (currentSection.empty()) {
            m_lastError = "第 " + std::to_string(lineNumber) + " 行：键值对不在任何节中";
            return false;
        }

        // 解析键值对
        std::string key, value;
        if (parseKeyValue(line, key, value)) {
            m_data[currentSection][key] = value;
        }
        else {
            m_lastError = "第 " + std::to_string(lineNumber) + " 行：格式错误";
            return false;
        }
    }

    return true;
}

std::vector<std::string> IniParser::getSections() const {
    std::vector<std::string> sections;
    for (const auto& pair : m_data) {
        sections.push_back(pair.first);
    }
    return sections;
}

std::vector<std::string> IniParser::getKeys(const std::string& section) const {
    std::vector<std::string> keys;
    auto it = m_data.find(section);
    if (it != m_data.end()) {
        for (const auto& pair : it->second) {
            keys.push_back(pair.first);
        }
    }
    return keys;
}

bool IniParser::hasSection(const std::string& section) const {
    return m_data.find(section) != m_data.end();
}

bool IniParser::hasKey(const std::string& section, const std::string& key) const {
    auto secIt = m_data.find(section);
    if (secIt == m_data.end()) return false;
    return secIt->second.find(key) != secIt->second.end();
}

std::string IniParser::getString(const std::string& section,
    const std::string& key,
    const std::string& defaultValue) const {
    auto secIt = m_data.find(section);
    if (secIt != m_data.end()) {
        auto keyIt = secIt->second.find(key);
        if (keyIt != secIt->second.end()) {
            return keyIt->second;
        }
    }
    return defaultValue;
}

int IniParser::getInt(const std::string& section,
    const std::string& key,
    int defaultValue) const {
    std::string value = getString(section, key, "");
    if (value.empty()) return defaultValue;

    try {
        // 处理十六进制
        if (value.size() > 2 && value[0] == '0' && (value[1] == 'x' || value[1] == 'X')) {
            return static_cast<int>(std::stoul(value, nullptr, 16));
        }
        return std::stoi(value);
    }
    catch (const std::exception&) {
        return defaultValue;
    }
}

long IniParser::getLong(const std::string& section,
    const std::string& key,
    long defaultValue) const {
    std::string value = getString(section, key, "");
    if (value.empty()) return defaultValue;

    try {
        if (value.size() > 2 && value[0] == '0' && (value[1] == 'x' || value[1] == 'X')) {
            return std::stoul(value, nullptr, 16);
        }
        return std::stol(value);
    }
    catch (const std::exception&) {
        return defaultValue;
    }
}

float IniParser::getFloat(const std::string& section,
    const std::string& key,
    float defaultValue) const {
    std::string value = getString(section, key, "");
    if (value.empty()) return defaultValue;

    try {
        return std::stof(value);
    }
    catch (const std::exception&) {
        return defaultValue;
    }
}

double IniParser::getDouble(const std::string& section,
    const std::string& key,
    double defaultValue) const {
    std::string value = getString(section, key, "");
    if (value.empty()) return defaultValue;

    try {
        return std::stod(value);
    }
    catch (const std::exception&) {
        return defaultValue;
    }
}

bool IniParser::getBool(const std::string& section,
    const std::string& key,
    bool defaultValue) const {
    std::string value = getString(section, key, "");
    if (value.empty()) return defaultValue;

    std::string lowerValue = toLower(value);

    // 常见的真值表示
    if (lowerValue == "true" || lowerValue == "1" ||
        lowerValue == "yes" || lowerValue == "on" ||
        lowerValue == "enable" || lowerValue == "enabled") {
        return true;
    }

    // 常见的假值表示
    if (lowerValue == "false" || lowerValue == "0" ||
        lowerValue == "no" || lowerValue == "off" ||
        lowerValue == "disable" || lowerValue == "disabled") {
        return false;
    }

    return defaultValue;
}

void IniParser::setString(const std::string& section,
    const std::string& key,
    const std::string& value) {
    m_data[section][key] = value;
}

void IniParser::setInt(const std::string& section,
    const std::string& key,
    int value) {
    m_data[section][key] = std::to_string(value);
}

void IniParser::setBool(const std::string& section,
    const std::string& key,
    bool value) {
    m_data[section][key] = value ? "true" : "false";
}

bool IniParser::removeKey(const std::string& section, const std::string& key) {
    auto secIt = m_data.find(section);
    if (secIt == m_data.end()) return false;

    auto keyIt = secIt->second.find(key);
    if (keyIt == secIt->second.end()) return false;

    secIt->second.erase(keyIt);

    // 如果节为空，可以选择删除该节
    if (secIt->second.empty()) {
        m_data.erase(secIt);
    }

    return true;
}

bool IniParser::removeSection(const std::string& section) {
    auto it = m_data.find(section);
    if (it == m_data.end()) return false;

    m_data.erase(it);
    return true;
}

void IniParser::clear() {
    m_data.clear();
    m_lastError.clear();
}

bool IniParser::save(const std::string& filePath, bool keepComments) const {
    std::ofstream file(filePath);
    if (!file.is_open()) {
        m_lastError = "无法创建文件: " + filePath;
        return false;
    }

    file << toString();

    if (file.fail()) {
        m_lastError = "写入文件失败";
        return false;
    }

    return true;
}

std::string IniParser::toString() const {
    std::ostringstream result;

    for (const auto& sectionPair : m_data) {
        // 写入节名称
        result << "[" << sectionPair.first << "]\n";

        // 写入该节的所有键值对
        for (const auto& keyValuePair : sectionPair.second) {
            // 如果值包含特殊字符或空格，用引号括起来
            std::string value = keyValuePair.second;
            if (value.find_first_of(" \t;#=") != std::string::npos) {
                value = "\"" + value + "\"";
            }
            result << keyValuePair.first << " = " << value << "\n";
        }

        result << "\n";  // 节之间空一行
    }

    return result.str();
}