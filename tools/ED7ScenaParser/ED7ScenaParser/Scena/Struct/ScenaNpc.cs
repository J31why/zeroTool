namespace ED7ScenaParser.Scena.Struct;

public struct ScenaNpc:IScenaOut
{
    [ReaderFieldIgnore] public string Name;
    [ReaderField(3)] public int[] Pos;
    public short Angle;
    public ushort Flags;
    public ushort Unk2;
    public ushort Chip;
    [ReaderField(2)] public byte[] Init;
    [ReaderField(2)] public byte[] Talk;
    public uint Unk4;
}