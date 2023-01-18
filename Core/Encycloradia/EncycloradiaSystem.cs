﻿using Microsoft.Xna.Framework.Graphics;
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
                entries.Add(entry);
            }
        }

        public static void AddToEntry(EncycloradiaEntry entry, EncycloradiaPage page)
        {
            if (page.GetType() == typeof(TextPage) && page.text != null)
            {
                #region shit

                //if (GetEndingWord(page as TextPage).Item2 != null)
                //{
                //    bool madeFirstPage = false;
                //    TextPage currentPage = page as TextPage;
                //    while (GetEndingWord(currentPage).Item2 != null) //while the page still has < 3550 pixels of space
                //    {
                //        (int, CustomTextSnippet) intSnippet = GetEndingWord(currentPage);
                //        int word = intSnippet.Item1;
                //        CustomTextSnippet endSnippet = intSnippet.Item2;

                //        List<CustomTextSnippet> snippetList = new();
                //        CustomTextSnippet snippet = new CustomTextSnippet("", endSnippet.color, endSnippet.backgroundColor);

                //        if (!madeFirstPage) //make the first page
                //        {
                //            TextPage firstPage = new TextPage(); //the first page
                //            CustomTextSnippet endFirstSnippet = new CustomTextSnippet("", endSnippet.color, endSnippet.backgroundColor); //the snippet that ends the page
                //            List<CustomTextSnippet> firstSnippetList = new(); //list of snippets that go in `firstPage`

                //            for (int i = 0; i < Array.IndexOf(page.text, endSnippet); i++)
                //                firstSnippetList.Add(page.text[i]);
                //            for (int i = 0; i < word; i++)
                //                endFirstSnippet.text += (i == 0 ? "" : " ") + endSnippet.text.Split()[i];
                //            firstSnippetList.Add(endFirstSnippet);

                //            firstPage.text = firstSnippetList.ToArray();
                //            ForceAddPage(entry, firstPage);
                //            madeFirstPage = true;
                //        }

                //        for (int i = word; i < endSnippet.text.Split().Length; i++)
                //            snippet.text += (i == word ? "" : " ") + endSnippet.text.Split()[i];
                //        snippetList.Add(snippet);

                //        for (int i = Array.IndexOf(currentPage.text, endSnippet) + 1; i < currentPage.text.Length; i++)
                //            snippetList.Add(currentPage.text[i]);

                //        TextPage newPage = new TextPage() { text = snippetList.ToArray() };
                //        currentPage = newPage;
                //        ForceAddPage(entry, newPage);
                //    }
                //    return;
                //}
                #endregion shit

                DynamicSpriteFont font = FontAssets.MouseText.Value;

                float gap = 3550;
                string sizeLine = string.Empty;
                foreach (CustomTextSnippet snippet in page.text)
                {
                    foreach (string word in snippet.text.Split())
                    {
                        if (word == "NEWLINE") 
                        {
                            gap -= textDistance;
                            continue;
                        }
                        sizeLine += word;
                        if(font.MeasureString(sizeLine).X > gap)
                            goto breakLoop;
                    }
                }
                Console.WriteLine(entry.name + " skipped long loading.");
                ForceAddPage(entry, page);
                return;

                breakLoop:
                List<CustomTextSnippet> snippetsToAddToPage = new();
                TextPage textPage = new TextPage();
                float textSize = 0;
                int wordIndex = 0;
                int snippetIndex = 0;
                for (int h = snippetIndex; h < page.text.Length; h++)
                {
                    snippetIndex++;
                    CustomTextSnippet newSnippet = new CustomTextSnippet(string.Empty, page.text[h].color, page.text[h].backgroundColor);
                    for (int i = wordIndex; i < page.text[h].text.Split().Length; i++)
                    {
                        Console.WriteLine(wordIndex + ", " + snippetIndex + ", ");
                        wordIndex++;
                        textSize += page.text[h].text.Split()[i] == "NEWLINE" ? textDistance : font.MeasureString(page.text[h].text.Split()[i]).X;
                        if (textSize > 3550)
                        {
                            textSize = 0;
                            textPage.text = snippetsToAddToPage.ToArray();
                            ForceAddPage(entry, textPage);
                            snippetsToAddToPage.Clear();
                            textPage = new TextPage();
                            break;
                        }
                        newSnippet.text += page.text[h].text.Split()[i];
                        if (wordIndex == page.text[h].text.Split().Length)
                        {
                            wordIndex = 0;
                            snippetsToAddToPage.Add(newSnippet);
                        }
                    }
                }
            }
        }

        public static void ForceAddPage(EncycloradiaEntry entry, EncycloradiaPage page)
        {
            page.number = entry.pageIndex;
            entry.pages.Add(page);
            entry.pageIndex++;
        }

        public static (int, CustomTextSnippet) GetEndingWord(TextPage page)
        {
            DynamicSpriteFont font = FontAssets.MouseText.Value;
            string oneBigAssLine = String.Empty;
            int gap = 3550;
            foreach (CustomTextSnippet snippet in page.text)
            {
                int wordIndex = 0;
                foreach (string word in snippet.text.Split())
                {
                    if (word == "NEWLINE") gap -= textDistance;
                    oneBigAssLine += word;
                    wordIndex++;
                    if (font.MeasureString(oneBigAssLine).X >= gap)
                        return (wordIndex, snippet);
                }
            }
            return (0, null);
        }

        public static EncycloradiaEntry FindEntry(string name) => entries.FirstOrDefault(x => x.name == name) == default(EncycloradiaEntry) ? null : entries.FirstOrDefault(x => x.name == name);

        public static EncycloradiaEntry FindEntryByFastNavInput(string input) => entries.FirstOrDefault(x => x.fastNavInput == input) == default(EncycloradiaEntry) ? null : entries.FirstOrDefault(x => x.fastNavInput == input);
    }
}