#region

using Enums;
using zCodec.Dats.As;

#endregion

namespace Extensions;

public static class AsOpcodesExtension
{
    public static (AsOpcodes, Func<AsCodec, ParameterType[]>) As(this AsOpcodes opcodes)
    {
        return (opcodes, _ => []);
    }

    public static (AsOpcodes, Func<AsCodec, ParameterType[]>) As(this AsOpcodes opcodes, params ParameterType[] paramTypes)
    {
        return (opcodes, _ => paramTypes);
    }

    public static (AsOpcodes, Func<AsCodec, ParameterType[]>) As(this AsOpcodes opcodes, Func<AsCodec, ParameterType[]> func)
    {
        return (opcodes, func);
    }
}