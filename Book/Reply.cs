using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;

namespace ProjectLawfulEbook.Book;

public class Reply
{
    public readonly string ParentThreadID;
    public readonly string ID;
    
    public readonly DateTime CreatedAt;
    public readonly DateTime UpdatedAt;
    
    public readonly string? CharacterID;
    public readonly string? CharacterName;
    public readonly string? CharacterScreenName;
    public readonly string? CharacterAltName;
    
    public readonly string? IconID;
    public readonly string? IconKeyword;
    
    public readonly string? UserID;
    public readonly string? UserName;
    
    public string HTMLContent;

    private List<(string, string)> Paragraphs;
    
    public Reply(string parentID, string id, DateTime createdAt, DateTime updatedAt, 
                      string? characterID, string? characterName, string? characterScreenName, string? characterAltName,
                      string? iconID, string? iconKeyword, 
                      string? userID, string? userName, 
                      string htmlContent)
    {
        ParentThreadID = parentID;
        ID = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        CharacterID = characterID;
        CharacterName = characterName;
        CharacterScreenName = characterScreenName;
        CharacterAltName = characterAltName;
        IconID = iconID;
        IconKeyword = iconKeyword;
        UserID = userID;
        UserName = userName;
        HTMLContent = htmlContent;
    }
    
    public static Reply Parse(string parentID, JToken json)
    {
        var id = json["id"]!.Value<int>();
        var characterID = json["character"]!.HasValues ? json["character"]!["id"]?.Value<int>() : null;
        var characterName = json["character"]!.HasValues ? json["character"]!["name"]?.Value<string>() : null;
        var characterScreenName = json["character"]!.HasValues ? json["character"]!["screenname"]?.Value<string>() : null;
        var characterName2 = json["character_name"]?.Value<string>();
        var content = json["content"]!.Value<string>()!;
        var createdat = DateTime.Parse(json["created_at"]!.Value<string>()!);
        var updatedat = DateTime.Parse(json["updated_at"]!.Value<string>()!);
        var iconID = json["icon"]!.HasValues ? json["icon"]!["id"]?.Value<int>() : null;
        var iconKeyword = json["icon"]!.HasValues ? json["icon"]!["keyword"]!.Value<string>()! : null;
        var userID = json["user"]!["id"]!.Value<int>();
        var userName = json["user"]!["username"]!.Value<string>()!;

        var characterAltName = (string?) null;

        if (characterName != characterName2) characterAltName = characterName2;
        
        return new Reply(
            parentID,
            id.ToString(), 
            createdat, updatedat, 
            characterID?.ToString(), characterName, characterScreenName, characterAltName, 
            iconID?.ToString(), iconKeyword, 
            userID.ToString(), userName, 
            content);
    }

    public void Cleanup()
    {
        HTMLContent = HTMLContent.ReplaceLineEndings("");
    }
    
