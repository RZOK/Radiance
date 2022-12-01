﻿using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.RadianceCells;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
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
            Title,
            Influencing,
            Contraption,
            Instrument,
            Pedestalwork,
            VoidPhenomena
        }
        #region Pages

        
        
        public abstract class EncycloradiaPage
        {
            public int number = 0;
            public string text = String.Empty;
        }

        public class TextPage : EncycloradiaPage
        {
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

        public abstract class EncycloradiaEntry
        {
            public string name = String.Empty;
            public UnlockBoolean incomplete = UnlockBoolean.unlockedByDefault;
            public UnlockBoolean unlock = UnlockBoolean.unlockedByDefault;
            public EntryCategory category = EntryCategory.None;
            public Texture2D icon = TextureAssets.Item[ItemID.ManaCrystal].Value;
            public List<EncycloradiaPage> pages;

            public abstract void SetDefaults();
            public abstract void PageAssembly();
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
        }
        public override void Unload()
        {
            entries.Clear();
        }

        public static EncycloradiaEntry FindEntry(string name) => entries.FirstOrDefault(x => x.name == name) == default(EncycloradiaEntry) ? null : entries.FirstOrDefault(x => x.name == name);
    }
}