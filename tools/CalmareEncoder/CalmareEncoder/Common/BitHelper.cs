namespace Common;

public static class BitHelper
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
        var replacementsMade = 0;
        var replaced = false;
        var i = 0;
        var remainingReplacements = maxReplacements == -1 ? int.MaxValue : maxReplacements;

        while (i < source.Length)
        {
            // 检查是否在替换范围内且还能进行替换
            var canReplace = i >= start && i < end &&
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

    public static byte[] Replace(ReadOnlySpan<byte> source, ReadOnlySpan<byte> oldPattern,
        ReadOnlySpan<byte> newPattern)
    {
        if (oldPattern.Length == 0)
            throw new ArgumentException("旧模式不能为空", nameof(oldPattern));

        var result = new List<byte>(source.Length);
        var i = 0;

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

    public static byte[] TrimEnd(byte[] input, byte trim)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input), "输入字节数组不能为空");
        if (input.Length == 0)
            return [];

        var lastNonTrimIndex = input.Length - 1;
        while (lastNonTrimIndex >= 0 && input[lastNonTrimIndex] == trim) lastNonTrimIndex--;

        if (lastNonTrimIndex < 0)
            return [];

        var result = new byte[lastNonTrimIndex + 1];
        Array.Copy(input, result, result.Length);

        return result;
    }
}