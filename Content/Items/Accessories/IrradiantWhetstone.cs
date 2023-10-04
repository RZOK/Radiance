using Microsoft.Xna.Framework.Input;
using Radiance.Core.Systems;
using System.Collections.Generic;

namespace Radiance.Content.Items.Accessories
{
    public class IrradiantWhetstone : ModItem, IOnTransmutateEffect, ITransmutationRecipe
    {
        private readonly string name = "Irradiant Whetstone";
        public int maxPrefixes = 4;
        public int timesReforged = 0;
        public int timesReforgedFake = 0;

        public int[] prefixes;
        public byte[] lockedSlots;
        private bool EverythingLocked => lockedSlots.All(x => x == 1);
        public int CurrentIndex => timesReforgedFake % maxPrefixes;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault(name);
            Tooltip.SetDefault("Can have four prefixes at once\nTransmutate to lock the currently selected slot\nPlaceholder Line");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            prefixes = new int[maxPrefixes];
            lockedSlots = new byte[maxPrefixes];

            Item.width = 28;
            Item.height = 18;
            Item.value = Item.sellPrice(0, 2, 50);
            Item.rare = ItemRarityID.Lime;
            Item.accessory = true;
        }

        public void OnTransmutate()
        {
            if (EverythingLocked)
            {
                for (int i = 0; i < lockedSlots.Length; i++)
                {
                    lockedSlots[i] = 0;
                }
                timesReforgedFake = 0;
                return;
            }
            if (lockedSlots[CurrentIndex] == 1)
            {
                lockedSlots[CurrentIndex] = 0;
                return;
            }
            lockedSlots[CurrentIndex] = 1;
            GoToNextOpenSlot();
        }

        private void GoToNextOpenSlot()
        {
            if (!EverythingLocked)
            {
                do
                {
                    timesReforgedFake++;
                } while (lockedSlots[CurrentIndex] == 1);
            }
        }

        public override void UpdateInventory(Player player)
        {
            string str = string.Empty;
            foreach (int prefix in prefixes)
            {
                if (prefix != 0)
                    str += Lang.prefix[prefix] + " ";
            }
            Item.SetNameOverride(str + name);
            SetValue();
        }

