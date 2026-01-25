# 《The Legend of Heroes: Trails from Zero》NISA版汉化补丁

## 【简介】
本补丁为PC平台上NISA版《英雄传说 零之轨迹》通过HOOK技术提供了中文支持。  
补丁中的绝大部分文本与图片资源移植自云豹版《零之轨迹：改》官方中文，并使用OpenCC对文本转简处理，虽然经过了处理，但由于文本量巨大，可能存在少量台湾用语，欢迎反馈，我会通过后续补丁进行调整。针对系统字符串及部分图片进行了手动翻译。  

> 如在游戏中遇到任何BUG、文本错误或有相关建议，欢迎反馈。  

---

## 【QA】

**Q：为什么不移植欢乐百世文本？**  
A：现官方轨迹发行商是云豹，云豹版的图片、影片资源更加高清。如果有朋友想要移植欢乐百世版本的文本，我可以提供部分帮助。

**Q：「更多头像Zero」MOD是什么？**  
A：这是外网 shinkiseki 制作的MOD，在游戏中添加了晓之轨迹、碧之轨迹和原创的头像、立绘。
汉化版本为 v1.5.1（2025-12-10发布），详情请参阅：[MorePortraitsInZero](https://github.com/shinkiseki/MorePortraitsInZero)

**Q：没有添加EVO的独占任务吗？**  
A：没有，不过如果有朋友愿意制作MOD的话，我可以提供部分帮助。

**Q：能解锁成就吗？**  
A：应该可以，补丁没有修改程序任何关于成就的地方。

**Q：我可以使用云豹存档吗？**  
A：并不支持云豹存档。

**Q：字体是什么？**  
A：默认字体为「屏显臻宋」，下载地址有其他字体可以选择。

**Q：原版存档怎么在汉化版中显示乱码？**  
A：不影响加载存档。

**Q：碧之轨迹补丁什么时候出？**  
A：基本完成，等待零轨补丁的反馈进行调整。

---

## 【下载安装】
补丁采用无损挂载方式，不覆盖原始游戏文件。启动游戏时添加 `-nohook` 参数可禁用汉化。

**下载地址：**  
度盘：https://pan.baidu.com/s/1PlC0dYKd6b6EbBuUY5HRQw?pwd=6666 

**安装方法：**
1. 将下载的 `零轨汉化补丁.zip` 解压至游戏目录（即 `zero.exe` 所在目录）。
2. 将下载的 `零轨汉化补丁资源包.zip` 解压至游戏目录下。

**可选安装：**
- **更换字体**：完成上述步骤后，将对应字体的压缩包中的`font.itf`文件替换掉`游戏\data_cn\system\fontdat`目录中的`font.itf`。
- **安装「更多头像Zero」MOD**：完成上述步骤后，将下载的MOD压缩包解压并覆盖游戏目录。

**卸载方法：**  
仅需删除游戏目录下的 `data_cn` 文件夹和 `dxgi.dll` 文件。

---

## 【制作人员】
- Jelly
- 黄昏浅蓝
- KloseRinz

---

## 【项目地址】
GitHub：[https://github.com/J31why/zeroTool](https://github.com/J31why/zeroTool)

---

## 【特别鸣谢】
- **Kyuuhachi**：提供了 scena 脚本、图片处理工具包 [Aureole](https://github.com/Kyuuhachi/Aureole)。
- **Ouroboros**：提供了 [EDDecompiler](https://github.com/Ouroboros/EDDecompiler)。
- **shinkiseki**：制作并分享了[更多头像](https://github.com/shinkiseki)MOD。
- **BYVoid & laisuk**：提供了 OpenCC 与 .NET 版本。
- **fxsjy & anderscui**：提供了 结巴分词 与 .NET 版本。
- **lxgw**：提供了[霞鹜臻楷](https://github.com/lxgw/LxgwZhenKai)字体。
- **chncwk**：提供了[屏显臻宋](https://www.cnprint.org/bbs/thread/165/357942)字体。
- **Adobe & Google**：提供了思源系列字体。

---

## 【免责声明】

* 本补丁为《The Legend of Heroes: Trails from Zero》的中文支持插件，由爱好者制作，**免费发布**。
* 本补丁使用的文本与图片资源绝大部分移植自官方发行的云豹版《零之轨迹：改》，其**版权归原权利人所有**（Falcom/NISA/云豹）。
* 本补丁仅为学习与研究目的发布，**不提供任何保证和担保**，使用者需自行承担风险。
* **严禁任何商业用途**。若权利方认为存在侵权，请联系处理。请支持官方正版游戏。

---

## 【发布日志】
**V1.0（2026/1/26）：**  
首次发布。