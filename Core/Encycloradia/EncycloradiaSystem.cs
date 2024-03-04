using Microsoft.CodeAnalysis.CSharp.Syntax;
using Radiance.Content.EncycloradiaEntries;
using Radiance.Core.Systems;
using ReLogic.Graphics;
using System.Text.RegularExpressions;
using Terraria.Localization;

namespace Radiance.Core.Encycloradia
{
    public class EncycloradiaSystem
    {
        public static EncycloradiaSystem Instance { get; set; }
        public static List<EncycloradiaEntry> EncycloradiaEntries = new List<EncycloradiaEntry>();
        public static Dictionary<EntryCategory, List<EncycloradiaEntry>> EntriesByCategory;

        public static void Load()
        {
            AssembleEntries();
            WorldFile.OnWorldLoad += BuildAndSortEntries;
            LoadEntries();
        }

        public static void Unload()
        {
            WorldFile.OnWorldLoad -= BuildAndSortEntries;
            Instance = null;
            EncycloradiaEntries = null;
        }

        private static void BuildAndSortEntries()
        {
            AssembleEntries();
            RebuildCategoryPages();
        }

        public static void LoadEntries()
        {
            UnlockSystem.ResetUnlockConditions();

            foreach (Type type in Radiance.Instance.Code.GetTypes().Where(t => t.IsSubclassOf(typeof(EncycloradiaEntry)) && !t.IsAbstract))
            {
                EncycloradiaEntry entry = (EncycloradiaEntry)Activator.CreateInstance(type);
                AddEntry(entry);
            }
        }

        public static void AssembleEntries()
        {
            // create a dict entry for each category type, place all entries into their respective category list, and then sort them all
            EntriesByCategory = new Dictionary<EntryCategory, List<EncycloradiaEntry>>();
            foreach (EntryCategory category in Enum.GetValues(typeof(EntryCategory)))
            {
                EntriesByCategory[category] = new List<EncycloradiaEntry>();
            }
            foreach (EncycloradiaEntry entry in EncycloradiaEntries)
            {
                EntriesByCategory[entry.category].Add(entry);
            }
            SortEntries();
        }

        public static void SortEntries()
        {
            foreach (var kvp in EntriesByCategory)
            {
                EntriesByCategory[kvp.Key] = EntriesByCategory[kvp.Key].OrderBy(x => x.unlockedStatus).ThenBy(x => x.GetLocalizedName()).ToList();
            }
        }

        public static void RebuildCategoryPages()
        {
            int leftIndex = -1;
            foreach (EncycloradiaEntry entry in EncycloradiaEntries.Where(x => x.GetType().IsSubclassOf(typeof(CategoryEntry))))
            {
                entry.pages.RemoveAll(x => x.GetType() == typeof(CategoryPage));
                List<EncycloradiaEntry> entriesToBeShown = new List<EncycloradiaEntry>();
                entriesToBeShown.AddRange(EntriesByCategory[entry.category].Where(x => x.visible == EntryVisibility.Visible || (x.visible == EntryVisibility.NotVisibleUntilUnlocked && x.unlockedStatus == UnlockedStatus.Unlocked)));

                int pagesToAdd = (int)MathF.Ceiling(entriesToBeShown.Count / (float)EncycloradiaUI.ENTRIES_PER_CATEGORY_PAGE);
                for (int i = 0; i < pagesToAdd; i++)
                {
                    entry.pages.Add(new CategoryPage(entry.category) { index = entry.pages.Count });
                }
            }
        }
        public static string GetUninitializedEntryName(EncycloradiaEntry entry)
        {
            if (entry.name == string.Empty)
                return entry.GetType().Name;

            return entry.name;
        }
        public static void AddEntry(EncycloradiaEntry entry)
        {
            EncycloradiaEntry entryToAdd = (EncycloradiaEntry)entry.Clone();
            if (entryToAdd.name == string.Empty)
                entryToAdd.name = entryToAdd.GetType().Name;

            LocalizedText displayName = Language.GetOrRegister($"Mods.{entryToAdd.mod.Name}.Encycloradia.Entries.{entryToAdd.name}.DisplayName");
            Language.GetOrRegister($"Mods.{entryToAdd.mod.Name}.Encycloradia.Entries.{entryToAdd.name}.Tooltip");

            for (int i = 0; i < entryToAdd.pages.Count; i++)
            {
                EncycloradiaPage page = entryToAdd.pages[i];
                page.key = Language.GetOrRegister($"Mods.{entry.mod.Name}.Encycloradia.Entries.{entryToAdd.name}.{page.GetType().Name}_{i}");
            }
            List<EncycloradiaPage> pagesToAdd = new List<EncycloradiaPage>();
            foreach (EncycloradiaPage page in entryToAdd.pages)
            {
                if (page.GetType() == typeof(TextPage))
                {
                    List<TextPage> textPages = ProcessTextPage(page.key);
                    textPages.ForEach(pagesToAdd.Add);
                    continue;
                }
                pagesToAdd.Add(page);
            }

            int index = 0;
            pagesToAdd.ForEach(x => x.index = index++);
            entryToAdd.pages = pagesToAdd;

            EncycloradiaEntries.Add(entryToAdd);
#if DEBUG
            Radiance.Instance.Logger.Info($"Loaded Encycloradia entry \"{displayName.Value}\" ({entryToAdd.name}).");
#endif
            #region CategoryPage Testing
            //            string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            //            for (int i = 0; i < UnlockSystem.currentUnlockConditionStates.Count; i++)
            //            {
            //                EncycloradiaEntry entryToAdd = (EncycloradiaEntry)entry.Clone();
            //                entryToAdd.unlock = UnlockSystem.currentUnlockConditionStates.Keys.ToList()[i];

            //                if (entryToAdd.name == string.Empty)
            //                    entryToAdd.name = entryToAdd.GetType().Name;
            //                if (i > 0)
            //                    entryToAdd.name += i;

            //                entryToAdd.displayName = alphabet[i] + "-" + entryToAdd.displayName;
            //                entryToAdd.visible = EntryVisibility.NotVisibleUntilUnlocked;

            //                EncycloradiaEntries.Add(entryToAdd);
            //#if DEBUG
            //                Radiance.Instance.Logger.Info($"Loaded Encycloradia entry \"{entryToAdd.displayName}\" ({entryToAdd.name}).");
            //#endif
            //            }
            #endregion
        }

