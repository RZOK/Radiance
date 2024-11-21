using MonoMod.RuntimeDetour;
using Radiance.Content.EncycloradiaEntries;
using ReLogic.Graphics;
using Terraria.Localization;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.Utils;

namespace Radiance.Core.Encycloradia
{
    public class EncycloradiaSystem
    {
        public static List<EncycloradiaEntry> EncycloradiaEntries = new List<EncycloradiaEntry>();
        public static Dictionary<EntryCategory, List<EncycloradiaEntry>> EntriesByCategory;
        public static bool shouldUpdateLocalization = false;
        private static Hook LocalizationLoader_ReloadLanguage_Hook;

        public static void Load()
        {
            // we only want to load the encycloradia for clients/singleplayer
            if (Main.netMode == NetmodeID.Server)
                return;

            WorldFile.OnWorldLoad += BuildAndSortEntries;
            //LanguageManager.Instance.OnLanguageChanged += ReloadEncycloradiaOnLanguageChange;
            LocalizationLoader_ReloadLanguage_Hook ??= new Hook(typeof(LanguageManager).GetMethod("ReloadLanguage", BindingFlags.Instance | BindingFlags.NonPublic), ReloadOnLangFileChange);


            LoadEntries();
        }
        //private static void ReloadEncycloradiaOnLocalizationUpdate(ILContext il)
        //{
        //    ILCursor cursor = new ILCursor(il);

        //    if (!cursor.TryGotoNext(MoveType.After,
        //        i => i.MatchCall(typeof(Utils), nameof(Utils.LogAndChatAndConsoleInfoMessage))))
        //    {
        //        LogIlError("Encycloradia System Localization Reloading", "Couldn't navigate to after LogAndChatAndConsoleInfoMessage");
        //        return;
        //    }
        //    cursor.EmitDelegate(ReloadEncycloradia);
        //}

        //private static void ReloadEncycloradiaOnLanguageChange(LanguageManager languageManager) => ReloadEncycloradia();

        private static void ReloadOnLangFileChange(Action<LanguageManager, bool> orig, LanguageManager self, bool resetValuesToKeysFirst)
        { 
            orig(self, resetValuesToKeysFirst);
            ReloadEncycloradia();
        }

        public static void Unload()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            WorldFile.OnWorldLoad -= BuildAndSortEntries;
            //LanguageManager.Instance.OnLanguageChanged -= ReloadEncycloradiaOnLanguageChange;
            if (LocalizationLoader_ReloadLanguage_Hook.IsApplied)
                LocalizationLoader_ReloadLanguage_Hook.Undo();

            EncycloradiaEntries = null;
        }

        private static void BuildAndSortEntries()
        {
            AssembleEntries();
            RebuildCategoryPages();
        }

        /// <summary>
        /// Add an instance of all entries to load to the list of entries loaded.
        /// </summary>
        public static void LoadEntries()
        {
            foreach (Type type in Radiance.Instance.Code.GetTypes().Where(t => t.IsSubclassOf(typeof(EncycloradiaEntry)) && !t.IsAbstract))
            {
                EncycloradiaEntry entry = (EncycloradiaEntry)Activator.CreateInstance(type);
                AddEntry(entry);
            }
        }

        /// <summary>
        /// Takes all entries loaded and sorts them into <see cref="EntriesByCategory"/> by category enum.
        /// </summary>
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

        /// <summary>
        /// Sorts all categorized entries by whether they are unlocked or incomplete, and then by name.
        /// </summary>
        public static void SortEntries()
        {
            foreach (var kvp in EntriesByCategory)
            {
                EntriesByCategory[kvp.Key] = EntriesByCategory[kvp.Key].OrderBy(x => x.unlockedStatus).ThenBy(x => x.internalName).ToList();
            }
        }

        public static void RebuildCategoryPages()
        {
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
            if (entry.internalName == string.Empty)
                return entry.GetType().Name;

            return entry.internalName;
        }

        public static void AddEntry(EncycloradiaEntry entry)
        {
            EncycloradiaEntry entryToAdd = (EncycloradiaEntry)entry.Clone();
            if (entryToAdd.internalName == string.Empty)
                entryToAdd.internalName = entryToAdd.GetType().Name;

            LocalizedText displayName = LanguageManager.Instance.GetOrRegister($"Mods.{entryToAdd.mod.Name}.Encycloradia.Entries.{entryToAdd.internalName}.DisplayName");
            LanguageManager.Instance.GetOrRegister($"Mods.{entryToAdd.mod.Name}.Encycloradia.Entries.{entryToAdd.internalName}.Tooltip");

            AddPagesToEntry(entryToAdd);
            EncycloradiaEntries.Add(entryToAdd);
#if DEBUG
            Radiance.Instance.Logger.Info($"Loaded Encycloradia entry \"{GetUninitializedEntryName(entryToAdd)}\" ({entryToAdd.internalName}).");
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

            #endregion CategoryPage Testing
        }

        public static void AddPagesToEntry(EncycloradiaEntry entry)
        {
            for (int i = 0; i < entry.pages.Count; i++)
            {
                EncycloradiaPage page = entry.pages[i];
                page.keys ??= new LocalizedText[] { LanguageManager.Instance.GetOrRegister($"Mods.{entry.mod.Name}.Encycloradia.Entries.{entry.internalName}.{page.GetType().Name}_{i}") };
            }
            List<EncycloradiaPage> pagesToAdd = new List<EncycloradiaPage>();
            foreach (EncycloradiaPage page in entry.pages)
            {
                if (page.GetType() == typeof(TextPage))
                {
                    List<TextPage> textPages = ProcessTextPage(page.keys);
                    textPages.ForEach(pagesToAdd.Add);
                    continue;
                }
                pagesToAdd.Add(page);
            }

            int index = 0;
            pagesToAdd.ForEach(x => x.index = index++);
            entry.pages = pagesToAdd;
        }

