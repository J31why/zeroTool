using System.Text;
using System.Text.RegularExpressions;

namespace Common;

public static partial class ExtraEncoding
{
    public static Encoding GBK { get; }
    public static Encoding SJIS { get; }
    public static Regex DoubleByteCharReg = DoubleByteCharRegex();

    static ExtraEncoding()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        GBK = CodePagesEncodingProvider.Instance.GetEncoding("GBK") ?? throw new ArgumentException("error codepage");
        SJIS = CodePagesEncodingProvider.Instance.GetEncoding(932) ?? throw new ArgumentException("error codepage");
    }

    [GeneratedRegex("[\u00FF-\uffff]", RegexOptions.Multiline)]
    private static partial Regex DoubleByteCharRegex();
}