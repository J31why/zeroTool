namespace ED7ScenaParser.Scena.Struct;

public struct ScenaLookPoint:IScenaOut
{
    [ReaderField(3)] public int[] Pos;
    public uint Radius;
    [ReaderField(3)] public int[] BubblePos;
    public byte Unk1;
    public ushort Unk2;
    [ReaderField(2)] public byte[] Function;
    public byte Unk3;
    public ushort Unk4;
}