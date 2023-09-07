using Newtonsoft.Json.Linq;

namespace ProjectLawfulEbook.Cache;

public class CacheReply
{
    public readonly int ID;
    
    public readonly DateTime CreatedAt;
    public readonly DateTime UpdatedAt;
    
    public readonly int? CharacterID;
    public readonly string? CharacterName;
    public readonly string? CharacterScreenName;
    public readonly string? CharacterAltName;
    
    public readonly int? IconID;
    public readonly string? IconKeyword;
    
    public readonly int? UserID;
    public readonly string? UserName;
    
    public readonly string HTMLContent;

    public CacheReply(int id, DateTime createdAt, DateTime updatedAt, 
                      int? characterID, string? characterName, string? characterScreenName, string? characterAltName,
                      int? iconID, string? iconKeyword, 
                      int? userID, string? userName, 
                      string htmlContent)
    {
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

    public static CacheReply Parse(JToken json)
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
        
        return new CacheReply(id, createdat, updatedat, characterID, characterName, characterScreenName, characterAltName, iconID,
            iconKeyword, userID, userName, content);
    }
}