        internal static List<TextPage> ProcessTextPage(LocalizedText[] keys)
        {
            DynamicSpriteFont font = FontAssets.MouseText.Value;
            List<TextPage> pagesToAdd = new();

            List<string> currentLineForDrawing = new List<string>();
            List<string> currentLineForSizeComparison = new List<string>();
            List<string> linesForCurrentPage = new List<string>();
            string[] pageSplitIntoWords = string.Join(" ", keys.Select(x => x.Value)).Split(" ");

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
                        // if the first line of the page is exclusively a newline, skip over it so that there isn't strange empty line at the start of some pages
                        if (linesForCurrentPage.Count == 0 && wordWithParsing == "\n")
                        {
                            wordWithParsing = string.Empty;
                            continue;
                        }

                        // due to how it's designed, if the line would spill over with the newline-including word, push that word down a line instead and then go back to the start of this newline
                        bool spillOver = font.MeasureString(string.Join(" ", currentLineForSizeComparison) + wordWithoutParsing).X * EncycloradiaUI.LINE_SCALE >= EncycloradiaUI.MAX_PIXELS_PER_LINE;
                        //wordWithParsing = wordWithParsing.TrimEnd('&');

                        if (!spillOver)
                            currentLineForDrawing.Add(wordWithParsing);

                        // add line to list of lines for current page, check if the 'lines to add' list is at the max line count (and add a page to entry if it is) and then reset for next line
                        linesForCurrentPage.Add(string.Join(" ", currentLineForDrawing));
                        if (linesForCurrentPage.Count >= EncycloradiaUI.MAX_LINES_PER_PAGE)
                        {
                            pagesToAdd.Add(new TextPage() { text = string.Join("\n", linesForCurrentPage), keys = keys });
                            linesForCurrentPage.Clear();
                        }
                        currentLineForDrawing.Clear();
                        currentLineForSizeComparison.Clear();

                        if (spillOver)
                        {
                            currentLineForDrawing.Add(wordWithParsing);
                            j--;
                        }

                        wordWithoutParsing = string.Empty;
                        wordWithParsing = string.Empty;
                        continue;
                    }

                    #endregion Newline Parsing

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
                    if (character == ']')
                        continue;

                    if (colonsLeft > 0)
                    {
                        if (character == ':')
                            colonsLeft--;
                        continue;
                    }

                    #endregion Bracket Parsing

                    wordWithoutParsing += character;
                }

                // if length of line is greater than the max length, add current line to list of lines for page, create page if at line limit, and then reset for next line
                float stringLength = font.MeasureString(string.Join(" ", currentLineForSizeComparison) + wordWithoutParsing).X;
                if (stringLength * EncycloradiaUI.LINE_SCALE >= EncycloradiaUI.MAX_PIXELS_PER_LINE)
                {
                    linesForCurrentPage.Add(string.Join(" ", currentLineForDrawing));
                    if (linesForCurrentPage.Count >= EncycloradiaUI.MAX_LINES_PER_PAGE)
                    {
                        pagesToAdd.Add(new TextPage() { text = string.Join("\n", linesForCurrentPage), keys = keys });
                        linesForCurrentPage.Clear();
                    }
                    currentLineForDrawing.Clear();
                    currentLineForSizeComparison.Clear();
                }
                currentLineForDrawing.Add(wordWithParsing);
                currentLineForSizeComparison.Add(wordWithoutParsing);
            }

            // if there's still remainder of a page to be added, add it after looping through every word
            if (linesForCurrentPage.Count != 0 || currentLineForDrawing.Count != 0)
            {
                if (currentLineForDrawing.Count != 0)
                    linesForCurrentPage.Add(string.Join(" ", currentLineForDrawing));

                pagesToAdd.Add(new TextPage() { text = string.Join("\n", linesForCurrentPage), keys = keys });
            }
            return pagesToAdd;
        }

        public static void ReloadEncycloradia()
        {
            // if the entries haven't been sorted yet (and thus the player hasn't entered a world yet in this instance of the game), don't try to sort them, just load
            EncycloradiaEntries.Clear();
            LoadEntries();

            if (EntriesByCategory is null)
                return;

            string name = string.Empty;
            if(Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().currentEntry is not null)
                name = Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().currentEntry.internalName;

            AssembleEntries();
            RebuildCategoryPages();

            if(name != string.Empty)
                Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().currentEntry = FindEntry(name);
        }

        public static EncycloradiaEntry FindEntry<T>() where T : EncycloradiaEntry => EncycloradiaEntries.FirstOrDefault(x => x.GetType() == typeof(T));

        public static EncycloradiaEntry FindEntry(string name) => EncycloradiaEntries.FirstOrDefault(x => x.internalName == name);

        public static EncycloradiaEntry FindEntryByFastNavInput(string input) => EncycloradiaEntries.FirstOrDefault(x => x.fastNavInput == input);
        public static void ThrowEncycloradiaError(string text, bool reset = false)
        {
            Main.NewText($"Encycloradia Error: {text}", 255, 0, 103);
            if(reset)
            {
                Encycloradia encycloradiaInstance = EncycloradiaUI.Instance.encycloradia;
                encycloradiaInstance.GoToEntry(FindEntry<TitleEntry>());
            }
        }
    }
}