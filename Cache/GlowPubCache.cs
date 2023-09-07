using System.Collections;

namespace ProjectLawfulEbook.Cache;

public class GlowPubCache
{
    private Dictionary<int, CacheThread> _threads = new Dictionary<int, CacheThread>();
    
    public GlowPubCache()
    {
        
    }

    public void Load()
    {
        foreach (var direc in Directory.EnumerateDirectories("glowpub_cache/posts"))
        {
            var id = int.Parse(Path.GetFileName(direc)!);
            var thread = CacheThread.Load(id);
            _threads.Add(id, thread);
            Console.WriteLine($"Loaded thread {thread.ID} - {thread.Subject}");
        }
    }

    public CacheThread Get(int id)
    {
        return _threads[id]!;
    }

    public IEnumerable<CacheThread> List()
    {
        return _threads.Values.ToList();
    }
}