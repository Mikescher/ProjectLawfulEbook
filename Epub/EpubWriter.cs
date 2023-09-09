using System.Text;
using System.Xml.Linq;
using Ionic.Zip;
using ProjectLawfulEbook.Book;

namespace ProjectLawfulEbook.Epub;

public class EpubWriter
{
    public readonly string Destination;
    public readonly bool WriteEpub;

    private FileStream? _fs;
    private ZipOutputStream? _zipstream;
    
    public EpubWriter(string dest, bool epubfile)
    {
        Destination = dest;
        WriteEpub = epubfile;
    }

    public void Open()
    {
        if (WriteEpub)
        {
	        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            
	        if (File.Exists(Destination)) File.Delete(Destination);
	        
            _fs = File.Open(Destination, FileMode.Create, FileAccess.ReadWrite);
            _zipstream = new ZipOutputStream(_fs);
        }
        else
        {
            // nothing
        }
    }

    public void Save()
    {
        if (WriteEpub)
        {
            _zipstream?.Close();
            _fs?.Close();
        }
    }
    
    private void WritePubString(string n, string c, bool deflate, Encoding? e = null)
    {
        if (WriteEpub)
        {
            var f = _zipstream!.PutNextEntry(n);
            f.CompressionLevel = deflate ? Ionic.Zlib.CompressionLevel.BestCompression : Ionic.Zlib.CompressionLevel.None;

            byte[] buffer = (e ?? Encoding.UTF8).GetBytes(c);
            _zipstream.Write(buffer, 0, buffer.Length);
        }
        else
        {
            var dst = Path.Combine(Destination, n.Replace("\\", "/"));
            Directory.CreateDirectory(Path.GetDirectoryName(dst)!);
            File.WriteAllBytes(dst, (e ?? Encoding.UTF8).GetBytes(c));
        }
    }

    public void WriteBin(string fn, byte[] bin)
    {
	    if (WriteEpub)
	    {
		    var f = _zipstream!.PutNextEntry(fn);
		    f.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
		    
		    _zipstream.Write(bin, 0, bin.Length);
	    }
	    else
	    {
		    var dst = Path.Combine(Destination, fn.Replace("\\", "/"));
		    Directory.CreateDirectory(Path.GetDirectoryName(dst)!);
		    File.WriteAllBytes(dst, bin);
	    }
    }

    public void WriteMimeType()
    {
        WritePubString(@"mimetype", GetEpubMimetype(), false);
    }

    public void WriteContainerXML()
    {
        WritePubString(@"META-INF\container.xml", GetEpubContainerXML(), false);
    }
    
    public void WriteContentOPF(List<Chapter> chapters, Options opts)
    {
	    WritePubString(@"OEBPS\content.opf", GetEpubContentOPF(chapters, opts), false);
    }

    public void WriteTOC(List<Chapter> chapters)
    {
	    WritePubString(@"OEBPS\toc.ncx", GetEpubTOC(chapters), false);
    }

    private string GetEpubMimetype()
    {
        return "application/epub+zip";
    }

    private string GetEpubContainerXML()
    {
        var doc = new XDocument(new XDeclaration("1.0", "UTF-8", null),
            new XElement(XName.Get("container", "urn:oasis:names:tc:opendocument:xmlns:container"),
                new XAttribute("version", "1.0"),
                new XElement(XName.Get("rootfiles", "urn:oasis:names:tc:opendocument:xmlns:container"),
                    new XElement(XName.Get("rootfile", "urn:oasis:names:tc:opendocument:xmlns:container"),
                        new XAttribute("full-path", "OEBPS/content.opf"),
                        new XAttribute("media-type", "application/oebps-package+xml")))));

        using var writer = new Utf8StringWriter();
        
        doc.Save(writer);
        var r = writer.ToString();
        r = r.Replace("encoding=\"utf-8\"", "encoding=\"UTF-8\"");
        return r.Trim() + "\r\n";
    }

