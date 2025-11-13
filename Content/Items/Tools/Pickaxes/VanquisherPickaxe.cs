using Mono.Cecil.Cil;
using MonoMod.Cil;
using Radiance.Content.Items.Materials;
using Radiance.Content.Particles;
using Radiance.Core.Systems.ParticleSystems;

namespace Radiance.Content.Items.Tools.Pickaxes
{
    public class VanquisherPickaxe : ModItem
    {
        public override void Load()
        {
            On_WorldGen.KillTile_DropItems += RefineEvilOres;
            IL_Player.GetPickaxeDamage += AllowMiningOfEvilStones;
        }

        public override void Unload()
        {
            On_WorldGen.KillTile_DropItems -= RefineEvilOres;
            IL_Player.GetPickaxeDamage -= AllowMiningOfEvilStones;
        }

        private readonly Dictionary<int, int> evilOreReplacement = new Dictionary<int, int>()
        {
            { TileID.Demonite, ModContent.ItemType<EnrichedDemonite>() },
            { TileID.Crimtane, ModContent.ItemType<BloatedCrimtane>() },
        };
        private void RefineEvilOres(On_WorldGen.orig_KillTile_DropItems orig, int x, int y, Tile tileCache, bool includeLargeObjectDrops, bool includeAllModdedLargeObjectDrops)
        {
            Player player = Main.player[Player.FindClosest(new Vector2(x, y) * 16f, 16, 16)];

            if ((player.GetPlayerHeldItem().type == ModContent.ItemType<SubjugationPickaxe>() || player.GetPlayerHeldItem().type == Type) && player.ItemAnimationActive && evilOreReplacement.ContainsKey(tileCache.TileType))
            {
                int amount = Main.rand.Next(1, 4);
                int item = Item.NewItem(new EntitySource_TileBreak(x, y), x * 16, y * 16, 16, 16, evilOreReplacement[tileCache.TileType], amount, noBroadcast: false, -1);
                Main.item[item].TryCombiningIntoNearbyItems(item);
                WorldParticleSystem.system.AddParticle(new StarFlare(new Vector2(x, y).ToWorldCoordinates(), 10, new Color(200, 180, 100), new Color(200, 180, 100), 0.05f));
                WorldParticleSystem.system.AddParticle(new Burst(new Vector2(x, y).ToWorldCoordinates(), 10, new Color(200, 180, 100), CommonColors.RadianceColor2, 0.1f));
                return;
            }
            orig(x, y, tileCache, includeLargeObjectDrops, includeAllModdedLargeObjectDrops);
        }
        private void AllowMiningOfEvilStones(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            ILLabel labelToGoTo = cursor.DefineLabel();

            if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchLdarg(3),
                i => i.MatchLdcI4(65),
                i => i.MatchBge(out labelToGoTo)
                ))
            {
                LogIlError("Vanquisher Pickaxe Evil Stone Mining", "Couldn't navigate after stone check");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<Player, bool>>(x => x.GetPlayerHeldItem().type == Type);
            cursor.Emit(OpCodes.Brtrue, labelToGoTo);
        }


        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vanquisher Pickaxe");
            Tooltip.SetDefault("Can mine Ebonstone and Crimstone\nEnriches mined Demonite and Crimtane");
        }

        public override void SetDefaults()
        {
            Item.damage = 9;
            Item.DamageType = DamageClass.Melee;
            Item.width = 24;
            Item.height = 24;
            Item.useTime = 15;
            Item.useAnimation = 19;
            Item.pick = 55;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3;
            Item.value = Item.sellPrice(0, 0, 25, 0);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.tileBoost += 1;
            Item.scale = 1.15f;
        }
    }
    public class VanquisherPickaxeRecipeSystem : ModSystem
    {
        public override void AddRecipes()
        {
            Recipe.Create(ItemID.DemoniteBar)
                .AddIngredient(ModContent.ItemType<EnrichedDemonite>(), 3)
                .AddTile(TileID.Furnaces)
                .Register();

            Recipe.Create(ItemID.CrimtaneBar)
                .AddIngredient(ModContent.ItemType<BloatedCrimtane>(), 3)
                .AddTile(TileID.Furnaces)
                .Register();

            Recipe.Create(ItemID.DemoniteBrick, 5)
                .AddIngredient(ModContent.ItemType<EnrichedDemonite>(), 1)
                .AddIngredient(ItemID.EbonstoneBlock, 5)
                .AddTile(TileID.Furnaces)
                .Register();

            Recipe.Create(ItemID.CrimtaneBrick, 5)
                .AddIngredient(ModContent.ItemType<BloatedCrimtane>(), 1)
                .AddIngredient(ItemID.CrimstoneBlock, 5)
                .Register();
        }
        public override void PostAddRecipes()
        {
            foreach (Recipe recipe in Main.recipe)
            {
                // disable demonite/crimtane bar uncrafting to prevent dupe exploit
                if (((recipe.createItem.type == ItemID.DemoniteBar || recipe.createItem.type == ItemID.DemoniteBrick) && recipe.requiredItem[0].type == ItemID.DemoniteOre) || 
                    ((recipe.createItem.type == ItemID.CrimtaneBar || recipe.createItem.type == ItemID.CrimtaneBrick) && recipe.requiredItem[0].type == ItemID.CrimtaneOre)) 
                    recipe.DisableDecraft();
            }
        }
    }
}