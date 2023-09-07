using System.Collections.Immutable;
using ProjectLawfulEbook.Cache;

namespace ProjectLawfulEbook.Book;

public class PlanecrashBook
{
    private readonly GlowPubCache cache;

    private readonly List<Chapter> chapter = new List<Chapter>();
    
    public PlanecrashBook(GlowPubCache cache)
    {
        this.cache = cache;
    }

    public void Build()
    {
        var cidx = 0;
        
        chapter.Add(new Chapter(++cidx, "01.1", cache.Get(4582).TakeUntil(1721818, false), cache.Get(4582).Subject));
        chapter.Add(new Chapter(++cidx, "01.2", cache.Get(5310).TakeAll(), cache.Get(5310).Subject));
        chapter.Add(new Chapter(++cidx, "01.3", cache.Get(4582).TakeAfter(1721818, false), cache.Get(4582).Subject + " (cont.)"));
            
        chapter.Add(new Chapter(++cidx, "02", cache.Get(5504).TakeAll(), cache.Get(5504).Subject));
        
        chapter.Add(new Chapter(++cidx, "03", cache.Get(5506).TakeAll(), cache.Get(5506).Subject));
        
        chapter.Add(new Chapter(++cidx, "04.1", cache.Get(5508).TakeUntil(1756345, false), cache.Get(5508).Subject));
        chapter.Add(new Chapter(++cidx, "04.2", cache.Get(5610).TakeAll(), cache.Get(5610).Subject));
        chapter.Add(new Chapter(++cidx, "04.3", cache.Get(5508).TakeBetween(1756345, 1760768, false, true), cache.Get(5508).Subject + " (cont.)"));
        chapter.Add(new Chapter(++cidx, "04.4", cache.Get(5638).TakeAll(), cache.Get(5638).Subject));
        chapter.Add(new Chapter(++cidx, "04.5", cache.Get(5508).TakeAfter(1760768, false), cache.Get(5508).Subject + " (cont.)"));
        chapter.Add(new Chapter(++cidx, "04.6", cache.Get(5775).TakeAll(), "Sandbox: " + cache.Get(5775).Subject));
        chapter.Add(new Chapter(++cidx, "04.7", cache.Get(5778).TakeAll(), "Sandbox: " + cache.Get(5778).Subject));
        
        chapter.Add(new Chapter(++cidx, "05.1.1", cache.Get(5694).TakeUntil(1777291, false), cache.Get(5694).Subject));
        chapter.Add(new Chapter(++cidx, "05.2.1", cache.Get(5785).TakeUntil(1784214, true), cache.Get(5785).Subject));
        chapter.Add(new Chapter(++cidx, "05.2.2", cache.Get(5826).TakeAll(), cache.Get(5826).Subject));
        chapter.Add(new Chapter(++cidx, "05.2.3", cache.Get(5785).TakeAfter(1784214, false), cache.Get(5785).Subject + " (cont.)"));
        chapter.Add(new Chapter(++cidx, "05.3.1", cache.Get(5694).TakeBetween(1777291, 1786765, false, false), cache.Get(5694).Subject + " (cont.)"));
        chapter.Add(new Chapter(++cidx, "05.4.1", cache.Get(5864).TakeAll(), cache.Get(5864).Subject));
        chapter.Add(new Chapter(++cidx, "05.5.1", cache.Get(5694).TakeAfter(1786765, false), cache.Get(5694).Subject));
        chapter.Add(new Chapter(++cidx, "05.6.1", cache.Get(5880).TakeAll(), "Sandbox: " + cache.Get(5880).Subject));

        chapter.Add(new Chapter(++cidx, "06.1", cache.Get(5930).TakeAll(), cache.Get(5930).Subject));
        chapter.Add(new Chapter(++cidx, "06.2", cache.Get(6029).TakeAll(), "Sandbox: " + cache.Get(6029).Subject));
        
        chapter.Add(new Chapter(++cidx, "07", cache.Get(5977).TakeAll(), cache.Get(5977).Subject));
        
        chapter.Add(new Chapter(++cidx, "08.1", cache.Get(6075).TakeAll(), cache.Get(6075).Subject));
        chapter.Add(new Chapter(++cidx, "08.2", cache.Get(6124).TakeAll(), "Sandbox: " + cache.Get(6124).Subject));
        
        chapter.Add(new Chapter(++cidx, "09", cache.Get(6131).TakeAll(), cache.Get(6131).Subject));

        chapter.Add(new Chapter(++cidx, "10", cache.Get(6132).TakeAll(), cache.Get(6132).Subject));

        chapter.Add(new Chapter(++cidx, "11", cache.Get(6334).TakeAll(), cache.Get(6334).Subject));

        chapter.Add(new Chapter(++cidx, "12", cache.Get(6480).TakeAll(), cache.Get(6480).Subject));

        chapter.Add(new Chapter(++cidx, "13", cache.Get(6827).TakeAll(), cache.Get(6827).Subject));

        // -------------------------------------------------------------------------------------------------------------

        foreach (var c in chapter)
        {
            Console.WriteLine($"[{c.Identifier}] {c.Title}");
        }

        SanityCheck();

        //TODO test if all posts are handled (and no dups)
    }

    private void SanityCheck()
    {
        var d = new Dictionary<int, int>();

        foreach (var thread in cache.List())
        {
            foreach (var post in thread.TakeAll())
            {
                if (d.ContainsKey(post.ID)) throw new Exception();
                d.Add(post.ID, 0);
            }
        }

        foreach (var post in chapter.SelectMany(cptr => cptr.Posts)) d[post.ID] += 1;
        
        Console.WriteLine();

        foreach (var entry in d)
        {
            if (entry.Value == 0)
            {
                TODO    
            } 
            else if (entry.Value > 1)
            {
                TODO
            }
        }
        
        Console.WriteLine();
        
        throw new NotImplementedException();
    }
}