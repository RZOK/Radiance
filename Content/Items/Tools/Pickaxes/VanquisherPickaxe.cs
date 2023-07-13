//namespace Radiance.Content.Items.Tools.Pickaxes
//{
//    public class VanquisherPickaxe : ModItem
//    {
//        public override void Load()
//        {
//            On_WorldGen.KillTile_DropItems += RefineEvilOres;
//        }

//        public override void Unload()
//        {
//            On_WorldGen.KillTile_DropItems -= RefineEvilOres;
//        }
//        private readonly Dictionary<int, int> evilOreReplacement = new Dictionary<int, int>()
//        {
//            { TileID.Demonite, ModContent.ItemType<EnrichedDemonite>() },
//            { TileID.Crimtane, ModContent.ItemType<EnrichedCrimtane>() },
//        };
//        private void RefineEvilOres(On_WorldGen.orig_KillTile_DropItems orig, int x, int y, Tile tileCache, bool includeLargeObjectDrops, bool includeAllModdedLargeObjectDrops)
//        {
//            if(evilOreReplacement.ContainsKey(tileCache.TileType))
//            {
//                int amount = Main.rand.Next(2, 5);
//                int item = Item.NewItem(new EntitySource_TileBreak(x, y), x * 16, y * 16, 16, 16, evilOreReplacement[tileCache.TileType], amount, noBroadcast: false, -1);
//                Main.item[item].TryCombiningIntoNearbyItems(item);
//                return;
//            }
//            orig(x, y, tileCache, includeLargeObjectDrops, includeAllModdedLargeObjectDrops);
//        }


//        public override void SetStaticDefaults()
//        {
//            DisplayName.SetDefault("Vanquisher Pickaxe");
//            Tooltip.SetDefault("Can mine Ebonstone and Crimstone\nEnriches mined Demonite and Crimtane");
//        }

//        public override void SetDefaults()
//        {
//            Item.damage = 12;
//            Item.DamageType = DamageClass.Melee;
//            Item.width = 40;
//            Item.height = 40;
//            Item.useTime = 15;
//            Item.useAnimation = 19;
//            Item.pick = 55;
//            Item.useStyle = ItemUseStyleID.Swing;
//            Item.knockBack = 1;
//            Item.value = Item.sellPrice(0, 0, 25, 0);
//            Item.rare = ItemRarityID.Green;
//            Item.UseSound = SoundID.Item1;
//            Item.autoReuse = true;
//            Item.useTurn = true;
//            Item.tileBoost += 1;
//        }
//    }
//    public class VanquisherPickaxeRecipeSystem : ModSystem
//    {
//        public override void AddRecipes()
//        {
            
//        }
//    }
//}