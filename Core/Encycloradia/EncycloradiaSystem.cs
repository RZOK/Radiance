using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public const int textDistance = 300;

        public static List<EncycloradiaEntry> entries;

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
            public string text;
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
            public (Item, int) result; //todo
            public string extras = String.Empty;
        }

        public class TransmutationPage : EncycloradiaPage
        {
            public TransmutationRecipe recipe = new TransmutationRecipe();
            public int currentItemIndex = 0;
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
        }

        public override void Load()
        {
            entries = new List<EncycloradiaEntry>();
        }

        public override void Unload()
        {
            entries = null;
        }

        public void LoadEntries()
        {
            foreach (Type type in Mod.Code.GetTypes().Where(t => t.IsSubclassOf(typeof(EncycloradiaEntry))))
            {
                EncycloradiaEntry entry = (EncycloradiaEntry)Activator.CreateInstance(type);
                entry.name = entry.GetType().Name;
                //string entryString = "Entry";
                //entry.displayName = string.Join(" ", Regex.Split(entry.name.EndsWith(entryString) ? entry.name.Remove(entry.name.LastIndexOf(entryString, StringComparison.Ordinal)) : entry.name, @"(?<!^)(?=[A-Z]|\d)")); //remove 'Entry' from the end of the entry, split it by uppercase chars and numbers, join it all with whitespace
                entries.Add(entry);
            }
        }

        public static void AddToEntry(EncycloradiaEntry entry, EncycloradiaPage page)
        {
            if (page.GetType() == typeof(TextPage) && page.text != null)
            {
                DynamicSpriteFont font = FontAssets.MouseText.Value;
                List<string> stringList = new() { @"\r" };
                List<string> lineList = new();
                float lineLength = textDistance / Radiance.encycolradiaLineScale;
                int lineCount = 0;
                for (int h = 0; h < page.text.Split().Length; h++)
                {
                    string word = page.text.Split()[h];
                    if (word == "|")
                    {
                        if (lineCount == 0 && lineList.Count == 0)
                            continue;
                        lineCount += 2;
                        lineList.Clear();
                        stringList.Add("|");
                        if (lineCount >= 15)
                        {
                            lineCount = 0;
                            TextPage textPage = new TextPage() { text = string.Join(" ", stringList) };
                            stringList.Clear();
                            ForceAddPage(entry, textPage);
                            h--;
                            continue;
                        }
                    }
                    else if (!word.StartsWith(@"\"))
                        lineList.Add(word);

                    if (font.MeasureString(string.Join(" ", lineList)).X >= lineLength + lineCount * 0.33f)
                    {
                        lineCount++;
                        lineList.Clear();
                        lineList.Add(word);
                        if (lineCount >= 15)
                        {
                            lineCount = 0;
                            TextPage textPage = new TextPage() { text = string.Join(" ", stringList) };
                            ForceAddPage(entry, textPage);
                            stringList.Clear();
                            h--;
                            continue;
                        }
                        else
                            stringList.Add("|");
                    }
                    stringList.Add(word);
                    if (h == page.text.Split().Length - 1)
                        ForceAddPage(entry, new TextPage() { text = string.Join(" ", stringList) });
                }
            }
            else
                ForceAddPage(entry, page);
        }

        public static void ForceAddPage(EncycloradiaEntry entry, EncycloradiaPage page)
        {
            page.number = entry.pages.Count;
            entry.pages.Add(page);
            entry.pageIndex++;
        }

        public static EncycloradiaEntry FindEntry<T>() where T : EncycloradiaEntry => entries.FirstOrDefault(x => x.GetType() == typeof(T));

        public static EncycloradiaEntry FindEntry(string name) => entries.FirstOrDefault(x => x.name == name);

        public static EncycloradiaEntry FindEntryByFastNavInput(string input) => entries.FirstOrDefault(x => x.fastNavInput == input);
    }
}