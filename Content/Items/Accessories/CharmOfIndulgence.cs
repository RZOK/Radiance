﻿using Radiance.Content.Items.BaseItems;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Default;

namespace Radiance.Content.Items.Accessories
{
    public class CharmOfIndulgence : BaseAccessory
    {
        public List<ItemDefinition> consumedFoods = new List<ItemDefinition>();
        private static readonly float FOOD_RATIO = 40f;
        private static readonly int ITEMS_PER_ROW = 16;
        internal static readonly List<int> FOOD_BUFF_TYPES = new List<int>() { BuffID.WellFed, BuffID.WellFed2, BuffID.WellFed3 };
        public override void Load()
        {
            On_Player.AddBuff += ModifyFoodBuffTime;
        }

        private void ModifyFoodBuffTime(On_Player.orig_AddBuff orig, Player self, int type, int timeToAdd, bool quiet, bool foodHack)
        {
            if (FOOD_BUFF_TYPES.Contains(type) && self.Equipped<CharmOfIndulgence>())
                timeToAdd = (int)(timeToAdd * (1f + self.GetModPlayer<CharmOfIndulgencePlayer>().equippedCharmOfIndulgence.consumedFoods.Count / FOOD_RATIO));

            orig(self, type, timeToAdd, quiet, foodHack);
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Charm of Indulgence");
            Tooltip.SetDefault("Effects of food are enhanced for each unique food item consumed while wearing the charm");
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
            if (consumedFoods.AnyAndExists())
            {
                for (int i = 0; i < Math.Ceiling((float)consumedFoods.Count / ITEMS_PER_ROW); i++)
                {
                    int realAmountToDraw = Math.Min(ITEMS_PER_ROW, consumedFoods.Count - i * ITEMS_PER_ROW);
                    TooltipLine itemDisplayLine = new(Mod, "CharmOfIndulgenceItems" + i, "");
                    if (i == 0)
                        itemDisplayLine.Text = new String('M', 2 * realAmountToDraw + 3) + i;
                    else
                        itemDisplayLine.Text = ".";

                    tooltips.Insert(tooltips.FindIndex(x => x.Name == "Tooltip0") + 1, itemDisplayLine);
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
                RadianceDrawing.DrawItemGrid(items, new Vector2(line.X, line.Y), bgTex, ITEMS_PER_ROW);
            }
            return !line.Name.StartsWith("CharmOfIndulgenceItems");
        }

        public override void SafeUpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<CharmOfIndulgencePlayer>().equippedCharmOfIndulgence = this;
            if (consumedFoods.Count != 0)
            {
                float modifier = 0;
                if (player.HasBuff(BuffID.WellFed))
                    modifier = 1f;
                else if (player.HasBuff(BuffID.WellFed2))
                    modifier = 1.5f;
                else if (player.HasBuff(BuffID.WellFed3))
                    modifier = 2f;

                modifier *= consumedFoods.Count / FOOD_RATIO;

                player.statDefense += (int)(2f * modifier);
                player.GetCritChance(DamageClass.Generic) += 2f * modifier;
                player.GetDamage(DamageClass.Generic) += 0.05f * modifier;
                player.GetAttackSpeed(DamageClass.Melee) += 0.05f * modifier;
                player.GetKnockback(DamageClass.Summon) += 0.5f * modifier;
                player.moveSpeed += 0.2f * modifier;
                player.pickSpeed -= 0.05f * modifier;
            }
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
             if (CharmOfIndulgence.FOOD_BUFF_TYPES.Contains(item.buffType) && !consumedFoods.Select(x => x.Type).Contains(item.type))
                consumedFoods.Add(new ItemDefinition(item.type));

            return base.ConsumeItem(item, player);
        }
    }
}