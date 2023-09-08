using ProjectLawfulEbook.Cache;
using ProjectLawfulEbook.Epub;

namespace ProjectLawfulEbook.Book;

public class PlanecrashBook
{
    private readonly GlowPubCache cache;

    private readonly List<Chapter> chapters = new List<Chapter>();
    
    public PlanecrashBook(GlowPubCache cache)
    {
        this.cache = cache;
    }

    public void Build()
    {
        var cidx = 0;
        
        chapters.Add(new Chapter(++cidx, "1.1", cache.Get(4582).TakeUntil(1721818, false), cache.Get(4582).Subject));
        chapters.Add(new Chapter(++cidx, "1.2", cache.Get(5310).TakeAll(), cache.Get(5310).Subject));
        chapters.Add(new Chapter(++cidx, "1.3", cache.Get(4582).TakeAfter(1721818, false), cache.Get(4582).Subject + " (cont.)"));
            
        chapters.Add(new Chapter(++cidx, "2", cache.Get(5504).TakeAll(), cache.Get(5504).Subject));
        
        chapters.Add(new Chapter(++cidx, "3", cache.Get(5506).TakeAll(), cache.Get(5506).Subject));
        
        chapters.Add(new Chapter(++cidx, "4.1", cache.Get(5508).TakeUntil(1756345, false), cache.Get(5508).Subject));
        chapters.Add(new Chapter(++cidx, "4.2", cache.Get(5610).TakeAll(), cache.Get(5610).Subject));
        chapters.Add(new Chapter(++cidx, "4.3", cache.Get(5508).TakeBetween(1756345, 1760768, false, true), cache.Get(5508).Subject + " (cont.)"));
        chapters.Add(new Chapter(++cidx, "4.4", cache.Get(5638).TakeAll(), cache.Get(5638).Subject));
        chapters.Add(new Chapter(++cidx, "4.5", cache.Get(5508).TakeAfter(1760768, false), cache.Get(5508).Subject + " (cont.)"));
        chapters.Add(new Chapter(++cidx, "4.6", cache.Get(5775).TakeAll(), "Sandbox: " + cache.Get(5775).Subject));
        chapters.Add(new Chapter(++cidx, "4.7", cache.Get(5778).TakeAll(), "Sandbox: " + cache.Get(5778).Subject));
        
        chapters.Add(new Chapter(++cidx, "5.1.1", cache.Get(5694).TakeUntil(1777291, false), cache.Get(5694).Subject));
        chapters.Add(new Chapter(++cidx, "5.2.1", cache.Get(5785).TakeUntil(1784214, true), cache.Get(5785).Subject));
        chapters.Add(new Chapter(++cidx, "5.2.2", cache.Get(5826).TakeAll(), cache.Get(5826).Subject));
        chapters.Add(new Chapter(++cidx, "5.2.3", cache.Get(5785).TakeAfter(1784214, false), cache.Get(5785).Subject + " (cont.)"));
        chapters.Add(new Chapter(++cidx, "5.3.1", cache.Get(5694).TakeBetween(1777291, 1786765, false, false), cache.Get(5694).Subject + " (cont.)"));
        chapters.Add(new Chapter(++cidx, "5.4.1", cache.Get(5864).TakeAll(), cache.Get(5864).Subject));
        chapters.Add(new Chapter(++cidx, "5.5.1", cache.Get(5694).TakeAfter(1786765, false), cache.Get(5694).Subject));
        chapters.Add(new Chapter(++cidx, "5.6.1", cache.Get(5880).TakeAll(), "Sandbox: " + cache.Get(5880).Subject));

        chapters.Add(new Chapter(++cidx, "6.1", cache.Get(5930).TakeAll(), cache.Get(5930).Subject));
        chapters.Add(new Chapter(++cidx, "6.2", cache.Get(6029).TakeAll(), "Sandbox: " + cache.Get(6029).Subject));
        
        chapters.Add(new Chapter(++cidx, "7", cache.Get(5977).TakeAll(), cache.Get(5977).Subject));
        
        chapters.Add(new Chapter(++cidx, "8.1", cache.Get(6075).TakeAll(), cache.Get(6075).Subject));
        chapters.Add(new Chapter(++cidx, "8.2", cache.Get(6124).TakeAll(), "Sandbox: " + cache.Get(6124).Subject));
        
        chapters.Add(new Chapter(++cidx, "9", cache.Get(6131).TakeAll(), cache.Get(6131).Subject));

        chapters.Add(new Chapter(++cidx, "10", cache.Get(6132).TakeAll(), cache.Get(6132).Subject));

        chapters.Add(new Chapter(++cidx, "11", cache.Get(6334).TakeAll(), cache.Get(6334).Subject));

        chapters.Add(new Chapter(++cidx, "12", cache.Get(6480).TakeAll(), cache.Get(6480).Subject));

        chapters.Add(new Chapter(++cidx, "13", cache.Get(6827).TakeAll(), cache.Get(6827).Subject));
        
        Console.WriteLine($"Created {cidx} chapter objects");
    }

