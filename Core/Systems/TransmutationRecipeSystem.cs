using MonoMod.RuntimeDetour;
using Radiance.Content.Tiles.Transmutator;
using System.Text.RegularExpressions;
using Terraria.Localization;

namespace Radiance.Core.Systems
{
    public class TransmutationRecipeSystem
    {
        public static List<TransmutationRecipe> recipes;
        public static Dictionary<int, TransmutationRecipe> byInputItem;
        private static Hook AddConsumeIngredientCallback_Hook;
        private static List<int> potionTypes;
        private const string POTION_DISPERSAL_STRING = "_PotionDispersal";

        public static void Load()
        {
            AddConsumeIngredientCallback_Hook ??= new Hook(typeof(Recipe).GetMethod(nameof(Recipe.AddConsumeIngredientCallback)), CreatePotionDispersalRecipe);

            if (!AddConsumeIngredientCallback_Hook.IsApplied)
                AddConsumeIngredientCallback_Hook.Apply();

            recipes = new List<TransmutationRecipe>();
            byInputItem = new Dictionary<int, TransmutationRecipe>();
            potionTypes = new List<int>();
            AddTransmutationRecipes();
        }

        // the most robust way of doing potion dispersal recipes i've found
        private static Recipe CreatePotionDispersalRecipe(Func<Recipe, Recipe.IngredientQuantityCallback, Recipe> orig, Recipe self, Recipe.IngredientQuantityCallback callback)
        {
            if (callback.GetType() == Recipe.IngredientQuantityRules.Alchemy.GetType())
            {
                Item item = self.createItem;
                if (!potionTypes.Contains(item.type) && item.buffType > 0 && item.buffTime > 0 && item.consumable && item.maxStack > 1)
                {
                    TransmutationRecipe potionRecipe = new TransmutationRecipe();
                    if (item.type < ItemID.Count)
                        potionRecipe.id = $"{ItemID.Search.GetName(item.type)}{POTION_DISPERSAL_STRING}";
                    else
                        potionRecipe.id = $"{ItemLoader.GetItem(item.type).Name}{POTION_DISPERSAL_STRING}";

                    potionRecipe.inputItems = new int[] { item.type };
                    potionRecipe.requiredRadiance = 100;
                    potionRecipe.unlock = UnlockCondition.DownedEvilBoss;
                    AddRecipe(potionRecipe);

                    potionTypes.Add(item.type);
                }
            }
            return orig(self, callback);
        }

        public static void Unload()
        {
            recipes = null;
        }

        public static void AddTransmutationRecipes()
        {
            for (int i = 0; i < ItemLoader.ItemCount; i++)
            {
                // just in case
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
                if (recipe.id.EndsWith(POTION_DISPERSAL_STRING))
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

            TransmutationRecipe RainSummon = new TransmutationRecipe();
            RainSummon.id = nameof(RainSummon);
            RainSummon.inputItems = new int[] { ItemID.WaterCandle };
            RainSummon.requiredRadiance = 20;
            TransmutatorTileEntity.PreTransmutateItemEvent += (transmutator, recipe) =>
            {
                if (recipe.id == nameof(RainSummon) && Main.netMode != NetmodeID.MultiplayerClient)
                    Main.StartRain();

                return true;
            };
            AddRecipe(RainSummon);

            TransmutationRecipe RainStop = new TransmutationRecipe();
            RainStop.id = nameof(RainStop);
            RainStop.inputItems = new int[] { ItemID.PeaceCandle };
            RainStop.requiredRadiance = 20;
            TransmutatorTileEntity.PreTransmutateItemEvent += (transmutator, recipe) =>
            {
                if (recipe.id == nameof(RainStop) && Main.netMode != NetmodeID.MultiplayerClient)
                    Main.StopRain();

                return true;
            };
            AddRecipe(RainStop);

            #endregion Weather Control Recipes
        }

        public static TransmutationRecipe FindRecipe(string id) => recipes.First(x => x.id == id);

        public static void AddRecipe(TransmutationRecipe recipe)
        {
            if (recipe.id == string.Empty)
            {
                if (recipe.outputItem < ItemID.Count)
                    recipe.id = Regex.Replace(ItemID.Search.GetName(recipe.outputItem), @"\s+", "");
                else
                    recipe.id = ItemLoader.GetItem(recipe.outputItem).Name;
            }

            if (recipes.Any(x => x.id == recipe.id))
                Radiance.Instance.Logger.Warn($"Tried to add recipe with already existing ID '{recipe.id}'");
#if DEBUG
            else
                Radiance.Instance.Logger.Info($"Loaded Transmutation recipe '{recipe.id}'");
#endif
            recipes.Add(recipe);
            foreach (int item in recipe.inputItems)
            {
                byInputItem.Add(item, recipe);
            }
        }
    }

    public class TransmutationRecipe
    {
        public int[] inputItems = Array.Empty<int>();
        public int outputItem = ItemID.None;
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