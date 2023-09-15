using ProjectLawfulEbook.Book;
using ProjectLawfulEbook.Epub;

namespace ProjectLawfulEbook;

public static class Program
{
    public static string PLE_COMMIT => GitHashAttribute.Get();
    public const string PLE_VERSION = "1.2";
    public const string TITLE = "Project Lawful";
    public const string AUTHOR_1 = "Eliezer Yudkowsky";
    public const string AUTHOR_2 = "Lintamande";
    public const string RELEASE = "2023-04-03";
    public const string LANGUAGE = "en";

    public static void Main()
    {
        Console.WriteLine(PLE_COMMIT);
        
        var outDirHTML = Path.Combine(Environment.CurrentDirectory, "_out_html");
        if (Directory.Exists(outDirHTML)) Directory.Delete(outDirHTML, true);
        Directory.CreateDirectory(outDirHTML);

        var outDirEpub = Path.Combine(Environment.CurrentDirectory, "_out_epub");
        Directory.CreateDirectory(outDirEpub);
        
        // ---------------------------------------------------------------------------------------------------
        
        var cache = new GlowPubCache();
        cache.Load();
        ConsoleWriteDelimiter();
        
        cache.CacheImages();
        Console.WriteLine("cache::cache-images");
        ConsoleWriteDelimiter();

        cache.ParseParagraphs();
        Console.WriteLine("cache::parse-paragraphs");
        ConsoleWriteDelimiter();
   
        //cache.PrintIconKeywordsList();
        //ConsoleWriteDelimiter();

        // ---------------------------------------------------------------------------------------------------
        
        MakeHTML(cache, outDirHTML,                                              new Options(false, true,  false, false, false, 128,     false, false, false, false));

        Make(cache, outDirEpub, "project-lawful-inline",                         new Options(false, true,  false, false, false, 128,     false, false, true,  true )); // <-
        Make(cache, outDirEpub, "project-lawful-biggerhtml",                     new Options(false, true,  false, false, false, 100_000, false, false, true,  true ));
        Make(cache, outDirEpub, "project-lawful-moreinfo",                       new Options(false, false, false, true,  false, 128,     false, false, false, false));
        Make(cache, outDirEpub, "project-lawful-avatars",                        new Options(false, true,  true,  false, false, 128,     false, false, true,  true )); // <-
        Make(cache, outDirEpub, "project-lawful-avatars-moreinfo",               new Options(false, false, true,  true , true,  128,     false, false, false, false));

        Make(cache, outDirEpub, "project-lawful-sfw-inline",                     new Options(false, true,  false, false, false, 128,     true,  false, true,  true ));
        Make(cache, outDirEpub, "project-lawful-sfw-biggerhtml",                 new Options(false, true,  false, false, false, 100_000, true,  false, true,  true ));
        Make(cache, outDirEpub, "project-lawful-sfw-moreinfo",                   new Options(false, false, false, true,  false, 128,     true,  false, false, false));
        Make(cache, outDirEpub, "project-lawful-sfw-avatars",                    new Options(false, true,  true,  false, false, 128,     true,  false, true,  true ));
        Make(cache, outDirEpub, "project-lawful-sfw-avatars-moreinfo",           new Options(false, false, true,  true,  true,  128,     true,  false, false, false));

        Make(cache, outDirEpub, "project-lawful-onlymainstory-inline",           new Options(false, true,  false, false, false, 128,     false, true,  true,  true ));
        Make(cache, outDirEpub, "project-lawful-onlymainstory-biggerhtml",       new Options(false, true,  false, false, false, 100_000, false, true,  true,  true ));
        Make(cache, outDirEpub, "project-lawful-onlymainstory-moreinfo",         new Options(false, false, false, true,  false, 128,     false, true,  false, false));
        Make(cache, outDirEpub, "project-lawful-onlymainstory-avatars",          new Options(false, true,  true,  false, false, 128,     false, true,  true,  true ));
        Make(cache, outDirEpub, "project-lawful-onlymainstory-avatars-moreinfo", new Options(false, false, true,  true,  true,  128,     false, true,  false, false));

        // ---------------------------------------------------------------------------------------------------

        Console.WriteLine("Done.");
    }

    private static void Make(GlowPubCache cache, string outDirEpub, string fn, Options opts)
    {
        Console.WriteLine();
        Console.WriteLine(Enumerable.Repeat("=", 80).Aggregate((a,b)=>a+b));
        Console.WriteLine("    GENERATE " + fn);
        Console.WriteLine(Enumerable.Repeat("=", 80).Aggregate((a,b)=>a+b));
        Console.WriteLine();

        cache.Reset();
        
        var book = new PlanecrashBook(cache);
        book.Build(opts);
        ConsoleWriteDelimiter();
        
        book.SanityCheck(opts);
        ConsoleWriteDelimiter();

        book.PrintChapters();
        ConsoleWriteDelimiter();

        //book.Generate(new EpubWriter(Path.Combine(outDirEpub, fn + ".zip"), true), opts);
        //ConsoleWriteDelimiter();

        book.Generate(new EpubWriter(Path.Combine(outDirEpub, fn + ".epub"), true), opts);
    }

    private static void MakeHTML(GlowPubCache cache, string outDir, Options opts)
    {
        Console.WriteLine();
        Console.WriteLine(Enumerable.Repeat("=", 80).Aggregate((a,b)=>a+b));
        Console.WriteLine("    GENERATE HTML");
        Console.WriteLine(Enumerable.Repeat("=", 80).Aggregate((a,b)=>a+b));
        Console.WriteLine();

        cache.Reset();
        
        var book = new PlanecrashBook(cache);
        book.Build(opts);
        ConsoleWriteDelimiter();
        
        book.SanityCheck(opts);
        ConsoleWriteDelimiter();

        book.PrintChapters();
        ConsoleWriteDelimiter();

        book.Generate(new EpubWriter(outDir, false), opts);
    }

    private static void ConsoleWriteDelimiter()
    {
        Console.WriteLine();
        Console.WriteLine(Enumerable.Repeat("-", 80).Aggregate((a,b)=>a+b));
        Console.WriteLine();
    }

    public static Guid ID_OPF()
    {
        return Guid.Parse("f02a094e-fb62-fc76-a16f-cbf620b5cb96");
    }

    public static Guid ID_CAL()
    {
        return Guid.Parse("a703750e-326b-50d5-78f7-10c38457500e");
    }

}