        internal static List<TextPage> ProcessTextPage(LocalizedText key)
        {
            DynamicSpriteFont font = FontAssets.MouseText.Value;
            List<TextPage> pagesToAdd = new();

            List<string> currentLineForDrawing = new List<string>();
            List<string> currentLineForSizeComparison = new List<string>();
            List<string> linesForCurrentPage = new List<string>();
            string text = key.Value;
            string[] pageSplitIntoWords = text.Split(" ");

            string bracketsString = string.Empty;
            int colonsLeft = 0;

            for (int i = 0; i < pageSplitIntoWords.Length; i++)
            {
                string word = pageSplitIntoWords[i];
                string wordWithParsing = string.Empty;
                string wordWithoutParsing = string.Empty;
                for (int j = 0; j < word.Length; j++)
                {
                    char character = word[j];

                    #region Newline Parsing
                    if (character == '\n')
                    {
                        // if the first line is exclusively a newline, skip over it so that there isn't strange empty line at the start of some pages
                        if (linesForCurrentPage.Count == 0 && wordWithParsing == "\n")
                        {
                            wordWithParsing = string.Empty;
                            j++;
                            continue;
                        }

                        // due to how it's designed, if the line would spill over with the newline-including word, push that word down a line instead and then go back to the start of this newline
                        bool spillOver = font.MeasureString(string.Join(" ", currentLineForSizeComparison) + wordWithoutParsing).X * EncycloradiaUI.LINE_SCALE >= EncycloradiaUI.MAX_PIXELS_PER_LINE;
                        //wordWithParsing = wordWithParsing.TrimEnd('&');

                        if (!spillOver)
                            currentLineForDrawing.Add(wordWithParsing);

                        // add line to list of lines for current page, check if the lines is at the max line count (and add a page to entry if it is) and then reset for next line
                        linesForCurrentPage.Add(string.Join(" ", currentLineForDrawing));
                        if (linesForCurrentPage.Count >= EncycloradiaUI.MAX_LINES_PER_PAGE)
                        {
                            pagesToAdd.Add(new TextPage() { text = string.Join("\n", linesForCurrentPage), key = key });
                            linesForCurrentPage.Clear();
                        }
                        currentLineForDrawing.Clear();
                        currentLineForSizeComparison.Clear();

                        if (spillOver)
                            currentLineForDrawing.Add(wordWithParsing);

                        wordWithoutParsing = string.Empty;
                        wordWithParsing = string.Empty;
                        continue;
                    }
                    #endregion

                    wordWithParsing += character;

                    if (character == EncycloradiaUI.PARSE_CHARACTER)
                    {
                        char parseCharacter = word[j + 1];
                        wordWithParsing += parseCharacter;
                        j++;
                        continue;
                    }

                    #region Bracket Parsing 
                    if (character == '[')
                    {
                        colonsLeft = 2;
                        continue;
                    }
                    if(character == ']')
                        continue;

                    if(colonsLeft > 0)
                    {
                        if (character == ':')
                            colonsLeft--;
                        continue;
                    }
                    #endregion
                   
                    wordWithoutParsing += character;
                }
                
                // if length of line is greater than the max length, add current line to list of lines for page, create page if at line limit, and then reset for next line
                float stringLength = font.MeasureString(string.Join(" ", currentLineForSizeComparison) + wordWithoutParsing).X;
                if (stringLength * EncycloradiaUI.LINE_SCALE >= EncycloradiaUI.MAX_PIXELS_PER_LINE)
                {
                    linesForCurrentPage.Add(string.Join(" ", currentLineForDrawing));
                    if (linesForCurrentPage.Count >= EncycloradiaUI.MAX_LINES_PER_PAGE)
                    {
                        pagesToAdd.Add(new TextPage() { text = string.Join("\n", linesForCurrentPage), key = key });
                        linesForCurrentPage.Clear();
                    }
                    currentLineForDrawing.Clear();
                    currentLineForSizeComparison.Clear();
                }
                currentLineForDrawing.Add(wordWithParsing);
                currentLineForSizeComparison.Add(wordWithoutParsing);
            }

            // if there's still remainder of a page to be added, add it after looping through every word
            if (linesForCurrentPage.Any() || currentLineForDrawing.Any())
            {
                if (currentLineForDrawing.Any())
                    linesForCurrentPage.Add(string.Join(" ", currentLineForDrawing));

                pagesToAdd.Add(new TextPage() { text = string.Join("\n", linesForCurrentPage), key = key });
            }
            return pagesToAdd;
        }

        public static EncycloradiaEntry FindEntry<T>() where T : EncycloradiaEntry => EncycloradiaEntries.FirstOrDefault(x => x.GetType() == typeof(T));

        public static EncycloradiaEntry FindEntry(string name) => EncycloradiaEntries.FirstOrDefault(x => x.name == name);

        public static EncycloradiaEntry FindEntryByFastNavInput(string input) => EncycloradiaEntries.FirstOrDefault(x => x.fastNavInput == input);
    }
}