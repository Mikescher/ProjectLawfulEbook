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

    public void ParseParagraphs()
    {
        foreach (var post in Posts) post.ParseParagraphs();
    }

    public string EpubFilename()
    {
        return string.Format("{0:000}_{1}.html", Order, Uri.EscapeDataString(Helper.Filenamify(Title, true)));
    }

    public string EpubID()
    {
        return string.Format("x{0:000}_{1}.html", Order, Uri.EscapeDataString(Helper.Filenamify(Title, true).Replace(".", "")));
    }

    public string GetEpubHTML()
    {
        var xml = new StringBuilder();
        
        xml.AppendLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
        xml.AppendLine(@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.1//EN"" ""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"" > ");
        xml.AppendLine(@"<html xmlns=""http://www.w3.org/1999/xhtml"">");
        xml.AppendLine(@"<head>");
        xml.AppendLine("<title>" + Identifier +  " - " + HtmlEntity.Entitize(Title) + "</title>");
        xml.AppendLine(@"</head>");
        xml.AppendLine(@"<body>");
        xml.AppendLine("<h1>" + Identifier +  " - " + HtmlEntity.Entitize(Title) + "</h1>");

        foreach (var post in Posts)
        {
            xml.AppendLine(post.GetEpubHTML());
            xml.AppendLine();
            xml.AppendLine("<br/>");
            xml.AppendLine();
        }
        
        xml.AppendLine(@"</body>");
        xml.AppendLine(@"</html>");

        return xml.ToString();
    }
}