    private string GetEpubContentOPF(List<Chapter> chapters, Options opts)
	{
		XNamespace dc = "http://purl.org/dc/elements/1.1/";
		XNamespace opf = "http://www.idpf.org/2007/opf";

		var doc = new XDocument(new XDeclaration("1.0", "UTF-8", null));

		var package = new XElement(opf + "package",
						new XAttribute("unique-identifier", "BookId"),
						new XAttribute("version", "2.0"));

		doc.Add(package);

		var meta = new XElement(opf + "metadata",
						new XAttribute(XNamespace.Xmlns + "dc", dc),
						new XAttribute(XNamespace.Xmlns + "opf", opf),
						new XElement(dc + "title", Program.TITLE),
						new XElement(dc + "creator", Program.AUTHOR),
						new XElement(dc + "identifier",
							new XAttribute("id", "BookId"),
							new XAttribute(opf + "scheme", "UUID"),
							"urn:uuid:" + Program.ID_OPF().ToString("D")),
						new XElement(dc + "date",
							new XAttribute(opf + "event", "publication"),
							Program.RELEASE),
						new XElement(dc + "date",
							new XAttribute(opf + "event", "modification"),
							DateTime.Now.ToString("yyyy'-'MM'-'dd")),
						new XElement(dc + "date",
							new XAttribute(opf + "event", "creation"),
							DateTime.Now.ToString("yyyy'-'MM'-'dd")),
						new XElement(dc + "language", Program.LANGUAGE),
						new XElement(dc + "identifier",
							new XAttribute(opf + "scheme", "UUID"),
							Program.ID_CAL().ToString("D")),
						new XElement(opf + "meta",
							new XAttribute("content", "1.0"),
							new XAttribute("name", "Wordpress_eBook_scraper_version")),
						new XElement(opf + "meta",
							new XAttribute("content", DateTime.Now.ToString("yyyy-MM-dd")),
							new XAttribute("name", "ProjectLawfulEbook_cdate")),
						new XElement(opf + "meta",
							new XAttribute("content", "cover"),
							new XAttribute("name", "cover")));

		package.Add(meta);

		var manifest = new XElement(opf + "manifest");
		
		manifest.Add(new XElement(opf + "item",
			new XAttribute("href", "Text/000_titlepage.html"),
			new XAttribute("id", "titlepage"),
			new XAttribute("media-type", "application/xhtml+xml")));
		
		foreach (var chtr in chapters)
		{
			for (var i = 0; i < chtr.GetSplitCount(opts); i++)
			{
				manifest.Add(new XElement(opf + "item",
					new XAttribute("href", "Text/" + chtr.EpubFilename(i)),
					new XAttribute("id", chtr.EpubID(i)),
					new XAttribute("media-type", "application/xhtml+xml")));
			}
		}
		foreach (var f in Directory.EnumerateFiles(Path.Combine(Environment.CurrentDirectory, "image_cache")))
		{
			var mime = "?";
			if (f.EndsWith(".png")) mime = "image/png";
			else if (f.EndsWith(".jpg")) mime = "image/jpeg";
			else if (f.EndsWith(".jpeg")) mime = "image/jpeg";
			else if (f.EndsWith(".gif")) mime = "image/gif";
			else if (f.EndsWith(".webp")) mime = "image/webp";
			else Console.WriteLine("[!!!] Failed to get mimetype of file: " + f);
			
			manifest.Add(new XElement(opf + "item",
				new XAttribute("href", "Images/" + Path.GetFileName(f)),
				new XAttribute("id", "img_" + Helper.Filenamify(Path.GetFileName(f), true).Replace(".", "")),
				new XAttribute("media-type", mime)));
		}

		if (opts.INCLUDE_AVATARS)
		{
			foreach (var imgfn in Directory.EnumerateFiles(Path.Combine(Environment.CurrentDirectory, "glowpub_cache", "images")))
			{
				var mime = "?";
				if (imgfn.EndsWith(".png")) mime = "image/png";
				else if (imgfn.EndsWith(".jpg")) mime = "image/jpeg";
				else if (imgfn.EndsWith(".jpeg")) mime = "image/jpeg";
				else if (imgfn.EndsWith(".gif")) mime = "image/gif";
				else if (imgfn.EndsWith(".webp")) mime = "image/webp";
				else Console.WriteLine("[!!!] Failed to get mimetype of file: " + imgfn);
				
				manifest.Add(new XElement(opf + "item",
					new XAttribute("href", "Avatars/" + Path.GetFileName(imgfn)),
					new XAttribute("id", "ava_" + Path.GetFileNameWithoutExtension(imgfn)),
					new XAttribute("media-type", mime)));
			}
		}
		
		manifest.Add(new XElement(opf + "item",
			new XAttribute("href", "toc.ncx"),
			new XAttribute("id", "ncx"),
			new XAttribute("media-type", "application/x-dtbncx+xml")));

		manifest.Add(new XElement(opf + "item",
			new XAttribute("href", "cover.png"),
			new XAttribute("id", "cover"),
			new XAttribute("media-type", "image/png")));

		package.Add(manifest);

		var spine = new XElement(opf + "spine", new XAttribute("toc", "ncx"));
		spine.Add(new XElement(opf + "itemref", new XAttribute("idref", "titlepage")));
		foreach (var chptr in chapters)
		{
			for (var i = 0; i < chptr.GetSplitCount(opts); i++)
			{
				spine.Add(new XElement(opf + "itemref", new XAttribute("idref", chptr.EpubID(i))));
			}
		}

		package.Add(spine);

		package.Add(new XElement(opf + "guide"));

		using Utf8StringWriter writer = new Utf8StringWriter();
		
		doc.Save(writer);
		return writer.ToString();
	}

