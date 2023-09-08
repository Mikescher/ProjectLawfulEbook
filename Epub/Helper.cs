using HtmlAgilityPack;

namespace ProjectLawfulEbook.Epub;

public class Helper
{
    
    public static string Filenamify(string v, bool repl = false)
    {
        var s = new String(v.Replace((char)160, ' ').ToCharArray().Where(p =>
            (p >= '0' && p <= '9') || 
            (p >= 'A' && p <= 'Z') || 
            (p >= 'a' && p <= 'z') || 
            p == ' ' || 
            p == '.' ||
            p == '-' ||
            p == '*' ||
            p == '_' ||
            p == '.' ||
            p == ',').ToArray());

        if (repl) s = s.Replace(' ', '_');
			
        return s;
    }

    public static string TitleFmt(string raw)
	{
		raw = HtmlEntity.DeEntitize(raw);

		raw = raw.Replace('–', '-');
		raw = raw.Replace((char)160, ' ');

		raw = raw.Trim().Trim('-', ':', '_', '#').Trim();
		if (raw.ToLower().StartsWith("tde")) raw = raw.Substring(3);

		raw = raw.Trim().Trim('-', ':', '_', '#').Trim();

		if (raw.Length >= 2) raw = char.ToUpper(raw[0]) + raw.Substring(1);

		return raw;
	}

    public static string Striptease(HtmlNode raw)
	{
		{
			var rm = raw.SelectNodes(@"//script");
			if (rm != null && rm.Any())
			{
				var copy = HtmlNode.CreateNode($"<{raw.Name}></{raw.Name}>");
				copy.CopyFrom(raw);
				raw = copy;

				rm = raw.SelectNodes(@"//script");
				if (rm != null) foreach (var e in rm) e.Remove();
			}
		}

		{
			var rm = raw.SelectNodes(@"//meta");
			if (rm != null && rm.Any())
			{
				var copy = HtmlNode.CreateNode($"<{raw.Name}></{raw.Name}>");
				copy.CopyFrom(raw);
				raw = copy;

				rm = raw.SelectNodes(@"//meta");
				if (rm != null) foreach (var e in rm) e.Remove();
			}
		}

		return Striptease(HtmlEntity.DeEntitize(raw.InnerText));
	}

    public static string Striptease(string raw)
	{
		var r = string.Join(string.Empty,
			raw
			.ToCharArray()
			.Select(c => char.IsWhiteSpace(c) ? ' ' : c)
			.Where(c => char.IsLetterOrDigit(c) ||char.IsWhiteSpace(c))
			.Select(c => char.ToLower(c))).Trim();
		return r;
	}

    public static string CombineAuthority(string url, string suffix)
	{
		var left = new Uri(url).GetLeftPart(UriPartial.Authority);
		if (!left.EndsWith("/")) left = left + "/";
		if (suffix.StartsWith("/")) suffix = suffix.TrimStart('/');
		return left + suffix;
	}

    public static string CombineUri(string uri1, string uri2)
	{
		if (uri1.Contains("/")) uri1 = uri1.Substring(0, uri1.LastIndexOf("/"));
		uri1 = uri1.TrimEnd('/');
		uri2 = uri2.TrimStart('/');
		return string.Format("{0}/{1}", uri1, uri2);
	}

    public static IEnumerable<HtmlNode> RecursiveDescendants(HtmlNode n)
    {
	    foreach (var d1 in n.Descendants())
	    {
		    yield return d1;
		    foreach (var d2 in RecursiveDescendants(d1)) yield return d2;
	    }
    }
}