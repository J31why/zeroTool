using System.Runtime.InteropServices;

namespace ITFCreator.ITF;

public enum ITFHeaderFlag
{
    System = 0xD,
    SystemUs = 0xE
}

[StructLayout(LayoutKind.Explicit, Size = 0x40)]
public class ITFHeader
{
    [FieldOffset(0)] public short Symbol = 257;
    [FieldOffset(2)]  public short Resolution; 
    [FieldOffset(4)]  public int ActualNodeCount;
    [FieldOffset(8)]  public int TotalNodeCount;
    [FieldOffset(0xc)] public ITFHeaderFlag Flag;
}