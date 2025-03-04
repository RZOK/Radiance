
using Radiance.Content.Items.BaseItems;

namespace Radiance.Content.Items.Tools.Misc
{
    public class CharmOfIndulgence : BaseAccessory
    {
        public List<int> consumedFoods = new List<int>();
        public override string Texture => "Radiance/Content/ExtraTextures/Debug";
        public override void Load()
        {
            On_Player.AddBuff += ModifyFoodBuffTime;
            RadiancePlayer.PostUpdateEquipsEvent += ResetCurrentFoodCount;
        }

        private void ResetCurrentFoodCount(Player player)
        {
            if (!player.Equipped<CharmOfIndulgence>())
                player.SetTimer<CharmOfIndulgence>(0);
        }

        private void ModifyFoodBuffTime(On_Player.orig_AddBuff orig, Player self, int type, int timeToAdd, bool quiet, bool foodHack)
        {
            if(CharmOfIndulgenceSystem.buffTypes.Contains(type) && self.Equipped<CharmOfIndulgence>())
                timeToAdd = (int)(timeToAdd * (1f + self.GetTimer<CharmOfIndulgence>() / 40f));

            orig(self, type, timeToAdd, quiet, foodHack);
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Charm of Indulgence");  
            Tooltip.SetDefault("Effects of food are enhanced for each unique food item consumed while wearing the charm\nPlaceholder Line");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            for (int i = 0; i < 50; i++)
            {
                consumedFoods.Add(i);
            }
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
        public override void SafeUpdateAccessory(Player player, bool hideVisual)
        {
            player.SetTimer<CharmOfIndulgence>(consumedFoods.Count);
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