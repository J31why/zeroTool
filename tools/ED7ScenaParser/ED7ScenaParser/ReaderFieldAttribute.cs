namespace ED7ScenaParser;

[AttributeUsage(AttributeTargets.Field)]
public class ReaderFieldAttribute(int length) : Attribute
{
    public int Length { get; }= length;
}
[AttributeUsage(AttributeTargets.Field)]
public class ReaderFieldIgnoreAttribute: Attribute
{
}
