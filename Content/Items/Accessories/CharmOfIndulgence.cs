
using Radiance.Content.Items.BaseItems;

namespace Radiance.Content.Items.Tools.Misc
{
    public class CharmOfIndulgence : BaseAccessory
    {
        public List<int> consumedFoods = new List<int>();
        private float wellFedTimeModifier => 1f + consumedFoods.Count / 40f;
        public override string Texture => "Radiance/Content/ExtraTextures/DebugTexture";
        public override void Load()
        {
            On_Player.AddBuff += ModifyFoodBuffTime;
        }

        private void ModifyFoodBuffTime(On_Player.orig_AddBuff orig, Player self, int type, int timeToAdd, bool quiet, bool foodHack)
        {
            if(CharmOfIndulgenceSystem.buffTypes.Contains(type) && self.Equipped<CharmOfIndulgence>())
            {
                timeToAdd *= self.GetModPlayer<CharmOfIndulgencePlayer>();
            }
            orig(self, type, timeToAdd, quiet, foodHack);
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Charm of Indulgence");
            Tooltip.SetDefault("Food effects last longer and are more potent depending on how many unique food items have been consumed while wearing the charm\nPlaceholder Line");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 34;
            Item.maxStack = 1;
            Item.accessory = true;
            Item.rare = ItemRarityID.Green;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            foreach (TooltipLine line in tooltips)
            {
                if(line.Name == "Tooltip1")
                {
                    if(Main.keyState.PressingShift())
                    {

                    }
                    else 
                    {
                        line.Text = "-Hold SHIFT to view consumed food items-";
                        line.OverrideColor = new Color(112, 122, 122);
                    }
                }
            }
        }
        public override void SaveData(TagCompound tag)
        {
            tag[nameof(consumedFoods)] = consumedFoods;
        }
        public override void LoadData(TagCompound tag)
        {
            consumedFoods = tag.GetList<int>(nameof(consumedFoods)).ToList();
        }
    }
    public class CharmOfIndulgencePlayer : ModPlayer
    {
        public override void ResetEffects()
        {
            base.ResetEffects();
        }
    }
    public class CharmOfIndulgenceSystem : ModSystem
    {
        public static HashSet<int> foodIDs = new HashSet<int>();
        public static int[] buffTypes = new int[] { BuffID.WellFed, BuffID.WellFed2, BuffID.WellFed3 };
        public override void PostSetupContent()
        {
            foreach (var item in ContentSamples.ItemsByType)
            {
                if(buffTypes.Contains(item.Value.buffType))
                    foodIDs.Add(item.Key);
            }
        }
    }
}