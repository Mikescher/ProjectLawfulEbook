using System.Collections.Immutable;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json.Linq;

namespace ProjectLawfulEbook.Cache;

public class CacheThread
{
    public readonly string ID;
    public readonly DateTime CreatedAt;
    
    public readonly string Subject;
    public readonly string Description;
    
    public readonly string NumReplies;
    
    public readonly CacheReply FirstPost;
    
    public readonly IReadOnlyList<CacheReply> Replies;

    private CacheThread(string id, DateTime createdAt, string description, string numReplies, string subject, CacheReply firstpost, IReadOnlyList<CacheReply> replies)
    {
        ID = id;
        CreatedAt = createdAt;
        Description = description;
        NumReplies = numReplies;
        Subject = subject;
        FirstPost = firstpost;
        Replies = replies;
    }

    public static CacheThread Load(int fileid)
    {
        var postFile = $"glowpub_cache/posts/{fileid}/post.json";
        var repliesFile = $"glowpub_cache/posts/{fileid}/replies.json";

        var jpost = JObject.Parse(File.ReadAllText(postFile))["Ok"]!;

        var id = jpost["id"]!.Value<int>();
        var characterID = jpost["character"]!.HasValues ? jpost["character"]!["id"]?.Value<int>() : null;
        var characterName = jpost["character"]!.HasValues ? jpost["character"]!["name"]!.Value<string>() : null;
        var characterScreenName = jpost["character"]!.HasValues ? jpost["character"]!["screenname"]!.Value<string>() : null;
        var content = jpost["content"]!.Value<string>()!;
        var createdat = DateTime.Parse(jpost["created_at"]!.Value<string>()!);
        var description = jpost["description"]!.Value<string>()!;
        var numreplies = jpost["num_replies"]!.Value<string>()!;
        var subject = jpost["subject"]!.Value<string>()!;
        var iconID = jpost["icon"]!.HasValues ? jpost["icon"]!["id"]?.Value<int>() : null;
        var iconKeyword = jpost["icon"]!.HasValues ?jpost["icon"]!["keyword"]!.Value<string>()! : null;
        
        var fpost = new CacheReply($"@{id}::first", createdat, createdat, characterID?.ToString(), characterName, characterScreenName, null, iconID?.ToString(), iconKeyword, null, null, content);
        
        var jreplies = JObject.Parse(File.ReadAllText(repliesFile))["Ok"]!.Values<JToken>();

        var replies = jreplies.Select(CacheReply.Parse!).ToImmutableList();
        
        return new CacheThread(id.ToString(), createdat, description, numreplies, subject, fpost, replies);
    }

    public IEnumerable<CacheReply> TakeUntil(int id, bool inclusive)
    {
        yield return FirstPost;
        foreach (var post in Replies)
        {
            if (post.ID == id.ToString())
            {
                if (inclusive) yield return post;
                yield break;
            }
            yield return post;
        }

        throw new Exception("id not found");
    }

    public IEnumerable<CacheReply> TakeAll()
    {
        yield return FirstPost;
        foreach (var post in Replies) yield return post;
    }

    public IEnumerable<CacheReply> TakeAfter(int id, bool inclusive)
    {
        var skip = true;
        foreach (var post in Replies)
        {
            if (skip)
            {
                if (post.ID == id.ToString())
                {
                    if (inclusive) yield return post;
                    skip = false;
                }
                continue;
            }
            yield return post;
        }

        if (skip) throw new Exception("id not found");
    }

    public IEnumerable<CacheReply> TakeBetween(int idStart, int idEnd, bool inclusiveStart, bool inclusiveEnd)
    {
        var skip = true;
        foreach (var post in Replies)
        {
            if (skip)
            {
                if (post.ID == idStart.ToString())
                {
                    if (inclusiveStart) yield return post;
                    skip = false;
                }
                continue;
            }
            
            if (post.ID == idEnd.ToString())
            {
                if (inclusiveEnd) yield return post;
                yield break;
            }
            yield return post;
        }

        throw new Exception("idStart/idEnd not found");
    }
}