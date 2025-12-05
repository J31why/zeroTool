using System.Collections;
using System.Reflection;
using System.Text;

namespace ED7ScenaParser.Scena.Struct;
public interface IScenaOut { }

public static class ScenaOutExtensions
{
    public static string Out(this IScenaOut obj, int indentLevel=0)
    {
        var sb = new StringBuilder();
        var type = obj.GetType();
        
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => !f.Name.StartsWith('p') && !f.Name.StartsWith('n'))
            .OrderBy(field => field.MetadataToken).ToList();
        var indent = new string(' ', indentLevel*4);
        foreach (var field in fields)
        {
            var value = field.GetValue(obj);
            if (value is IScenaOut scenaOut)
            {
                sb.AppendLine($"{indent}{field.Name}:\n{scenaOut.Out(indentLevel+1)}");
                continue;
            }
            var elementType = value?.GetType().GetElementType();
            if (elementType != null && elementType.IsAssignableTo(typeof(IScenaOut)))
            {
                var array = value as Array;
                for (int i = 0; i < array!.Length; i++)
                {
                    sb.Append($"\n{indent}{field.Name}[{i}]:\n{((IScenaOut)array.GetValue(i)!).Out(indentLevel + 1)}");
                }
            }
            else
                sb.AppendLine($"{indent}{field.Name}: {FormatValue(value)}");
        }
        return sb.ToString();
    }

    private static string FormatValue(object? value)
    {
        ArgumentNullException.ThrowIfNull(value);
      
        if (value is Array array)
            return $"[{string.Join(", ", array.Cast<object>())}]";
        if (value is string str)
            return $"\"{str}\"";
        return value.ToString() ?? throw new InvalidOperationException();
    }
    
}