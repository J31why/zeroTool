namespace ED7ScenaParser.Scena.Struct;

public struct ScenaAnimation:IScenaOut
{
    public ushort Speed;
    public byte CheckByte;
    public byte Count;
    [ReaderField(8)] public byte[] Frames;
}