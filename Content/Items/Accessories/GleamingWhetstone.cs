﻿using Radiance.Content.Tiles.Transmutator;
using Radiance.Core.Systems;

namespace Radiance.Content.Items.Accessories
{
    public class GleamingWhetstone : ModItem, ITransmutationRecipe
    {
        public override void Load()
        {
            TransmutatorTileEntity.PreTransmutateItemEvent += ReforgeWhetstone;
        }
        public override void Unload()
        {
            TransmutatorTileEntity.PreTransmutateItemEvent -= ReforgeWhetstone;
        }

        private bool ReforgeWhetstone(TransmutatorTileEntity transmutator, TransmutationRecipe recipe)
        {
            if (transmutator.GetSlot(0).type == Type)
            {
                Item item = transmutator.GetSlot(0).Clone();
                item.Prefix(-2);
                transmutator.SetItemInSlot(1, item);
                transmutator.GetSlot(0).TurnToAir();
                return false;
            }
            return true;
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Gleaming Whetstone");
            Tooltip.SetDefault("Provides a small boost to its modifier\nCan be Transmutated endlessly to reforge itself");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 18;
            Item.value = Item.sellPrice(0, 0, 25);
            Item.rare = ItemRarityID.Green;
            Item.accessory = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            foreach (TooltipLine line in tooltips)
            {
                if (line.IsModifier)
                {
                    for (int i = 0; i < line.Text.Length; i++)
                    {
                        if (int.TryParse(line.Text[i].ToString(), out int result) && result != 0)
                        {
                            line.Text = line.Text.Remove(i, 1);
                            line.Text = line.Text.Insert(i, (result + 2).ToString());
                        }
                    }
                }
            }
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            GetPrefixStats(Item.prefix, out int defense, out int mana, out int crit, out float damage, out float moveSpeed, out float meleeSpeed);
            if (defense != 0)
                player.statDefense += 2;
            else if (mana != 0)
                player.statManaMax2 += 20;
            else if (crit != 0)
                player.GetCritChance(DamageClass.Generic) += 2;
            else if (damage != 0)
                player.GetDamage(DamageClass.Generic).Flat += 0.02f;
            else if (moveSpeed != 0)
                player.moveSpeed += 0.02f;
            else if (meleeSpeed != 0)
                player.GetAttackSpeed(DamageClass.Melee) += 0.02f;
            // :(
        }

        public void AddTransmutationRecipe(TransmutationRecipe recipe)
        {
            recipe.inputItems = new int[] { Item.type };
            recipe.requiredRadiance = 40;
            recipe.id = "GleamingWhetstoneReforge";
        }
    }
}