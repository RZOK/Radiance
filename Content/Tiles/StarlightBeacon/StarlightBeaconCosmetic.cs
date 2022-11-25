using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.TileItems;
using Radiance.Core;
using Radiance.Utilities;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles.StarlightBeacon
{
    public class StarlightBeaconCosmetic : ModTile
    {
        public override string Texture => "Radiance/Content/Tiles/StarlightBeacon/StarlightBeacon"; 
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.CoordinateHeights = new int[2] { 16, 18 };

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Starlight Beacon");
            AddMapEntry(new Color(76, 237, 202), name);

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<StarlightBeaconCosmeticTileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }

        public double easingFunction(float x)
        {
            return x < 0.5 ? 8 * x * x * x * x : 1 - Math.Pow(-2 * x + 2, 4) / 2;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out StarlightBeaconCosmeticTileEntity entity))
            {
                Tile tile = Main.tile[i, j];
                if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
                {
                    float deployTimer = entity.deployTimer;
                    Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
                    Texture2D legsTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/StarlightBeacon/StarlightBeaconLegs").Value;
                    Texture2D mainTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/StarlightBeacon/StarlightBeaconMain").Value;
                    Texture2D mainGlowTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/StarlightBeacon/StarlightBeaconMainGlow").Value;
                    Texture2D coverTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/StarlightBeacon/StarlightBeaconCover").Value;
                    Texture2D coverGlowTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/StarlightBeacon/StarlightBeaconCoverGlow").Value;

                    Color glowColor = Color.Lerp(new Color(255, 50, 50), new Color(0, 255, 255), deployTimer / 100);

                    Vector2 legsPosition = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero;
                    Vector2 mainPosition = legsPosition + Vector2.UnitY * 20 - Vector2.UnitY * (float)(20 * easingFunction(deployTimer / 600));
                    Vector2 coverOffset1 = new(-coverTexture.Width + 2, -4);
                    Vector2 coverOffset2 = new(2, 4);
                    float coverRotation = (float)((MathHelper.PiOver4 + 2) * easingFunction(deployTimer / 600));
                    //legs
                    Main.spriteBatch.Draw
                    (
                        legsTexture,
                        legsPosition,
                        null,
                        Color.White,
                        0,
                        Vector2.Zero,
                        1,
                        SpriteEffects.None,
                        0
                    );

                    //main
                    Main.spriteBatch.Draw
                    (
                        mainTexture,
                        mainPosition,
                        null,
                        Color.White,
                        0,
                        Vector2.Zero,
                        1,
                        SpriteEffects.None,
                        0
                    );
                    Main.spriteBatch.Draw
                    (
                        mainGlowTexture,
                        mainPosition,
                        null,
                        glowColor,
                        0,
                        Vector2.Zero,
                        1,
                        SpriteEffects.None,
                        0
                    );
                    //covers
                    Main.spriteBatch.Draw
                    (
                        coverTexture,
                        mainPosition + Vector2.UnitX * coverTexture.Width - coverOffset1,
                        null,
                        Color.White,
                        coverRotation,
                        -coverOffset1,
                        1,
                        SpriteEffects.None,
                        0
                    );
                    Main.spriteBatch.Draw
                    (
                        coverTexture,
                        mainPosition + coverOffset2,
                        null,
                        Color.White,
                        -coverRotation,
                        coverOffset2,
                        1,
                        SpriteEffects.FlipHorizontally,
                        0
                    );
                    Main.spriteBatch.Draw
                    (
                        coverGlowTexture,
                        mainPosition + Vector2.UnitX * coverTexture.Width - coverOffset1,
                        null,
                        glowColor,
                        coverRotation,
                        -coverOffset1,
                        1,
                        SpriteEffects.None,
                        0
                    );
                    Main.spriteBatch.Draw
                    (
                        coverGlowTexture,
                        mainPosition + coverOffset2,
                        null,
                        glowColor,
                        -coverRotation,
                        coverOffset2,
                        1,
                        SpriteEffects.FlipHorizontally,
                        0
                    );
                    if (deployTimer > 0)
                    {
                        Vector2 pos = new Vector2(i * 16, j * 16) + zero + new Vector2(entity.width / 2, 0.7f) * 16 + Vector2.UnitX * 8;
                        float mult = (float)Math.Clamp(Math.Abs(RadianceUtils.SineTiming(120)), 0.7f, 1f);
                        for (int h = 0; h < 2; h++)
                            RadianceDrawing.DrawBeam(pos, new Vector2(pos.X, 0), h == 1 ? new Color(255, 255, 255, entity.beamTimer).ToVector4() * mult : new Color(0, 255, 255, entity.beamTimer).ToVector4() * mult, 0.2f, h == 1 ? 10 : 14, Matrix.Identity);
                        RadianceDrawing.DrawSoftGlow(pos, new Color(0, 255, 255, entity.beamTimer) * mult, 0.25f, Matrix.Identity);
                    }
                }
            }
            return false;
        }

        public override void MouseOver(int i, int j)
        {
            
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 16, ModContent.ItemType<StarlightBeaconCosmeticItem>());
            Point16 origin = RadianceUtils.GetTileOrigin(i, j);
            ModContent.GetInstance<StarlightBeaconCosmeticTileEntity>().Kill(origin.X, origin.Y);
        }
    }

    public class StarlightBeaconCosmeticTileEntity : ModTileEntity
    {
        
        public float deployTimer = 600;
        public int beamTimer = 0;
        public int pickupTimer = 0;
        public bool deployed = false;
        public int width = 3;
        public int height = 2;
        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<StarlightBeaconCosmetic>();
        }
        public override void OnNetPlace()
        {
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
        }
        public override void Update()
        {
            if (!Main.dayTime)
            {
                if (deployTimer < 600)
                {
                    if (deployTimer == 40)
                        SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/BeaconLift"), Position.ToVector2() * 16 + new Vector2(width / 2, -height / 2));
                    deployTimer++;
                }
                if (deployTimer >= 600)
                {
                    if (beamTimer < 255)
                        beamTimer++;
                }
            }
            else if (beamTimer > 0 && deployTimer < 600)
                beamTimer -= Math.Clamp(beamTimer, 0, 2);
            else if (deployTimer > 0)
            {
                pickupTimer = 0;
                if (deployTimer == 550)
                    SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/BeaconLift"), Position.ToVector2() * 16 + new Vector2(width / 2, -height / 2)); //todo: make sound not freeze game for a moment when played for the first time in an instance
                deployTimer--;
            }
        }
        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, width, height);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);
            }
            int placedEntity = Place(i - 1, j - 1);
            return placedEntity;
        }
    }
}