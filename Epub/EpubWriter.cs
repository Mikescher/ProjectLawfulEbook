using System.Text;
using System.Xml.Linq;
using Ionic.Zip;
using ProjectLawfulEbook.Book;

namespace ProjectLawfulEbook.Epub;

public class EpubWriter
{
    public readonly string Destination;
    public readonly bool WriteEpub;

    private FileStream fs;
    private ZipOutputStream zipstream;
    
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
	        
            fs = File.Open(Destination, FileMode.Create, FileAccess.ReadWrite);
            zipstream = new ZipOutputStream(fs);
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
            zipstream.Close();
            fs.Close();
        }
    }
    
    private void WritePubString(string n, string c, Encoding? e = null)
    {
        if (WriteEpub)
        {
            var f = zipstream.PutNextEntry(n);
            f.CompressionLevel = Ionic.Zlib.CompressionLevel.None;

            byte[] buffer = (e ?? Encoding.UTF8).GetBytes(c);
            zipstream.Write(buffer, 0, buffer.Length);
        }
        else
        {
            var dst = Path.Combine(Destination, n.Replace("\\", "/"));
            Directory.CreateDirectory(Path.GetDirectoryName(dst)!);
            File.WriteAllBytes(dst, (e ?? Encoding.UTF8).GetBytes(c));
        }
    }

    public void WriteMimeType()
    {
        WritePubString(@"mimetype", GetEpubMimetype());
    }

    public void WriteContainerXML()
    {
        WritePubString(@"META-INF\container.xml", GetEpubContainerXML());
    }
    
    public void WriteContentOPF(List<Chapter> chapters)
    {
	    WritePubString(@"OEBPS\content.opf", GetEpubContentOPF(chapters));
    }

    public void WriteTOC(List<Chapter> chapters)
    {
	    WritePubString(@"OEBPS\toc.ncx", GetEpubTOC(chapters));
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

        StringBuilder builder = new StringBuilder();
        using (Utf8StringWriter writer = new Utf8StringWriter())
        {
            doc.Save(writer);
            var r = writer.ToString();
            r = r.Replace("encoding=\"utf-8\"", "encoding=\"UTF-8\"");
            return r.Trim() + "\r\n";
        }
    }

    private string GetEpubContentOPF(List<Chapter> chapters)
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
							new XAttribute("name", "Wordpress_eBook_scraper_creation_time")));

		package.Add(meta);

		var manifest = new XElement(opf + "manifest");
		foreach (var chtr in chapters)
		{
			manifest.Add(new XElement(opf + "item",
				new XAttribute("href", "Text/" + chtr.EpubFilename()),
				new XAttribute("id", chtr.EpubID()),
				new XAttribute("media-type", "application/xhtml+xml")));
		}
		
		manifest.Add(new XElement(opf + "item",
			new XAttribute("href", "toc.ncx"),
			new XAttribute("id", "ncx"),
			new XAttribute("media-type", "application/x-dtbncx+xml")));

		package.Add(manifest);

		var spine = new XElement(opf + "spine", new XAttribute("toc", "ncx"));
		foreach (var chptr in chapters)
		{
			spine.Add(new XElement(opf + "itemref", new XAttribute("idref", chptr.EpubID())));
		}

		package.Add(spine);

		package.Add(new XElement(opf + "guide"));

		using Utf8StringWriter writer = new Utf8StringWriter();
		
		doc.Save(writer);
		return writer.ToString();
	}

    string GetEpubTOC(List<Chapter> chapters)
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
				    new XAttribute("src", "Text/" + chptr.EpubFilename()))));
	    }

	    root.Add(nav);

	    using var writer = new Utf8StringWriter();
	    
	    doc.Save(writer);
	    return writer.ToString();
    }

    public void WriteChapter(Chapter chptr)
    {
	    WritePubString(@"OEBPS\Text\" + chptr.EpubFilename(), chptr.GetEpubHTML());
    }
}