namespace ED7ScenaParser.Scena;

public static class ScenaOpcodeFuncBuilder
{
    public static Func<string> Op(this string op, Func<string?>[]? param=null)
    {
        return () =>
        {
            var p = param == null ? "" : string.Join(',', param.Select(x => x.Invoke()).ToArray());
            return $"{op}(p)";
        };
    }
}