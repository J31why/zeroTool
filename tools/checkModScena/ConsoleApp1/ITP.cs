using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1
{
    internal class ITP
    {
        public static void main()
        {
            #region MyRegion
            string inputPath = "chrimg00.itp";
            string outputPath = "chrimg00.dds";
            if (!File.Exists(inputPath)) return;

            using (BinaryReader br = new BinaryReader(File.OpenRead(inputPath)))
            {
                // 1. 验证魔数
                if (br.ReadUInt32() != 0xFF505449) throw new Exception("不是有效的 ITP 文件");

                // 2. 寻找 IDAT 块 (简单定位，实际应遍历 Chunk)
                br.BaseStream.Seek(0x68, SeekOrigin.Begin);
                if (new string(br.ReadChars(4)) != "IDAT") throw new Exception("未找到 IDAT 块");

                int idatChunkSize = br.ReadInt32();
                Console.WriteLine(br.ReadInt32()); // 8
                Console.WriteLine(br.ReadInt16()); // 0
                Console.WriteLine(br.ReadInt16()); // mip index

                // 3. 这里的 0x80000001 是 Minor 10 的标志
                if (br.ReadUInt32() != 0x80000001) throw new Exception("不支持的压缩模式");

                int nChunks = br.ReadInt32();
                int totalCSize = br.ReadInt32();
                int largestCSize = br.ReadInt32();
                int totalUSize = br.ReadInt32(); // 这个应该是 (W*H)

                List<byte> decompressedData = new List<byte>();

                for (int i = 0; i < nChunks; i++)
                {
                    //lzss.Decompress(decompressedData);
                    Decompress(br, decompressedData);
                    //0x27e5-0x8c
                }
                //0x8c 0x7d011
                // 5. 保存为 DDS
                //File.WriteAllBytes(outputPath, decompressedData.ToArray());
                a(decompressedData.ToArray());

                //SaveDds(outputPath, decompressedData.ToArray(), 1024, 512);
                Console.WriteLine("提取完成: " + outputPath);
            }
            #endregion
        }
        public static void a(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms);
            using var outMs = new MemoryStream();
            using var bw = new BinaryWriter(outMs);
            var chunkSize = 8;
            var len = data.Length / chunkSize;
            var maxlen = 0;
            var last = 0;
            for (int i = 0; i < chunkSize; i++)
            {
                var buffer = br.ReadBytes(len);
                Compress(buffer, bw, 8);
                var nowLen = bw.BaseStream.Length - last;
                maxlen = Math.Max(maxlen, (int)nowLen);
                last = (int)bw.BaseStream.Length;
            }
            File.WriteAllBytes("chrimg00.itp.lzss1", outMs.ToArray());
        }
        static void Compress(byte[] input, BinaryWriter bw, int mode = 8)
        {
            int startPos = (int)bw.BaseStream.Position;
            // 占位符：csize, usize, mode
            bw.Write(0);
            bw.Write(input.Length);
            bw.Write(mode);

            int cursor = 0;
            int maxOp = (1 << mode) - 1;       // op 的最大值
            int maxLookback = (1 << (16 - mode)) - 1; // num 的最大值

            while (cursor < input.Length)
            {
                int bestMatchLen = 0;
                int bestMatchDist = 0;

                // 在滑动窗口内寻找最长匹配 (LZ77 逻辑)
                // 注意：根据解压逻辑，op > 0 时最后会多读一个 byte，所以匹配长度限制在 maxOp
                int searchStart = Math.Max(0, cursor - maxLookback - 1);
                for (int j = searchStart; j < cursor; j++)
                {
                    int matchLen = 0;
                    while (matchLen < maxOp &&
                           cursor + matchLen < input.Length - 1 && // 留一个字节给随后的 ReadByte
                           input[j + matchLen] == input[cursor + matchLen])
                    {
                        matchLen++;
                    }

                    if (matchLen >= bestMatchLen)
                    {
                        bestMatchLen = matchLen;
                        bestMatchDist = cursor - j - 1;
                    }
                }

                if (bestMatchLen > 0)
                {
                    // 写入 字典引用 模式 (op > 0)
                    ushort control = (ushort)((bestMatchDist << mode) | (bestMatchLen & maxOp));
                    bw.Write(control);
                    cursor += bestMatchLen;

                    // 写入随后的那一个字节 (outData.Add(br.ReadByte()))
                    bw.Write(input[cursor]);
                    cursor++;
                }
                else
                {
                    // 写入 原始数据 模式 (op == 0)
                    // 这里简单处理，每次只写1个字节的原始数据
                    ushort control = (ushort)(1 << mode); // num = 1, op = 0
                    bw.Write(control);
                    bw.Write(input[cursor]);
                    cursor++;
                }
            }

            // 回填 csize (总长度 - 4)
            int endPos = (int)bw.BaseStream.Position;
            int csize = endPos - startPos;
            bw.BaseStream.Seek(startPos, SeekOrigin.Begin);
            bw.Write(csize);
            bw.BaseStream.Seek(endPos, SeekOrigin.Begin);
        }
        static void Decompress(BinaryReader br, List<byte> outData)
        {
            int csize = br.ReadInt32();
            int usize = br.ReadInt32();
            int mode = br.ReadInt32(); // 这里通常是 4, 5 或 6, 但现在是8

            int startPos = outData.Count;

            if (mode == 0)
            {
                outData.AddRange(br.ReadBytes(csize - 4));
            }
            else
            {
                int endPos = startPos + usize;
                while (outData.Count < endPos)
                {
                    ushort x = br.ReadUInt16();
                    int op = x & ((1 << mode) - 1);
                    int num = x >> mode;

                    if (op == 0)
                    {
                        outData.AddRange(br.ReadBytes(num));
                    }
                    else
                    {
                        for (int i = 0; i < op; i++)
                        {
                            // 字典回溯
                            outData.Add(outData[outData.Count - num - 1]);
                        }
                        outData.Add(br.ReadByte());
                    }
                }
            }
        }
        static void SaveDds(string path, byte[] data, int w, int h)
        {
            using (BinaryWriter bw = new BinaryWriter(File.Create(path)))
            {
                bw.Write(0x20534444); // Magic
                bw.Write(124); bw.Write(0x1 | 0x2 | 0x4 | 0x1000 | 0x20000);
                bw.Write(h); bw.Write(w); bw.Write(0); bw.Write(0); bw.Write(1);
                for (int i = 0; i < 11; i++) bw.Write(0);
                bw.Write(32); bw.Write(0x4); bw.Write(0x30315844); // "DX10"
                for (int i = 0; i < 5; i++) bw.Write(0);
                bw.Write(0x1000); bw.Write(0); bw.Write(0); bw.Write(0); bw.Write(0);

                // DX10 扩展头
                bw.Write(98); // DXGI_FORMAT_BC7_UNORM
                bw.Write(3); bw.Write(0); bw.Write(1); bw.Write(0);

                bw.Write(data);
            }
        }
    }
}
