using System.Collections.Immutable;

namespace ProjectLawfulEbook.Book;

public class Chapter
{
    public readonly int Order;
    public readonly string Identifier;
    public readonly IReadOnlyList<Reply> Posts;
    public readonly string Title;
    
    public Chapter(int order, string identifier, IEnumerable<Reply> posts, string title)
    {
        Order = order;
        Identifier = identifier;
        Posts = posts.ToImmutableList();
        Title = title;
    }

    public void ParseParagraphs()
    {
        foreach (var post in Posts) post.ParseParagraphs();
    }
}