using ProjectLawfulEbook.Cache;

namespace ProjectLawfulEbook;

public static class Program
{
    public static void Main()
    {
        var cache = new GlowPubCache();
        cache.Load();
    }
}