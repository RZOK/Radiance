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
            if (CharmOfIndulgenceSystem.buffTypes.Contains(type) && self.Equipped<CharmOfIndulgence>())
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
                if (line.Name == "Tooltip1")
                {
                    if (consumedFoods.AnyAndExists())
                        line.Text = "-Hold SHIFT to view consumed food items-";
                    else
                        line.Text = string.Empty;

                    line.OverrideColor = new Color(112, 122, 122);
                }
            }
            if (Main.keyState.PressingShift() && consumedFoods.AnyAndExists())
            {
                tooltips.Find(x => x.Name == "Tooltip1").Text = string.Empty;
                for (int i = 0; i < Math.Ceiling((float)consumedFoods.Count / 16); i++)
                {
                    int realAmountToDraw = Math.Min(16, consumedFoods.Count - i * 16);
                    TooltipLine itemDisplayLine = new(Mod, "CharmOfIndulgenceItems" + i, "");
                    if (i == 0)
                        itemDisplayLine.Text = new String('M', 2 * realAmountToDraw + 3) + i;
                    else
                        itemDisplayLine.Text = ".";

                    tooltips.Insert(tooltips.FindIndex(x => x.Name == "Tooltip1"), itemDisplayLine);
                }
            }
        }

        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            if (line.Name == "CharmOfIndulgenceItems0")
            {
                List<Item> items = new List<Item>();
                //Texture2D bgTex = ModContent.Request<Texture2D>($"{Texture}_InventoryBackground").Value;
                Texture2D bgTex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/Items/Accessories/CharmOfIndulgence_InventoryBackground").Value;
                foreach (int item in consumedFoods)
                {
                    items.Add(new Item(item));
                }
                RadianceDrawing.DrawItemGrid(items, new Vector2(line.X, line.Y), bgTex, 16);
            }

            return !line.Name.StartsWith("CharmOfIndulgenceItems");
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
                if (buffTypes.Contains(item.Value.buffType))
                    foodIDs.Add(item.Key);
            }
        }
    }
}