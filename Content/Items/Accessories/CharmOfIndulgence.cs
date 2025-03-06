using Radiance.Content.Items.BaseItems;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Default;

namespace Radiance.Content.Items.Accessories
{
    public class CharmOfIndulgence : BaseAccessory
    {
        public List<ItemDefinition> consumedFoods = new List<ItemDefinition>();

        public override void Load()
        {
            On_Player.AddBuff += ModifyFoodBuffTime;
        }
        private void ModifyFoodBuffTime(On_Player.orig_AddBuff orig, Player self, int type, int timeToAdd, bool quiet, bool foodHack)
        {
            if (CharmOfIndulgenceSystem.foodBuffTypes.Contains(type) && self.Equipped<CharmOfIndulgence>())
                timeToAdd = (int)(timeToAdd * (1f + self.GetModPlayer<CharmOfIndulgencePlayer>().equippedCharmOfIndulgence.consumedFoods.Count / 40f));

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
                Texture2D bgTex = ModContent.Request<Texture2D>($"{Texture}_InventoryBackground").Value;
                foreach (ItemDefinition item in consumedFoods)
                {
                    if (item.Type == -1)
                        items.Add(new Item(ModContent.ItemType<UnloadedItem>()));
                    else
                        items.Add(new Item(item.Type));
                }
                RadianceDrawing.DrawItemGrid(items, new Vector2(line.X, line.Y), bgTex, 16);
            }

            return !line.Name.StartsWith("CharmOfIndulgenceItems");
        }

        public override void SafeUpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<CharmOfIndulgencePlayer>().equippedCharmOfIndulgence = this;
        }

        public override void SaveData(TagCompound tag)
        {
            tag[nameof(consumedFoods)] = consumedFoods;
        }

        public override void LoadData(TagCompound tag)
        {
            consumedFoods = tag.GetList<ItemDefinition>(nameof(consumedFoods)).ToList();
        }
    }

    public class CharmOfIndulgencePlayer : ModPlayer
    {
        internal CharmOfIndulgence equippedCharmOfIndulgence;
        public override void ResetEffects()
        {
            equippedCharmOfIndulgence = null;
        }
    }
    public class CharmOfIndulgenceItem : GlobalItem
    {
        public override bool ConsumeItem(Item item, Player player)
        {
            List<ItemDefinition> consumedFoods = player.GetModPlayer<CharmOfIndulgencePlayer>().equippedCharmOfIndulgence.consumedFoods;
             if (CharmOfIndulgenceSystem.foodTypes.Contains(item.type) && !consumedFoods.Select(x => x.Type).Contains(item.type))
                consumedFoods.Add(new ItemDefinition(item.type));

            return base.ConsumeItem(item, player);
        }
    }

    public class CharmOfIndulgenceSystem : ModSystem
    {
        public static HashSet<int> foodTypes = new HashSet<int>();
        internal static int[] foodBuffTypes = new int[] { BuffID.WellFed, BuffID.WellFed2, BuffID.WellFed3 };

        public override void PostSetupContent()
        {
            foreach (var item in ContentSamples.ItemsByType)
            {
                if (foodBuffTypes.Contains(item.Value.buffType))
                    foodTypes.Add(item.Key);
            }
        }
    }
}