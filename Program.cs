using ProjectLawfulEbook.Book;
using ProjectLawfulEbook.Cache;
using ProjectLawfulEbook.Epub;

namespace ProjectLawfulEbook;

public static class Program
{
    public const string TITLE = "Project Lawful";
    public const string AUTHOR = "Eliezer Yudkowsky";
    public const string RELEASE = "2023-04-03";
    public const string LANGUAGE = "en";

    public static readonly bool INCLUDE_AVATAR_KEYWORDS = false;
    public static readonly bool TRY_INLINE_CHARACTER_NAME = true;
    public static readonly bool INCLUDE_AVATARS = false;
    public static readonly int MAX_POST_PER_FILE = 128;
    
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

        book.Cleanup();
        ConsoleWriteDelimiter();

        book.ParseParagraphs();
        ConsoleWriteDelimiter();

        
        var outDirEpub = Path.Combine(Environment.CurrentDirectory, "_out_epub");
        Directory.CreateDirectory(outDirEpub);
        
        var outDirHTML = Path.Combine(Environment.CurrentDirectory, "_out_html");
        if (Directory.Exists(outDirHTML)) Directory.Delete(outDirHTML, true);
        Directory.CreateDirectory(outDirHTML);
        
        book.Generate(new EpubWriter(outDirHTML, false));
        ConsoleWriteDelimiter();

        book.Generate(new EpubWriter(Path.Combine(outDirEpub, "project-lawful.zip"), true));
        ConsoleWriteDelimiter();

        book.Generate(new EpubWriter(Path.Combine(outDirEpub, "project-lawful.epub"), true));
        ConsoleWriteDelimiter();
        
        Console.WriteLine("Done.");
    }

    private static void ConsoleWriteDelimiter()
    {
        Console.WriteLine();
        Console.WriteLine(Enumerable.Repeat("-", 80).Aggregate((a,b)=>a+b));
        Console.WriteLine();
    }

    public static Guid ID_OPF()
    {
        
        var u = new Random(TITLE.GetHashCode() ^ AUTHOR.GetHashCode());
        var g = new byte[16];
        u.NextBytes(g);
        return new Guid(g);
    }

    public static Guid ID_CAL()
    {
        
        var u = new Random(TITLE.GetHashCode() ^ AUTHOR.GetHashCode());
        var g = new byte[16];
        u.NextBytes(g);
        u.NextBytes(g);
        return new Guid(g);
    }

}