using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.RadianceCells;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Radiance.Core.Systems.TransmutationRecipeSystem;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Core.Encycloradia
{
    public class EncycloradiaSystem : ModSystem
    {
        public static EncycloradiaSystem Instance { get; set; }

        public EncycloradiaSystem()
        {
            Instance = this;
        }

        public const int textDistance = 184;

        public static List<EncycloradiaEntry> entries = new();

        public enum PageType
        {
            Blank,
            Title,
            Category,
            Text,
            CraftingRecipe,
            TransmutationRecipe,
            Image
        }

        public enum EntryCategory
        {
            None,
            Influencing,
            Transmutation,
            Apparatuses,
            Instruments,
            Pedestalworks,
            Phenomena
        }

        #region Pages

        public abstract class EncycloradiaPage
        {
            public int number = 0;
            public CustomTextSnippet[] text;
        }

        public class TextPage : EncycloradiaPage
        {
        }

        public class CategoryPage : EncycloradiaPage
        {
            public EntryCategory category = EntryCategory.None;
        }

        public class ImagePage : EncycloradiaPage
        {
            public Texture2D texture;
        }

        public class RecipePage : EncycloradiaPage
        {
            public Dictionary<int, int> items;
            public Item station;
            public Item result;
            public string extras = String.Empty;
        }

        public class TransmutationPage : EncycloradiaPage
        {
            public TransmutationRecipe recipe = new TransmutationRecipe();
            public BaseContainer container = Radiance.Instance.GetContent<StandardRadianceCell>() as BaseContainer;
        }

        public class MiscPage : EncycloradiaPage
        {
            public string type = String.Empty;
        }

        #endregion Pages

        public class EncycloradiaEntry
        {
            public string name = String.Empty;
            public string displayName = String.Empty;
            public string tooltip = String.Empty;
            public string fastNavInput = String.Empty;
            public UnlockBoolean incomplete = UnlockBoolean.unlockedByDefault;
            public UnlockBoolean unlock = UnlockBoolean.unlockedByDefault;
            public EntryCategory category = EntryCategory.None;
            public int icon = ItemID.ManaCrystal;
            public List<EncycloradiaPage> pages = new();
            public bool visible = true;
            public int pageIndex = 0;

            public virtual void SetDefaults()
            { }

            public virtual void PageAssembly()
            { }
        }

        public override void Unload()
        {
            entries.Clear();
        }

        public void LoadEntries()
        {
            foreach (Type type in Mod.Code.GetTypes().Where(t => t.IsSubclassOf(typeof(EncycloradiaEntry))))
            {
                string entryString = "Entry";
                EncycloradiaEntry entry = (EncycloradiaEntry)Activator.CreateInstance(type);
                entry.name = entry.GetType().Name;
                entry.displayName = string.Join(" ", Regex.Split(entry.name.EndsWith(entryString) ? entry.name.Remove(entry.name.LastIndexOf(entryString, StringComparison.Ordinal)) : entry.name, @"(?<!^)(?=[A-Z]|\d)")); //remove 'Entry' from the end of the entry, split it by uppercase chars and numbers, join it all with whitespace
                entry.SetDefaults();
                entry.PageAssembly();
                if (entry.pages.Count % 2 == 1)
                    entry.pages.Add(new MiscPage() { number = entry.pages.Count }); //add an extra blank page to the end of each entry if it has an odd amount of pages. temporay solution until i just make it not read the right page if it doesn't exist
                entries.Add(entry);
            }
        }

        public static void AddToEntry(EncycloradiaEntry entry, EncycloradiaPage page)
        {
            if (page.GetType() == typeof(TextPage) && page.text != null)
            {
                DynamicSpriteFont font = FontAssets.MouseText.Value;
                List<CustomTextSnippet> snippetsToAddToPage = new();
                List<string> stringList = new();
                List<string> lineList = new();
                TextPage textPage = new TextPage();
                float lineLength = textDistance;
                int lineCount = 1;
                int snippetIndex = 0;
                for (int h = snippetIndex; h < page.text.Length; h++)
                {
                    snippetIndex++;
                    CustomTextSnippet snippet = page.text[h];
                    CustomTextSnippet newSnippet = new CustomTextSnippet(string.Empty, snippet.color, snippet.backgroundColor);

                    int wordIndex = 0;
                    for (int i = wordIndex; i < snippet.text.Split().Length; i++)
                    {
                        wordIndex++;
                        string word = snippet.text.Split()[i];
                        stringList.Add(word);
                        if (word == "|")
                        {
                            lineCount++;
                            lineList.Clear();
                            if (lineCount >= 15)
                            {
                                lineCount = 1;
                                newSnippet.text = string.Join(" ", stringList);
                                snippetsToAddToPage.Add(newSnippet);
                                textPage.text = snippetsToAddToPage.ToArray();
                                stringList.Clear();
                                snippetsToAddToPage.Clear();
                                ForceAddPage(entry, textPage);
                                newSnippet.text = string.Empty;
                                textPage = new TextPage();
                                continue;
                            }
                        }
                        else
                            lineList.Add(word);
                        if (font.MeasureString(string.Join(" ", lineList)).X >= lineLength)
                        {
                            lineCount++;
                            lineList.Clear();
                            if (lineCount >= 15)
                            {
                                lineCount = 1;
                                newSnippet.text = string.Join(" ", stringList);
                                snippetsToAddToPage.Add(newSnippet);
                                textPage.text = snippetsToAddToPage.ToArray();
                                stringList.Clear();
                                snippetsToAddToPage.Clear();
                                ForceAddPage(entry, textPage);
                                newSnippet.text = string.Empty;
                                textPage = new TextPage();
                                continue;
                            }
                            else
                                stringList.Add("|");
                        }
                        if (wordIndex == snippet.text.Split().Length)
                        {
                            newSnippet.text = string.Join(" ", stringList);
                            snippetsToAddToPage.Add(newSnippet);
                            stringList.Clear();
                            if (snippetIndex == page.text.Length)
                            {
                                textPage.text = snippetsToAddToPage.ToArray();
                                ForceAddPage(entry, textPage);
                            }
                        }
                    }
                }
            }
            else
                ForceAddPage(entry, page);
        }

        public static void ForceAddPage(EncycloradiaEntry entry, EncycloradiaPage page)
        {
            page.number = entry.pageIndex;
            entry.pages.Add(page);

            if (page.GetType() == typeof(TextPage) && page.text != null)
            {
                foreach (CustomTextSnippet snip in page.text)
                {
                    Console.WriteLine(page.number + ", " + snip.text); //everything is displayed properly how it should. output: https://i.imgur.com/ejBvh2L.png
                }
            }

            entry.pageIndex++;
        }

        public static EncycloradiaEntry FindEntry(string name) => entries.FirstOrDefault(x => x.name == name) == default(EncycloradiaEntry) ? null : entries.FirstOrDefault(x => x.name == name);

        public static EncycloradiaEntry FindEntryByFastNavInput(string input) => entries.FirstOrDefault(x => x.fastNavInput == input) == default(EncycloradiaEntry) ? null : entries.FirstOrDefault(x => x.fastNavInput == input);
    }
}