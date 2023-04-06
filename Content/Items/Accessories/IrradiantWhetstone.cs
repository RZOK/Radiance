using Radiance.Core;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Radiance.Content.Items.Accessories
{
    public class IrradiantWhetstone : ModItem, IOnTransmutateEffect
    {
        private readonly string name = "Irradiant Whetstone";
        public int maxPrefixes = 4;
        public int timesReforged = 0;

        public List<int> prefixes = new List<int>();
        public bool[] lockedSlots = new bool[4];
        public int currentIndex => timesReforged % maxPrefixes;
        public override string Texture => "Terraria/Images/Item_" + ItemID.ManaCrystal;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault(name);
            Tooltip.SetDefault("Can have four prefixes at once\nTransmutate again to skip a slot\nPlaceholder Line");
            SacrificeTotal = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 18;
            Item.value = Item.sellPrice(0, 2, 50);
            Item.rare = ItemRarityID.Lime;
            Item.accessory = true;
        }
        public void OnTransmutate()
        {
            lockedSlots[currentIndex] = !lockedSlots[currentIndex];
        }
        public override void UpdateInventory(Player player)
        {
            string str = string.Empty;
            prefixes.ForEach(x => str += Lang.prefix[x] + " ");
            Item.SetNameOverride(str + name);
            Item.value = Item.sellPrice(0, 2, 50) * ((prefixes.Count / 2) + 1);
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            int defense = 0;
            int mana = 0;
            int crit = 0;
            float damage = 0;
            float moveSpeed = 0;
            float meleeSpeed = 0;
            foreach (int prefix in prefixes)
            {
                switch(prefix)
                {
                    case 62:
                        defense += 1;
                        break;
                    case 63:
                        defense += 2;
                        break;
                    case 64:
                        defense += 3;
                        break;
                    case 65:
                        defense += 4;
                        break;

                    case 66:
                        mana += 20;
                        break;

                    case 67:
                        crit += 2;
                        break;
                    case 68:
                        crit += 4;
                        break;

                    case 69:
                        damage += 0.01f;
                        break;
                    case 70:
                        damage += 0.02f;
                        break;
                    case 71:
                        damage += 0.03f;
                        break;
                    case 72:
                        damage += 0.04f;
                        break;

                    case 73:
                        moveSpeed += 0.01f;
                        break;
                    case 74:
                        moveSpeed += 0.02f;
                        break;
                    case 75:
                        moveSpeed += 0.03f;
                        break;
                    case 76:
                        moveSpeed += 0.04f;
                        break;

                    case 77:
                        meleeSpeed += 0.01f;
                        break;
                    case 78:
                        meleeSpeed += 0.02f;
                        break;
                    case 79:
                        meleeSpeed += 0.03f;
                        break;
                    case 80:
                        meleeSpeed += 0.04f;
                        break;
                }
            }
            player.statDefense += defense;
            player.statManaMax2 += mana;
            player.GetCritChance(DamageClass.Generic) += crit;
            player.GetDamage(DamageClass.Generic).Flat += damage;
            player.moveSpeed += moveSpeed;
            player.GetAttackSpeed(DamageClass.Melee) += meleeSpeed;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string str = "";
            for (int i = 0; i < maxPrefixes; i++)
            {
                //fix this
                string correct = prefixes[i] != 0 ? Lang.prefix[prefixes[i]].ToString() : "No prefix";
                string color = lockedSlots[i] ? "eb4034" : prefixes[i] != 0 ? "0dd1d4" : "666666";

                    str += $"[c/AAAAAA:[][c/{color}:{correct}][c/AAAAAA:]]";
                if (i == currentIndex)
                    str += @"[c/77FF42: <]";
                if (i != maxPrefixes - 1) 
                    str += "\n";
            }
            tooltips.Find(n => n.Name == "Tooltip2" && n.Mod == "Terraria").Text = str;

            string prefixesInName = string.Empty;
            prefixes.ForEach(x => prefixesInName += Lang.prefix[x] + " ");
            tooltips.Find(n => n.Name == "ItemName" && n.Mod == "Terraria").Text = prefixesInName + name;
        }
        public override bool PreReforge()
        {
            Player player = Main.player[Main.myPlayer];
            int prefix = Main.rand.Next(62, 81);
            if (prefixes.Count < maxPrefixes)
                prefixes[currentIndex] = prefix;
            else
                prefixes[currentIndex] = prefix;
            timesReforged++;
            
            //mostly vanilla reforge code
            int reforgePrice = Item.value;
            bool canApplyDiscount = true;
            if (ItemLoader.ReforgePrice(Item, ref reforgePrice, ref canApplyDiscount))
            {
                if (canApplyDiscount && player.discount)
                    reforgePrice = (int)(reforgePrice * 0.8);
                reforgePrice = (int)(reforgePrice * player.currentShoppingSettings.PriceAdjustment);
                reforgePrice /= 3;
            }

            player.BuyItem(reforgePrice);
            Item reforgeItem = Item.Clone();
            reforgeItem.position.X = player.position.X + (float)(player.width / 2) - (float)(reforgeItem.width / 2);
            reforgeItem.position.Y = player.position.Y + (float)(player.height / 2) - (float)(reforgeItem.height / 2);
            string str = string.Empty;
            prefixes.ForEach(x => str += Lang.prefix[x] + " ");
            reforgeItem.SetNameOverride(str + "Irradiant Whetstone");
            Item.SetNameOverride(str + "Irradiant Whetstone");
            Item.value = Item.sellPrice(0, 2, 50) * ((prefixes.Count / 2) + 1);
            ItemLoader.PostReforge(Item);
            PopupText.NewText(PopupTextContext.ItemReforge, reforgeItem, 1, noStack: true);
            SoundEngine.PlaySound(in SoundID.AbigailAttack);
            return false;
        }
        public override void SaveData(TagCompound tag)
        {
            tag["TimesReforged"] = timesReforged;
            tag["Prefixes"] = prefixes;
        }
        public override void LoadData(TagCompound tag)
        {
            timesReforged = tag.GetInt("TimesReforged");
            prefixes = (List<int>)tag.GetList<int>("Prefixes");
        }
    }
}
