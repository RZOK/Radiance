using Radiance.Content.Items;
using Radiance.Content.Items.Accessories;
using Radiance.Content.Items.PedestalItems;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Content.Items.RadianceCells;
using Radiance.Content.Items.StabilizationCrystals;
using Radiance.Content.Items.Tools.Misc;
using Radiance.Core.Encycloradia;
using Radiance.Core.Interfaces;
using Radiance.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Core.Systems
{
    public class TransmutationRecipeSystem : ModSystem
    {
        public static TransmutationRecipeSystem Instance;

        public TransmutationRecipeSystem()
        {
            Instance = this;
        }

        public class TransmutationRecipe
        {
            public int inputItem = 0;
            public int outputItem = 0;
            public int requiredRadiance = 0;
            public UnlockBoolean unlock = UnlockBoolean.unlockedByDefault;
            public string id = string.Empty;
            public int inputStack = 0;
            public int outputStack = 0;
            public SpecialRequirements[] specialRequirements = Array.Empty<SpecialRequirements>();
            public float specialEffectValue = 0;
            public SpecialEffects specialEffects = SpecialEffects.None;
            public ProjectorLensID lensRequired = ProjectorLensID.Flareglass;
            public float lensRequiredValue = 0;
        }

        public static TransmutationRecipe[] transmutationRecipe = new TransmutationRecipe[400];
        public static int numRecipes = 0;

        public enum SpecialRequirements
        {
            Test
        }

        public enum SpecialEffects
        {
            None,
            SummonRain,
            RemoveRain,
            PotionDisperse,
            ///<summary>Simply moves the input item to the output slot regardless of what the output item is.</summary>
            MoveToOutput
        }

        public static Dictionary<SpecialRequirements, string> reqStrings = new Dictionary<SpecialRequirements, string>()
        {
            { SpecialRequirements.Test, "Test requirement" }
        };

        public override void Load()
        {
            AddTransmutationRecipes();
            EncycloradiaSystem.Instance.LoadEntries(); //entries have to be loaded here so that recipes are loaded for recipe pages that pull recipe data directly
        }

        public override void Unload()
        {
            Array.Clear(transmutationRecipe);
            if (!Main.dedServ)
                Instance = null;
        }

        public static void AddTransmutationRecipes()
        {
            #region Influencing Recipes

            AddRecipe(new int[] { ItemID.Amethyst, ItemID.Topaz, ItemID.Sapphire, ItemID.Emerald, ItemID.Ruby, ItemID.Diamond, ItemID.Amber }, ModContent.ItemType<ShimmeringGlass>(), 5, "Flareglass", UnlockBoolean.unlockedByDefault);
            AddRecipe(new int[] { ItemID.PurificationPowder, ItemID.VilePowder, ItemID.ViciousPowder }, ModContent.ItemType<CalcificationPowder>(), 5, "CalcificationPowder", UnlockBoolean.unlockedByDefault, 1);
            AddRecipe(ModContent.ItemType<PetrifiedCrystal>(), ModContent.ItemType<StabilizationCrystal>(), 100, "StabilizationCrystal", UnlockBoolean.unlockedByDefault, 5);

            #endregion Influencing Recipes

            #region Transmutation Recipes

            for (int i = 0; i < ItemLoader.ItemCount; i++)
            {
                if (i <= 0 || i >= ItemLoader.ItemCount)
                    continue;

                Item item = RadianceUtils.GetItem(i);
                if (item.buffType > 0 && item.buffTime > 0 && item.consumable && item.maxStack > 1 && item.Name.Contains("Potion"))
                    AddRecipe(item.type, ItemID.None, 200, item.Name + "Dispersal", UnlockBoolean.downedEvilBoss, 1, 0, null, SpecialEffects.PotionDisperse, item.buffType);
            }
            AddRecipe(ItemID.WaterCandle, ItemID.None, 20, "RainSummon", UnlockBoolean.unlockedByDefault, 1, 0, default, SpecialEffects.SummonRain);
            AddRecipe(ItemID.PeaceCandle, ItemID.None, 20, "RainStop", UnlockBoolean.unlockedByDefault, 1, 0, default, SpecialEffects.RemoveRain);

            #endregion Transmutation Recipes

            #region Instrument Recipes

            AddRecipe(new int[] { ItemID.BandofRegeneration, ItemID.BandofStarpower }, ModContent.ItemType<RingofFrugality>(), 200, "RingofFrugality", UnlockBoolean.unlockedByDefault);
            AddRecipe(ItemID.AncientChisel, ModContent.ItemType<FerventMiningCharm>(), 400, "FerventMiningCharm", UnlockBoolean.downedEyeOfCthulhu);
            AddRecipe(ItemID.StoneSlab, ModContent.ItemType<GleamingWhetstone>(), 200, "GleamingWhetstone", UnlockBoolean.unlockedByDefault);
            AddRecipe(ModContent.ItemType<GleamingWhetstone>(), ModContent.ItemType<GleamingWhetstone>(), 40, "GleamingWhetstoneReforge", UnlockBoolean.unlockedByDefault, 1, 1, default, SpecialEffects.MoveToOutput);
            AddRecipe(ModContent.ItemType<IrradiantWhetstone>(), ModContent.ItemType<IrradiantWhetstone>(), 40, "IrradiantWhetstoneLock", UnlockBoolean.unlockedByDefault, 1, 1, default, SpecialEffects.MoveToOutput);

            #endregion Instrument Recipes

            #region Pedestalworks Recipes

            AddRecipe(ItemID.SoulofLight, ModContent.ItemType<OrchestrationCore>(), 100, "OrchestrationCore", UnlockBoolean.hardmode, 3);
            AddRecipe(ItemID.SoulofNight, ModContent.ItemType<AnnihilationCore>(), 100, "AnnihilationCore", UnlockBoolean.hardmode, 3);
            AddRecipe(new int[] { ItemID.CursedFlame, ItemID.Ichor }, ModContent.ItemType<FormationCore>(), 100, "FormationCore", UnlockBoolean.hardmode, 3);

            #endregion Pedestalworks Recipes
        }

        public static TransmutationRecipe FindRecipe(string id) => transmutationRecipe.FirstOrDefault(x => x != null && x.id == id);

        public static void AddRecipe(int[] inputItems, int outputItem, int requiredRadiance, string id, UnlockBoolean unlock, int inputStack = 1, int outputStack = 1, SpecialRequirements[] specialRequirement = null, SpecialEffects specialEffect = SpecialEffects.None, float specialEffectValue = 0, ProjectorLensID lens = ProjectorLensID.None, float lensValue = 0)
        {
            int idOffset = 0;
            foreach (int item in inputItems)
            {
                AddRecipe(item, outputItem, requiredRadiance, id + "_" + idOffset.ToString(), unlock, inputStack, outputStack, specialRequirement, specialEffect, specialEffectValue, lens, lensValue);
                idOffset++;
            }
        }

        public static void AddRecipe(int inputItem, int outputItem, int requiredRadiance, string id, UnlockBoolean unlock, int inputStack = 1, int outputStack = 1, SpecialRequirements[] specialRequirement = null, SpecialEffects specialEffect = SpecialEffects.None, float specialEffectValue = 0, ProjectorLensID lens = ProjectorLensID.None, float lensValue = 0)
        {
            TransmutationRecipe recipe = new()
            {
                inputItem = inputItem,
                outputItem = outputItem,
                requiredRadiance = requiredRadiance,
                id = id,
                unlock = unlock,
                inputStack = inputStack,
                outputStack = outputStack,
                specialRequirements = specialRequirement,
                specialEffects = specialEffect,
                specialEffectValue = specialEffectValue,
                lensRequired = lens,
                lensRequiredValue = lensValue
            };
            transmutationRecipe[numRecipes] = recipe;
            numRecipes++;
        }
    }
}