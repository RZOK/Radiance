using Microsoft.Xna.Framework.Input;
using Radiance.Content.Tiles.Transmutator;
using Radiance.Core.Systems;
using Terraria.Localization;

namespace Radiance.Content.Items.Accessories
{
    public class IrradiantWhetstone : ModItem, ITransmutationRecipe
    {
        private string ItemName => LanguageManager.Instance.GetOrRegister($"Mods.{nameof(Radiance)}.Items.IrradiantWhetstone.DisplayName").Value;
        public static readonly int MAX_PREFIXES = 4;
        public int timesReforged = 0;
        public int timesReforgedFake = 0;

        public int[] prefixes;
        public bool[] lockedSlots;
        private bool EverythingLocked => lockedSlots.All(x => x);
        public int CurrentIndex => timesReforgedFake % MAX_PREFIXES;
        
        public override void Load()
        {
            TransmutatorTileEntity.PreTransmutateItemEvent += LockSlots;
        }
        public override void Unload()
        {
            TransmutatorTileEntity.PreTransmutateItemEvent -= LockSlots;
        }

        private bool LockSlots(TransmutatorTileEntity transmutator, TransmutationRecipe recipe)
        {
            if (transmutator.GetSlot(0).type == Type)
            {
                Item item = transmutator.GetSlot(0).Clone();
                IrradiantWhetstone whetstone = item.ModItem as IrradiantWhetstone;
                transmutator.SetItemInSlot(1, item);
                transmutator.GetSlot(0).TurnToAir();

                if (whetstone.EverythingLocked)
                {
                    lockedSlots = new bool[MAX_PREFIXES];
                    whetstone.timesReforgedFake = 0;
                    return false;
                }
                whetstone.lockedSlots[whetstone.CurrentIndex] = true;
                whetstone.GoToNextOpenSlot();
                return false;
            }
            return true;
        }

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Can have four prefixes at once\nTransmutate to lock the currently selected slot\nPlaceholder Line");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            prefixes = new int[MAX_PREFIXES];
            lockedSlots = new bool[MAX_PREFIXES];

            Item.width = 28;
            Item.height = 18;
            Item.value = Item.sellPrice(0, 2, 50);
            Item.rare = ItemRarityID.Lime;
            Item.accessory = true;
        }
        
        private void GoToNextOpenSlot()
        {
            if (!EverythingLocked)
            {
                do
                {
                    timesReforgedFake++;
                } while (lockedSlots[CurrentIndex]);
            }
        }

        public override void UpdateInventory(Player player)
        {
            List<string> prefixesString = new List<string>();
            foreach (int i in prefixes)
            {
                if (i != 0)
                    prefixesString.Add(Lang.prefix[i].Value);
            }
            prefixesString.Add(ItemName);
            Item.SetNameOverride(string.Join(" ", prefixesString));
            SetValue();
        }

        private void SetValue()
        {
            float value = 1f;
            foreach (int prefix in prefixes)
            {
                if (prefix != 0)
                    value += 0.5f;
            }
            Item valueItem = new Item();
            valueItem.SetDefaults(Type);
            Item.value = (int)(valueItem.value * value);
        }


        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            foreach (int prefix in prefixes)
            {
                GetPrefixStats(prefix, out int defense, out int mana, out int crit, out float damage, out float moveSpeed, out float meleeSpeed);
                player.statDefense += defense;
                player.statManaMax2 += mana;
                player.GetCritChance(DamageClass.Generic) += crit;
                player.GetDamage(DamageClass.Generic) += damage;
                player.moveSpeed += moveSpeed;
                player.GetAttackSpeed(DamageClass.Melee) += meleeSpeed;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string str = "";
            string prefixesInName = string.Empty;
            for (int i = 0; i < MAX_PREFIXES; i++)
            {
                string statString = string.Empty;
                if (prefixes[i] != 0)
                {
                    // todo: localization
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
                string correct = prefixes[i] != 0 ? Lang.prefix[prefixes[i]].Value : "No prefix";
                string color = lockedSlots[i] ? "eb4034" : prefixes[i] != 0 ? "0dd1d4" : "666666";

                str += $"[c/AAAAAA:[][c/{color}:{correct}]{statString}[c/AAAAAA:]]";
                if (i == CurrentIndex && !EverythingLocked)
                    str += @"[c/77FF42: <]";

                if (i != MAX_PREFIXES - 1)
                    str += "\n";

                if (prefixes[i] != 0)
                    prefixesInName += Lang.prefix[prefixes[i]].Value + " ";
            }
            tooltips.Find(n => n.Name == "Tooltip2" && n.Mod == "Terraria").Text = str;
            tooltips.Find(n => n.Name == "ItemName" && n.Mod == "Terraria").Text = prefixesInName + ItemName;

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
                    reforgePrice *= (int)(reforgePrice * 0.8);

                reforgePrice = (int)(reforgePrice * player.currentShoppingSettings.PriceAdjustment);
                reforgePrice /= 3;
            }
            player.BuyItem(reforgePrice);

            Item reforgeItem = Item.Clone();
            reforgeItem.position.X = player.position.X + (float)(player.width / 2) - (float)(reforgeItem.width / 2);
            reforgeItem.position.Y = player.position.Y + (float)(player.height / 2) - (float)(reforgeItem.height / 2);

            List<string> prefixesString = new List<string>();
            foreach (int i in prefixes)
            {
                if (i != 0)
                    prefixesString.Add(Lang.prefix[i].Value);
            }
            prefixesString.Add(ItemName);
            reforgeItem.SetNameOverride(string.Join(" ", prefixesString));
            Item.SetNameOverride(string.Join(" ", prefixesString));
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
            recipe.id = "IrradiantWhetstoneLock";
        }

        public override void SaveData(TagCompound tag)
        {
            tag[nameof(timesReforged)] = timesReforged;
            tag[nameof(timesReforgedFake)] = timesReforgedFake;
            tag[nameof(prefixes)] = prefixes;
            tag[nameof(lockedSlots)] = lockedSlots;
        }

        public override void LoadData(TagCompound tag)
        {
            timesReforged = tag.GetInt(nameof(timesReforged));
            timesReforgedFake = tag.GetInt(nameof(timesReforgedFake));
            prefixes = tag.Get<int[]>(nameof(prefixes));
            lockedSlots = tag.Get<bool[]>(nameof(lockedSlots));

            if (prefixes.Length != MAX_PREFIXES)
                Array.Resize(ref prefixes, MAX_PREFIXES);
            if (lockedSlots.Length != MAX_PREFIXES)
                Array.Resize(ref lockedSlots, MAX_PREFIXES);
        }
        public override ModItem Clone(Item newItem)
        {
            IrradiantWhetstone item = base.Clone(newItem) as IrradiantWhetstone;
            item.timesReforged = timesReforged;
            item.timesReforgedFake = timesReforgedFake;
            item.prefixes = prefixes;
            item.lockedSlots = lockedSlots;
            return item;
        }
    }
}