    public void ParseParagraphs()
    {
        Paragraphs = new List<(string, string)>();
        
        var doc = new HtmlDocument();
        doc.LoadHtml(HTMLContent.ReplaceLineEndings(""));

        var root = doc.DocumentNode;

        var children = root.ChildNodes;

        foreach (var node in children)
        {
            if (node.Name == "p" && node.Attributes.Count == 0)
            {
                AssertValidLowestLevelParagraph(node);
                Paragraphs.Add(("p", node.OuterHtml));
            }
            else if (node.Name == "hr" && node.Attributes.Count == 0 && node.InnerHtml == "")
            {
                Paragraphs.Add(("hr", node.OuterHtml));
            }
            else if (node.Name == "details" && node.Attributes.All(p => p.Name == "open"))
            {
                var detailChilds = node.ChildNodes.Where(p => !(p.Name == "#text" && string.IsNullOrWhiteSpace(p.InnerText))).ToList();

                if (detailChilds.Count > 0 && detailChilds[0].Name == "summary")
                {
                    AssertValidLowestLevelParagraph(detailChilds[0]);
                    Paragraphs.Add(("details::summary", "<p><b>" + detailChilds[0].InnerHtml + "</b></p>"));
                    detailChilds.RemoveAt(0);
                }
                else
                {
                    Paragraphs.Add(("details::summary", "<p><b>Details</b></p>"));
                }

                foreach (var detailsNode in detailChilds)
                {
                    if (detailsNode.Name == "#text" && !string.IsNullOrWhiteSpace(detailsNode.InnerHtml))
                    {
                        Paragraphs.Add(("details::content::p", "<p>" + detailsNode.OuterHtml + "</p>")); // pseudo convert raw-text to <p>
                    }
                    else if (detailsNode.Name == "p" && detailsNode.Attributes.Count == 0)
                    {
                        AssertValidLowestLevelParagraph(detailsNode);
                        Paragraphs.Add(("details::content::p", detailsNode.OuterHtml));
                    }
                    else if (detailsNode.Name == "em" && detailsNode.Attributes.Count == 0)
                    {
                        AssertValidLowestLevelParagraph(detailsNode);
                        Paragraphs.Add(("details::content::em", detailsNode.OuterHtml));
                    }
                    else if (detailsNode.Name == "ul" && detailsNode.Attributes.Count == 0)
                    {
                        //AssertValidLowestLevelParagraph(detailsNode);
                        Paragraphs.Add(("details::content::ul", detailsNode.OuterHtml));
                    }
                    else if (detailsNode.Name == "br" && detailsNode.Attributes.Count == 0)
                    {
                        AssertValidLowestLevelParagraph(detailsNode);
                        Paragraphs.Add(("details::content::br", detailsNode.OuterHtml));
                    }
                    else if (detailsNode.Name == "#text")
                    {
                        AssertValidLowestLevelParagraph(detailsNode);
                        Paragraphs.Add(("details::content::p", "<p>" + detailsNode.OuterHtml + "</p>"));
                    }
                    else if (detailsNode.Name == "blockquote" && detailsNode.Attributes.Count == 0 && detailsNode.ChildNodes.All(p => p.Name is "p" or "pre"))
                    {
                        foreach (var cn in detailsNode.ChildNodes) AssertValidLowestLevelParagraph(cn);
                        Paragraphs.Add(("details::content::blockquote::multi", node.OuterHtml));
                    }
                    else if (detailsNode.Name == "img")
                    {
                        Console.WriteLine($"[TODO] <img> tag (with w+h)");
                        Paragraphs.Add(("details::content::img", detailsNode.OuterHtml));
                    }
                    else
                    {
                        Console.WriteLine($"[!] Invalid sub-detail-tag in reply {ParentThreadID}/{ID}: <{detailsNode.Name}>, skipping");
                    }
                }
            }
            else if (node.Name == "blockquote" && node.Attributes.Count == 0 && node.ChildNodes.Count == 1 && node.ChildNodes[0].Name == "#text")
            {
                AssertValidLowestLevelParagraph(node);
                Paragraphs.Add(("blockquote::text", node.OuterHtml));
            }
            else if (node.Name == "blockquote" && node.Attributes.Count == 0 && node.ChildNodes.All(p => p.Name is "p" or "pre" or "#text" or "em"))
            {
                if (ID != "1782438")
                {
                    foreach (var cn in node.ChildNodes) AssertValidLowestLevelParagraph(cn);
                }
                Paragraphs.Add(("blockquote::multi", node.OuterHtml));
            }
            else if (node.Name == "blockquote" && node.Attributes.Count == 0 && node.ChildNodes.Count == 1 && node.ChildNodes[0].Name == "pre")
            {
                AssertValidLowestLevelParagraph(node.ChildNodes[0]);
                Paragraphs.Add(("blockquote::pre", node.OuterHtml));
            }
            else if (node.Name == "blockquote" && node.Attributes.Count == 0 && node.ChildNodes.Count == 1 && node.ChildNodes[0].Name == "table" && IsValidTableAttributes(node.ChildNodes[0].Attributes))
            {
                Paragraphs.Add(("blockquote::table", node.OuterHtml));
            }
            else if (node.Name == "blockquote" && node.Attributes.Count == 0 && node.ChildNodes.Count == 1 && node.ChildNodes[0].Name == "details" && node.ChildNodes[0].ChildNodes.Count > 1 && node.ChildNodes[0].ChildNodes[0].Name == "summary")
            {
                var builder = "";
                
                AssertValidLowestLevelParagraph(node.ChildNodes[0].ChildNodes[0]);
                builder += "<p><b>" + node.ChildNodes[0].ChildNodes[0].InnerHtml + "</b></p>";
                foreach (var detailsChild in node.ChildNodes[0].ChildNodes.Skip(1))
                {
                    if (detailsChild.Name is "p" or "pre" or "blockquote" or "br" or "#text" or "em")
                    {
                        AssertValidLowestLevelParagraph(detailsChild);
                        builder += detailsChild.OuterHtml;
                    }
                    else
                    {
                        Console.WriteLine($"[!] Invalid sub-blockquote-details-node in reply {ParentThreadID}/{ID}: <{detailsChild.Name}>, skipping");
                    }
                }
                Paragraphs.Add(("blockquote::details", builder));

            }
            else if (node.Name == "table" && IsValidTableAttributes(node.Attributes))
            {
                Paragraphs.Add(("table", node.OuterHtml));
            }
            else if (node.Name == "#text" && !string.IsNullOrWhiteSpace(node.InnerHtml) && !string.IsNullOrWhiteSpace(node.InnerText))
            {
                Console.WriteLine($"[~] WARN: Root #text in reply {ParentThreadID}/{ID}: <{node.Name}>");
                Paragraphs.Add(("p", "<p>" + node.OuterHtml + "</p>")); // pseudo convert raw-text to <p>
            }
            else if (node.Name == "#text" && string.IsNullOrWhiteSpace(node.InnerHtml) && string.IsNullOrWhiteSpace(node.InnerText))
            {
                Console.WriteLine($"[i] Skip empty <p> in {ParentThreadID}/{ID}: <{node.Name}>");
                //skip
            }
            else if (node.Name == "pre" && node.Attributes.Count == 0 && node.InnerText == node.InnerHtml)
            {
                Paragraphs.Add(("pre", node.OuterHtml));
            }
            else if (node.OuterHtml == "<p dir=\"ltr\" style=\"line-height: 1.38; margin-top: 0pt; margin-bottom: 0pt;\"><span style=\"font-size: 11pt; font-family: Arial; color: #000000; background-color: transparent; font-weight: 400; font-style: normal; font-variant: normal; text-decoration: none; vertical-align: baseline; white-space: pre-wrap;\">Keltham can take his time.&nbsp;</span></p>")
            {
                Paragraphs.Add(("manual", "<p>" + node.InnerText + "</p>")); // whatever...
            }
            else
            {
                Console.WriteLine($"[!] Invalid node in reply {ParentThreadID}/{ID}: <{node.Name}>, skipping");
            }
        }
    }

