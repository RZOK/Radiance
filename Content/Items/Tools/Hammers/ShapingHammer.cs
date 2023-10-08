
using Radiance.Content.UI.NewEntryAlert;
using System.Reflection;
using Terraria.GameInput;

namespace Radiance.Content.Items.Tools.Hammers
{
    public class ShapingHammer : ModItem
    {
        public override void Load()
        {
            On_WorldGen.SlopeTile += ProperHammerSlope;
            On_WorldGen.PoundTile += ProperHammerHalfBlock;
        }

        private bool ProperHammerHalfBlock(On_WorldGen.orig_PoundTile orig, int i, int j)
        {
            Player player = Main.player[Player.FindClosest(new Vector2(i, j) * 16f, 16, 16)];
            ShapingHammerPlayer shapingHammerPlayer = player.GetModPlayer<ShapingHammerPlayer>();
            Tile tile = Framing.GetTileSafely(i, j);

            if (shapingHammerPlayer.currentSetting == ShapingHammerPlayer.ShapingHammerSettings.HalfBlock && tile.IsHalfBlock)
                return false;

            if (shapingHammerPlayer.currentSetting != ShapingHammerPlayer.ShapingHammerSettings.HalfBlock)
                return WorldGen.SlopeTile(i, j, (int)shapingHammerPlayer.currentSetting);

            if (tile.Slope != SlopeType.Solid)
                tile.ReflectionGetMethod("slope", BindingFlags.NonPublic | BindingFlags.Instance, typeof(byte)).Invoke(tile, new object[1] { (byte)0 });

            return orig(i, j);
        }

        private static bool ProperHammerSlope(On_WorldGen.orig_SlopeTile orig, int i, int j, int slope, bool noEffects)
        {
            Player player = Main.player[Player.FindClosest(new Vector2(i, j) * 16f, 16, 16)];
            ShapingHammerPlayer shapingHammerPlayer = player.GetModPlayer<ShapingHammerPlayer>();
            Tile tile = Framing.GetTileSafely(i, j);

            if (!shapingHammerPlayer.shapingHammerEnabled || player.HeldItem.type != ModContent.ItemType<ShapingHammer>())
                return orig(i, j, slope, noEffects);

            if ((int)shapingHammerPlayer.currentSetting == (int)tile.Slope)
                return false;

            if (shapingHammerPlayer.currentSetting == ShapingHammerPlayer.ShapingHammerSettings.HalfBlock)
                return WorldGen.PoundTile(i, j);


            if (tile.IsHalfBlock)
                tile.ReflectionGetMethod("halfBrick", BindingFlags.NonPublic | BindingFlags.Instance, typeof(bool)).Invoke(tile, new object[1] { false });
            
            return orig(i, j, (int)player.GetModPlayer<ShapingHammerPlayer>().currentSetting, noEffects);

        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Worldshaper's Hammer");
            Tooltip.SetDefault("Right click while holding to edit hammer settings");
        }

        public override void SetDefaults()
        {
            Item.damage = 10;
            Item.DamageType = DamageClass.Melee;
            Item.width = 26;
            Item.height = 26;
            Item.useTime = 22;
            Item.useAnimation = 27;
            Item.hammer = 55;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5.5f;
            Item.value = Item.sellPrice(0, 0, 70, 0);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.tileBoost += 1;
            Item.scale = 1.15f;
        }
        public override void HoldItem(Player player)
        {
            if(!player.IsCCd() && !player.ItemAnimationActive)
            {
                if(Main.mouseRight && Main.mouseRightRelease && !ShapingHammerUI.Instance.active)
                {
                    Main.mouseRightRelease = false;
                    ShapingHammerUI.Instance.active = true;

                    if (PlayerInput.UsingGamepad && Main.SmartCursorWanted)
                        ShapingHammerUI.Instance.position = new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
                    else
                        ShapingHammerUI.Instance.position = Main.MouseScreen;
                }
            }
        }
    }
    public class ShapingHammerPlayer : ModPlayer
    {
        public RadialUI ShapingHammerUI;
        public bool shapingHammerEnabled = true;
        public ShapingHammerSettings currentSetting = ShapingHammerSettings.HalfBlock;
        public enum ShapingHammerSettings
        {
            HalfBlock,
            DownLeft,
            DownRight,
            UpLeft,
            UpRight
        }
    }
    public class ShapingHammerUI : RadialUI
    {
        public static ShapingHammerUI Instance;
        public ShapingHammerUI()
        {
            Instance = this;
        }
        public override void LoadElements()
        {
            Main.LocalPlayer.TryGetModPlayer(out ShapingHammerPlayer k);
            center = new RadialUIElement(this, "Radiance/Content/Items/Tools/Hammers/ShapingHammerIcon", 
            () =>
            {
                if (Main.LocalPlayer.TryGetModPlayer(out ShapingHammerPlayer t))
                    t.shapingHammerEnabled = !t.shapingHammerEnabled;
            },
            () =>
            {
                if (Main.LocalPlayer.TryGetModPlayer(out ShapingHammerPlayer t))
                    return t.shapingHammerEnabled;

                return false;
            });
            for (int i = 0; i < 5; i++)
            {
                string texture = "Radiance/Content/Items/Tools/Hammers/ShapingHammerSlope" + i;
                surroundingElements.Add(new RadialUIElement(this, texture, 
                () =>
                {
                    if (Main.LocalPlayer.TryGetModPlayer(out ShapingHammerPlayer t))
                        t.currentSetting = (ShapingHammerPlayer.ShapingHammerSettings)int.Parse(texture.Last().ToString());
                },
                () =>
                {
                    if (Main.LocalPlayer.TryGetModPlayer(out ShapingHammerPlayer t))
                        return t.currentSetting == (ShapingHammerPlayer.ShapingHammerSettings)int.Parse(texture.Last().ToString());

                    return false;

                }));
            }
        }
    }
}