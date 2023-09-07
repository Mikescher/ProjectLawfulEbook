using System.Collections.Immutable;
using ProjectLawfulEbook.Cache;

namespace ProjectLawfulEbook.Book;

public class Chapter
{
    public readonly int Order;
    public readonly string Identifier;
    public readonly IReadOnlyList<CacheReply> Posts;
    public readonly string Title;
    
    public Chapter(int order, string identifier, IEnumerable<CacheReply> posts, string title)
    {
        Order = order;
        Identifier = identifier;
        Posts = posts.ToImmutableList();
        Title = title;
    }
}