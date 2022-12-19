using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.RadianceCells;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Radiance.Core.Systems.UnlockSystem;
using ReLogic.Graphics;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using Radiance.Utilities;

namespace Radiance.Core.Encycloradia
{
    public class EncycloradiaSystem : ModSystem
    {
        public static EncycloradiaSystem Instance { get; set; }
        public EncycloradiaSystem()
        {
            Instance = this;
        }


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
            public int station;
            public int result;
            public int resultStack = 1;
            public string extras = String.Empty;
        }

        public class TransmutationPage : EncycloradiaPage
        {
            public int input;
            public int output = ItemID.None;
            public int inputStack = 1;
            public int outputStack = 1;
            public float radianceRequired;
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
            public UnlockBoolean incomplete = UnlockBoolean.unlockedByDefault;
            public UnlockBoolean unlock = UnlockBoolean.unlockedByDefault;
            public EntryCategory category = EntryCategory.None;
            public int icon = ItemID.ManaCrystal;
            public List<EncycloradiaPage> pages = new();
            public bool visible = true;
            public int doublePageSize { get => (int)Math.Ceiling((float)pages.Count / 2); }
            public int pageIndex = 0;

            public virtual void SetDefaults() { }
            public virtual void PageAssembly() { }
        }
        public override void Load()
        {
            foreach (Type type in Mod.Code.GetTypes().Where(t => t.IsSubclassOf(typeof(EncycloradiaEntry))))
            {
                EncycloradiaEntry entry = (EncycloradiaEntry)Activator.CreateInstance(type);
                entry.SetDefaults();
                entry.PageAssembly();
                entries.Add(entry);
            }
            foreach (var item in Enum.GetValues(typeof(EntryCategory)))
            {
                EntryCategory category = (EntryCategory)item;
                if(category != EntryCategory.None)
                {
                    EncycloradiaEntry entry = new() 
                    {
                        name = category.ToString() + "Entry",
                        displayName = category.ToString(),
                        incomplete = UnlockBoolean.unlockedByDefault,
                        unlock = UnlockBoolean.unlockedByDefault,
                        category = category,
                        icon = ItemID.ManaCrystal,
                        visible = false
                    };
                    CustomTextSnippet[] text = Array.Empty<CustomTextSnippet>();
                    switch(category)
                    { 
                        case EntryCategory.Influencing:
                            text = new CustomTextSnippet[] { RadianceUtils.influencingSnippet,
                                new CustomTextSnippet("is the art of manipulating", Color.White, Color.Black),
                                RadianceUtils.radianceSnippet,
                                new CustomTextSnippet("with cells, rays, pedestals, and other similar means. NEWLINE NEWLINE Within this section you will find anything and everything directly related to moving and storing", Color.White, Color.Black),
                                RadianceUtils.radianceSnippet,
                                new CustomTextSnippet("in and throughout", Color.White, Color.Black),
                                RadianceUtils.apparatusesSnippet,
                                new CustomTextSnippet("and", Color.White, Color.Black),
                                RadianceUtils.instrumentsSnippetPeriod
                            };
                            break;
                    }
                    AddToEntry(entry, new TextPage() { number = 0, text = text });
                    AddToEntry(entry, new CategoryPage() { category = category });
                    entries.Add(entry);
                }
            }
        }
        public override void Unload()
        {
            entries.Clear();
        }
        public static void AddToEntry(EncycloradiaEntry entry, EncycloradiaPage page)
        {
            if (page.GetType() == typeof(TextPage) && page.text != null)
            {
                if (GetTextPagePixels(page as TextPage).Item2 != null)
                {
                    bool madeFirstPage = false;
                    TextPage currentPage = page as TextPage;
                    while (GetTextPagePixels(currentPage).Item2 != null)
                    {
                        (int, CustomTextSnippet) intSnippet = GetTextPagePixels(currentPage);
                        int word = intSnippet.Item1;
                        CustomTextSnippet endSnippet = intSnippet.Item2;

                        List<CustomTextSnippet> snippetList = new();
                        CustomTextSnippet snippet = new CustomTextSnippet("", endSnippet.color, endSnippet.backgroundColor);

                        if (madeFirstPage == false) //make the first page
                        {
                            TextPage firstPage = new TextPage();
                            CustomTextSnippet endFirstSnippet = new CustomTextSnippet("", endSnippet.color, endSnippet.backgroundColor);
                            List<CustomTextSnippet> firstSnippetList = new();

                            for (int i = 0; i < Array.IndexOf(page.text, endSnippet); i++)
                                firstSnippetList.Add(page.text[i]);
                            for (int i = 0; i < word; i++)
                                endFirstSnippet.text += (i == 0 ? "" : " ") + endSnippet.text.Split()[i];
                            firstSnippetList.Add(endFirstSnippet);

                            firstPage.text = firstSnippetList.ToArray();
                            ForceAddPage(entry, firstPage);
                            madeFirstPage = true;
                        }

                        for (int i = word; i < endSnippet.text.Split().Length; i++)
                            snippet.text += (i == word ? "" : " ") + endSnippet.text.Split()[i];
                        snippetList.Add(snippet);

                        for (int i = Array.IndexOf(currentPage.text, endSnippet) + 1; i < currentPage.text.Length; i++)
                            snippetList.Add(currentPage.text[i]);

                        TextPage newPage = new TextPage() { text = snippetList.ToArray() };
                        currentPage = newPage;
                        ForceAddPage(entry, newPage);
                    }
                    return;
                }
            }
            ForceAddPage(entry, page);
        }
        protected static void ForceAddPage(EncycloradiaEntry entry, EncycloradiaPage page)
        {
            page.number = entry.pageIndex;
            entry.pages.Add(page);
            entry.pageIndex++;
        }
        public static (int, CustomTextSnippet) GetTextPagePixels(TextPage page)
        {
            DynamicSpriteFont font = FontAssets.MouseText.Value;
            string oneBigAssLine = String.Empty;
            int gap = 3500;
            foreach (CustomTextSnippet snippet in page.text)
            {
                foreach (string word in snippet.text.Split())
                {
                    if (word == "NEWLINE") gap -= 162;
                    oneBigAssLine += word;
                    if (font.MeasureString(oneBigAssLine).X > gap)
                        return (Array.IndexOf(snippet.text.Split(), word), snippet);
                }
            }
            return (0, null);
        }
        public static EncycloradiaEntry FindEntry(string name) => entries.FirstOrDefault(x => x.name == name) == default(EncycloradiaEntry) ? null : entries.FirstOrDefault(x => x.name == name);
    }
}