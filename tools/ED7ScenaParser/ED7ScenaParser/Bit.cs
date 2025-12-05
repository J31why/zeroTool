namespace ED7ScenaParser;

public static class Bit
{
    public static (byte[] result, bool replaced) Replace(
        ReadOnlySpan<byte> source, 
        ReadOnlySpan<byte> oldPattern, 
        ReadOnlySpan<byte> newPattern,
        int start,
        int end = -1,
        int maxReplacements = -1)
    {
        if (oldPattern.Length == 0)
            throw new ArgumentException("旧模式不能为空", nameof(oldPattern));
    
        if (start < 0 || start >= source.Length)
            throw new ArgumentOutOfRangeException(nameof(start), "起始位置超出范围");
    
        if (end == -1)
            end = source.Length;
        else if (end < start || end > source.Length)
            throw new ArgumentOutOfRangeException(nameof(end), "结束位置超出范围");
    
        if (maxReplacements < -1 || maxReplacements == 0)
            throw new ArgumentOutOfRangeException(nameof(maxReplacements), "替换次数必须为-1或大于0");

        var result = new List<byte>(source.Length);
        int replacementsMade = 0;
        bool replaced = false;
        int i = 0;
        int remainingReplacements = maxReplacements == -1 ? int.MaxValue : maxReplacements;

        while (i < source.Length)
        {
            // 检查是否在替换范围内且还能进行替换
            bool canReplace = i >= start && i < end && 
                              replacementsMade < remainingReplacements &&
                              i + oldPattern.Length <= source.Length;
        
            if (canReplace)
            {
                var slice = source.Slice(i, oldPattern.Length);
            
                if (slice.SequenceEqual(oldPattern))
                {
                    result.AddRange(newPattern.ToArray());
                    i += oldPattern.Length;
                    replacementsMade++;
                    replaced = true;
                    continue;
                }
            }
        
            result.Add(source[i]);
            i++;
        }

        return (result.ToArray(), replaced);
    }
    public static byte[] Replace(ReadOnlySpan<byte> source, ReadOnlySpan<byte> oldPattern, ReadOnlySpan<byte> newPattern)
    {
        if (oldPattern.Length == 0)
            throw new ArgumentException("旧模式不能为空", nameof(oldPattern));

        var result = new List<byte>(source.Length);
        int i = 0;

        while (i < source.Length)
        {
            if (i + oldPattern.Length <= source.Length)
            {
                var slice = source.Slice(i, oldPattern.Length);
            
                if (slice.SequenceEqual(oldPattern))
                {
                    result.AddRange(newPattern.ToArray());
                    i += oldPattern.Length;
                    continue;
                }
            }
        
            result.Add(source[i]);
            i++;
        }

        return result.ToArray();
    }
}