    public void PrintChapters()
    {
        foreach (var c in chapters)
        {
            Console.WriteLine($"[{c.Identifier}] {c.Title}");
        }
    }
    
    public void SanityCheck()
    {
        var d = new Dictionary<string, int>();

        foreach (var thread in cache.List())
        {
            if (thread.ID == "5403") continue; // unused: sfw tldr kissing is not a human universal
            if (thread.ID == "5521") continue; // unused: tldr some human relationships
            if (thread.ID == "5618") continue; // unused: sfw tldr cheating is cuddleroom technique
            if (thread.ID == "5671") continue; // unused: sfw tldr we could have been trade partners
            
            foreach (var post in thread.TakeAll())
            {
                if (post.ID == "1721818") continue; // unused: -> kissing is not a human universal
                if (post.ID == "1756345") continue; // unused: -> cheating is cuddleroom technique
                if (post.ID == "1777291") continue; // unused: -> to Hell with SCIENCE!
                if (post.ID == "1786765") continue; // unused: -> the alien maths of dath ilan

                if (d.ContainsKey(post.ID)) throw new Exception($"Post {post.ID} exists in multiple threads");
                d.Add(post.ID, 0);
            }
        }

        foreach (var post in chapters.SelectMany(cptr => cptr.Posts))
        {
            if (!d.ContainsKey(post.ID)) throw new Exception($"Post {post.ID} exists in chapter but not in cache");
            d[post.ID] += 1;
        }
        
        foreach (var entry in d)
        {
            if (entry.Value == 0)
            {
                Console.WriteLine($"Unused Chapter: {entry.Key}");
            } 
            else if (entry.Value > 1)
            {
                Console.WriteLine($"Multiple Chapter: {entry.Key} [{entry.Value}]");
            }
        }
        
        Console.WriteLine("Sanity check finished");
    }

    public void ParseParagraphs()
    {
        foreach (var chptr in chapters) chptr.ParseParagraphs();
        Console.WriteLine("Parsed Paragraphs");
    }

    public void Cleanup()
    {
        foreach (var chptr in chapters) chptr.Cleanup();
        Console.WriteLine("Cleaned HTML");
    }

    public void CacheImages()
    {
        foreach (var chptr in chapters) chptr.CacheImages();
        Console.WriteLine("Cached Images");
    }
    
    public void Generate(EpubWriter writer)
    {
        try
        {
            writer.Open();

            writer.WriteMimeType();
            writer.WriteContainerXML();
            writer.WriteContentOPF(chapters);
            writer.WriteTOC(chapters);

            writer.WriteCover();
            
            foreach (var chptr in chapters)
            {
                writer.WriteChapter(chptr);
            }

            foreach (var f in Directory.EnumerateFiles(Path.Combine(Environment.CurrentDirectory, "image_cache")))
            {
                writer.WriteBin(@"OEBPS\Images\"+Path.GetFileName(f), File.ReadAllBytes(f));
            }
        }
        finally
        {
            writer.Save();
            Console.WriteLine("Generated book to: " + writer.Destination);
        }
    }

    public void PrintIconKeywordsList()
    {
        var keywords = chapters
            .SelectMany(p => p.Posts)
            .Select(p => p.IconKeyword)
            .Where(p => p != null)
            .Distinct()
            .OrderBy(p => p)
            .ToList();

        foreach (var kw in keywords) Console.WriteLine(kw);
    }
}