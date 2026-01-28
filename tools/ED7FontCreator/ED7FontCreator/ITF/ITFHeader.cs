using System.Runtime.InteropServices;

namespace ITF;

public enum ITFHeaderFlag
{
    System = 0xD,
    SystemUs = 0xE
}


public class ITFHeader
{
    public short Symbol = 257;
    public short Resolution; 
    public int ActualNodeCount;
    public int TotalNodeCount;
    public ITFHeaderFlag Flag;
}