    private void AssertValidLowestLevelParagraph(HtmlNode node)
    {
        if (node.Name == "#text" && node.InnerHtml == node.InnerText) return; //okay

        foreach (var n in node.ChildNodes)
        {
            if (n.Name == "em"     && n.Attributes.Count == 0) { AssertValidLowestLevelParagraph(n); continue; }
            if (n.Name == "strong" && n.Attributes.Count == 0) { AssertValidLowestLevelParagraph(n); continue; }
            if (n.Name == "small"  && n.Attributes.Count == 0) { AssertValidLowestLevelParagraph(n); continue; }
            if (n.Name == "big"    && n.Attributes.Count == 0) { AssertValidLowestLevelParagraph(n); continue; }
            if (n.Name == "i"      && n.Attributes.Count == 0) { AssertValidLowestLevelParagraph(n); continue; }
            if (n.Name == "b"      && n.Attributes.Count == 0) { AssertValidLowestLevelParagraph(n); continue; }

            if (n.Name == "span" && IsValidSpanAttributes(n.Attributes))
            {
                AssertValidLowestLevelParagraph(n);
                continue;
            }
            
            if (n.Name == "#text" && n.Attributes.Count == 0 && n.InnerHtml == n.InnerText) continue;

            if (n.Name == "br" && n.Attributes.Count == 0 && n.InnerHtml == "")
            {
                AssertValidLowestLevelParagraph(n);
                continue;
            }

            if (n.Name == "a" && n.Attributes.All(p => new[]{"href", "target", "rel"}.Contains(p.Name)))
            {
                AssertValidLowestLevelParagraph(n);
                continue;
            }

            if (n.Name == "abbr" && n.Attributes.All(p => new[]{"title"}.Contains(p.Name)))
            {
                AssertValidLowestLevelParagraph(n);
                continue;
            }

            if (n.Name == "img")
            {
                Console.WriteLine($"[TODO] <img> tag (with w+h)");
                continue;
            }

            if (n.OuterHtml == "<span id=\"docs-internal-guid-61a6bffd-7fff-9f60-362d-f6bc1972080f\">&nbsp;</span>") continue; // whatever
            if (n.OuterHtml == "<span style=\"color: #ba372a;\"><strong>K\u0335e\u0335l\u0336t\u0337h\u0334a\u0338m\u0338</strong></span>") continue; // -.-
            if (n.OuterHtml == "<span style=\"color: #ba372a;\">K\u0335e\u0335l\u0336t\u0337h\u0334a\u0338m\u0338</span>") continue; // -.-
            
            Console.WriteLine($"[?] Not a primitive paragraph in {ParentThreadID}/{ID}: '{n.OuterHtml}'");
        }
    }

