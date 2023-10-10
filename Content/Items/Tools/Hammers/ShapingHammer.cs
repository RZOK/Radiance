using Radiance.Content.UI;
using System.Drawing.Drawing2D;
using Terraria.GameInput;

namespace Radiance.Content.Items.Tools.Hammers
{
    public class ShapingHammer : ModItem
    {
        public override void Load()
        {
            On_WorldGen.SlopeTile += ProperHammerSlope;
            On_WorldGen.PoundTile += ProperHammerHalfBlock;
            RadialUIMouseIndicatorSystem.radialUIMouseIndicatorData.Add(new RadialUIMouseIndicatorData(() =>
            !ShapingHammerUI.Instance.active && Main.LocalPlayer.HeldItem.type == ModContent.ItemType<ShapingHammer>() && Main.LocalPlayer.cursorItemIconEnabled && RadialUIMouseIndicator.realCursorItemType == ModContent.ItemType<ShapingHammer>(), 
            DrawShapingHammerMouseUI,
            1f));
        }
        private static void DrawShapingHammerMouseUI(SpriteBatch spriteBatch, float opacity)
        {
            ShapingHammerPlayer shapingHammerPlayer = Main.LocalPlayer.GetModPlayer<ShapingHammerPlayer>();
            Texture2D tex = ModContent.Request<Texture2D>("Radiance/Content/Items/Tools/Hammers/ShapingHammerSlopeSmall" + (int)shapingHammerPlayer.currentSetting).Value;
            Vector2 position = Main.MouseScreen + new Vector2(GetItemTexture(RadialUIMouseIndicator.realCursorItemType).Width / -2f + 12f, 24f);
            Color color = Color.White;
            if (!shapingHammerPlayer.shapingHammerEnabled)
                color = new Color(100, 100, 100);

            spriteBatch.Draw(tex, position, null, color * opacity, 0, tex.Size() / 2, 1f, SpriteEffects.None, 0);
        }
        #region Detours
        private bool ProperHammerHalfBlock(On_WorldGen.orig_PoundTile orig, int i, int j)
        {
            if (WorldGen.gen)
                return orig(i, j);

            Player player = Main.player[Player.FindClosest(new Vector2(i, j) * 16f, 16, 16)];
            ShapingHammerPlayer shapingHammerPlayer = player.GetModPlayer<ShapingHammerPlayer>();
            Tile tile = Framing.GetTileSafely(i, j);

            // if hammer is disabled or not in hand, return
            if (!shapingHammerPlayer.shapingHammerEnabled || player.HeldItem.type != ModContent.ItemType<ShapingHammer>())
                return orig(i, j);

            // if hammer is in full block setting, make it a full block and then return
            if (shapingHammerPlayer.currentSetting == ShapingHammerPlayer.ShapingHammerSettings.FullBlock)
            {
                tile.IsHalfBlock = false;
                tile.Slope = SlopeType.Solid;
                AdjustPlayerPosition(i, j);
                WorldGen.KillTile(i, j, fail: true, effectOnly: true);
                SoundEngine.PlaySound(SoundID.Dig, new Vector2(i, j).ToWorldCoordinates());
                return false;
            }

            // if half block setting but also the block is already a half block, just do the visual effects
            if (shapingHammerPlayer.currentSetting == ShapingHammerPlayer.ShapingHammerSettings.HalfBlock && tile.IsHalfBlock)
            {
                WorldGen.KillTile(i, j, fail: true, effectOnly: true);
                SoundEngine.PlaySound(SoundID.Dig, new Vector2(i, j).ToWorldCoordinates());
                return false;
            }
            
            // if current setting is not half block, go do slope stuff instead
            if (shapingHammerPlayer.currentSetting != ShapingHammerPlayer.ShapingHammerSettings.HalfBlock)
                return WorldGen.SlopeTile(i, j, (int)shapingHammerPlayer.currentSetting);

            // by this point everything that isn't half block should be elimnated, so adjust it to a slope first before making it a half block
            if (tile.Slope != SlopeType.Solid)
            {
                tile.Slope = SlopeType.Solid;
                AdjustPlayerPosition(i, j);
            }
            return orig(i, j);
        }

