#pragma once

#ifndef INI_PARSER_H
#define INI_PARSER_H

#include <string>
#include <map>
#include <vector>

/**
 * @brief INI 文件解析器类
 *
 * 支持读取和解析标准的 INI 配置文件格式
 * 支持节（section）、键值对、注释和引号字符串
 */
class IniParser {
private:
    // 数据结构：section -> key -> value
    std::map<std::string, std::map<std::string, std::string>> m_data;

    /**
     * @brief 去除字符串首尾的空白字符
     * @param str 输入字符串
     * @return 处理后的字符串
     */
    std::string trim(const std::string& str) const;

    /**
     * @brief 解析键值对行
     * @param line 原始行内容
     * @param key 输出解析后的键
     * @param value 输出解析后的值
     * @return 是否成功解析
     */
    bool parseKeyValue(const std::string& line, std::string& key, std::string& value) const;

    /**
     * @brief 解析节名称
     * @param line 原始行内容
     * @param section 输出解析后的节名称
     * @return 是否成功解析
     */
    bool parseSection(const std::string& line, std::string& section) const;

    /**
     * @brief 移除值中的引号
     * @param value 原始值字符串
     * @return 移除引号后的字符串
     */
    std::string removeQuotes(const std::string& value) const;

public:
    /**
     * @brief 构造函数
     */
    IniParser() = default;

    /**
     * @brief 析构函数
     */
    ~IniParser() = default;

    /**
     * @brief 加载 INI 文件
     * @param filePath INI 文件路径
     * @return 是否成功加载
     */
    bool load(const std::string& filePath);

    /**
     * @brief 从字符串加载 INI 内容
     * @param content INI 格式的字符串内容
     * @return 是否成功加载
     */
    bool loadFromString(const std::string& content);

    /**
     * @brief 获取所有节的名称列表
     * @return 节名称的向量
     */
    std::vector<std::string> getSections() const;

    /**
     * @brief 获取指定节中的所有键名
     * @param section 节名称
     * @return 键名的向量
     */
    std::vector<std::string> getKeys(const std::string& section) const;

    /**
     * @brief 检查是否存在指定节
     * @param section 节名称
     * @return 是否存在
     */
    bool hasSection(const std::string& section) const;

    /**
     * @brief 检查指定节中是否存在指定键
     * @param section 节名称
     * @param key 键名
     * @return 是否存在
     */
    bool hasKey(const std::string& section, const std::string& key) const;

    /**
     * @brief 获取字符串值
     * @param section 节名称
     * @param key 键名
     * @param defaultValue 默认值
     * @return 字符串值
     */
    std::string getString(const std::string& section,
        const std::string& key,
        const std::string& defaultValue = "") const;

    /**
     * @brief 获取整数值
     * @param section 节名称
     * @param key 键名
     * @param defaultValue 默认值
     * @return 整数值
     */
    int getInt(const std::string& section,
        const std::string& key,
        int defaultValue = 0) const;

    /**
     * @brief 获取长整数值
     * @param section 节名称
     * @param key 键名
     * @param defaultValue 默认值
     * @return 长整数值
     */
    long getLong(const std::string& section,
        const std::string& key,
        long defaultValue = 0L) const;

    /**
     * @brief 获取浮点数值
     * @param section 节名称
     * @param key 键名
     * @param defaultValue 默认值
     * @return 浮点数值
     */
    float getFloat(const std::string& section,
        const std::string& key,
        float defaultValue = 0.0f) const;

    /**
     * @brief 获取双精度浮点数值
     * @param section 节名称
     * @param key 键名
     * @param defaultValue 默认值
     * @return 双精度浮点数值
     */
    double getDouble(const std::string& section,
        const std::string& key,
        double defaultValue = 0.0) const;

    /**
     * @brief 获取布尔值
     * @param section 节名称
     * @param key 键名
     * @param defaultValue 默认值
     * @return 布尔值
     */
    bool getBool(const std::string& section,
        const std::string& key,
        bool defaultValue = false) const;

    /**
     * @brief 设置字符串值（内存中，不会自动保存到文件）
     * @param section 节名称
     * @param key 键名
     * @param value 字符串值
     */
    void setString(const std::string& section,
        const std::string& key,
        const std::string& value);

    /**
     * @brief 设置整数值（内存中，不会自动保存到文件）
     * @param section 节名称
     * @param key 键名
     * @param value 整数值
     */
    void setInt(const std::string& section,
        const std::string& key,
        int value);

    /**
     * @brief 设置布尔值（内存中，不会自动保存到文件）
     * @param section 节名称
     * @param key 键名
     * @param value 布尔值
     */
    void setBool(const std::string& section,
        const std::string& key,
        bool value);

    /**
     * @brief 删除指定键
     * @param section 节名称
     * @param key 键名
     * @return 是否成功删除
     */
    bool removeKey(const std::string& section, const std::string& key);

    /**
     * @brief 删除指定节及其所有键
     * @param section 节名称
     * @return 是否成功删除
     */
    bool removeSection(const std::string& section);

    /**
     * @brief 清空所有数据
     */
    void clear();

    /**
     * @brief 将当前数据保存到文件
     * @param filePath 文件路径
     * @param keepComments 是否保留注释（当前实现简单，不保留注释）
     * @return 是否成功保存
     */
    bool save(const std::string& filePath, bool keepComments = false) const;

    /**
     * @brief 将当前数据转换为字符串
     * @return INI 格式的字符串
     */
    std::string toString() const;

    /**
     * @brief 获取解析错误信息
     * @return 错误信息字符串
     */
    std::string getLastError() const { return m_lastError; }

private:
    mutable std::string m_lastError;  // 最后一次错误信息
};

#endif // INI_PARSER_H