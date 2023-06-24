using Radiance.Core.Encycloradia;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

        public static List<TransmutationRecipe> transmutationRecipes;

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
            transmutationRecipes = new List<TransmutationRecipe>();
            AddTransmutationRecipes();
            EncycloradiaSystem.Instance.LoadEntries(); //entries have to be loaded here so that recipes are loaded for recipe pages that pull recipe data directly
        }

        public override void Unload()
        {
            transmutationRecipes = null;
            if (!Main.dedServ)
                Instance = null;
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

                if (item.buffType > 0 && item.buffTime > 0 && item.consumable && item.maxStack > 1 && item.Name.Contains("Potion"))
                {
                    TransmutationRecipe potionRecipe = new TransmutationRecipe();
                    if (item.type < ItemID.Count)
                        potionRecipe.id = Regex.Replace(GetItem(item.type).Name, @"\s+", "") + "Dispersal";
                    else
                        potionRecipe.id = ItemLoader.GetItem(item.type).Name + "Dispersal";

                    potionRecipe.inputItems = new int[] { item.type };
                    potionRecipe.requiredRadiance = 100;
                    potionRecipe.specialEffects = SpecialEffects.PotionDisperse;
                    potionRecipe.specialEffectValue = item.type;
                    potionRecipe.unlock = UnlockBoolean.downedEvilBoss;
                    AddRecipe(potionRecipe);
                }
            }
            TransmutationRecipe rainRecipe = new TransmutationRecipe();
            rainRecipe.id = "RainSummon";
            rainRecipe.inputItems = new int[] { ItemID.WaterCandle };
            rainRecipe.requiredRadiance = 20;
            rainRecipe.specialEffects = SpecialEffects.SummonRain;
            AddRecipe(rainRecipe);

            TransmutationRecipe rainClearRecipe = new TransmutationRecipe();
            rainClearRecipe.id = "RainStop";
            rainClearRecipe.inputItems = new int[] { ItemID.PeaceCandle };
            rainClearRecipe.requiredRadiance = 20;
            rainClearRecipe.specialEffects = SpecialEffects.RemoveRain;
            AddRecipe(rainClearRecipe);
        }

        public static TransmutationRecipe FindRecipe(string id) => transmutationRecipes.FirstOrDefault(x => x.id == id);

        public static void AddRecipe(TransmutationRecipe recipe)
        {
            if (recipe.id == string.Empty)
                if (recipe.outputItem < ItemID.Count)
                    recipe.id = Regex.Replace(GetItem(recipe.outputItem).Name, @"\s+", "");
                else
                    recipe.id = ItemLoader.GetItem(recipe.outputItem).Name; 
                
            if (transmutationRecipes.Any(x => x.id == recipe.id))
                throw new Exception("Radiance Error: Tried to add recipe with already existing id \"" + recipe.id + "\"");

             transmutationRecipes.Add(recipe);
        }
    }

    public class TransmutationRecipe
    {
        public int[] inputItems = Array.Empty<int>();
        public int outputItem = 0;
        public int requiredRadiance = 0;
        public UnlockBoolean unlock = UnlockBoolean.unlockedByDefault;
        public string id = string.Empty;
        public int inputStack = 0;
        public int outputStack = 0;
        public TransmutationRecipeSystem.SpecialRequirements[] specialRequirements = Array.Empty<TransmutationRecipeSystem.SpecialRequirements>();
        public float specialEffectValue = 0;
        public TransmutationRecipeSystem.SpecialEffects specialEffects = TransmutationRecipeSystem.SpecialEffects.None;
        public ProjectorLensID lensRequired = ProjectorLensID.Flareglass;
        public float lensRequiredValue = 0;
    }
}