namespace ED7ScenaParser.Scena.Struct;

public struct ScenaMonster:IScenaOut
{
    [ReaderField(3)] public int[] Pos;
    public short Angle;
    public ushort Flags;
    public ushort BattleId;
    public ushort Flag;
    public ushort Chip;
    public ushort Unk2;
    public uint StandAnimation;
    public uint WalkAnimation;
}