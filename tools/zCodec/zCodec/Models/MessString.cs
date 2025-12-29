#region

using zCodec.Calmare;

#endregion

namespace Models;

public class MessString
{
    public int Index { get; set; }
    public string Key { get; set; }
    public string CN { get; set; }
    public string EN { get; set; }
    public string JP { get; set; }

    public string ToLine()
    {
        return $"{Key}:\"{Replace(CN)}\"";
    }

    private static string Replace(string cn)
    {
        foreach (var c in CalmareCoder.Remaps)
            if (cn.Contains(c.Key))
                cn = cn.Replace(c.Key, c.Value);
        return cn;
    }
}