using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using ProjectLawfulEbook.Epub;

namespace ProjectLawfulEbook.Book;

public class Chapter
{
    public readonly string ThreadID;
    public readonly int Order;
    public readonly string Identifier;
    public readonly IReadOnlyList<Reply> Posts;
    public readonly IReadOnlyDictionary<string, int> PostIndizes;
    public readonly string Title;
    
    public Chapter(int order, string identifier, IEnumerable<Reply> posts, string title)
    {
        Order = order;
        Identifier = identifier;
        Posts = posts.ToImmutableList();
        Title = title;
        ThreadID = Posts[0].ParentThreadID;

        PostIndizes = Posts.Select((p, i) => (p, i)).ToDictionary(p => p.p.ID, p => p.i).ToImmutableDictionary();
    }

    public string EpubFilename(int split)
    {
        return string.Format("{0:000}_{1:0000}_{2}.html", Order, split, Uri.EscapeDataString(Helper.Filenamify(Title, true).Replace(".", "")));
    }

    public string EpubID(int split)
    {
        return string.Format("xid_{0:000}_{1:0000}_{2}", Order, split, Uri.EscapeDataString(Helper.Filenamify(Title, true).Replace(".", "")));
    }

    public int GetSplitCount(Options opts)
    {
        return (int)Math.Ceiling(this.Posts.Count / (opts.MAX_POST_PER_FILE * 1.0));
    }
    
