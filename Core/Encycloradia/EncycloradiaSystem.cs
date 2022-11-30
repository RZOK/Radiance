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
using Radiance.Content.Items.BaseItems;
using System.ComponentModel;
using Radiance.Content.Items.RadianceCells;

namespace Radiance.Core.Encycloradia
{
    public class EncycloradiaSystem : ModSystem
    {
        public class UnlockDictionary<Tkey, Tvalue> : Dictionary<Tkey, Tvalue>
        {
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
            public ExtendedDictonary() : base() { }
            
            public event EventHandler ValueChanged;
            public void OnValueChanged(Object sender, EventArgs e)
            {
                EventHandler handler = ValueChanged;
                if (null != handler) handler(this, EventArgs.Empty);
            }
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

        }
        #endregion

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

        }
        public static EncycloradiaEntry FindEntry(string name) => entries.FirstOrDefault(x => x.name == name) == default(EncycloradiaEntry) ? null : entries.FirstOrDefault(x => x.name == name);
    }
}
