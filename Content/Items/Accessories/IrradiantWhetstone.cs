using Radiance.Core;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Radiance.Content.Items.Accessories
{
    public class IrradiantWhetstone : ModItem, IOnTransmutateEffect
    {
        public List<int> prefixes;
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.ManaCrystal;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Irradiant Whetstone");
            Tooltip.SetDefault("Can be Transmutated endlessly to reforge its prefix\nPlaceholder Line");
            SacrificeTotal = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 18;
            Item.value = Item.sellPrice(0, 10, 0);
            Item.rare = ItemRarityID.Lime;
            Item.accessory = true;
        }
        public void OnTransmutate()
        {
            Item.Prefix(0);
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string str = @"[c/AAAAAA:[][c/666666:No Prefix][c/AAAAAA:]]";
            string str1 = @"[c/AAAAAA:[][c/666666:No Prefix][c/AAAAAA:]]";
            string str2 = @"[c/AAAAAA:[][c/666666:No Prefix][c/AAAAAA:]]";
            tooltips.Find(n => n.Name == "Tooltip1" && n.Mod == "Terraria").Text = str + "\n" + str1 + "\n" + str2;
        }
        public override void PostReforge()
        {
            if (prefixes.Count < 3)
                prefixes.Add(Item.prefix);
            Item.Prefix(0);
        }
        public override void Load()
        {
            prefixes = new List<int>();
        }
        public override void Unload()
        {
            prefixes = null;
        }
        public override void SaveData(TagCompound tag)
        {
            tag["Prefixes"] = prefixes;
        }
        public override void LoadData(TagCompound tag)
        {
            prefixes = (List<int>)tag.GetList<int>("Prefixes");
        }
    }
}
