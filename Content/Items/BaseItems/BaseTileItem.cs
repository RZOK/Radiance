﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.BaseItems
{
    public abstract class BaseTileItem : ModItem
    {
        public string name = "";
        public string tile;

        public int sacrificeTotal;
        public int value;
        public int rare;

        public BaseTileItem(string name, string tile, int sacrificeTotal = 1, int value = 0, int rare = ItemRarityID.White)
        {
            this.name = name;
            this.tile = tile;
            this.sacrificeTotal = sacrificeTotal;
            this.value = value;
            this.rare = rare;
        }

        public override string Name => name != default ? name : base.Name;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = sacrificeTotal;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 999;
            Item.value = value;
            Item.rare = rare;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = Mod.Find<ModTile>(tile).Type;
        }
    }
}