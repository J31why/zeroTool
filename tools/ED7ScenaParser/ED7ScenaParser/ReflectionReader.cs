using System.Reflection;
using System.Text;

namespace ED7ScenaParser;

public class ReflectionReader:BinaryReader
{
    public Encoding Encoding { get; set; } = Encoding.ASCII;
    public bool IsCString { get; set; } = true;
    
    public void ParseStructure<T>(ref T structure) where T: struct
    {
        var type = structure.GetType();
        if (!type.IsValueType || type is not { IsPrimitive: false, IsEnum: false })
            return;
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
            .OrderBy(field => field.MetadataToken).ToList();
        var typedRef = __makeref(structure);
        foreach (var field in fields)
        {
            if (field.IsDefined(typeof(ReaderFieldIgnoreAttribute), false))
                continue;
            var length = field.GetCustomAttribute<ReaderFieldAttribute>();
            object? obj = null;
            if (field.FieldType == typeof(string) && length?.Length > 0)
            {
                var pos =  BaseStream.Position;
                obj = Read(field.FieldType, length.Length);
                BaseStream.Seek(pos + length.Length, SeekOrigin.Begin);
            }
            else
            {
                obj = Read(field.FieldType, length?.Length);
            }
            field.SetValueDirect(typedRef, obj);
        }
    }
    public string? ReadCString()
    {
        if (BaseStream.Position >= BaseStream.Length)
            return null;
        var bytes = new List<byte>();
        while (BaseStream.Position < BaseStream.Length)
        {
            var b = ReadByte();
            if(b== 0)
                break;
            bytes.Add(b);
        }
        return Encoding.GetString(bytes.ToArray());
    }
    public object Read(Type type, int? scenaDataLength = null)
    {
        return type.Name switch
        {
            nameof(Byte) => ReadByte(),
            nameof(Int16) => ReadInt16(),
            nameof(UInt16) => ReadUInt16(),
            nameof(Int32) => ReadInt32(),
            nameof(UInt32) => ReadUInt32(),
            nameof(Int64) => ReadInt64(),
            nameof(UInt64) => ReadUInt64(),
            nameof(Single) => ReadSingle(),
            nameof(Double) => ReadDouble(),
            nameof(String) => (IsCString ? ReadCString() : ReadString()) ?? throw new Exception("read error"),
            _ => ReadArray(type, scenaDataLength)
        };
    }

    public object ReadArray(Type type, int? dataLength = null)
    {
        if (!type.IsArray || !(dataLength > 0) || type.GetElementType() is not { } elementType)
            throw new InvalidDataException("不支持的数据类型");
        var array = Array.CreateInstance(elementType, dataLength.Value);
        for (var i = 0; i < array.Length; i++)
            array.SetValue(Read(elementType, dataLength.Value), i);
        return array;
    }
    public ReflectionReader(Stream input) : base(input)
    {
    }

    public ReflectionReader(Stream input, Encoding encoding) : base(input, encoding)
    {
        
    }

    public ReflectionReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
    {
        
    }
}