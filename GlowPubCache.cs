using Thread = ProjectLawfulEbook.Book.Thread;

namespace ProjectLawfulEbook;

public class GlowPubCache
{
    private readonly Dictionary<int, Thread> _threads = new();
    
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

    public void CacheImages()
    {
        foreach (var thread in _threads.Values) thread.CacheImages();
    }

    public void ParseParagraphs()
    {
        foreach (var thread in _threads.Values) thread.ParseParagraphs();
    }

    public void Reset()
    {
        foreach (var thread in _threads.Values) thread.Reset();
    }
}