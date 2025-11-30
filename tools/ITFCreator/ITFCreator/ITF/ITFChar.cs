using System;
using System.Runtime.InteropServices;

namespace ITFCreator.ITF;

[Flags]
public enum CharAttr : ushort
{
    Full=0,
    Half=1,
    /// <summary>
    /// ′〟’』。》，”〉」】〕 ゜．°″、 ゛）］｝
    /// </summary>
    RightPunctuation = 2,
    /// <summary>
    /// （‘“〈《「『【〔〝［｛
    /// </summary>
    LeftPunctuation  = 4,
}

public class ITFChar
{
    public ushort PixelWidth;
    public ushort PixelHeight;
    public short Top;
    public short Left;
    /// <summary>
    /// total width = left + width
    /// </summary>
    public ushort Width; 
    public CharAttr Attr;
    public byte[]? Data;

    public int Offset;
    public override string ToString()
    {
        return $"{nameof(Offset)}: 0x{Offset:X}, "+
               $"{nameof(PixelWidth)}: 0x{PixelWidth:X}, " +
               $"{nameof(PixelHeight)}: 0x{PixelHeight:X}, " +
               $"{nameof(Left)}: 0x{Left:X}, " +
               $"{nameof(Top)}: 0x{Top:X}, " +
               $"{nameof(Width)}: 0x{Width:X}, " +
               $"{nameof(Attr)}: {Attr.ToString()}";
    }
}