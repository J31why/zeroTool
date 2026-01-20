namespace zCodec.Dats;

public static class DatCodecFactory
{
    private static readonly List<IDatCodec> Codecs = [];

    public static void Initialize(bool isAo)
    {
        Codecs.Add(new NameDat(isAo));
        Codecs.Add(new BookMemoDat(isAo));
        Codecs.Add(new IttxtDat(isAo));
        Codecs.Add(new TownDat(isAo));
        Codecs.Add(new ShopDat(isAo));
        Codecs.Add(new RecordDat(isAo));
        Codecs.Add(new QuestDat(isAo));
        Codecs.Add(new MagicDat(isAo));
        Codecs.Add(new FishDat(isAo));
        Codecs.Add(new ExvisDat(isAo));
        Codecs.Add(new ExmovDat(isAo));
        Codecs.Add(new ExchrDat(isAo));
        Codecs.Add(new DbmonDat(isAo));
        Codecs.Add(new CookDat(isAo));
        Codecs.Add(new MgameDat(isAo));
        Codecs.Add(new MstqrtDat());
        Codecs.Add(new StoryDat());
    }
    public static IDatCodec? Get(string file)
    {
        return Codecs.FirstOrDefault(x => x.CanDecompile(file));
    }
}