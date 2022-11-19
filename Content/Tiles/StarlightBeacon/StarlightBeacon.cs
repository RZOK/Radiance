using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Common;
using Radiance.Content.Items.TileItems;
using Radiance.Utils;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles.StarlightBeacon
{
    public class StarlightBeacon : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.CoordinateHeights = new int[2] { 16, 18 };

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Starlight Beacon");
            AddMapEntry(new Color(76, 237, 202), name);

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<StarlightBeaconTileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (TileUtils.TryGetTileEntityAs(i, j, out StarlightBeaconTileEntity entity))
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
                    Vector2 mainPosition = legsPosition + Vector2.UnitY * 20 - Vector2.UnitY * (float)(20 * MathUtils.EaseInOutQuart(deployTimer / 600));
                    Vector2 coverOffset1 = new Vector2(-coverTexture.Width + 2, -4);
                    Vector2 coverOffset2 = new Vector2(2, 4);
                    float coverRotation = (float)((MathHelper.PiOver4 + 2) * MathUtils.EaseInOutQuart(deployTimer / 600));
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
                        Vector2 pos = new Vector2(i * 16, j * 16) + zero + new Vector2(entity.Width / 2, 0.7f) * 16 + Vector2.UnitX * 8; //tile world coords + half entity width (center of multitiletile) + a bit of increase
                        float mult = (float)Math.Clamp(Math.Abs(MathUtils.sineTiming(120)), 0.85f, 1f); //color multiplier
                        for (int h = 0; h < 2; h++)
                            RadianceDrawing.DrawBeam(pos, new Vector2(pos.X, 0), h == 1 ? new Color(255, 255, 255, entity.beamTimer).ToVector4() * mult : new Color(0, 255, 255, entity.beamTimer).ToVector4() * mult, 0.2f, h == 1 ? 10 : 14, Matrix.Identity);
                        RadianceDrawing.DrawSoftGlow(pos - Vector2.UnitY * 2, new Color(0, 255, 255, entity.beamTimer) * mult, 0.25f, Matrix.Identity);
                    }
                }
            }
            return false;
        }

        public override bool RightClick(int i, int j)
        {
            if (TileUtils.TryGetTileEntityAs(i, j, out StarlightBeaconTileEntity entity))
            {
                Player player = Main.LocalPlayer;
                Item item = MiscUtils.GetPlayerHeldItem();
                if (item.type == ItemID.SoulofFlight)
                {
                    SoundEngine.PlaySound(SoundID.Item42);
                    entity.soulCharge += (item.stack * 5);
                    item.TurnToAir();
                }
            }
            return false;
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            RadiancePlayer mp = player.GetModPlayer<RadiancePlayer>();
            if (TileUtils.TryGetTileEntityAs(i, j, out StarlightBeaconTileEntity entity))
            {
                mp.radianceContainingTileHoverOverCoords = new Vector2(i, j);
                mp.hoveringOverSpecialTextTileCoords = new Vector2(i, j);
                mp.hoveringOverSpecialTextTileColor = new Color(157, 232, 232, 255);
                mp.hoveringOverSpecialTextTileString = entity.soulCharge.ToString();
                mp.hoveringOverSpecialTextTileItemTagString = "[i:" + ItemID.SoulofFlight + "]";
                if (entity.deployTimer == 600)
                {
                    Vector2 pos = MathUtils.MultitileCenterWorldCoords(i, j) + Vector2.UnitX * entity.Width * 8;
                    mp.aoeCirclePosition = pos;
                    mp.aoeCircleColor = new Color(0, 255, 255, 0).ToVector4();
                    mp.aoeCircleScale = 250;
                    mp.aoeCircleMatrix = Main.GameViewMatrix.ZoomMatrix;
                }
                player.noThrow = 2;
                player.cursorItemIconEnabled = true;
                player.cursorItemIconID = ItemID.SoulofFlight;
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (TileUtils.TryGetTileEntityAs(i, j, out StarlightBeaconTileEntity entity) && entity.soulCharge >= 5)
            {
                int stackCount = entity.soulCharge / 5;
                int num = (int)Math.Ceiling((double)stackCount / 999);
                for (int h = 0; h < num; h++)
                {
                    Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 16, ItemID.SoulofFlight, Math.Min(999, stackCount));
                    stackCount -= Math.Min(999, stackCount);
                }
            }
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 16, ModContent.ItemType<StarlightBeaconItem>());
            Point16 origin = TileUtils.GetTileOrigin(i, j);
            ModContent.GetInstance<StarlightBeaconTileEntity>().Kill(origin.X, origin.Y);
        }
    }

    public class StarlightBeaconTileEntity : RadianceUtilizingTileEntity
    {
        #region Fields

        private float maxRadiance = 20;
        private float currentRadiance = 0;
        private int width = 3;
        private int height = 2;
        private List<int> inputTiles = new List<int>() { 4, 6 };
        private List<int> outputTiles = new List<int>() { };
        private int parentTile = ModContent.TileType<StarlightBeacon>();
        public float deployTimer = 600;
        public int beamTimer = 0;
        public int pickupTimer = 0;
        public int soulCharge = 0;
        public bool deployed = false;

        #endregion Fields

        #region Propeties

        public override float MaxRadiance
        {
            get => maxRadiance;
            set => maxRadiance = value;
        }

        public override float CurrentRadiance
        {
            get => currentRadiance;
            set => currentRadiance = value;
        }

        public override int Width
        {
            get => width;
            set => width = value;
        }

        public override int Height
        {
            get => height;
            set => height = value;
        }

        public override int ParentTile
        {
            get => parentTile;
            set => parentTile = value;
        }

        public override List<int> InputTiles
        {
            get => inputTiles;
            set => inputTiles = value;
        }

        public override List<int> OutputTiles
        {
            get => outputTiles;
            set => outputTiles = value;
        }
        #endregion Propeties
        public override void SaveData(TagCompound tag)
        {
            if (CurrentRadiance > 0)
                tag["CurrentRadiance"] = CurrentRadiance;
            if (soulCharge > 0)
                tag["SoulCharge"] = soulCharge;
        }
        public override void LoadData(TagCompound tag)
        {
            CurrentRadiance = tag.Get<float>("CurrentRadiance");
            soulCharge = tag.Get<int>("SoulCharge");
        }
        public override void Update()
        {
            if (!Main.dayTime && currentRadiance >= 1 && soulCharge >= 1)
            {
                Vector2 position = new Vector2(Position.X, Position.Y) * 16 + new Vector2(Width / 2, 0.7f) * 16 + Vector2.UnitX * 8;
                if (deployTimer < 600)
                {
                    if (deployTimer == 40)
                        SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/BeaconLift"), position + new Vector2(width * 8, -height * 8));
                    deployTimer++;
                }
                if (deployTimer >= 600)
                {
                    if (beamTimer < 255)
                        beamTimer++;
                    pickupTimer++;
                    if (pickupTimer >= 60)
                    {
                        for (int i = 0; i < Main.maxItems; i++)
                        {
                            if (Main.item[i].active && Main.item[i].type == ItemID.FallenStar && Vector2.Distance(position, Main.item[i].Center) > 250 && Vector2.Distance(position, Main.item[i].Center) < 51200) //51200 is width of a medium world in pixels halved
                            {
                                currentRadiance--;
                                soulCharge--;
                                Item item = Main.item[i];
                                Vector2 pos = position;
                                pos += Terraria.Utils.DirectionTo(pos, item.Center + item.velocity * 2) * 200;
                                Vector2 itemPos = item.Center;
                                item.Center = pos;
                                pos -= Terraria.Utils.DirectionFrom(position, pos) * 500;
                                item.velocity = Terraria.Utils.DirectionFrom(position, pos) * 10 * Main.rand.NextFloat(0.8f, 1.2f) + new Vector2(0, -5);
                                int a = Vector2.Distance(itemPos, position) > 1100 ? 60 : 30;
                                SoundEngine.PlaySound(SoundID.NPCHit5, position);
                                for (int j = 0; j < a; j++)
                                {
                                    Vector2 velocity = Terraria.Utils.DirectionFrom(position, pos) * 10;
                                    Vector2 dustPosition = pos + Terraria.Utils.DirectionFrom(position, pos) * Main.rand.NextFloat(0, 300);
                                    if (j % 2 == 0 && a == 60)
                                    {
                                        if (j % 6 == 0)
                                            Gore.NewGore(new EntitySource_TileEntity(this), dustPosition, new Vector2(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2)) + velocity / 2, Main.rand.Next(16, 18), 1f);
                                        velocity = Terraria.Utils.DirectionFrom(pos, itemPos) * 10;
                                        dustPosition = itemPos + Terraria.Utils.DirectionFrom(itemPos, pos) * Main.rand.NextFloat(-300, 0);
                                    }
                                    if (j % 3 == 0)
                                    {
                                        Dust b = Dust.NewDustPerfect(dustPosition, 15, velocity * 2, 150, default, 2);
                                        b.noGravity = true;
                                        b.velocity = velocity * 2.5f;
                                        b.fadeIn = 1.4f;
                                        b.position += new Vector2(Main.rand.NextFloat(-16, 16), Main.rand.NextFloat(-16, 16));
                                    }
                                    if (j % 6 == 0)
                                        Gore.NewGore(new EntitySource_TileEntity(this), dustPosition, new Vector2(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2)) + velocity / 2, Main.rand.Next(16, 18), 1f);

                                    Dust d = Dust.NewDustPerfect(dustPosition, 15, velocity * 2, 150, default, 2);
                                    d.noGravity = true;
                                    d.velocity = velocity * 2.5f;
                                    d.fadeIn = 1.4f;
                                    d.position += new Vector2(Main.rand.NextFloat(-8, 8), Main.rand.NextFloat(-8, 8));
                                }
                                pickupTimer = 0;
                                break;
                            }
                        }
                    }
                }
            }
            else if (beamTimer > 0 && deployTimer < 600)
                beamTimer -= Math.Clamp(beamTimer, 0, 2);
            else if (deployTimer > 0)
            {
                Vector2 position = new Vector2(Position.X, Position.Y) * 16 + new Vector2(Width / 2, 0.7f) * 16 + Vector2.UnitX * 8;
                pickupTimer = 0;
                if (deployTimer == 550)
                    SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/BeaconLift"), position + new Vector2(width * 8, -height * 8)); //todo: make sound not freeze game for a moment when played for the first time in an instance
                deployTimer--;
            }
            AddToCoordinateList();
            inputsConnected.Clear();
            outputsConnected.Clear();
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
        public override void OnKill()
        {
            RemoveFromCoordinateList();
        }
    }
}