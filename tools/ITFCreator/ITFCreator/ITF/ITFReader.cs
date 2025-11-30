using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Media.Imaging;
using SkiaSharp;

namespace ITFCreator.ITF;

public class ITFReader:BinaryReader
{
    public readonly ITFHeader Header = new();
    
    public ITFReader(Stream input) : base(input)
    {
        ParseHeader();
    }

    public ITFReader(Stream input, Encoding encoding) : base(input, encoding)
    {
        ParseHeader();
    }

    public ITFReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
    {
        ParseHeader();
    }

    private void ParseHeader()
    {
        BaseStream.Seek(0x2, SeekOrigin.Begin);
        Header.Resolution = ReadInt16();
        Header.ActualNodeCount =  ReadInt32();
        Header.TotalNodeCount =  ReadInt32();
        Header.Flag = (ITFHeaderFlag)ReadInt32();
    }

    /// <summary>
    /// 模拟游戏unicode搜索
    /// </summary>
    /// <param name="unicode"></param>
    /// <returns></returns>
    public int Search(uint unicode)
    {
        var hSize = Marshal.SizeOf<ITFHeader>();
        var nodeIndex = 1;
        while (true)
        {
            var arrayIndex = nodeIndex - 1;
            if (arrayIndex >= Header.TotalNodeCount)
                return -1;
            BaseStream.Seek(hSize + arrayIndex * 8, SeekOrigin.Begin);
            var currentCode = ReadUInt32();
            if (unicode==currentCode)
                return arrayIndex;
            nodeIndex *= 2;
            if (unicode >= currentCode)
                nodeIndex |= 1;
        }
    }

    public ITFChar? GetChar(int index)
    {
        if (index <0 || index >= Header.TotalNodeCount)
            return null;
        var hSize = Marshal.SizeOf<ITFHeader>();
        BaseStream.Seek(hSize + index * 8+4, SeekOrigin.Begin);
        var offset = ReadInt32();
        if (offset <= 0)
            return null;
        var ch = ReadChar(offset);
        return ch;
    }

    private ITFChar ReadChar(int offset)
    {
        BaseStream.Seek(offset, SeekOrigin.Begin);
        var ch = new ITFChar
        {
            Offset = offset,
            PixelWidth = ReadUInt16(),
            PixelHeight = ReadUInt16(),
            Top = ReadInt16(),
            Left = ReadInt16(),
            Width = ReadUInt16(),
            Attr = (CharAttr)ReadUInt16()
        };
        ch.Data = ReadBytes((int)Math.Ceiling(ch.PixelWidth * ch.PixelHeight / 2d));
        return ch;
    }
    public Bitmap? CharDataToBitmap(byte[] data,int width,int height,byte r,byte g,byte b)
    {
        if (data.Length <= 0 || width * height > data.Length * 2)
            return null;
        var imageInfo = new SKImageInfo(
            width: 100,
            height: 100,
            colorType: SKColorType.Bgra8888,
            alphaType: SKAlphaType.Premul
        );
        using var skBitmap = new SKBitmap(imageInfo);
        using var skCanvas = new SKCanvas(skBitmap);

        var isHigh = false;
        var index = 0;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var aByte = isHigh ? data[index] >> 4 & 0xf : data[index] & 0xf;
                var a = (byte)(aByte * 17);
                if (a > 0)
                    skCanvas.DrawPoint(x, y, new SKColor(r, g, b, a));
                if(isHigh)
                    index++;
                isHigh = !isHigh;
            }
        }
        using var stream = new MemoryStream();
        skBitmap.Encode(stream, SKEncodedImageFormat.Png, quality: 100);
        stream.Seek(0, SeekOrigin.Begin);
        return new Bitmap(stream);
    }
}