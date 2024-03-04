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

                if (item.buffType > 0 && item.buffTime > 0 && item.consumable && item.maxStack > 1 && item.Name.Contains("Potion"))
                {
                    TransmutationRecipe potionRecipe = new TransmutationRecipe();
                    if (item.type < ItemID.Count)
                        potionRecipe.id = Regex.Replace(GetItem(item.type).Name, @"\s+", "") + "Dispersal";
                    else
                        potionRecipe.id = ItemLoader.GetItem(item.type).Name + "Dispersal";

                    potionRecipe.inputItems = new int[] { item.type };
                    potionRecipe.requiredRadiance = 100;
                    potionRecipe.specialEffects.Add(TransmutationEffect.beginPotionEffect);
                    potionRecipe.unlock = UnlockCondition.downedEvilBoss;
                    AddRecipe(potionRecipe);
                }
            }
            TransmutationRecipe rainRecipe = new TransmutationRecipe();
            rainRecipe.id = "RainSummon";
            rainRecipe.inputItems = new int[] { ItemID.WaterCandle };
            rainRecipe.requiredRadiance = 20;
            rainRecipe.specialEffects.Add(TransmutationEffect.startRain);
            AddRecipe(rainRecipe);

            TransmutationRecipe rainClearRecipe = new TransmutationRecipe();
            rainClearRecipe.id = "RainStop";
            rainClearRecipe.inputItems = new int[] { ItemID.PeaceCandle };
            rainClearRecipe.requiredRadiance = 20;
            rainClearRecipe.specialEffects.Add(TransmutationEffect.endRain);
            AddRecipe(rainClearRecipe);
        }

        public static TransmutationRecipe FindRecipe(string id) => transmutationRecipes.First(x => x.id == id);
        

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
        public UnlockCondition unlock = UnlockCondition.unlockedByDefault;
        public string id = string.Empty;
        public int inputStack = 1;
        public int outputStack = 1;
        public List<TransmutationRequirement> transmutationRequirements = new List<TransmutationRequirement>();
        public List<TransmutationEffect> specialEffects = new List<TransmutationEffect>();
        public ProjectorLensID lensRequired = ProjectorLensID.Flareglass;
        public float lensRequiredValue = 0;
    }
    public record TransmutationRequirement(Func<TransmutatorTileEntity, bool> condition, LocalizedText tooltip)
    {
        public Func<TransmutatorTileEntity, bool> condition = condition;
        public LocalizedText tooltip;

        public static TransmutationRequirement testRequirement = new TransmutationRequirement((x) => true, Language.GetOrRegister($"Mods.{nameof(Radiance)}.Transmutation.TransmutationRequirements.TestRequirement"));
    }
    public record TransmutationEffect(TransmutationEffect.TransmutationEffectDelegate effect)
    {
        public TransmutationEffectDelegate effect = effect;

        public delegate void TransmutationEffectDelegate(TransmutatorTileEntity entity, params object[] parameters);
        public static TransmutationEffect testEffect = new TransmutationEffect((_, _) => { });
        public static TransmutationEffect startRain = new TransmutationEffect((_, _) => Main.StartRain());
        public static TransmutationEffect endRain = new TransmutationEffect((_, _) => Main.StopRain());
        public static TransmutationEffect beginPotionEffect = new TransmutationEffect((transmutator, potionEffectData) => 
        { 

        });
    }
}