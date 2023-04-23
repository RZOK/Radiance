using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Radiance.Core;
using Radiance.Utilities;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
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
            HitSound = SoundID.Item52;
            DustType = -1;

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Starcatcher Beacon");
            AddMapEntry(new Color(76, 237, 202), name);

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<StarlightBeaconTileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }

        public override void HitWire(int i, int j)
        {
            RadianceUtils.ToggleTileEntity(i, j);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out StarlightBeaconTileEntity entity))
            {
                Tile tile = Main.tile[i, j];
                if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
                {
                    float deployTimer = entity.deployTimer;
                    Texture2D legsTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/StarlightBeacon/StarlightBeaconLegs").Value;
                    Texture2D mainTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/StarlightBeacon/StarlightBeaconMain").Value;
                    Texture2D mainGlowTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/StarlightBeacon/StarlightBeaconMainGlow").Value;
                    Texture2D coverTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/StarlightBeacon/StarlightBeaconCover").Value;
                    Texture2D coverGlowTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/StarlightBeacon/StarlightBeaconCoverGlow").Value;
                    Color tileColor = Lighting.GetColor(i, j);
                    Color glowColor = Color.Lerp(new Color(255, 50, 50), new Color(0, 255, 255), deployTimer / 100);

                    Vector2 legsPosition = new Vector2(i, j) * 16 - Main.screenPosition + RadianceUtils.tileDrawingZero;
                    Vector2 mainPosition = legsPosition + Vector2.UnitY * 20 - Vector2.UnitY * (float)(20 * RadianceUtils.EaseInOutQuart(deployTimer / 600));
                    Vector2 coverOffset1 = new(-coverTexture.Width + 2, -4);
                    Vector2 coverOffset2 = new(2, 4);
                    float coverRotation = (float)((MathHelper.PiOver4 + 2) * RadianceUtils.EaseInOutQuart(deployTimer / 600));
                    //legs
                    Main.spriteBatch.Draw
                    (
                        legsTexture,
                        legsPosition,
                        null,
                        tileColor,
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
                        tileColor,
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
                        tileColor,
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
                        tileColor,
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
                        Vector2 pos = new Vector2(i * 16, j * 16) + RadianceUtils.tileDrawingZero + new Vector2(entity.Width / 2, 0.7f) * 16 + Vector2.UnitX * 8; //tile world coords + half entity width (center of multitiletile) + a bit of increase
                        float mult = (float)Math.Clamp(Math.Abs(RadianceUtils.SineTiming(120)), 0.85f, 1f); //color multiplier
                        for (int h = 0; h < 2; h++)
                            RadianceDrawing.DrawBeam(pos, new Vector2(pos.X, 0), h == 1 ? new Color(255, 255, 255, entity.beamTimer).ToVector4() * mult : new Color(0, 255, 255, entity.beamTimer).ToVector4() * mult, 0.2f, h == 1 ? 10 : 14, RadianceDrawing.DrawingMode.Tile);

                        RadianceDrawing.DrawSoftGlow(pos - Vector2.UnitY * 2, new Color(0, 255, 255, entity.beamTimer) * mult, 0.25f, RadianceDrawing.DrawingMode.Tile);
                    }
                }
            }
            return false;
        }

        public override bool RightClick(int i, int j)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out StarlightBeaconTileEntity entity))
            {
                Player player = Main.LocalPlayer;
                Item item = RadianceUtils.GetPlayerHeldItem();
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
            RadianceInterfacePlayer mp = player.GetModPlayer<RadianceInterfacePlayer>();
            if (RadianceUtils.TryGetTileEntityAs(i, j, out StarlightBeaconTileEntity entity))
            {
                List<HoverUIElement> data = new List<HoverUIElement>()
                {
                    new RadianceBarUIElement(entity.currentRadiance, entity.maxRadiance, Vector2.UnitY * 40),
                    new TextUIElement(entity.soulCharge.ToString(), new Color(157, 232, 232), -Vector2.UnitY * 40 + new Vector2(-2 * RadianceUtils.SineTiming(33), 2 * RadianceUtils.SineTiming(55))),
                    new ItemUIElement(ItemID.SoulofFlight, new Vector2(-FontAssets.MouseText.Value.MeasureString(entity.soulCharge.ToString()).X / 2 - 16, -42) + new Vector2(-2 * RadianceUtils.SineTiming(33), 2 * RadianceUtils.SineTiming(55)))
                };
                if (entity.deployTimer == 600)
                    data.Add(new CircleUIElement(250, new Color(0, 255, 255)));

                mp.currentHoveredObjects.Add(new HoverUIData(entity, entity.TileEntityWorldCenter(), data.ToArray()));

                player.noThrow = 2;
                player.cursorItemIconEnabled = true;
                player.cursorItemIconID = ItemID.SoulofFlight;
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out StarlightBeaconTileEntity entity) && entity.soulCharge >= 5)
            {
                int stackCount = entity.soulCharge / 5;
                int num = (int)Math.Ceiling((double)stackCount / 999);
                for (int h = 0; h < num; h++)
                {
                    Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 16, ItemID.SoulofFlight, Math.Min(999, stackCount));
                    stackCount -= Math.Min(999, stackCount);
                }
            }
            Point16 origin = RadianceUtils.GetTileOrigin(i, j);
            ModContent.GetInstance<StarlightBeaconTileEntity>().Kill(origin.X, origin.Y);
        }
    }

    public class StarlightBeaconTileEntity : RadianceUtilizingTileEntity
    {
        public StarlightBeaconTileEntity() : base(ModContent.TileType<StarlightBeacon>(), 20, new() { 4, 6 }, new(), false) { }

        public float deployTimer = 600;
        public int beamTimer = 0;
        public int pickupTimer = 0;
        public int soulCharge = 0;
        public bool deployed = false;

        public override void SaveData(TagCompound tag)
        {
            if (soulCharge > 0)
                tag["SoulCharge"] = soulCharge;
            base.SaveData(tag);
        }

        public override void LoadData(TagCompound tag)
        {
            soulCharge = tag.Get<int>("SoulCharge");
            base.LoadData(tag);
        }

        public override void Update()
        {
            if (!Main.dayTime && currentRadiance >= 1 && soulCharge >= 1 && enabled)
            {
                Vector2 position = new Vector2(Position.X, Position.Y) * 16 + new Vector2(Width / 2, 0.7f) * 16 + Vector2.UnitX * 8;
                if (deployTimer < 600)
                {
                    if (deployTimer == 40)
                        SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/BeaconLift"), position + new Vector2(Width * 8, -Height * 8));
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
                                pos += Utils.DirectionTo(pos, item.Center + item.velocity * 2) * 200;
                                Vector2 itemPos = item.Center;
                                item.Center = pos;
                                pos -= Utils.DirectionFrom(position, pos) * 500;
                                item.velocity = Utils.DirectionFrom(position, pos) * 10 * Main.rand.NextFloat(0.8f, 1.2f) + new Vector2(0, -5);
                                int a = Vector2.Distance(itemPos, position) > 1100 ? 60 : 30;
                                SoundEngine.PlaySound(SoundID.NPCHit5, position);
                                for (int j = 0; j < a; j++)
                                {
                                    Vector2 velocity = Utils.DirectionFrom(position, pos) * 10;
                                    Vector2 dustPosition = pos + Utils.DirectionFrom(position, pos) * Main.rand.NextFloat(0, 300);
                                    if (j % 2 == 0 && a == 60)
                                    {
                                        if (j % 6 == 0)
                                            Gore.NewGore(new EntitySource_TileEntity(this), dustPosition, new Vector2(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2)) + velocity / 2, Main.rand.Next(16, 18), 1f);
                                        velocity = Utils.DirectionFrom(pos, itemPos) * 10;
                                        dustPosition = itemPos + Utils.DirectionFrom(itemPos, pos) * Main.rand.NextFloat(-300, 0);
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
                    SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/BeaconLift"), position + new Vector2(Width * 8, -Height * 8)); //todo: make sound not freeze game for a moment when played for the first time in an instance
                deployTimer--;
            }
        }
    }

    public class StarlightBeaconItem : BaseTileItem
    {
        public StarlightBeaconItem() : base("StarlightBeaconItem", "Starcatcher Beacon", "Draws in all stars in a massive radius when deployed\nRequires a small amount of Radiance and Souls of Flight to operate", "StarlightBeacon", 1, Item.sellPrice(0, 0, 50, 0), ItemRarityID.LightRed) { }
    }
}