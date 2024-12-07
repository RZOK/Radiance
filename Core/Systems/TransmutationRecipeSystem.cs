using System.Text.RegularExpressions;
using Radiance.Content.Tiles.Transmutator;
using Terraria.Localization;
using MonoMod.RuntimeDetour;

namespace Radiance.Core.Systems
{
    public class TransmutationRecipeSystem
    {
        public static List<TransmutationRecipe> transmutationRecipes;
        private static Hook AddConsumeItemCallback_Hook;
        public static List<int> potionTypes;
        public static void Load()
        {
            AddConsumeItemCallback_Hook ??= new Hook(typeof(Recipe).GetMethod("AddConsumeItemCallback"), CreatePotionDispersalRecipe);

            if (!AddConsumeItemCallback_Hook.IsApplied)
                AddConsumeItemCallback_Hook.Apply();

            transmutationRecipes = new List<TransmutationRecipe>();
            potionTypes = new List<int>();
            AddTransmutationRecipes();
        }
        // the most robust way of doing potion dispersal recipes i've found
        private static Recipe CreatePotionDispersalRecipe(Func<Recipe, Recipe.ConsumeItemCallback, Recipe> orig, Recipe self, Recipe.ConsumeItemCallback callback)
        {
            if (callback.GetType() == Recipe.ConsumptionRules.Alchemy.GetType())
            {
                Item item = self.createItem;
                if (item.buffType > 0 && item.buffTime > 0 && item.consumable && item.maxStack > 1)
                {
                    TransmutationRecipe potionRecipe = new TransmutationRecipe();
                    if (item.type < ItemID.Count)
                        potionRecipe.id = $"{ItemID.Search.GetName(item.type)}_PotionDispersal";
                    else
                        potionRecipe.id = $"{ItemLoader.GetItem(item.type).Name}_PotionDispersal";

                    potionRecipe.inputItems = new int[] { item.type };
                    potionRecipe.requiredRadiance = 100;
                    potionRecipe.unlock = UnlockCondition.DownedEvilBoss;
                    AddRecipe(potionRecipe);

                    potionTypes.Add(self.createItem.type);
                }
            }
            return orig(self, callback);
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
                    if (item.ModItem is ModItem modItem && modItem is ITransmutationRecipe recipeHaver)
                    {
                        TransmutationRecipe recipe = new TransmutationRecipe();
                        recipe.outputItem = item.type;
                        recipeHaver.AddTransmutationRecipe(recipe);
                        AddRecipe(recipe);
                    }
                }
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

        public UnlockCondition unlock = UnlockCondition.UnlockedByDefault;
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