        public void SetValue()
        {
            float value = 0;
            foreach (int prefix in prefixes)
            {
                if (prefix != 0)
                    value += 0.5f;
            }
            Item.value = (int)((float)Item.sellPrice(0, 2, 50) * value + 1f);
        }


        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            foreach (int prefix in prefixes)
            {
                GetPrefixStats(prefix, out int defense, out int mana, out int crit, out float damage, out float moveSpeed, out float meleeSpeed);
                player.statDefense += defense;
                player.statManaMax2 += mana;
                player.GetCritChance(DamageClass.Generic) += crit;
                player.GetDamage(DamageClass.Generic).Flat += damage;
                player.moveSpeed += moveSpeed;
                player.GetAttackSpeed(DamageClass.Melee) += meleeSpeed;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string str = "";
            string prefixesInName = string.Empty;
            for (int i = 0; i < maxPrefixes; i++)
            {
                string statString = "";
                if (prefixes[i] != 0)
                {
                    statString += " - [c/649E64:";
                    GetPrefixStats(prefixes[i], out int defense, out int mana, out int crit, out float damage, out float moveSpeed, out float meleeSpeed);
                    if (defense > 0)
                        statString += $"+{defense} defense";
                    if (mana > 0)
                        statString += $"+{mana} mana";
                    if (crit > 0)
                        statString += $"+{crit}% critical strike chance";
                    if (damage > 0)
                        statString += $"+{(int)(damage * 100)}% damage";
                    if (moveSpeed > 0)
                        statString += $"+{(int)(moveSpeed * 100)}% movement speed";
                    if (meleeSpeed > 0)
                        statString += $"+{(int)(meleeSpeed * 100)}% melee speed";
                    statString += "]";
                }
                string correct = prefixes[i] != 0 ? Lang.prefix[prefixes[i]].ToString() : "No prefix";
                string color = lockedSlots[i] == 1 ? "eb4034" : prefixes[i] != 0 ? "0dd1d4" : "666666";

                str += $"[c/AAAAAA:[][c/{color}:{correct}]" + statString + "[c/AAAAAA:]]";
                if (i == CurrentIndex && !EverythingLocked)
                    str += @"[c/77FF42: <]";
                if (i != maxPrefixes - 1)
                    str += "\n";
                if (prefixes[i] != 0)
                    prefixesInName += Lang.prefix[prefixes[i]] + " ";
            }
            tooltips.Find(n => n.Name == "Tooltip2" && n.Mod == "Terraria").Text = str;
            tooltips.Find(n => n.Name == "ItemName" && n.Mod == "Terraria").Text = prefixesInName + name;

            if (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift))
            {
                TooltipLine line = new TooltipLine(Mod, "TimesReforged", $"Times reforged: {timesReforged}");
                line.OverrideColor = Color.DarkGray;
                tooltips.Insert(tooltips.FindIndex(x => x.Name == "Tooltip2" && x.Mod == "Terraria") + 1, line);
            }
        }
        public override bool CanReforge()
        {
            if (EverythingLocked)
                return false;

            Player player = Main.LocalPlayer;
            int prefix = Main.rand.Next(62, 81);
            prefixes[CurrentIndex] = prefix;
            timesReforged++;
            GoToNextOpenSlot();

            //mostly vanilla reforge code
            int reforgePrice = Item.value;
            bool canApplyDiscount = true;
            if (ItemLoader.ReforgePrice(Item, ref reforgePrice, ref canApplyDiscount))
            {
                if (canApplyDiscount && player.discountAvailable)
                    reforgePrice = (int)(reforgePrice * 0.8);
                reforgePrice = (int)(reforgePrice * player.currentShoppingSettings.PriceAdjustment);
                reforgePrice /= 3;
            }
            player.BuyItem(reforgePrice);

            Item reforgeItem = Item.Clone();
            reforgeItem.position.X = player.position.X + (float)(player.width / 2) - (float)(reforgeItem.width / 2);
            reforgeItem.position.Y = player.position.Y + (float)(player.height / 2) - (float)(reforgeItem.height / 2);
            string str = string.Empty;
            foreach (int i in prefixes)
            {
                if (i != 0)
                    str += Lang.prefix[i] + " ";
            }
            reforgeItem.SetNameOverride(str + "Irradiant Whetstone");
            Item.SetNameOverride(str + "Irradiant Whetstone");
            SetValue();
            ItemLoader.PostReforge(Item);
            PopupText.NewText(PopupTextContext.ItemReforge, reforgeItem, 1, noStack: true);
            SoundEngine.PlaySound(SoundID.AbigailAttack);
            return false;
        }
        public void AddTransmutationRecipe(TransmutationRecipe recipe)
        {
            recipe.inputItems = new int[] { Item.type };
            recipe.requiredRadiance = 40;
            recipe.specialEffects = TransmutationRecipeSystem.SpecialEffects.MoveToOutput;
            recipe.id = "IrradiantWhetstoneLock";
        }

        public override void SaveData(TagCompound tag)
        {
            tag["TimesReforged"] = timesReforged;
            tag["Prefixes"] = prefixes;
            tag["LockedSlots"] = lockedSlots;
        }

        public override void LoadData(TagCompound tag)
        {
            timesReforged = tag.GetInt("TimesReforged");
            prefixes = tag.Get<int[]>("Prefixes");
            lockedSlots = tag.Get<byte[]>("LockedSlots");

            if (prefixes.Length != 4)
                prefixes = new int[4];
            if (lockedSlots.Length != 4)
                lockedSlots = new byte[4];
        }
    }
}