    private string GetEpubTOC(List<Chapter> chapters)
    {
	    XNamespace dc = "http://www.daisy.org/z3986/2005/ncx/";
	    XNamespace ncx = "http://www.idpf.org/2007/opf";

	    var doc = new XDocument(
		    new XDeclaration("1.0", "UTF-8", null),
		    new XDocumentType("ncx", "-//NISO//DTD ncx 2005-1//EN", "http://www.daisy.org/z3986/2005/ncx-2005-1.dtd", null));

	    var root = new XElement(ncx + "ncx",
		    new XAttribute("version", "2005-1"),
		    new XElement(ncx + "head",
			    new XElement(ncx + "meta",
				    new XAttribute("content", "urn:uuid:" + Program.ID_OPF().ToString("D")),
				    new XAttribute("name", "dtb:uid")),
			    new XElement(ncx + "meta",
				    new XAttribute("content", 1),
				    new XAttribute("name", "dtb:depth")),
			    new XElement(ncx + "meta",
				    new XAttribute("content", 0),
				    new XAttribute("name", "dtb:totalPageCount")),
			    new XElement(ncx + "meta",
				    new XAttribute("content", 0),
				    new XAttribute("name", "dtb:maxPageNumber"))));

	    doc.Add(root);

	    root.Add(new XElement(ncx + "docTitle",
		    new XElement(ncx + "text", "Unknown")));

	    var nav = new XElement(ncx + "navMap");
	    foreach (var chptr in chapters)
	    {
		    nav.Add(new XElement(ncx + "navPoint",
			    new XAttribute("id", "navPoint-" + chptr.Order),
			    new XAttribute("playOrder", chptr.Order),
			    new XElement(ncx + "navLabel",
				    new XElement(ncx + "text", chptr.Identifier + " - " + chptr.Title)),
			    new XElement(ncx + "content",
				    new XAttribute("src", "Text/" + chptr.EpubFilename(0)))));
	    }

	    root.Add(nav);

	    using var writer = new Utf8StringWriter();
	    
	    doc.Save(writer);
	    return writer.ToString();
    }

    private string GetTitlepageHTML()
    {
	    var html = new StringBuilder();
	    
	    html.AppendLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
	    html.AppendLine(@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.1//EN""");
	    html.AppendLine(@"  ""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"">");
	    html.AppendLine(@"");
	    html.AppendLine(@"<html xmlns=""http://www.w3.org/1999/xhtml"" xml:lang=""en"">");
		html.AppendLine(@"    <head>");
	    html.AppendLine(@"        <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />");
	    html.AppendLine(@"        <meta name=""calibre:cover"" content=""true"" />");
	    html.AppendLine(@"        <title>Cover</title>");
	    html.AppendLine(@"        <style type=""text/css"" title=""override_css"">");
	    html.AppendLine(@"            @page {padding: 0pt; margin:0pt}");
	    html.AppendLine(@"            body { text-align: center; padding:0pt; margin: 0pt; }");
	    html.AppendLine(@"        </style>");
	    html.AppendLine(@"    </head>");
	    html.AppendLine(@"    <body>");
	    html.AppendLine(@"        <div>");
	    html.AppendLine(@"            <svg version=""1.1"" xmlns=""http://www.w3.org/2000/svg""");
	    html.AppendLine(@"                xmlns:xlink=""http://www.w3.org/1999/xlink""");
	    html.AppendLine(@"                width=""100%"" height=""100%"" viewBox=""0 0 1000 1600""");
	    html.AppendLine(@"                preserveAspectRatio=""xMidYMid meet"">");
	    html.AppendLine(@"                <image width=""1000"" height=""1600"" xlink:href=""../cover.png""/>");
	    html.AppendLine(@"            </svg>");
	    html.AppendLine(@"        </div>");
	    html.AppendLine(@"    </body>");
	    html.AppendLine(@"</html>");

	    return html.ToString();
    }
    
    public void WriteChapter(Chapter chptr, Options opts)
    {
	    var sc = chptr.GetSplitCount(opts);
	    for (var i = 0; i < sc; i++)
	    {
		    WritePubString(@"OEBPS\Text\" + chptr.EpubFilename(i), chptr.GetEpubHTML(i, opts), true);
	    }
    }

    public void WriteCover()
    {
	    WriteBin(@"OEBPS\cover.png", File.ReadAllBytes(Path.Combine(Environment.CurrentDirectory, "data", "cover.png")));
	    WritePubString(@"OEBPS\Text\000_titlepage.html", GetTitlepageHTML(), true);
    }
}