
# Project Lawful Ebook

This is my attempt at creating a ebook version of the [project lawful](https://www.projectlawful.com/) story/glowfic.

 - I used the cache file of [glowpub](https://github.com/QuartzLibrary/glowpub) as a basis, 
   because I didn't want to write my own scraper (thanks to github.com/QuartzLibrary)
 - Then I hacked together a bit of C# code to generate epub files
 - All external images (and optionally avatars) are included in the epub
 - I went through all the avatar files and made them squares (simplified my css, and looked better - most were already squares or almost-squares)
 - Various properties of the final file/layout can be changed, I generated the variants I found useful, but if anyone wants something special it should be enough to close the repo, edit `Program.cs` and run `make` 

## Variants

I recommend either the `project-lawful-default.epub` or the `project-lawful-avatars.epub` variants
(the `-avatars` version contains the post profile-pictures, but is bigger and takes longer to open on older devices).

There are 3 different versions of the actual **content** of the files:
  - the normal one includes all threads from [projectlawful.com](https://www.projectlawful.com/), including sandboxes and lectures. If there is a SFW version of a thread, it is not included (the default/nsfw is included instead)
  - the `*-sfw-*` versions have the NSFW threads replaced wih their SFW counterparts (see below under [Order](#order))
  - the `*-onlymainstory-*` versions do not include the sandbox threads or the (optional) lecture threads (see below under [Order](#order))

There are also 5 different layout options (that I pre-built) for every version above:
  - the `*-default` variant only includes the character-name of each post, and (if possible) the character-name is set inline with the first paragraph
  - the `*-moreinfo` variant also includes the alignment text of each post/character after the character-name.
  - the `*-avatars` variant show the character avatar beside each post. This forces us to include all 600-ish avatars in the epub file, which increases filesize and loading time
  - the `*-avatars-moreinfo` also includes the alignment text (additionally to the avatar). Here the avatar/character-name/alignment-text are also no longer inlined, but get their own paragraph at the start of every post.
  - the `*-biggerhtml` variant looks the same as `*-default`, but internally every chapter is a single big file (instead of multiple splitted files), this makes the initial loading of the epub slower, but prevents a forced page-break after every 128th post.

As said above, it should be possible to create epub's with other combination of options (see `Program.cs` and `Options.cs`)

## Tested on

 - Kindle Paperwhite + KOReader
 - Foliate
 - epub.js
 - Calibre Ebook viewer

----

----

*Links:*

## Project Lawful

 - https://www.projectlawful.com/
 - https://glowfic.com/boards/215

 # Glowpub

 - https://github.com/QuartzLibrary/glowpub
 - https://github.com/QuartzLibrary/glowpub/issues/12
 - https://github.com/rlpowell/glowpub

 # Order

~~~~~~~

        ("01", 4582),                       // MAIN                : mad investor chaos and the woman of asmodeus
        ("01-subthread-01", 5310),          // SUBTHREAD           : kissing is not a human universal                                   ( replaces replies/1721818#reply-1721818 )
[xx]    ("01-subthread-01-sfw-tldr", 5403), // ALTERNATE SUBTHREAD : sfw tldr kissing is not a human universal
        ("02", 5504),                       // MAIN                : some human relationships are less universal than others
[##]    ("02-and-03-alternate", 5521),      // ALTERNATE           : tldr some human relationships
        ("03", 5506),                       // MAIN                : take this report back and bring her a better report
        ("04", 5508),                       // MAIN                : project lawful and their oblivious boyfriend
        ("04-subthread-01", 5610),          // SUBTHREAD           : cheating is cuddleroom technique                                   ( replaces replies/1756345#reply-1756345 )
[xx]    ("04-subthread-01-sfw-tldr", 5618), // ALTERNATE SUBTHREAD : sfw tldr cheating is cuddleroom technique
        ("04-subthread-02", 5638),          // SUBTHREAD           : in another world we could have been trade partners                 ( after replies/1760768#reply-1760768 )
[xx]    ("04-subthread-02-sfw-tldr", 5671), // ALTERNATE SUBTHREAD : sfw tldr we could have been trade partners
 [o]    ("04-sandbox-01", 5775),            // SANDBOX             : totally not evil
 [o]    ("04-sandbox-02", 5778),            // SANDBOX             : welcome to project lawful
        ("05", 5694),                       // MAIN                : my fun research project has more existential risk than I anticipated
 [o]    ("05-subthread-01", 5785),          // SUBTHREAD / LECTURE : to hell with science                                                   ( replaces 1777291#reply-1777291 )
 [o]    ("05-subthread-02", 5826),          // SUBTHREAD SORT OF?  : to earth with science                                                  ( after 1784214#reply-1784214 in prev subthread )
 [o]    ("05-subthread-03", 5864),          // SUBTHREAD / LECTURE : the alien maths of dath ilan                                           ( replaces 1786765#reply-1786765 )
 [o]    ("05-sandbox-01", 5880),            // SANDBOX             : I reject your alternate reality and substitute my own
        ("06", 5930),                       // MAIN                : what the truth can destroy
 [o]    ("06-sandbox-01", 6029),            // SANDBOX             : it is a beautiful day in Cheliax and you are a horrible medianworld romance novel
        ("07", 5977),                       // MAIN                : crisis of faith
        ("08", 6075),                       // MAIN                : the woman of irori
 [o]    ("08-sandbox-01", 6124),            // SANDBOX             : dear abrogail
        ("09", 6131),                       // MAIN                : flashback: this is not a threat
        ("10", 6132),                       // MAIN                : null action
        ("11", 6334),                       // MAIN                : the meeting of their minds 
        ("12", 6480),                       // MAIN                : null action act ii: unact harder
        ("13", 6827),                       // MAIN                : null action act iii: the consequences of my own nonactions

~~~~~~~

## Options:

 - TODO (see Program.cs)


## More

- `epubcheck --fatal "_out_epub/project-lawful.epub"`
- `epubcheck "_out_epub/project-lawful.epub"`