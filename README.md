基于NISA GOG版v1.4.13的零之轨迹和v1.0.1云豹版零轨制作而成。  
 
### NISA
Dll1：GBK读取DLL，运行参数+debug可开启console 。载入后读取localization目录中的GBK编码mess_strings_cn.txt文件。  
ITFCreator：字体创建器  
Injector：dll注入器  
### 云豹 
CloudDecrypt： 解密云豹文件，如bin和_dt文件  
charTable：云豹双字节字符的自定义字库，解密后为3字节的utf8数据，前四个字节是字库大小，双字节ab, index = a<<8 - 0x8900 + b，步进为3，比如8A D1 = 1D1，pos=0x1D1*3，取3字节E7 BE 85羅。  
