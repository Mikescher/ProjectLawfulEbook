namespace ProjectLawfulEbook;

public class Options
{
    public readonly bool INCLUDE_AVATAR_KEYWORDS;
    public readonly bool TRY_INLINE_CHARACTER_NAME;
    public readonly bool INCLUDE_AVATARS;
    public readonly bool INCLUDE_SCREEN_NAME;
    public readonly int  MAX_POST_PER_FILE;
    public readonly bool USE_SFW_CHAPTER;
    public readonly bool ONLY_MAIN_STORY;

    public Options(bool includeAvatarKeywords, bool tryInlineCharacterName, bool includeAvatars, bool includeScreenName, int maxPostPerFile, bool useSfwChapter, bool onlyMainStory)
    {
        INCLUDE_AVATAR_KEYWORDS = includeAvatarKeywords;
        INCLUDE_AVATARS = includeAvatars;
        TRY_INLINE_CHARACTER_NAME = tryInlineCharacterName;
        INCLUDE_SCREEN_NAME = includeScreenName;
        MAX_POST_PER_FILE = maxPostPerFile;
        USE_SFW_CHAPTER = useSfwChapter;
        ONLY_MAIN_STORY = onlyMainStory;
    }
}