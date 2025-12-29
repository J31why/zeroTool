#region

using Enums;
using zCodec.Dats.As;

#endregion

namespace Extensions;

public static class AsOpcodesExtension
{
    public static (AsOpcodes, Func<AsCoder, ParamType[]>) As(this AsOpcodes opcodes)
    {
        return (opcodes, _ => []);
    }

    public static (AsOpcodes, Func<AsCoder, ParamType[]>) As(this AsOpcodes opcodes, params ParamType[] paramTypes)
    {
        return (opcodes, _ => paramTypes);
    }

    public static (AsOpcodes, Func<AsCoder, ParamType[]>) As(this AsOpcodes opcodes, Func<AsCoder, ParamType[]> func)
    {
        return (opcodes, func);
    }
}