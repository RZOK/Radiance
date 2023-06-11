using Radiance.Core;
using Radiance.Core.Interfaces;
using Radiance.Core.Systems;
using Radiance.Utilities;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.Accessories
{
    public class GleamingWhetstone : ModItem, IOnTransmutateEffect, ITransmutationRecipe
    {
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
            RadianceUtils.GetPrefixStats(Item.prefix, out int defense, out int mana, out int crit, out float damage, out float moveSpeed, out float meleeSpeed);
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

        public void OnTransmutate()
        {
            Item.Prefix(-2);
        }

        public void AddTransmutationRecipe(TransmutationRecipe recipe)
        {
            recipe.inputItems = new int[] { Item.type };
            recipe.requiredRadiance = 40;
            recipe.specialEffects = TransmutationRecipeSystem.SpecialEffects.MoveToOutput;
            recipe.id = "GleamingWhetstoneReforge";
        }
    }
}