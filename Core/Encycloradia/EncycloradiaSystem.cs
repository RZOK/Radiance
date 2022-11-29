using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.Core.TmodFile;
using Terraria.ModLoader.Core;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.ID;
using rail;

namespace Radiance.Core.Encycloradia
{
    public class EncycloradiaLoader : ModSystem
    {
        public List<EncycloradiaEntry> entries = new();
        public enum PageType
        {
            Blank,
            Title,
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
        public enum UnlockBoolean
        {
            unlockedByDefault,

            #region Prehardmode
            downedEyeOfCthulhu,
            downedGoblins,
            downedEvilBoss,
            downedQueenBee,
            downedSkeletron,
            #endregion

            #region Hardmode
            hardmode,
            downedAnyMech,
            downedDestroyer,
            downedTwins,
            downedSkeletronPrime,
            downedPlantera,
            downedGolem,
            downedCultist,
            downedMoonlord
            #endregion
        }
        public Dictionary<UnlockBoolean, bool> unlockMethods = new()
        {
            { UnlockBoolean.unlockedByDefault, true },

            #region Prehardmode
            { UnlockBoolean.downedEyeOfCthulhu, NPC.downedBoss1 },
            { UnlockBoolean.downedGoblins, NPC.downedGoblins },
            { UnlockBoolean.downedEvilBoss, NPC.downedBoss2 },
            { UnlockBoolean.downedQueenBee, NPC.downedQueenBee },
            { UnlockBoolean.downedSkeletron, NPC.downedBoss3 },
            #endregion

            #region Hardmode
            { UnlockBoolean.hardmode, Main.hardMode },
            { UnlockBoolean.downedAnyMech, NPC.downedMechBossAny },
            { UnlockBoolean.downedDestroyer, NPC.downedMechBoss1 },
            { UnlockBoolean.downedTwins, NPC.downedMechBoss2 },
            { UnlockBoolean.downedSkeletronPrime, NPC.downedMechBoss3 },
            { UnlockBoolean.downedPlantera, NPC.downedPlantBoss },
            { UnlockBoolean.downedGolem, NPC.downedGolemBoss },
            { UnlockBoolean.downedCultist, NPC.downedAncientCultist },
            { UnlockBoolean.downedMoonlord, NPC.downedMoonlord },
            #endregion
        };

        public class EncycloradiaPage
        {
            public int number = 0;
            public string text = String.Empty;
            public PageType pageType = PageType.Blank;
        }
        public class EncycloradiaEntry
        {
            public string name = String.Empty;
            public UnlockBoolean incomplete = UnlockBoolean.unlockedByDefault;
            public UnlockBoolean unlock = UnlockBoolean.unlockedByDefault;
            public EntryCategory category = EntryCategory.None;
            public Texture2D icon = null;
            public List<EncycloradiaPage> pages = new();
        }

        //[AttributeUsage(AttributeTargets.Class, Inherited = false)]
        //public sealed class AddEntryAttribute : Attribute
        //{
        //    public AddEntryAttribute(params string[] names)
        //    {

        //    }
        //}

        public override void Load()
        {
            AddManualEntries();
            //LoadTextEntries();
        }
        public void AddEntry(string name, UnlockBoolean unlock, UnlockBoolean incomplete, EntryCategory category, Texture2D icon, params EncycloradiaPage[] pages)
        {
            EncycloradiaEntry entry = new()
            {
                name = name,
                unlock = unlock,
                incomplete = incomplete,
                category = category,
                icon = icon,
                pages = pages.ToList()
            };
            entries.Add(entry);
        }
        public void AddManualEntries()
        {
            AddEntry("TestEntry", UnlockBoolean.unlockedByDefault, UnlockBoolean.unlockedByDefault, EntryCategory.None, TextureAssets.Item[ItemID.ManaCrystal].Value, new EncycloradiaPage[]
            {
                new EncycloradiaPage() {number = 0, pageType = PageType.Text, text = "Test Page One"},
                new EncycloradiaPage() {number = 1, pageType = PageType.Text, text = "Test Page Two"},
            });
        }
        public EncycloradiaEntry FindEntry(string name) => entries.FirstOrDefault(x => x.name == name) == default(EncycloradiaEntry) ? null : entries.FirstOrDefault(x => x.name == name);
        //public void LoadTextEntries()
        //{
        //    if (Main.dedServ)
        //        return;

        //    MethodInfo info = typeof(Mod).GetProperty("File", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
        //    var file = (TmodFile)info.Invoke(Radiance.Instance, null);

        //    var shaders = file.Where(n => n.Name.StartsWith("Core/Encycloradia/Entries/") && n.Name.EndsWith(".txt"));

        //    foreach (FileEntry entry in shaders)
        //    {
        //        var name = entry.Name.Replace(".txt", "").Replace("Core/Encycloradia/Entries/", "");
        //    }
        //}
    }
}
