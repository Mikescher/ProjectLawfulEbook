using System.Collections;
using Thread = ProjectLawfulEbook.Book.Thread;

namespace ProjectLawfulEbook.Cache;

public class GlowPubCache
{
    private Dictionary<int, Thread> _threads = new Dictionary<int, Thread>();
    
    public GlowPubCache()
    {
        
    }

    public void Load()
    {
        foreach (var direc in Directory.EnumerateDirectories("glowpub_cache/posts"))
        {
            var id = int.Parse(Path.GetFileName(direc)!);
            var thread = Thread.Load(id);
            _threads.Add(id, thread);
            Console.WriteLine($"Loaded thread {thread.ID} - {thread.Subject}");
        }
    }

    public Thread Get(int id)
    {
        return _threads[id]!;
    }

    public IEnumerable<Thread> List()
    {
        return _threads.Values.ToList();
    }
}