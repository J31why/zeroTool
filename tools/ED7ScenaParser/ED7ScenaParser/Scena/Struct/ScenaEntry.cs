namespace ED7ScenaParser.Scena.Struct;
public struct ScenaEntry:IScenaOut
{
    [ReaderField(3)] public int[] Pos;
    public int Unk1;
    [ReaderField(3)] public int[] CamForm;
    public int CamPers;
    public short Unk2;
    public short CamDeg;
    [ReaderField(2)] public short[] CamLimit;
    [ReaderField(3)] public int[] CamAt;
    public short Unk3;
    public short Unk4;
    public short Flags;
    public short Town;
    [ReaderField(2)] public byte[] Init;
    [ReaderField(2)] public byte[] Reinit;
}