        private static bool ProperHammerSlope(On_WorldGen.orig_SlopeTile orig, int i, int j, int slope, bool noEffects)
        {
            if (WorldGen.gen)
                return orig(i, j, slope, noEffects);

            Player player = Main.player[Player.FindClosest(new Vector2(i, j) * 16f, 16, 16)];
            ShapingHammerPlayer shapingHammerPlayer = player.GetModPlayer<ShapingHammerPlayer>();
            Tile tile = Framing.GetTileSafely(i, j);

            // if hammer is disabled or not in hand, return
            if (!shapingHammerPlayer.shapingHammerEnabled || player.HeldItem.type != ModContent.ItemType<ShapingHammer>())
                return orig(i, j, slope, noEffects);

            // if hammer is in full block setting, make it a full block and then return
            if (shapingHammerPlayer.currentSetting == ShapingHammerPlayer.ShapingHammerSettings.FullBlock)
            {
                tile.IsHalfBlock = false;
                tile.Slope = SlopeType.Solid;
                AdjustPlayerPosition(i, j);
                WorldGen.KillTile(i, j, fail: true, effectOnly: true);
                SoundEngine.PlaySound(SoundID.Dig, new Vector2(i, j).ToWorldCoordinates());
                return false;
            }

            // if slope setting but also the block is already the slope, just do the visual effects
            if ((int)shapingHammerPlayer.currentSetting == (int)tile.Slope)
            {
                WorldGen.KillTile(i, j, fail: true, effectOnly: true);
                SoundEngine.PlaySound(SoundID.Dig, new Vector2(i, j).ToWorldCoordinates());
                return false;
            }

            // if current setting is not slope, go do half block stuff instead
            if (shapingHammerPlayer.currentSetting == ShapingHammerPlayer.ShapingHammerSettings.HalfBlock)
                return WorldGen.PoundTile(i, j);

            // by this point everything that isn't slope should be elimnated, so adjust it to a half block first before making it a slope
            if (tile.IsHalfBlock)
            {
                tile.IsHalfBlock = false;
                AdjustPlayerPosition(i, j);
            }
            return orig(i, j, (int)player.GetModPlayer<ShapingHammerPlayer>().currentSetting, noEffects);

        }
        private static void AdjustPlayerPosition(int i, int j)
        {
            WorldGen.SquareTileFrame(i, j);
            if (!Main.tile[i, j].IsHalfBlock || Main.tile[i, j].Slope == SlopeType.Solid)
            {
                Rectangle rectangle = new Rectangle(i * 16, j * 16, 16, 16);
                for (int k = 0; k < 255; k++)
                {
                    if (Main.player[k].active && !Main.player[k].dead && rectangle.Intersects(new Rectangle((int)Main.player[k].position.X, (int)Main.player[k].position.Y, Main.player[k].width, Main.player[k].height)))
                    {
                        Main.player[k].gfxOffY += Main.player[k].position.Y + Main.player[k].height - rectangle.Y;
                        Main.player[k].position.Y = rectangle.Y - Main.player[k].height;
                    }
                }
            }
        }
        #endregion
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Formshaping Hammer");
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
        public ShapingHammerSettings currentSetting = ShapingHammerSettings.FullBlock;
        public enum ShapingHammerSettings
        {
            FullBlock,
            DownLeft,
            DownRight,
            UpLeft,
            UpRight,
            HalfBlock
        }
    }
    public class ShapingHammerUI : RadialUI
    {
        public static ShapingHammerUI Instance;
        public ShapingHammerUI()
        {
            Instance = this;
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
            for (int i = 0; i < Enum.GetNames(typeof(ShapingHammerPlayer.ShapingHammerSettings)).Length; i++)
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