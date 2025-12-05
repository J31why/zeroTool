namespace ED7ScenaParser.Scena.Struct;

public struct ScenaTrigger:IScenaOut
{
    [ReaderField(3)] public float[] Pos;
    public float Radius;
    [ReaderField(4 * 4)] public float[] Transform;
    public byte Unk1;
    public ushort Unk2;
    [ReaderField(2)] public byte[] Function;
    public byte Unk3;
    public ushort Unk4;
    public uint Unk5;
    public uint Unk6;
}