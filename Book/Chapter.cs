using System.Collections.Immutable;
using System.Text;
using HtmlAgilityPack;
using ProjectLawfulEbook.Epub;

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

    public void Cleanup()
    {
        foreach (var post in Posts) post.Cleanup();
    }

    public void ParseParagraphs()
    {
        foreach (var post in Posts) post.ParseParagraphs();
    }

    public void CacheImages()
    {
        foreach (var post in Posts) post.CacheImages();
    }

    public string EpubFilename(int split)
    {
        return string.Format("{0:000}_{1:0000}_{2}.html", Order, split, Uri.EscapeDataString(Helper.Filenamify(Title, true).Replace(".", "")));
    }

    public string EpubID(int split)
    {
        return string.Format("xid_{0:000}_{1:0000}_{2}", Order, split, Uri.EscapeDataString(Helper.Filenamify(Title, true).Replace(".", "")));
    }

    public int GetSplitCount()
    {
        return (int)Math.Ceiling(this.Posts.Count / (Program.MAX_POST_PER_FILE * 1.0));
    }
    
    public string GetEpubHTML(int split)
    {
        var xml = new StringBuilder();
        
        xml.AppendLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
        xml.AppendLine(@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.1//EN"" ""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"" > ");
        xml.AppendLine(@"<html xmlns=""http://www.w3.org/1999/xhtml"">");
        xml.AppendLine(@"<head>");
        xml.AppendLine("<title>" + HtmlEntity.Entitize(Identifier) +  " - " + HtmlEntity.Entitize(Title) + "</title>");
        xml.AppendLine(@"</head>");
        xml.AppendLine(@"<body>");

        if (split == 0) xml.AppendLine("<h1>" + HtmlEntity.Entitize(Identifier) +  " - " + HtmlEntity.Entitize(Title) + "</h1>");
        
        foreach (var post in Posts.Skip(split * Program.MAX_POST_PER_FILE).Take(Program.MAX_POST_PER_FILE))
        {
            xml.AppendLine();
            xml.AppendLine("<!-- [ " + post.ParentThreadID + " / " + post.ID + " ] -->");
            xml.AppendLine();

            xml.Append(post.GetEpubHTML());
            
            xml.AppendLine();
            xml.AppendLine("<br/>");
            xml.AppendLine();
        }
        
        xml.AppendLine(@"</body>");
        xml.AppendLine(@"</html>");

        return xml.ToString();
    }
}