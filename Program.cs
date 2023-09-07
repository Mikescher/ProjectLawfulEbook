using ProjectLawfulEbook.Book;
using ProjectLawfulEbook.Cache;

namespace ProjectLawfulEbook;

public static class Program
{
    public static void Main()
    {
        var cache = new GlowPubCache();
        cache.Load();

        Console.WriteLine();
        Console.WriteLine("--------");
        Console.WriteLine();
        
        var book = new PlanecrashBook(cache);
        book.Build();
    }
}