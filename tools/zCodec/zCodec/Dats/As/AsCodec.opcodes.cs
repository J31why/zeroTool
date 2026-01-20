#region

using Enums;
using Extensions;
using static zCodec.Dats.As.AsOpcodes;
using static Enums.ParameterType;

#endregion

namespace zCodec.Dats.As;

public partial class AsCodec
{
    private readonly Dictionary<byte, (AsOpcodes code, Func<AsCodec, ParameterType[]> Params)> _opFuncs = new()
    {
        { 0x00, Return.As() },
        { 0x01, Goto.As(sp) },
        { 0x02, SubChip.As(b, b) },
        { 0x03, OP_3.As(b, s) },
        { 0x04, OP_4.As(b, b, s) },
        { 0x05, OP_5.As(b, b, i) },
        { 0x06, Sleep.As(codec => codec.IsAo ? [s] : [i]) },
        { 0x07, Update.As() },
        { 0x08, Teleport.As(b, b, i, i, i) },
        { 0x09, OP_9.As(b, b, i, i, i) },
        { 0x0A, OP_A.As(b, b, b, i) },
        { 0x0B, Turn.As(codec => codec.IsAo ? [s,s] : [s,i]) },
        { 0x0C, OP_C.As(codec =>
        {
            byte x = 0;
            if (codec._isRead)
            {
                codec._bReader!.BaseStream.Position++;
                 x = codec._bReader.ReadByte();
                codec._bReader!.BaseStream.Position -= 2;
            }
            else
            {
                x = Convert.ToByte(codec._currentIns.param[1], 16);
            }
            return x == 0xfc ? [b,b,s,s,s,s,s,b] : [b,b,s,s,b];
        }) },
        { 0x0D, Jump.As(b, b, i, i, i, s, s) },
        { 0x0E, OP_E.As(b) },
        { 0x0F, JumpToTarget.As(s, s) },
        { 0x10, JumpBack.As(s, s) },
        { 0x11, Move.As(b, b, i, i, i, i, b) },
        { 0x12, AddEff.As(s, str) },
        { 0x13, ReleaseEff.As(codec =>codec.IsAo ?[b]:[s]) },
        { 0x14, WaitEff.As(codec =>codec.IsAo ?[b]:[s]) },
        { 0x15, WaitEff2.As(b, b) },
        { 0x16, FinishEff.As(b, b) },
        { 0x17, CancelEff.As(b, b) },
        {
            0x18,
            ShowEff.As(codec =>
                codec.IsAo ? [b, b, b, b, i, i, i, s, s, s, s, s, s, b] : [b, b, s, s, i, i, i, s, s, s, s, s, s, b])
        },
        { 0x19, Show3DEff.As(codec =>codec.IsAo ?[b,b,str,b,b,i,i,i,s,s,s,s,s,s,b]:[b, b, str, s, s, s, s, s, s, s, s, s, s, s, s, s, s, b]) },
        { 0x1A, OP_1A.As(b, b, s) },
        { 0x1B, SelectChip.As(b, b) },
        { 0x1C, Damage.As(codec=>codec.IsAo?[b]:[b,b]) },
        { 0x1D, DamageAnime.As(codec =>codec.IsAo ?[b,b,b]:[b, b, i]) },
        { 0x1E, OP_1E.As(i) },
        { 0x1F, OP_1F.As(i) },
        { 0x20, OP_20.As(b, b, b, i, i) },
        { 0x21, OP_21.As(b, b, i, i) },
        { 0x22, BeginThread.As(b, b, sp, b) },
        { 0x23, WaitThread.As(b, b) },
        { 0x24, SetChipModeFlag.As(b, b, s) },
        { 0x25, ClearChipModeFlag.As(b, b, s) },
        { 0x26, OP_26.As(b, b, s) },
        { 0x27, OP_27.As(b, b, s) },
        { 0x28, TalkText.As(b, str, i) },
        { 0x29, OP_29.As(b) },
        { 0x2A, TipText.As(str, i) },
        { 0x2B, OP_2B.As() },
        { 0x2C, ShadowBegin.As(b, s, s) },
        { 0x2D, ShadowEnd.As(b) },
        { 0x2E, ShakeChar.As(b, i, i, i) },
        { 0x2F, SuspendThread.As(b, b) },
        //
        { 0x31, OP_31.As(codec => codec.IsAo ? [b, s] : [b, i]) },
        { 0x32, OP_32.As(b, b) },
        { 0x33, OP_33.As(b, b) },
        { 0x34, OP_34.As() },
        { 0x35, KeepAngle.As(codec => codec.IsAo ? [b, i, i, i, s] : [b, i, i, i, i]) },
        { 0x36, OP_36.As(codec => codec.IsAo ? [b] : [s]) },
        //
        { 0x39, SetAngle.As(codec => codec.IsAo ? [s,s,s,s] : [s,s,s,i]) },
        { 0x3A, TiltAngle.As(codec => codec.IsAo ? [s, s] : [s, i]) },
        { 0x3B, RotationAngleHorz.As(codec => codec.IsAo ? [i, s] : [i, i]) },
        { 0x3C, OP_3C.As(codec => codec.IsAo ? [s, s] : [s, i]) },
        { 0x3D, ShakeScreen.As(codec => codec.IsAo ? [s, s, s, s] : [i, i, i, i]) },
        { 0x3E, OP_3E.As(codec => codec.IsAo ? [s, s] : [i, i]) },
        { 0x3F, OP_3F.As(b) },
        { 0x40, OP_40.As(b) },
        { 0x41, LockAngle.As(b) },
        { 0x42, OP_42.As(b, i, b) },
        { 0x43, SetBkColor.As(codec => codec.IsAo ? [b, s, i] : [b, i, i]) },
        { 0x44, ZoomAngle.As(codec => codec.IsAo ? [b, s, i] : [b, i, i]) },
        { 0x45, OP_45.As(b, i) },
        { 0x46, OP_46.As(b, i, i) },
        { 0x47, OP_47.As(b) },
        { 0x48, OP_48.As(b, i) },
        { 0x49, OP_49.As(b) },
        //
        { 0x4B, Rand.As(b, b, i, sp) },
        { 0x4C, LoopTargetBegin.As(sp) },
        { 0x4D, ResetLoopTarget.As() },
        { 0x4E, LoopTargetEnd.As() },
        { 0x4F, OP_4F.As(b, b) },
        { 0x50, Call.As(sp) },
        { 0x51, Ret.As() },
        { 0x52, OP_52.As(b) },
        { 0x53, OP_53.As(b) },
        { 0x54, OP_54.As(b) },
        { 0x55, MagicCastBegin.As(s) },
        { 0x56, MagicCastEnd.As() },
        { 0x57, OP_57.As(b, b) },
        { 0x58, BeatBack.As(b) },
        { 0x5A, OP_5A.As(codec => codec.IsAo ? [b, s,b] : [b,b, i]) },
        { 0x5B, OP_5B.As(codec => codec.IsAo ? [s] : [i]) },
        { 0x5C, Show.As(codec =>codec.IsAo ? [b,s] : [b,i]) },
        { 0x5D, Hide.As(codec =>codec.IsAo ? [b,s] : [b,i]) },
        { 0x5E, OP_5E.As(b) },
        { 0x5F, OP_5F.As(b, b) },
        { 0x60, OP_60.As(b) },
        { 0x61, SetBattleSpeed.As(i) },
        { 0x62, OP_62.As(b, s, s, s, s, b) },
        //
        { 0x64, SE.As(s) },
        { 0x65, SeEx.As(s, b) },
        { 0x66, OP_66.As(s) },
        { 0x67, ScraftCutIn.As(s, b, b) },
        //
        { 0x6A, LoadSChip.As(b, i, b) },
        { 0x6B, ResetSCraftChip.As() },
        { 0x6C, Die.As() },
        { 0x6D, OP_6D.As(i) },
        { 0x6E, OP_6E.As(i) },
        { 0x73, OP_73.As(b) },
        { 0x78, OP_78.As(b) },
        { 0x79, OP_79.As(b) },
        { 0x7A, CraftEnd.As(b) },
        { 0x7B, CraftEndFlag.As(s) },
        { 0x7C, OP_7C.As(b, b) },
        { 0x7E, OP_7E.As(i) },
        { 0x7F, Blur.As(codec =>codec.IsAo ? [s,i,b,b,b] : [i, i, i, b, i]) },
        { 0x80, OP_80.As(i) },
        //
        { 0x82, OP_82.As() },
        { 0x83, SortTarget.As(b) },
        { 0x84, RotateChar.As(b, s, s, s, i, b) },
        { 0x85, OP_85.As(b, b, i) },
        //
        { 0x89, SaveCurPos.As(b) },
        { 0x8A, Clone.As(b, b) },
        { 0x8B, UseItemBegin.As() },
        { 0x8C, UseItemEnd.As() },
        { 0x8D, OP_8D.As(b, i, i, i, i) },
        {
            0x8E, LoadXFile.As(codec =>
            {
                byte x;
                if (codec._isRead)
                {
                    x = codec._bReader!.ReadByte();
                    codec._bReader.BaseStream.Position--;
                }
                else
                {
                    x = Convert.ToByte(codec._currentIns.param[0], 16);
                }

                return x switch
                {
                    1 => [b, b, str],
                    0xD => [b, b, i, i, i, i, i],
                    _ => [b, b, i, i, i, i]
                };
            })
        },
        { 0x8F, OP_8F.As(b) },
        //{ 0x90, OP_90.As(b) },
        { 0x92, OP_92.As(b, b, i, i, i, s, i) },
        { 0x93, OP_93.As(b, b, str) },
        { 0x94, OP_94.As(b, str, i) },
        { 0x95, OP_95.As() },
        {
            0x96, SetAngleTarget.As(codec =>
            {
                if (codec.IsAo)
                    return [b, str, b];
                byte x;
                if (codec._isRead)
                {
                    x = codec._bReader!.ReadByte();
                    codec._bReader.BaseStream.Position--;
                }
                else
                {
                    x = Convert.ToByte(codec._currentIns.param[0], 16);
                }

                return x switch
                {
                    2 => [b, str, b], //as90000.dat里dev段尾部只有1字节
                    _ => [b, str, s] //正常都是2字节,不过是无用数据
                };
            })
        },
        { 0x97, MoveAngle.As(codec => codec.IsAo ? [s, s] : [i, s, s]) },
        //
        { 0x99, OP_99.As(b) },
        { 0x9A, OP_9A.As(i) },
        { 0x9B, OP_9B.As(b) },
        { 0x9C, ResetChipStatus.As(b) },
        { 0x9E, RefRes.As(b, str) },
        //
        { 0x9F, SetBattleMode.As(b, i) },
        { 0xA0, OP_A0.As(b, i) },
        { 0xA1, OP_A1.As(b, i) },
        //
        { 0xA6, ScaleAnim.As(b, b, i, i, b) },
        { 0xA7, SetObjAnim.As(b, b, s, s, s, s, s, s, s) },
        { 0xA8, DamageVoice.As(codec => codec.IsAo ? [b,b] : [b, s]) },
        { 0xA9, OP_A9.As(b, b, i) },
        { 0xAA, OP_AA.As(i, i) },
        //
        { 0xAC, OP_AC.As(codec => codec.IsAo ? [i,i] : [b, b, i, i, b]) },
        { 0xAE, OP_AE.As(b, b, b) },
        { 0xAF, OP_AF.As(b, b, i, i, i) },
        { 0xB0, OP_B0.As(codec => codec.IsAo ? [s, s] : [s, i]) },
        { 0xB1, OP_B1.As(codec => codec.IsAo ? [b,str,b,i] : [b, s]) },
        { 0xB5, OP_B5.As(i, s, b, b) },
        { 0xB6, OP_B6.As(i, s, b, b) },
        { 0xB7, OP_B7.As() }
    };
}