    private bool IsValidTableAttributes(HtmlAttributeCollection attr)
    {
        if (attr.Count == 0) return true;

        if (attr.Count == 1 && attr[0].Name == "style")
        {
            if (attr[0].Value == "width: auto;") return true;
            if (attr[0].Value == "max-width: 30em;") return true;
            if (attr[0].Value == "max-width: 30em") return true;
            if (attr[0].Value == "max-width:30em") return true;
            if (attr[0].Value == "max-width:30em;") return true;
            if (attr[0].Value == "max-width: 20em;") return true;
            if (attr[0].Value == "max-width: 20em") return true;
            if (attr[0].Value == "max-width:20em") return true;
            if (attr[0].Value == "max-width:20em;") return true;
        }

        return false;
    }

    private bool IsValidSpanAttributes(HtmlAttributeCollection attr)
    {
        if (attr.Count == 0) return true;

        if (attr.Count == 1 && attr[0].Name == "style")
        {
            if (attr[0].Value == "text-decoration: underline;") return true;
            if (attr[0].Value == "text-decoration: line-through;") return true;
            if (attr[0].Value == "text-decoration-line: line-through;") return true;
        }

        return false;
    }

    public string GetEpubHTML()
    {
        var xml = new StringBuilder();

        var pgSkip = 0;
        
        if (CharacterName != null)
        {
            var prefix = "";
            
            prefix += ($"<b>{CharacterName}");
            if (CharacterAltName != null && CharacterAltName.ToLower() != CharacterName.ToLower())
            {
                prefix += ($" ({CharacterAltName})");
            }
            prefix += ":</b>";
            if (Program.INCLUDE_AVATAR_KEYWORDS && IconKeyword != null && IconKeyword.ToLower() != CharacterName.ToLower() && IconKeyword != "image")
            {
                prefix += ($" <i>({IconKeyword})</i>");
            }

            if (Program.TRY_INLINE_CHARACTER_NAME)
            {
                if (Paragraphs.Count > 0 && Paragraphs[0].Item1 == "p" && Paragraphs[0].Item2.ToLower().StartsWith("<p>"))
                {
                    xml.AppendLine("<p>" + prefix + " " + Paragraphs[0].Item2[3..]);
                    pgSkip = 1;
                }
                else
                {
                    xml.AppendLine(prefix + "<br/>");
                }
            }
            else
            {
                xml.AppendLine(prefix + "<br/>");
            }
        }
        
        foreach (var (pgType, pgHTML) in Paragraphs.Skip(pgSkip))
        {
            xml.AppendLine(pgHTML);
        }
        
        var result = xml.ToString();

        result = Regex.Replace(result, @"<br>(?!</br>)", "<br/>");
        
        return result;
    }
}