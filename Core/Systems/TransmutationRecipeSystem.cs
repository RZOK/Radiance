using Radiance.Core.Encycloradia;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static Radiance.Core.Systems.UnlockSystem;
using static Radiance.Core.Systems.TransmutationRecipeSystem;
using Radiance.Content.Tiles.Transmutator;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Security.Permissions;
using Terraria.Localization;

namespace Radiance.Core.Systems
{
    public class TransmutationRecipeSystem
    {
        public static List<TransmutationRecipe> transmutationRecipes;

        public static void Load()
        {
            transmutationRecipes = new List<TransmutationRecipe>();
            AddTransmutationRecipes();
        }

        public static void Unload()
        {
            transmutationRecipes = null;
        }

        public static void AddTransmutationRecipes()
        {
            for (int i = 0; i < ItemLoader.ItemCount; i++)
            {
                if (i <= 0 || i >= ItemLoader.ItemCount)
                    continue;

                Item item = GetItem(i);
                if (item.type >= ItemID.Count)
                {
                    ModItem modItem = item.ModItem;
                    if (modItem != null && modItem is ITransmutationRecipe recipeHaver)
                    {
                        TransmutationRecipe recipe = new TransmutationRecipe();
                        recipe.outputItem = item.type;
                        recipeHaver.AddTransmutationRecipe(recipe);
                        AddRecipe(recipe);
                    }
                }
                #region Potion Dispersal
                if (item.buffType > 0 && item.buffTime > 0 && item.consumable && item.maxStack > 1 && item.Name.Contains("Potion")) // todo: item.Name gets only the localized name
                {
                    TransmutationRecipe potionRecipe = new TransmutationRecipe();
                    if (item.type < ItemID.Count)
                        potionRecipe.id = $"{ItemID.Search.GetName(item.type)}_PotionDispersal";
                    else
                        potionRecipe.id = $"{ItemLoader.GetItem(item.type).Name}_PotionDispersal";

                    potionRecipe.inputItems = new int[] { item.type };
                    potionRecipe.requiredRadiance = 100;
                    potionRecipe.unlock = UnlockCondition.downedEvilBoss;
                    AddRecipe(potionRecipe);
                }

                #endregion Potion Dispersal
            }
            TransmutatorTileEntity.PreTransmutateItemEvent += (transmutator, recipe) =>
            {
                if (recipe.id.EndsWith("_PotionDispersal"))
                {
                    Item item = transmutator.GetSlot(0);
                    if (transmutator.activeBuff == item.buffType)
                        transmutator.activeBuffTime += item.buffTime * 4;
                    else
                    {
                        transmutator.activeBuff = item.buffType;
                        transmutator.activeBuffTime = item.buffTime * 4;
                    }
                }
                return true;
            };

            #region Weather Control Recipes

            TransmutationRecipe rainRecipe = new TransmutationRecipe();
            rainRecipe.id = "RainSummon";
            rainRecipe.inputItems = new int[] { ItemID.WaterCandle };
            rainRecipe.requiredRadiance = 20;
            TransmutatorTileEntity.PreTransmutateItemEvent += (transmutator, recipe) =>
            {
                if (recipe.id == "RainSummon" && Main.netMode != NetmodeID.MultiplayerClient)
                    Main.StartRain();

                return true;
            };
            AddRecipe(rainRecipe);

            TransmutationRecipe rainClearRecipe = new TransmutationRecipe();
            rainClearRecipe.id = "RainStop";
            rainClearRecipe.inputItems = new int[] { ItemID.PeaceCandle };
            rainClearRecipe.requiredRadiance = 20;
            TransmutatorTileEntity.PreTransmutateItemEvent += (transmutator, recipe) =>
            {
                if (recipe.id == "RainStop" && Main.netMode != NetmodeID.MultiplayerClient)
                    Main.StopRain();

                return true;
            };
            AddRecipe(rainClearRecipe);

            #endregion Weather Control Recipes
        }

        public static TransmutationRecipe FindRecipe(string id) => transmutationRecipes.First(x => x.id == id);

        public static void AddRecipe(TransmutationRecipe recipe)
        {
            if (recipe.id == string.Empty)
            {
                if (recipe.outputItem < ItemID.Count)
                    recipe.id = Regex.Replace(ItemID.Search.GetName(recipe.outputItem), @"\s+", "");
                else
                    recipe.id = ItemLoader.GetItem(recipe.outputItem).Name;
            }

            if (transmutationRecipes.Any(x => x.id == recipe.id))
                Radiance.Instance.Logger.Fatal($"Tried to add recipe with already existing ID '{recipe.id}'");
#if DEBUG
            Radiance.Instance.Logger.Info($"Loaded Transmutation recipe '{recipe.id}'");
#endif
            transmutationRecipes.Add(recipe);
        }
    }

    public class TransmutationRecipe
    {
        public int[] inputItems = Array.Empty<int>();
        public int outputItem = 0;
        public int requiredRadiance = 0;

        public List<TransmutationRequirement> transmutationRequirements = new List<TransmutationRequirement>();
        public ProjectorLensID lensRequired = ProjectorLensID.Flareglass;

        public UnlockCondition unlock = UnlockCondition.unlockedByDefault;
        public string id = string.Empty;
        public int inputStack = 1;
        public int outputStack = 1;
    }

    public record TransmutationRequirement(Func<TransmutatorTileEntity, bool> condition, LocalizedText tooltip)
    {
        public Func<TransmutatorTileEntity, bool> condition = condition;
        public LocalizedText tooltip = tooltip;

        public static readonly TransmutationRequirement testRequirement = new TransmutationRequirement((x) => true, LanguageManager.Instance.GetOrRegister($"Mods.{nameof(Radiance)}.Transmutation.TransmutationRequirements.TestRequirement"));
    }
}