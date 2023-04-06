using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Radiance.Core;
using Radiance.Core.Interfaces;
using Radiance.Core.Systems;
using Radiance.Utilities;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using static Radiance.Core.Systems.TransmutationRecipeSystem;
using static Radiance.Utilities.InventoryUtils;

namespace Radiance.Content.Tiles.Transmutator
{
    public class Transmutator : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            Main.tileNoAttach[Type] = true;
            Main.tileSolidTop[Type] = true;
            TileObjectData.newTile.StyleHorizontal = true;
            Main.tileTable[Type] = true;
            HitSound = SoundID.Item52;
            DustType = -1;

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Transmutator");
            AddMapEntry(new Color(81, 85, 97), name);
            TileObjectData.newTile.AnchorBottom = new AnchorData(Terraria.Enums.AnchorType.AlternateTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorAlternateTiles = new int[1] { ModContent.TileType<Projector>() };

            TileObjectData.newTile.AnchorValidTiles = new int[] {
                ModContent.TileType<Projector>()
            };

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<TransmutatorTileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }

        public override void RandomUpdate(int i, int j)
        {
            base.RandomUpdate(i, j);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out TransmutatorTileEntity entity))
            {
                Tile tile = Main.tile[i, j];
                if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
                {
                    if (entity.activeBuff > 0 && entity.activeBuffTime > 0 && entity.projector != null && entity.projector.lensID == ProjectorLensID.Pathos)
                    {
                        Color color = PotionColors.ScarletPotions.Contains(entity.activeBuff) ? CommonColors.ScarletColor : PotionColors.CeruleanPotions.Contains(entity.activeBuff) ? CommonColors.CeruleanColor : PotionColors.VerdantPotions.Contains(entity.activeBuff) ? CommonColors.VerdantColor : PotionColors.MauvePotions.Contains(entity.activeBuff) ? CommonColors.MauveColor : Color.White;
                        string texString = PotionColors.ScarletPotions.Contains(entity.activeBuff) ? "Scarlet" : PotionColors.CeruleanPotions.Contains(entity.activeBuff) ? "Cerulean" : PotionColors.VerdantPotions.Contains(entity.activeBuff) ? "Verdant" : PotionColors.MauvePotions.Contains(entity.activeBuff) ? "Mauve" : string.Empty;
                        if (texString != string.Empty)
                        {
                            Vector2 pos = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + new Vector2(16, 16) + RadianceUtils.tileDrawingZero;
                            Texture2D texture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/" + texString + "Icon").Value;
                            RadianceDrawing.DrawSoftGlow(pos + Main.screenPosition, new Color(color.R, color.G, color.B, (byte)(15 + 10 * RadianceUtils.SineTiming(20))), 1.5f, RadianceDrawing.DrawingMode.Tile);

                            Main.spriteBatch.End();
                            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, default, default, default, null, Matrix.Identity);

                            Main.spriteBatch.Draw(texture, pos, null, new Color(color.R, color.G, color.B, (byte)(50 + 20 * RadianceUtils.SineTiming(20))), 0, texture.Size() / 2, 1.5f + 0.05f * RadianceUtils.SineTiming(20), SpriteEffects.None, 0);

                            Main.spriteBatch.End();
                            Main.spriteBatch.Begin(default, BlendState.AlphaBlend, default, default, default, null, Matrix.Identity);
                        }
                    }
                    Texture2D baseTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/Transmutator/TransmutatorBase").Value;
                    Texture2D glowTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/Transmutator/TransmutatorGlow").Value;
                    Color glowColor = Color.Lerp(new Color(255, 50, 50), new Color(0, 255, 255), entity.deployTimer / 35);
                    Color tileColor = Lighting.GetColor(i, j);
                    if (entity.projectorBeamTimer > 0)
                        glowColor = Color.Lerp(new Color(0, 255, 255), CommonColors.RadianceColor1, entity.projectorBeamTimer / 60);
                    Vector2 basePosition = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + RadianceUtils.tileDrawingZero;
                    //base
                    Main.spriteBatch.Draw
                    (
                        baseTexture,
                        basePosition,
                        null,
                        tileColor,
                        0,
                        Vector2.Zero,
                        1,
                        SpriteEffects.None,
                        0
                    );
                    //glow
                    Main.spriteBatch.Draw
                    (
                        glowTexture,
                        basePosition + new Vector2(12, 12),
                        null,
                        glowColor,
                        0,
                        Vector2.Zero,
                        1,
                        SpriteEffects.None,
                        0
                    );
                    if (entity.projectorBeamTimer > 0)
                        RadianceDrawing.DrawSoftGlow(RadianceUtils.GetMultitileWorldPosition(i, j) + RadianceUtils.tileDrawingZero + new Vector2(entity.Width, entity.Height) * 8, CommonColors.RadianceColor1 * (entity.projectorBeamTimer / 60), 0.5f * (entity.projectorBeamTimer / 60), RadianceDrawing.DrawingMode.Tile);

                    //if (deployTimer > 0)
                    //{
                    //    Vector2 pos = new Vector2(i * 16, j * 16) + zero + new Vector2(entity.Width / 2, 0.7f) * 16 + Vector2.UnitX * 8; //tile world coords + half entity width (center of multitiletile) + a bit of increase
                    //    float mult = (float)Math.Clamp(Math.Abs(RadianceUtils.SineTiming(120)), 0.85f, 1f); //color multiplier
                    //    for (int h = 0; h < 2; h++)
                    //        RadianceDrawing.DrawBeam(pos, new Vector2(pos.X, 0), h == 1 ? new Color(255, 255, 255, entity.beamTimer).ToVector4() * mult : new Color(0, 255, 255, entity.beamTimer).ToVector4() * mult, 0.2f, h == 1 ? 10 : 14, Matrix.Identity);
                    //    RadianceDrawing.DrawSoftGlow(pos - Vector2.UnitY * 2, new Color(0, 255, 255, entity.beamTimer) * mult, 0.25f, Matrix.Identity);
                    //}
                }
            }
            return false;
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            RadianceInterfacePlayer mp = player.GetModPlayer<RadianceInterfacePlayer>();
            if (RadianceUtils.TryGetTileEntityAs(i, j, out TransmutatorTileEntity entity))
            {
                player.noThrow = 2;
                player.cursorItemIconEnabled = true;
                player.cursorItemIconID = !entity.GetSlot(1).IsAir ? entity.GetSlot(1).type : !entity.GetSlot(0).IsAir ? entity.GetSlot(0).type : ModContent.ItemType<TransmutatorItem>();
                List<HoverUIElement> data = new List<HoverUIElement>()
                {
                    new TransmutatorUIElement(entity, false, new Vector2(-40, 0)),
                    new TransmutatorUIElement(entity, true, new Vector2(-40, 0))
                };

                if (entity.maxRadiance > 0)
                    data.Add(new RadianceBarUIElement(entity.currentRadiance, entity.maxRadiance, new Vector2(0, 40)));

                if (entity.projector.lensID == ProjectorLensID.Pathos)
                    data.Add(new CircleUIElement(600, Color.Red));
                if (entity.activeBuff > 0)
                    data.Add(new CircleUIElement(480, CommonColors.RadianceColor1));

                if (entity.activeBuff > 0 && entity.activeBuffTime > 0)
                {
                    //TimeSpan.MaxValue.TotalSeconds
                    TimeSpan time = TimeSpan.FromSeconds(entity.activeBuffTime / 60);
                    string str = entity.activeBuffTime < 216000 ? time.ToString(@"mm\:ss") : time.ToString(@"hh\:mm\:ss");
                    Color color = PotionColors.ScarletPotions.Contains(entity.activeBuff) ? CommonColors.ScarletColor : PotionColors.CeruleanPotions.Contains(entity.activeBuff) ? CommonColors.CeruleanColor : PotionColors.VerdantPotions.Contains(entity.activeBuff) ? CommonColors.VerdantColor : PotionColors.MauvePotions.Contains(entity.activeBuff) ? CommonColors.MauveColor : Color.White;
                    data.Add(new TextUIElement(string.Join(" ", Regex.Split(RadianceUtils.GetBuffName(entity.activeBuff), @"(?<!^)(?=[A-Z])")) + ": " + str, color, new Vector2(0, -40)));
                }
                mp.currentHoveredObjects.Add(new HoverUIData(entity, entity.TileEntityWorldCenter(), data.ToArray()));
            }
        }

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (RadianceUtils.TryGetTileEntityAs(i, j, out TransmutatorTileEntity entity) && Main.myPlayer == player.whoAmI && !player.ItemAnimationActive)
            {
                Item selItem = RadianceUtils.GetPlayerHeldItem();
                bool success = false;
                if (entity.GetSlot(1).IsAir || !selItem.IsAir)
                {
                    if (entity.GetSlot(0).type != selItem.type || entity.GetSlot(0).stack == entity.GetSlot(0).maxStack)
                        entity.DropItem(0, new Vector2(i * 16, j * 16), new EntitySource_TileInteraction(null, i, j));
                    entity.SafeInsertItemIntoSlot(0, ref selItem, out success);
                }
                else
                    entity.DropItem(1, new Vector2(i * 16, j * 16), new EntitySource_TileInteraction(null, i, j));
                if (success)
                    return true;
            }
            return false;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out TransmutatorTileEntity entity))
            {
                entity.DropAllItems(new Vector2(i * 16, j * 16), new EntitySource_TileBreak(i, j));
                Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 16, ModContent.ItemType<TransmutatorItem>());
                Point16 origin = RadianceUtils.GetTileOrigin(i, j);
                ModContent.GetInstance<TransmutatorTileEntity>().Kill(origin.X, origin.Y);
            }
        }
    }

    public class TransmutatorTileEntity : RadianceUtilizingTileEntity, IInventory
    {
        public TransmutatorTileEntity() : base(ModContent.TileType<Transmutator>(), 0, new(), new())
        {
        }

        public bool hasProjector = false;
        public ProjectorTileEntity projector;
        public float craftingTimer = 0;
        public float glowTime = 0;
        public float deployTimer = 0;
        public float projectorBeamTimer = 0;
        public bool isCrafting = false;
        public int activeBuff = 0;
        public int activeBuffTime = 0;

        public Item[] inventory { get; set; }
        public byte[] inputtableSlots => new byte[] { 0 };
        public byte[] outputtableSlots => new byte[] { 1 };

        public override void Update()
        {
            this.ConstructInventory(2);
            if (activeBuff > 0)
            {
                if (activeBuffTime > 0)
                {
                    activeBuffTime = (int)Math.Min(activeBuffTime, TimeSpan.MaxValue.TotalSeconds);
                    if (activeBuff > 0 && activeBuffTime > 0)
                    {
                        for (int d = 0; d < Main.maxPlayers; d++)
                        {
                            Player player = Main.player[d];
                            if (player.active && !player.ghost && player.Distance(Position.ToVector2() * 16) < 480)
                                player.AddBuff(activeBuff, 2);
                        }
                    }
                    activeBuffTime--;
                }
                else
                    activeBuff = 0;
            }
            hasProjector = Main.tile[Position.X, Position.Y + 2].TileType == ModContent.TileType<Projector>() && Main.tile[Position.X, Position.Y + 2].TileFrameX == 0;
            if (hasProjector)
            {
                if (deployTimer < 105)
                    deployTimer++;
                if (RadianceUtils.TryGetTileEntityAs(Position.X, Position.Y + 2, out ProjectorTileEntity entity))
                {
                    projector = entity;
                    if (!this.GetSlot(0).IsAir)
                    {
                        TransmutationRecipe activeRecipe = null;
                        for (int i = 0; i < numRecipes; i++)
                        {
                            if (transmutationRecipe[i] != null && transmutationRecipe[i].inputItem == this.GetSlot(0).type && UnlockSystem.UnlockMethods.GetValueOrDefault(transmutationRecipe[i].unlock) && transmutationRecipe[i].inputStack <= this.GetSlot(0).stack)
                            {
                                activeRecipe = transmutationRecipe[i];
                                break;
                            }
                        }
                        if (activeRecipe != null)
                        {
                            isCrafting = true;
                            bool flag = true;
                            if (activeRecipe.specialRequirements != null)
                            {
                                foreach (SpecialRequirements req in activeRecipe.specialRequirements)
                                {
                                    switch (req)
                                    {
                                        case SpecialRequirements.Test:
                                            flag = true;
                                            break;
                                    }
                                }
                            }
                            currentRadiance = projector.currentRadiance;
                            projector.maxRadiance = maxRadiance = activeRecipe.requiredRadiance;

                            if (activeRecipe != null && //has active recipe
                                (this.GetSlot(1).IsAir || activeRecipe.outputItem == this.GetSlot(1).type) && //output item is empty or same as recipe output
                                activeRecipe.outputStack <= this.GetSlot(1).maxStack - this.GetSlot(1).stack && //output item current stack is less than or equal to the recipe output stack
                                currentRadiance >= activeRecipe.requiredRadiance && //contains enough radiance to craft
                                projector.lensID != ProjectorLensID.None && //projector has lens in it
                                flag //special requirements are met
                                )
                            {
                                glowTime = Math.Min(glowTime + 2, 90);
                                craftingTimer++;
                                if (craftingTimer >= 120)
                                    Craft(activeRecipe);
                            }
                        }
                        else isCrafting = false;
                    }
                    else
                    {
                        isCrafting = false;
                        if (craftingTimer > 0)
                            craftingTimer--;
                        if (craftingTimer == 0)
                        {
                            currentRadiance = 0;
                            maxRadiance = 0;
                        }
                    }
                }
                else
                    projector = null;
            }
            else if (deployTimer > 0)
                deployTimer--;
            if (!hasProjector)
            {
                currentRadiance = maxRadiance = 0;
                craftingTimer = 0;
            }
            if (craftingTimer == 0 && glowTime > 0)
                glowTime -= Math.Clamp(glowTime, 0, 2);

            if (projectorBeamTimer > 0)
                projectorBeamTimer--;
        }

        public void Craft(TransmutationRecipe activeRecipe)
        {
            for (int i = 0; i < 70; i++)
            {
                Dust d = Dust.NewDustPerfect(RadianceUtils.GetMultitileWorldPosition(Position.X, Position.Y) + new Vector2(Width * 8, Height * 8), DustID.GoldFlame, Main.rand.NextVector2Circular(5, 5));
                d.noGravity = i % 7 != 0;
                d.scale = 1.2f;
                if (i % 2 == 0)
                {
                    Dust g = Dust.NewDustPerfect(RadianceUtils.GetMultitileWorldPosition(Position.X, Position.Y) + new Vector2(Width * 8, Height * 32) + Vector2.UnitX * Main.rand.NextFloat(-4, 4), DustID.GoldFlame);
                    g.noGravity = true;
                    g.scale = 1.2f;
                }
            }
            switch (activeRecipe.specialEffects)
            {
                case SpecialEffects.SummonRain:
                    for (int i = 0; i < 60; i++)
                    {
                        Dust d = Dust.NewDustPerfect(RadianceUtils.GetMultitileWorldPosition(Position.X, Position.Y) + new Vector2(Width * 8, Height * 8), 45, Main.rand.NextVector2Circular(5, 5), 255);
                        d.noGravity = true;
                        d.velocity *= 2;
                        d.fadeIn = 1.2f;
                    }
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Main.StartRain();

                    break;

                case SpecialEffects.RemoveRain:
                    for (int i = 0; i < 60; i++)
                    {
                        Dust d = Dust.NewDustPerfect(RadianceUtils.GetMultitileWorldPosition(Position.X, Position.Y) + new Vector2(Width * 8, Height * 8), 242, Main.rand.NextVector2Circular(5, 5));
                        d.noGravity = true;
                        d.velocity *= 2;
                        d.scale = 1.2f;
                    }
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Main.StopRain();

                    break;

                case SpecialEffects.PotionDisperse:
                    Item item = RadianceUtils.GetItem(activeRecipe.inputItem);
                    if (activeBuff == item.buffType)
                        activeBuffTime += item.buffTime * 4;
                    else
                        activeBuffTime = item.buffTime * 4;

                    activeBuff = item.buffType;
                    break;
            }
            if (activeRecipe.specialEffects != SpecialEffects.PotionDisperse)
                activeBuff = activeBuffTime = 0;

            this.GetSlot(0).stack -= activeRecipe.inputStack;
            if (this.GetSlot(0).stack <= 0)
                this.GetSlot(0).TurnToAir();

            if (this.GetSlot(1).IsAir)
                this.SetItemInSlot(1, new Item(activeRecipe.outputItem, activeRecipe.outputStack));
            else
                this.GetSlot(1).stack += activeRecipe.outputStack;

            if (this.GetSlot(1).ModItem is IOnTransmutateEffect transmutated)
                transmutated.OnTransmutate();

            projector.currentRadiance = currentRadiance = 0;
            projector.maxRadiance = maxRadiance = 0;
            craftingTimer = 0;
            projectorBeamTimer = 60;
            SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/ProjectorFire"), new Vector2(Position.X * 16 + Width * 8, Position.Y * 16 + -Height * 8));

            activeRecipe = null;
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, Width, Height);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j - 1, Type);
            }
            int placedEntity = Place(i, j - 1);
            return placedEntity;
        }

        public override void SaveData(TagCompound tag)
        {
            if (activeBuff > 0)
                tag["BuffType"] = activeBuff;
            if (activeBuffTime > 0)
                tag["BuffTime"] = activeBuffTime;
            this.SaveInventory(ref tag);
        }

        public override void LoadData(TagCompound tag)
        {
            activeBuff = tag.Get<int>("BuffType");
            activeBuffTime = tag.Get<int>("BuffTime");
            this.LoadInventory(ref tag, 2);
        }
    }

    public class TransmutatorUIElement : HoverUIElement
    {
        public TransmutatorTileEntity entity;
        public bool output = false;

        public TransmutatorUIElement(TransmutatorTileEntity entity, bool output, Vector2 targetPosition)
        {
            this.entity = entity;
            this.output = output;
            this.targetPosition = output ? -targetPosition : targetPosition;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            RadianceDrawing.DrawSoftGlow(elementPosition, (output ? Color.Red : Color.Blue) * timerModifier, Math.Max(0.4f * (float)Math.Abs(RadianceUtils.SineTiming(100)), 0.35f), RadianceDrawing.DrawingMode.Default);
            RadianceDrawing.DrawSoftGlow(elementPosition, Color.White * timerModifier, Math.Max(0.2f * (float)Math.Abs(RadianceUtils.SineTiming(100)), 0.27f), RadianceDrawing.DrawingMode.Default);
            RadianceDrawing.DrawHoverableItem(Main.spriteBatch, output ? entity.GetSlot(1).type : entity.GetSlot(0).type, realDrawPosition, output ? entity.GetSlot(1).stack : entity.GetSlot(0).stack, Color.White * timerModifier);
        }
    }

    public class TransmutatorItem : BaseTileItem
    {
        public TransmutatorItem() : base("TransmutatorItem", "Radiance Transmutator", "Uses concentrated Radiance to convert items into other objects\nCan only be placed above a Radiance Projector", "Transmutator", 1, Item.sellPrice(0, 0, 10, 0), ItemRarityID.Green)
        {
        }
    }
}