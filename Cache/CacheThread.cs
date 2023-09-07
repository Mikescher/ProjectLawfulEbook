using System.Collections.Immutable;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json.Linq;

namespace ProjectLawfulEbook.Cache;

public class CacheThread
{
    public readonly int ID;
    public readonly DateTime CreatedAt;
    
    public readonly string Subject;
    public readonly string Description;
    
    public readonly string NumReplies;
    
    public readonly CacheReply FirstPost;
    
    public readonly IReadOnlyList<CacheReply> Replies;

    private CacheThread(int id, DateTime createdAt, string description, string numReplies, string subject, CacheReply firstpost, IReadOnlyList<CacheReply> replies)
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
        
        var fpost = new CacheReply(0, createdat, createdat, characterID, characterName, characterScreenName, null, iconID, iconKeyword, null, null, content);
        
        var jreplies = JObject.Parse(File.ReadAllText(repliesFile))["Ok"]!.Values<JToken>();

        var replies = jreplies.Select(CacheReply.Parse!).ToImmutableList();
        
        return new CacheThread(id, createdat, description, numreplies, subject, fpost, replies);
    }
}