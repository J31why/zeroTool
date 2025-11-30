using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SkiaSharp;

namespace ITFCreator.ITF;


public class ITFWriter(Stream stream) : BinaryWriter(stream)
{
    private SKTypeface? _face;
    private SKFont? _font;
    private SKPaint? _paint;
    private float _baselineOffset;
    public bool Build(string codeRange,string fontName, int fontSize,  SKFontStyleWeight weight, float baselineOffset)
    {
        try
        {
            _baselineOffset = baselineOffset;
            var codes = ParseRange(codeRange);
            _face = SKTypeface.FromFamilyName(fontName, weight, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
            _font = new SKFont(_face, fontSize);
            _paint = new SKPaint();
            _paint.IsAntialias = true;
            _paint.Color = SKColors.Black;
            FilterRange(ref codes);
            var node = BuildNode(codes, 0, codes.Count);
            if (node == null)
                return false;
            var nodes = LevelOrderTraversal(node);
            OutStream.Seek(0, SeekOrigin.Begin);
            OutStream.SetLength(0);
            var header = new ITFHeader
            {
                Resolution = 96,
                ActualNodeCount = codes.Count,
                TotalNodeCount = nodes.Count,
                Flag = ITFHeaderFlag.System
            };
            var headerSize = Marshal.SizeOf<ITFHeader>();
            Write(new byte[headerSize]);
            WriteHeader(header);
            WriteNodes(nodes);
            
            Flush();
        }
        catch
        {
            return false;
        }
        finally
        {
            DisposeResources();
        }
        return true;
    }

    private void WriteHeader(ITFHeader header)
    {
        Seek(0, SeekOrigin.Begin);
        Write(header.Symbol);
        Write(header.Resolution);
        Write(header.ActualNodeCount);
        Write(header.TotalNodeCount);
        Write((int)header.Flag);
    }

    private void WriteNodes(List<ITFNode?> nodes)
    {
        var headerSize = Marshal.SizeOf<ITFHeader>();
        OutStream.Seek(headerSize, SeekOrigin.Begin);
        var avlPos = OutStream.Position;
        Write(new byte[nodes.Count * 8]);
        var dataPos = OutStream.Position;
        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            OutStream.Seek(avlPos + i * 8, SeekOrigin.Begin);
            if (node == null)
            {
                Write(-1L);
                continue;
            }
            Write(node.Code);
            Write(dataPos);
            //data
            OutStream.Seek(dataPos, SeekOrigin.Begin);
            GenerateITFChar((char)node.Code, _baselineOffset, out var ch);
            Write(ch.PixelWidth);
            Write(ch.PixelHeight);
            Write(ch.Top);
            Write(ch.Left);
            Write(ch.Width);
            Write((ushort)ch.Attr);
            Write(ch.Data ?? []);
            dataPos = OutStream.Position;
        }
    }

  
    public void GenerateITFChar(char c, float baselineYOffset,out ITFChar ch)
    {
        ch = new ITFChar
        {
            Attr = c switch
            {
                '′' or '〟' or '’' or '』' or '。' or '》' or '，' or '”' or '〉' or '」' or '】' or '〕' or ' ' or '゜' or '．' or '°'
                    or '″' or '、' or '゛' or '）' or '］' or '｝' 
                    => CharAttr.RightPunctuation,
                '（' or '‘' or '“' or '〈' or '《' or '「' or '『' or '【' or '〔' or '〝' or '［' or '｛' 
                    => CharAttr.LeftPunctuation,
                _ => c <= 0x7f ? CharAttr.Half : CharAttr.Full
            }
        };
        var str = c.ToString();
        var width =(ushort)_font!.MeasureText(str, out var charBounds, _paint);
        ch.PixelWidth = (ushort)Math.Ceiling(charBounds.Width);
        ch.PixelHeight = (ushort)Math.Ceiling(charBounds.Height);
        ch.Left = (short)charBounds.Left;
        ch.Width = (ushort)(width - ch.Left);
        ch.Top= (short)(-_font.Metrics.Ascent+ charBounds.Top + baselineYOffset);
        
        using var bitmap = new SKBitmap(ch.PixelWidth, ch.PixelHeight, SKColorType.Alpha8, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent); 
        var drawX = -charBounds.Left;
        var drawY = -charBounds.Top;
        canvas.DrawText(str, drawX, drawY, _font, _paint);
        var compData = new byte[(int)Math.Ceiling(bitmap.Bytes.Length / 2d)];
        var index = 0;
        var isHigh = false;
        foreach (var b in bitmap.Bytes)
        {
            if (!isHigh)
                compData[index] = (byte)(b / 17 & 0xf);
            else
                compData[index] = (byte)(compData[index] + ((b / 17 & 0xf) << 4));
            isHigh = !isHigh;
            if (!isHigh)
                index++;
        }

        ch.Data = compData;
    }
    private void FilterRange(ref List<int> codes)
    {
        if (_font is null)
            return;
        codes = codes.Distinct().ToList();
        for (var i = codes.Count - 1; i >= 0; i--)
        {
            if (!_font.ContainsGlyph(codes[i]))
                codes.RemoveAt(i);
        }
        codes.Sort((a,b)=>a.CompareTo(b));
    }
    private static ITFNode? BuildNode(List<int> codes, int start, int end)
    {
        if (start >= end || start >= codes.Count)
            return null;
        var index = start + (end - start) / 2;

        var node = new ITFNode
        {
            Code = codes[index],
        };
        if (start < index)
            node.Left = BuildNode(codes, start, index);
        if (index < end)
            node.Right = BuildNode(codes, index + 1, end);
        return node;
    }
    private static List<ITFNode?> LevelOrderTraversal(ITFNode root)
    {
        var result = new List<ITFNode?>();
        var queue = new Queue<ITFNode?>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            result.Add(node);
            if (node == null) continue;
            queue.Enqueue(node.Left);
            queue.Enqueue(node.Right);
        }
        for (var i = result.Count - 1; i >= 0; i--)
        {
            if (result[i] == null)
                result.RemoveAt(i);
            else
                break; 
        }
        return result;
    }

    private static List<int> ParseRange(string unicodeRange)
    {
        var codes = new List<int>(0xffff);
        var ranges = unicodeRange.Replace(" ","").Split(',');
        foreach (string range in ranges)
        {
            var r = range.Split('-');
            if (r.Length != 2)
                continue;
            var start = Convert.ToInt32(r[0].Replace("0x", ""), 16);
            var end =  Convert.ToInt32(r[1].Replace("0x", ""), 16);
            if(start > end)
                continue;
            codes.AddRange(Enumerable.Range(start, end - start + 1));
        }
        return codes;
    }
    private void DisposeResources()
    {
        _face?.Dispose();
        _font?.Dispose();
        _paint?.Dispose();
    }
    protected override void Dispose(bool disposing)
    {
        DisposeResources();
        base.Dispose(disposing);
    }

    public override ValueTask DisposeAsync()
    {
        DisposeResources();
        return base.DisposeAsync();
    }
    
}