public enum AsOpcodes
{
    Return,
    Goto,
    SubChip,
    OP_3,
    OP_4,
    OP_5,
    Sleep,
    Update,
    Teleport,
    OP_9,
    OP_A,
    Turn,
    OP_C,
    Jump,
    OP_E,
    JumpToTarget,
    JumpBack,
    Move,
    AddEff,
    ReleaseEff,
    WaitEff,
    WaitEff2,
    FinishEff,
    CancelEff,
    ShowEff,
    Show3DEff,
    OP_1A,
    SelectChip,
    Damage,
    DamageAnime,
    OP_1E,
    OP_1F,
    OP_20,
    OP_21,
    BeginThread,
    WaitThread,
    SetChipModeFlag,
    ClearChipModeFlag,
    OP_26,
    OP_27,
    TalkText,
    OP_29,
    TipText,
    OP_2B,
    ShadowBegin,
    ShadowEnd,
    ShakeChar,
    SuspendThread,
    TalkTextArr,
    OP_31,
    OP_32,
    OP_33,
    OP_34,
    KeepAngle,
    OP_36,
    RotationAngle,
    RotationAngleVert,
    SetAngle,
    TiltAngle,
    RotationAngleHorz,
    OP_3C,
    ShakeScreen,
    OP_3E,
    OP_3F,
    OP_40,
    LockAngle,
    OP_42,
    SetBkColor,
    ZoomAngle,
    OP_45,
    OP_46,
    OP_47,
    OP_48,
    OP_49,
    Rand,
    LoopTargetBegin,
    ResetLoopTarget,
    LoopTargetEnd,
    OP_4F,
    Call,
    Ret,
    OP_52,
    OP_53,
    OP_54,
    MagicCastBegin,
    MagicCastEnd,
    OP_57,
    BeatBack,
    OP_5A,
    OP_5B,
    Show,
    Hide,
    OP_5E,
    OP_5F,
    OP_60,
    SetBattleSpeed,
    OP_62,
    OP_63,
    SE,
    SeEx,
    OP_66,
    ScraftCutIn,
    ReleaseTexture,
    LoadSChip,
    ResetSCraftChip,
    Die,
    OP_6D,
    OP_6E,
    OP_73,
    OP_78,
    OP_79,
    CraftEnd,
    CraftEndFlag,
    OP_7C,
    OP_7E,
    Blur,
    OP_80,
    OP_81,
    OP_82,
    SortTarget,
    RotateChar,
    OP_85,
    OP_86,
    Voice,
    SaveCurPos,
    Clone,
    UseItemBegin,
    UseItemEnd,
    OP_8D,
    LoadXFile,
    OP_8F,
    OP_90,
    OP_92,
    OP_93,
    OP_94,
    OP_95,
    SetAngleTarget,
    MoveAngle,
    OP_98,
    OP_99,
    OP_9A,
    OP_9B,
    ResetChipStatus,
    OP_9D,
    RefRes,
    SetBattleMode,
    OP_A0,
    OP_A1,
    OP_A2,
    OP_A3,
    OP_A4,
    OP_A5,
    ScaleAnim,
    SetObjAnim,
    DamageVoice,
    OP_A9,
    OP_AA,
    OP_AB,
    OP_AC,
    OP_AE,
    OP_AF,
    OP_B0,
    OP_B1,
    OP_B5,
    OP_B6,
    OP_B7
}