    public string GetEpubHTML(int split, Options opts)
    {
        var xml = new StringBuilder();
        
        xml.AppendLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
        xml.AppendLine(@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.1//EN"" ""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"" > ");
        xml.AppendLine(@"<html xmlns=""http://www.w3.org/1999/xhtml"">");
        xml.AppendLine(@"<head>");
        xml.AppendLine(@"<meta name=""ProjectLawfulEbook_source"" content=""" + "https://github.com/Mikescher/ProjectLawfulEbook" + @"""/>");
        xml.AppendLine(@"<meta name=""ProjectLawfulEbook_homepage"" content=""" + "https://www.mikescher.com/" + @"""/>");
        xml.AppendLine(@"<meta name=""ProjectLawfulEbook_version"" content=""" + Program.PLE_VERSION + @"""/>");
        xml.AppendLine(@"<meta name=""ProjectLawfulEbook_commit"" content=""" + Program.PLE_COMMIT + @"""/>");
        xml.AppendLine(@"<meta name=""ProjectLawfulEbook_date"" content=""" + DateTime.Now.ToString("yyyy-MM-dd") + @"""/>");
        xml.AppendLine("<title>" + HtmlEntity.Entitize(Identifier) +  " - " + HtmlEntity.Entitize(Title) + "</title>");
        xml.AppendLine(@"<link href=""../stylesheet.css"" type=""text/css"" rel=""stylesheet""/>");
        xml.AppendLine(@"</head>");
        xml.AppendLine(@"<body>");

        if (split == 0) xml.AppendLine("<h1>" + HtmlEntity.Entitize(Identifier) +  " - " + HtmlEntity.Entitize(Title) + "</h1>");
        
        foreach (var post in Posts.Skip(split * opts.MAX_POST_PER_FILE).Take(opts.MAX_POST_PER_FILE))
        {
            xml.AppendLine();
            xml.AppendLine("<!-- [ " + post.ParentThreadID + " / " + post.ID + " ] -->");
            xml.AppendLine();

            xml.Append(post.GetEpubHTML(opts));
            
            xml.AppendLine();
            xml.AppendLine("<br/>");
            xml.AppendLine();
        }
        
        xml.AppendLine(@"</body>");
        xml.AppendLine(@"</html>");

        return xml.ToString();
    }

    public string PatchLinks(string html, List<Chapter> allChapters, Options opts)
    {
        return Regex.Replace(html, @"<a(?<sep>[^>]+)href=""(?<href>[^""]+)""", (Match m) =>
        {
            var full = m.Value;
            var midfix = m.Groups["sep"].Value;
            var href = m.Groups["href"].Value;

            var m0 = Regex.Match(href, @"^/replies/(?<id>[0-9]+)#reply-\1$");
            if (m0.Success)
            {
                var postid = m0.Groups["id"].Value;
                
                var result = allChapters
                    .Where(p => p.ContainsPost(postid))
                    .Select(p => p.EpubFilename(p.GetSplitOfPost(postid, opts)) + "#reply-" + postid)
                    .FirstOrDefault();

                if (result == null)
                {
                    Console.WriteLine("[!!!] Failed to patch internal href: " + href);
                    return full;
                }

                Console.WriteLine($"[~] Patch internal link from '{href}' to '{result}'");

                return "<a" + midfix + "href=\"" + result + "\"";
            }

            var m1 = Regex.Match(href, @"^/posts/(?<id>[0-9]+)$");
            if (m1.Success)
            {
                var threadid = m1.Groups["id"].Value;
                
                var result = allChapters
                    .Where(p => p.ThreadID == threadid)
                    .Select(p => p.EpubFilename(0))
                    .FirstOrDefault();

                if (result == null)
                {
                    Console.WriteLine("[!!!] Failed to patch internal href: " + href);
                    return full;
                }
                
                Console.WriteLine($"[~] Patch internal link from '{href}' to '{result}'");

                return "<a" + midfix + "href=\"" + result + "\"";
            }

            if (href.StartsWith("https://en.wikipedia.org/")) return full;
            if (href.StartsWith("https://en.m.wikipedia.org")) return full;
            if (href.StartsWith("https://www.lesswrong.com")) return full;
            if (href.StartsWith("https://twitter.com")) return full;
            if (href.StartsWith("https://www.lewissociety.org")) return full;
            if (href.StartsWith("https://grabbyaliens.com")) return full;
            if (href.StartsWith("https://hraf.yale.edu")) return full;
            if (href.StartsWith("https://www.aonprd.com/")) return full;
            if (href.StartsWith("https://sndup.net/q2rc")) return full;
            if (href.StartsWith("https://docs.google.com")) return full;
            if (href.StartsWith("https://discord.gg")) return full;
            if (href.StartsWith("https://discord.com")) return full;
            if (href.StartsWith("https://www.d20pfsrd.com")) return full;
            if (href.StartsWith("https://moral-autism.tumblr.com")) return full;
            if (href.StartsWith("https://www.facebook.com")) return full;
            if (href.StartsWith("https://arbital.com")) return full;
            if (href.StartsWith("https://www.youtube.com")) return full;
            if (href.StartsWith("http://consc.net")) return full;
            if (href.StartsWith("https://2.bp.blogspot.com")) return full;
            if (href.StartsWith("https://arxiv.org")) return full;
            if (href.StartsWith("https://www.google.com")) return full;
            if (href.StartsWith("https://www.flickr.com")) return full;
            if (href.StartsWith("https://www.readthesequences.com")) return full;
            if (href.StartsWith("https://www.pinterest.com")) return full;
            if (href.StartsWith("https://www.reddit.com")) return full;
            if (href.StartsWith("https://vocaroo.com")) return full;
            if (href.StartsWith("https://www.dsprelated.com")) return full;
            if (href.StartsWith("https://projectlawful.com")) return full;
            if (href.StartsWith("https://jjreeve.tumblr.com")) return full;
            if (href.StartsWith("https://www.goodreads.com")) return full;
            if (href.StartsWith("https://pathfinderwiki.com")) return full;
            if (href.StartsWith("https://imgur.com")) return full;
            if (href.StartsWith("https://paizo.com")) return full;
            if (href.StartsWith("")) return full;
            if (href.StartsWith("")) return full;
            if (href.StartsWith("")) return full;
            
            Console.WriteLine("[?] found unknown link - cannot patch: " + href);
            return full;
        });
    }

    private int GetSplitOfPost(string postid, Options opts)
    {
        return (int) Math.Floor(this.PostIndizes[postid] / (opts.MAX_POST_PER_FILE * 1.0));
    }

    private bool ContainsPost(string postid)
    {
        return this.PostIndizes.ContainsKey(postid);
    }
}