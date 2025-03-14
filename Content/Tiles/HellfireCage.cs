﻿using Radiance.Content.Items.BaseItems;
using Radiance.Content.Particles;
using Radiance.Core.Systems.ParticleSystems;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles
{
    public class HellfireCage : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileSolidTop[Type] = true;
            Main.tileLavaDeath[Type] = false;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.CoordinateHeights = new int[2] { 16, 18 };
            HitSound = SoundID.Item52;
            DustType = -1;

            LocalizedText name = CreateMapEntryName(); 
            name.SetDefault("Hellfire Cage");
            AddMapEntry(new Color(235, 103, 63), name);

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<HellfireCageTileEntity>().Hook_AfterPlacement, -1, 0, false);
            TileObjectData.addTile(Type);
        }

        public override void HitWire(int i, int j)
        {
            ToggleTileEntity(i, j);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (TryGetTileEntityAs(i, j, out HellfireCageTileEntity entity))
            {
                Tile tile = Main.tile[i, j];
                if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
                {
                    Texture2D tex = ModContent.Request<Texture2D>("Radiance/Content/Tiles/HellfireCageFull").Value;
                    float rotation = (float)Math.Sin(entity.bounceModifier / 5 * Math.PI) / 6;
                    spriteBatch.Draw(tex, new Vector2(i, j) * 16 - Main.screenPosition + TileDrawingZero + new Vector2(tex.Width / 2, tex.Height) - Vector2.UnitY * entity.bounceModifier / 5, null, Lighting.GetColor(new Point(i, j)), rotation, new Vector2(tex.Width / 2, tex.Height), new Vector2(1, 1 + (entity.bounceModifier / 100)), SpriteEffects.None, 0);
                }
            }
            return false;
        }

        public override void MouseOver(int i, int j)
        {
            if (TryGetTileEntityAs(i, j, out HellfireCageTileEntity entity))
                entity.AddHoverUI();
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Point origin = GetTileOrigin(i, j);
            ModContent.GetInstance<HellfireCageTileEntity>().Kill(origin.X, origin.Y);
        }
    }

    public class HellfireCageTileEntity : RadianceUtilizingTileEntity
    {
        public HellfireCageTileEntity() : base(ModContent.TileType<HellfireCage>(), 400, new() { 3, 4 }, new()) { }

        public int actionTimer = 0;
        public float transformTimer = 0;
        public float visualTimer = 0;
        public float bounceModifier = 0;
        protected override HoverUIData GetHoverData()
        {
            List<HoverUIElement> data = new List<HoverUIElement>()
                {
                    new RadianceBarUIElement("RadianceBar", storedRadiance, maxRadiance, Vector2.UnitY * 40),
                    new SquareUIElement("AoESquare", 168, new Color(235, 103, 63))
                };

            return new HoverUIData(this, this.TileEntityWorldCenter(), data.ToArray());
        }
        public override void OrderedUpdate()
        {
            if (Main.GameUpdateCount % 300 == 0)
                bounceModifier = 10;
            if (bounceModifier > 0)
                bounceModifier--;

            if (enabled)
            {
                if (visualTimer > 30 && actionTimer > 0 && Main.rand.NextBool(6))
                {
                    Vector2 tileCenter = (Position.ToVector2() + Vector2.One) * 16;
                    float offset = Main.rand.NextFloat(-16, 16);
                    Vector2 vectorOffset = Main.rand.Next(new[] { new Vector2(16, offset), new Vector2(offset, 16) });
                    WorldParticleSystem.system.AddParticle(new MiniLightning(tileCenter + vectorOffset, tileCenter - vectorOffset, new Color(235, 103, 63), 10));

                    visualTimer = 0;
                }
                visualTimer++;

                if (storedRadiance >= 50)
                    actionTimer++;
                else
                {
                    actionTimer = 0;
                    transformTimer = 0;
                }

                if (actionTimer >= 10)
                {
                    Vector2 center = Position.ToVector2();
                    Rectangle clampBox = new Rectangle(2, 2, Main.maxTilesX - 2, Main.maxTilesY - 2);
                    int radius = 10;
                    int leftBound = Utils.Clamp((int)center.X - radius, clampBox.Left, clampBox.Right);
                    int rightBound = Utils.Clamp((int)center.X + radius + 2, clampBox.Left, clampBox.Right);
                    int topBound = Utils.Clamp((int)center.Y - radius, clampBox.Top, clampBox.Bottom);
                    int bottomBound = Utils.Clamp((int)center.Y + radius + 2, clampBox.Top, clampBox.Bottom);
                    List<Point> obsidianPositions = new List<Point>();
                    for (int i = leftBound; i < rightBound; i++)
                    {
                        for (int j = topBound; j < bottomBound; j++)
                        {
                            if (Framing.GetTileSafely(i, j).TileType == TileID.Obsidian)
                                obsidianPositions.Add(new Point(i, j));
                        }
                    }

                    if (obsidianPositions.Count > 0)
                        transformTimer += (float)Math.Sqrt(obsidianPositions.Count) / 4 + 1;

                    if (transformTimer > 60)
                    {
                        storedRadiance -= 50;

                        Point randomPos = Main.rand.Next(obsidianPositions);
                        Vector2 tileCenter = randomPos.ToVector2() * 16 + Vector2.One * 8;
                        Tile tile = Framing.GetTileSafely(randomPos.X, randomPos.Y);

                        WorldParticleSystem.system.AddParticle(new Lightning(RadianceUtils.TileEntityWorldCenter(this), tileCenter, new Color(235, 103, 63), 14));
                        //SoundEngine.PlaySound(SoundID.Tink, tileCenter);
                        SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/LightningZap") with { PitchVariance = 0.5f, Volume = 2f }, tileCenter);
                        for (int i = 0; i < 12; i++)
                        {
                            WorldParticleSystem.system.AddParticle(new GlowOrb(tileCenter, Vector2.UnitX.RotatedByRandom(TwoPi) * Main.rand.NextFloat(2, 5), 60, 8, 12, 0, new Color(235, 103, 63), Color.White, true));

                            //ParticleSystem.AddParticle(new Sparkle(tileCenter, Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(2, 5), 60, 100, new Color(235, 103, 63)));

                            //Dust dust = Main.dust[Dust.NewDust(randomPos.ToVector2() * 16, 16, 16, DustID.WhiteTorch)];
                            //dust.velocity = Main.rand.NextVector2Circular(2, 2);
                            //dust.scale = Main.rand.NextFloat(1.2f, 1.5f);
                            //dust.color = new Color(235, 103, 63);
                            //dust.noGravity = true;
                        }

                        tile.IsHalfBlock = false;
                        tile.Slope = SlopeType.Solid;
                        tile.TileType = (ushort)ModContent.TileType<HellfireCageHellstone>();
                        WorldGen.SquareTileFrame(randomPos.X, randomPos.Y, true);
                        NetMessage.SendTileSquare(-1, randomPos.X, randomPos.Y, 1);
                        transformTimer = 0;
                    }
                    actionTimer = 0;
                }
            }
            else
                visualTimer = 0;
        }
    }

    public class HellfireCageItem : BaseTileItem
    {
        public HellfireCageItem() : base("HellfireCageItem", "Hellfire Cage", "Converts nearby obsidian blocks into Hellstone", "HellfireCage", 1, Item.sellPrice(0, 0, 50, 0), ItemRarityID.LightRed) { }
    }
}