using ProjectLawfulEbook.Book;
using ProjectLawfulEbook.Cache;

namespace ProjectLawfulEbook;

public static class Program
{
    public static void Main()
    {
        var cache = new GlowPubCache();
        cache.Load();
        ConsoleWriteDelimiter();
        
        var book = new PlanecrashBook(cache);
        book.Build();
        ConsoleWriteDelimiter();
        
        book.SanityCheck();
        ConsoleWriteDelimiter();

        book.PrintChapters();
        ConsoleWriteDelimiter();
    }

    private static void ConsoleWriteDelimiter()
    {
        Console.WriteLine();
        Console.WriteLine(Enumerable.Repeat("-", 80).Aggregate((a,b)=>a+b));
        Console.WriteLine();
    }
}