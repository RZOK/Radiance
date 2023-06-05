﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.NPCs;
using Radiance.Core;
using Radiance.Core.Interfaces;
using Radiance.Utilities;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles.CeremonialDish
{
    public class CeremonialDish : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.CoordinateHeights = new int[2] { 16, 18 };
            HitSound = SoundID.Dig;
            DustType = -1;

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Alluring Dish");
            AddMapEntry(new Color(0, 188, 207), name);

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<CeremonialDishTileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }

        public override void HitWire(int i, int j)
        {
            RadianceUtils.ToggleTileEntity(i, j);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out CeremonialDishTileEntity entity))
            {
                Tile tile = Framing.GetTileSafely(i, j);
                string texPath = entity.GetFirstSlotWithItem(out _) ? CeremonialDishTileEntity.filledTexture : CeremonialDishTileEntity.emptyTexture;
                Texture2D texture = ModContent.Request<Texture2D>(texPath).Value;
                if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
                {
                    Color tileColor = Lighting.GetColor(i, j);
                    Vector2 mainPosition = new Vector2(i, j) * 16 + new Vector2(entity.Width * 8, entity.Height * 16) + RadianceUtils.tileDrawingZero - Main.screenPosition;
                    Vector2 origin = new Vector2(texture.Width / 2, texture.Height);
                    Main.spriteBatch.Draw
                    (
                        texture,
                        mainPosition,
                        null,
                        tileColor,
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
            if (RadianceUtils.TryGetTileEntityAs(i, j, out CeremonialDishTileEntity entity))
            {
                Item item = RadianceUtils.GetPlayerHeldItem();
                byte slot = (byte)(item.type == ItemID.Grubby ? 0 : item.type == ItemID.Sluggy ? 1 : item.type == ItemID.Buggy ? 2 : 3);
                bool success = false;
                if (slot == 3 && entity.GetFirstSlotWithItem(out byte dropSlot))
                    entity.DropItem(dropSlot, new Vector2(i * 16, j * 16));
                if (slot != 3)
                {
                    if (entity.GetSlot(slot).stack == entity.GetSlot(slot).maxStack)
                        entity.DropItem(slot, new Vector2(i * 16, j * 16));
                    entity.SafeInsertItemIntoSlot(slot, ref item, out success);
                }
                if (success)
                    SoundEngine.PlaySound(SoundID.MenuTick);

                return true;
            }
            return false;
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (RadianceUtils.TryGetTileEntityAs(i, j, out CeremonialDishTileEntity entity))
            {
                player.noThrow = 2;
                player.cursorItemIconEnabled = true;
                player.cursorItemIconID = ItemID.Grubby;
                entity.AddHoverUI();
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out CeremonialDishTileEntity entity))
                entity.DropAllItems(entity.TileEntityWorldCenter());

            Point16 origin = RadianceUtils.GetTileOrigin(i, j);
            ModContent.GetInstance<CeremonialDishTileEntity>().Kill(origin.X, origin.Y);
        }
    }

    public class CeremonialDishTileEntity : ImprovedTileEntity, IInventory
    {
        public CeremonialDishTileEntity() : base(ModContent.TileType<CeremonialDish>(), 1)
        {
        }

        public Item[] inventory { get; set; }
        public byte[] inputtableSlots => new byte[] { 0, 1, 2 };
        public byte[] outputtableSlots => Array.Empty<byte>();

        internal const string emptyTexture = "Radiance/Content/Tiles/CeremonialDish/CeremonialDishEmpty";
        internal const string filledTexture = "Radiance/Content/Tiles/CeremonialDish/CeremonialDishFilled";
        private List<WyvernSaveData> wyvernSaves;
        public int feedingTimer = 0;
        #region i am so full of properties yum
        public List<byte> SlotsWithFood => this.GetSlotsWithItems();
        public bool HasFood => SlotsWithFood != null;
        public bool ReadyToFeed => feedingTimer >= 36000; //10 minutes
        public bool CanSpawnWyverns => WyvernsWithThisAsTheirHome.Count < 3 && WyvernsInWorld.Count < 21;
        public static List<NPC> WyvernsInWorld => Main.npc.Where(x => x.active && x.ModNPC is WyvernHatchling hatchling).ToList();
        public List<NPC> WyvernsWithThisAsTheirHome => WyvernsInWorld.Where(x => (x.ModNPC as WyvernHatchling).home == this).ToList();
        public bool WyvernCurrentlyComingToFeed => WyvernsWithThisAsTheirHome.Where(x => (x.ModNPC as WyvernHatchling).currentAction == WyvernHatchling.WyvernAction.FeedingDash).Any();
        public Vector2 BowlPos => this.TileEntityWorldCenter() - Vector2.UnitY * Height * 6;
        #endregion

        public override void Load()
        {
            ModContent.Request<Texture2D>(emptyTexture, AssetRequestMode.ImmediateLoad);
            ModContent.Request<Texture2D>(filledTexture, AssetRequestMode.ImmediateLoad);
        }

        public override void OrderedUpdate()
        {
            this.ConstructInventory(3);
            if (wyvernSaves != null) //load saved wyverns
            {
                foreach (WyvernSaveData data in wyvernSaves)
                {
                    WyvernHatchling hatchling = Main.npc[NPC.NewNPC(new EntitySource_TileEntity(this), (int)data.position.X, (int)data.position.Y, ModContent.NPCType<WyvernHatchling>())].ModNPC as WyvernHatchling;
                    hatchling.archWyvern = data.arch;
                    hatchling.NPC.velocity = Vector2.UnitX.RotatedBy(data.rotation);
                    hatchling.NPC.direction = data.direction ? 1 : -1;
                    hatchling.home = this;
                    hatchling.timer = Main.rand.Next(1200);
                    hatchling.wibbleOffset = Main.rand.Next(120);

                    if (hatchling.segments[0] != null)
                    {
                        foreach (WyvernHatchlingSegment segment in hatchling.segments)
                        {
                            if (segment.index != 0)
                            {
                                segment.position = segment.parent.position - Vector2.UnitX.RotatedBy(segment.parent.rotation) * segment.Width;
                                segment.rotation = segment.parent.rotation;
                            }
                            else
                                segment.rotation = data.rotation;
                        }
                    }
                }
                wyvernSaves = null;
            }

            if (HasFood)
            {
                if (!ReadyToFeed)
                    feedingTimer++;
                else if (Main.rand.NextBool(600) && CanSpawnWyverns)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 position = this.TileEntityWorldCenter() + Main.rand.NextVector2CircularEdge(1000, 600) - Vector2.UnitY * 300;
                        WyvernHatchling hatchling = Main.npc[NPC.NewNPC(new EntitySource_TileEntity(this), (int)position.X, (int)position.Y, ModContent.NPCType<WyvernHatchling>())].ModNPC as WyvernHatchling;
                        hatchling.NPC.direction = (this.TileEntityWorldCenter().X - hatchling.NPC.Center.X).NonZeroSign();
                        hatchling.home = this;
                        hatchling.currentAction = WyvernHatchling.WyvernAction.FeedingDash;
                    }
                }
            }
        }

        public void Feed(byte slot)
        {
            this.GetSlot(slot).stack -= 1;
            if (this.GetSlot(slot).stack == 0)
                this.GetSlot(slot).TurnToAir();
            feedingTimer = 0;
            SoundEngine.PlaySound(SoundID.Item2, BowlPos);
            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustPerfect(BowlPos + Vector2.UnitX * Main.rand.NextFloat(-6, 6), 249 + slot, new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), -RadianceUtils.EaseInCirc(Main.rand.NextFloat(0.5f, 1))));
                d.scale = 0.8f;
                d.noGravity = true;
                d.fadeIn = 1f;
            }
        }

        protected override HoverUIData ManageHoverUI()
        {
            List<HoverUIElement> data = new List<HoverUIElement>()
            {
                new CeremonialDishUIElement("GrubbyCount", 0, new Vector2(-40, -30)),
                new CeremonialDishUIElement("SluggyCount", 1, new Vector2(0, -40)),
                new CeremonialDishUIElement("BuggyCount", 2, new Vector2(40, -30)),
            };

            return new HoverUIData(this, this.TileEntityWorldCenter(), data.ToArray());
        }

        public override void SaveData(TagCompound tag)
        {
            //add feeding save/load
            wyvernSaves = new List<WyvernSaveData>();
            foreach (NPC npc in Main.npc.Where(x => x.active && x.ModNPC is WyvernHatchling hatchling && hatchling.home == this))
            {
                WyvernHatchling wyvern = npc.ModNPC as WyvernHatchling;
                wyvernSaves.Add(new WyvernSaveData(wyvern.NPC.Center, wyvern.archWyvern, wyvern.NPC.direction == 1, wyvern.rotation));
            }
            tag.Add("WyvernSaveData", wyvernSaves);
            this.SaveInventory(tag);
        }

        public override void LoadData(TagCompound tag)
        {
            wyvernSaves = (List<WyvernSaveData>)tag.GetList<WyvernSaveData>("WyvernSaveData");
            this.LoadInventory(tag, 3);
        }
    }

    public class CeremonialDishUIElement : HoverUIElement
    {
        public byte slot = 0;

        public CeremonialDishUIElement(string name, byte slot, Vector2 targetPosition) : base(name)
        {
            this.targetPosition = targetPosition;
            this.slot = slot;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            CeremonialDishTileEntity entity = parent.entity as CeremonialDishTileEntity;
            if (entity != null)
            {
                Color grubbyColor = new Color(139, 86, 218);
                Color sluggyColor = new Color(218, 182, 86);
                Color buggyColor = new Color(183, 59, 82);
                RadianceDrawing.DrawSoftGlow(elementPosition, (slot == 0 ? grubbyColor : slot == 1 ? sluggyColor : buggyColor) * timerModifier, Math.Max(0.3f * (float)Math.Abs(RadianceUtils.SineTiming(100)), 0.32f), RadianceDrawing.DrawingMode.Default);
                RadianceDrawing.DrawSoftGlow(elementPosition, Color.White * timerModifier, Math.Max(0.2f * (float)Math.Abs(RadianceUtils.SineTiming(100)), 0.22f), RadianceDrawing.DrawingMode.Default);
                RadianceDrawing.DrawHoverableItem(Main.spriteBatch, entity.GetSlot(slot).type, realDrawPosition, entity.GetSlot(slot).stack, Color.White * timerModifier);
            }
        }
    }

    internal struct WyvernSaveData : TagSerializable
    {
        //12 bytes + 2 bits of data per wyvern
        internal Vector2 position;
        internal float rotation;

        internal bool arch;
        internal bool direction;

        public WyvernSaveData(Vector2 position, bool arch, bool direction, float rotation)
        {
            this.position = position;
            this.arch = arch;
            this.direction = direction;
            this.rotation = rotation;
        }

        public static readonly Func<TagCompound, WyvernSaveData> DESERIALIZER = DeserializeData;

        public TagCompound SerializeData()
        {
            return new TagCompound()
            {
                ["Position"] = position,
                ["Arch"] = arch,
                ["Direction"] = direction,
                ["Rotation"] = rotation,
            };
        }

        public static WyvernSaveData DeserializeData(TagCompound tag)
        {
            WyvernSaveData wyvernSaveData = new()
            {
                position = tag.Get<Vector2>("Position"),
                arch = tag.GetBool("Arch"),
                direction = tag.GetBool("Direction"),
                rotation = tag.GetFloat("Rotation"),
            };
            return wyvernSaveData;
        }
    }

    public class CeremonialDishItem : BaseTileItem
    {
        public CeremonialDishItem() : base("CeremonialDishItem", "Alluring Dish", "Attracts Wyvern Hatchlings when proper bait is placed inside", "CeremonialDish", 1, Item.sellPrice(0, 1, 0, 0), ItemRarityID.Pink)
        {
        }
    }
}