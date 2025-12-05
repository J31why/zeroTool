// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

using System.Reflection;
using System.Reflection.Metadata;
using System.Text;

namespace ED7ScenaParser.Scena.Struct;

public struct ScenaHeader :IScenaOut
{
    [ReaderField(10)] public string HeadName1;
    [ReaderField(10)] public string HeadName2;
    [ReaderFieldIgnore] public string StringName;
    public ushort Town;
    public ushort Bgm;
    public uint Flags;
    [ReaderField(6)] public int[] Includes;
    public uint pStrings;
    public ushort pChips;
    public ushort pNpcs;
    public ushort pMonsters;
    public ushort pTriggers;
    public ushort pLookPoints;
    public ushort pFuncTable;
    public ushort FuncCount;
    public ushort pAnimations;
    public ushort pLabels;
    public byte nLabel;
    public byte Unk3;
    public byte nChips;
    public byte nNpcs;
    public byte nMonsters;
    public byte nTriggers;
    public byte nLookPoints;
    [ReaderField(2)] public byte[] itemUse;
    public byte Unk2;

}