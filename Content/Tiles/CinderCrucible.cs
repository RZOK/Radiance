using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Particles;
using Radiance.Core;
using Radiance.Core.Interfaces;
using Radiance.Core.Systems;
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

namespace Radiance.Content.Tiles
{
    public class CinderCrucible : ModTile
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
            name.SetDefault("Cinder Crucible");
            AddMapEntry(new Color(219, 33, 0), name);

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<CinderCrucibleTileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }

        public override void HitWire(int i, int j)
        {
            RadianceUtils.ToggleTileEntity(i, j);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out CinderCrucibleTileEntity entity))
            {
                Tile tile = Main.tile[i, j];
                if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
                {
                    Texture2D mainTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/CinderCrucibleMain").Value;
                    Texture2D glowTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/CinderCrucibleGlow").Value;
                    Color tileColor = Lighting.GetColor(i, j);
                    Color glowColor = Color.White;

                    Vector2 mainPosition = new Vector2(i, j) * 16 + new Vector2(entity.Width * 8, entity.Height * 16) + RadianceUtils.tileDrawingZero - Main.screenPosition;
                    Vector2 origin = new Vector2(mainTexture.Width / 2, mainTexture.Height);
                    Main.spriteBatch.Draw
                    (
                        mainTexture,
                        mainPosition,
                        null,
                        tileColor,
                        0,
                        origin,
                        1,
                        SpriteEffects.None,
                        0
                    );
                    Main.spriteBatch.Draw
                    (
                        glowTexture,
                        mainPosition,
                        null,
                        glowColor,
                        0,
                        origin,
                        1,
                        SpriteEffects.None,
                        0
                    );
                }
            }
            return false;
        }

        public override bool RightClick(int i, int j)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out CinderCrucibleTileEntity entity))
            {
                Item item = RadianceUtils.GetPlayerHeldItem();
                    bool success = false;
                    if (entity.GetSlot(0).type != item.type || entity.GetSlot(0).stack == entity.GetSlot(0).maxStack)
                        entity.DropItem(0, new Vector2(i * 16, j * 16), new EntitySource_TileInteraction(null, i, j));
                    if(item.type == ItemID.Hellstone || item.type == ItemID.HellstoneBar)

                        entity.SafeInsertItemIntoSlot(0, ref item, out success);
                    item.TurnToAir();
                    return true;
                
            }
            return false; 
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            RadianceInterfacePlayer mp = player.GetModPlayer<RadianceInterfacePlayer>();
            if (RadianceUtils.TryGetTileEntityAs(i, j, out CinderCrucibleTileEntity entity))
            {
                List<HoverUIElement> data = new List<HoverUIElement>()
                {
                    new StabilityBarElement(entity.stability, entity.idealStability, Vector2.One * -48)
                };
                if (entity.boostTime > 0)
                    data.Add(new SquareUIElement(360, new Color(219, 33, 0)));
                mp.currentHoveredObjects.Add(new HoverUIData(entity, entity.TileEntityWorldCenter() - Vector2.UnitY * 8, data.ToArray()));

                player.noThrow = 2;
                player.cursorItemIconEnabled = true;
                player.cursorItemIconID = ItemID.Hellstone;
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out CinderCrucibleTileEntity entity))
            {
                entity.DropAllItems(entity.TileEntityWorldCenter(), new EntitySource_TileBreak(i, j));
            }
            Point16 origin = RadianceUtils.GetTileOrigin(i, j);
            ModContent.GetInstance<CinderCrucibleTileEntity>().Kill(origin.X, origin.Y);
        }
    }

    public class CinderCrucibleTileEntity : ImprovedTileEntity, IInventory
    {
        public CinderCrucibleTileEntity() : base(ModContent.TileType<CinderCrucible>(), true) { }

        public int boostTime = 0;
        public int meltingTime = 0;

        public Item[] inventory { get; set; }
        public byte[] inputtableSlots => new byte[1] { 0 };
        public byte[] outputtableSlots => Array.Empty<byte>();

        public override void Update()
        {
            this.ConstructInventory(1);
            idealStability = 50;
            if (!this.GetSlot(0).IsAir)
            {
                meltingTime++;
                if(meltingTime > 300)
                {
                    boostTime += (int)(3600f * (this.GetSlot(0).type == ItemID.HellstoneBar ? 4 : 1) * (isStabilized ? 1 : 0.1f));
                    if (this.GetSlot(0).stack == 1)
                        this.GetSlot(0).TurnToAir();
                    else
                        this.GetSlot(0).stack--;
                    SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/CinderMelt"), this.TileEntityWorldCenter());
                    for (int i = 0; i < 36; i++)
                    {
                        Dust dust = Dust.NewDustPerfect(this.TileEntityWorldCenter() - Vector2.UnitY * 8 + Main.rand.NextVector2Circular(6, 2), DustID.LavaMoss);
                        dust.color = Color.Yellow;
                        dust.velocity = new Vector2(Main.rand.NextFloat(-0.1f, 0.1f), (-i - 1) / 4f * RadianceUtils.EaseInCirc(i / 35f)) * Main.rand.NextFloat(0.9f, 1.1f);
                        dust.scale = Main.rand.NextFloat(1.4f, 1.8f);
                        dust.fadeIn = Main.rand.NextFloat(1.4f, 1.8f);
                    }
                    meltingTime = 0;
                }
            }
            else if(meltingTime > 0)
                meltingTime--;
            if(boostTime > 0)
            {
                if(Main.GameUpdateCount % 30 == 0)
                {
                    if(Main.rand.NextBool(3))
                        ParticleSystem.AddParticle(new TreasureSparkle(this.TileEntityWorldCenter() + Vector2.UnitX * Main.rand.Next(-Width * 8, Width * 8), Vector2.UnitY * -0.1f, 600, 0, 0.6f, new Color(219, 33, 0)));
                }
            }
        }
        public override void SaveData(TagCompound tag)
        {
            this.SaveInventory(ref tag);
            if (boostTime > 0)
                tag["BoostTime"] = boostTime;
        }

        public override void LoadData(TagCompound tag)
        {
            this.LoadInventory(ref tag, 1);
            boostTime = tag.Get<int>("BoostTime");
            base.LoadData(tag);
        }

    }

    public class CinderCrucibleItem : BaseTileItem
    {
        public CinderCrucibleItem() : base("CinderCrucibleItem", "Cinder Crucible", "Consumes Hellstone to improve the efficiency of nearby Radiance Cells on Pedestals", "CinderCrucible", 1, Item.sellPrice(0, 0, 50, 0), ItemRarityID.LightRed) { }
    }
}