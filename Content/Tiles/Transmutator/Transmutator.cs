﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Core;
using Radiance.Content.Items.TileItems;
using Radiance.Utilities;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using static Radiance.Core.Systems.TransmutationRecipeSystem;
using Radiance.Core.Systems;
using System.Text.RegularExpressions;

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
                Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
                if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
                {
                    if (entity.activeBuff > 0 && entity.activeBuffTime > 0 && entity.projector != null && entity.projector.containedLens == ProjectorTileEntity.LensEnum.Pathos)
                    {
                        Color color = PotionColors.ScarletPotions.Contains(entity.activeBuff) ? CommonColors.ScarletColor : PotionColors.CeruleanPotions.Contains(entity.activeBuff) ? CommonColors.CeruleanColor : PotionColors.VerdantPotions.Contains(entity.activeBuff) ? CommonColors.VerdantColor : PotionColors.MauvePotions.Contains(entity.activeBuff) ? CommonColors.MauveColor : Color.White;
                        string texString = PotionColors.ScarletPotions.Contains(entity.activeBuff) ? "Scarlet" : PotionColors.CeruleanPotions.Contains(entity.activeBuff) ? "Cerulean" : PotionColors.VerdantPotions.Contains(entity.activeBuff) ? "Verdant" : PotionColors.MauvePotions.Contains(entity.activeBuff) ? "Mauve" : string.Empty;
                        if (texString != string.Empty)
                        {
                            Vector2 pos = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + new Vector2(16, 16) + zero;
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
                    if (entity.projectorBeamTimer > 0) glowColor = Color.Lerp(new Color(0, 255, 255), CommonColors.RadianceColor1, entity.projectorBeamTimer / 60);
                    Vector2 basePosition = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero;
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
                        RadianceDrawing.DrawSoftGlow(RadianceUtils.MultitileCenterWorldCoords(i, j) + zero + new Vector2(entity.Width, entity.Height) * 8, CommonColors.RadianceColor1 * (entity.projectorBeamTimer / 60), 0.5f * (entity.projectorBeamTimer / 60), RadianceDrawing.DrawingMode.Tile);

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

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            RadianceInterfacePlayer mp = player.GetModPlayer<RadianceInterfacePlayer>();
            if (RadianceUtils.TryGetTileEntityAs(i, j, out TransmutatorTileEntity entity))
            {
                int f = ModContent.ItemType<TransmutatorItem>();
                if (entity.inputItem.type != ItemID.None)
                    f = entity.inputItem.type;
                if (entity.outputItem.type != ItemID.None)
                    f = entity.outputItem.type;
                player.noThrow = 2;
                player.cursorItemIconEnabled = true;
                player.cursorItemIconID = f;
                if(entity.hasProjector)
                    mp.transmutatorIOCoords = new Vector2(i, j);
                if (entity.MaxRadiance > 0)
                    mp.radianceContainingTileHoverOverCoords = new Vector2(i, j);
                if(entity.projector.containedLens == ProjectorTileEntity.LensEnum.Pathos)
                {
                    mp.aoeCirclePosition = RadianceUtils.MultitileCenterWorldCoords(i, j) + new Vector2(16, 16); 
                    mp.aoeCircleColor = new Color(255, 0, 0, 0).ToVector4();
                    mp.aoeCircleScale = 600;
                    mp.aoeCircleMatrix = Main.GameViewMatrix.ZoomMatrix;
                }
                if(entity.activeBuff > 0 && entity.activeBuffTime > 0)
                {
                    //TimeSpan.MaxValue.TotalSeconds
                    TimeSpan time = TimeSpan.FromSeconds(entity.activeBuffTime / 60);
                    string str = entity.activeBuffTime < 216000 ? time.ToString(@"mm\:ss") : time.ToString(@"hh\:mm\:ss");
                    mp.hoveringOverSpecialTextTileCoords = new Vector2(i, j);
                    mp.hoveringOverSpecialTextTileColor = PotionColors.ScarletPotions.Contains(entity.activeBuff) ? CommonColors.ScarletColor : PotionColors.CeruleanPotions.Contains(entity.activeBuff) ? CommonColors.CeruleanColor : PotionColors.VerdantPotions.Contains(entity.activeBuff) ? CommonColors.VerdantColor : PotionColors.MauvePotions.Contains(entity.activeBuff) ? CommonColors.MauveColor : Color.White;
                    mp.hoveringOverSpecialTextTileString = string.Join(" ", Regex.Split(RadianceUtils.GetBuffName(entity.activeBuff), @"(?<!^)(?=[A-Z])")) + ": " + str;
                    
                }
            }
        }

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (RadianceUtils.TryGetTileEntityAs(i, j, out TransmutatorTileEntity entity))
            {
                if (!player.ItemAnimationActive)
                {
                    Item selItem = RadianceUtils.GetPlayerHeldItem();
                    if (selItem.type == ItemID.None)
                    {
                        if (entity.outputItem.type != ItemID.None)
                        {
                            DropItem(i, j, entity, entity.outputItem);
                            entity.outputItem.TurnToAir();
                            SoundEngine.PlaySound(SoundID.MenuTick);
                        }
                        else if (entity.inputItem.type != ItemID.None)
                        {
                            DropItem(i, j, entity, entity.inputItem);
                            entity.inputItem.TurnToAir();
                            SoundEngine.PlaySound(SoundID.MenuTick);
                        }
                    }
                    else
                    {
                        if (entity.inputItem.type == selItem.type && entity.inputItem.stack != entity.inputItem.maxStack) //selected item is same as input item and input stack isnt full
                        {
                            if (entity.inputItem.maxStack - entity.inputItem.stack > selItem.stack) //input item free space is more than selected item stack
                            {
                                entity.inputItem.stack += Math.Min(selItem.stack, entity.inputItem.maxStack - entity.inputItem.stack);
                                selItem.stack -= Math.Min(selItem.stack, entity.inputItem.maxStack - entity.inputItem.stack);
                                if (selItem.stack == 0)
                                    selItem.TurnToAir();
                                SoundEngine.PlaySound(SoundID.MenuTick);
                            }
                            else //only runs if input item stack would be full on use
                            {
                                selItem.stack -= entity.inputItem.maxStack - entity.inputItem.stack;
                                if (selItem.stack == 0)
                                    selItem.TurnToAir();
                                entity.inputItem.stack = entity.inputItem.maxStack;
                                SoundEngine.PlaySound(SoundID.MenuTick);
                            }
                        }
                        else
                        {
                            if (entity.inputItem.type != ItemID.None)
                                DropItem(i, j, entity, entity.inputItem);
                            entity.inputItem = selItem.Clone();
                            SoundEngine.PlaySound(SoundID.MenuTick);
                            selItem.TurnToAir();
                        }
                    }
                }
            }
            return false;
        }
        public static void DropItem(int i, int j, TransmutatorTileEntity entity, Item heldItem)
        {
            int num = Item.NewItem(new EntitySource_TileEntity(entity), i * 16, j * 16, 1, 1, heldItem.type, 1, false, 0, false, false);
            Item item = Main.item[num];

            item.netDefaults(heldItem.netID);
            item.velocity.Y = Main.rand.NextFloat(-4, -2);
            item.velocity.X = Main.rand.NextFloat(-2, 2);
            item.newAndShiny = false;
            item.stack = heldItem.stack;

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, num, 0f, 0f, 0f, 0, 0, 0);
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out TransmutatorTileEntity entity))
            {
                if (entity.inputItem.type != ItemID.None)
                    DropItem(i, j, entity, entity.inputItem);
                if (entity.outputItem.type != ItemID.None)
                    DropItem(i, j, entity, entity.outputItem);

                Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 16, ModContent.ItemType<TransmutatorItem>());
                Point16 origin = RadianceUtils.GetTileOrigin(i, j);
                ModContent.GetInstance<TransmutatorTileEntity>().Kill(origin.X, origin.Y);
            }
        }
    }

    public class TransmutatorTileEntity : RadianceUtilizingTileEntity
    {
        #region Fields

        private float maxRadiance = 0;
        private float currentRadiance = 0;
        private int width = 2;
        private int height = 2;
        private List<int> inputTiles = new() { };
        private List<int> outputTiles = new() { };
        private int parentTile = ModContent.TileType<Transmutator>();
        public Item inputItem = new(0, 1);
        public Item outputItem = new(0, 1);
        public bool hasProjector = false;
        public ProjectorTileEntity projector;
        public float craftingTimer = 0;
        public float glowTime = 0;
        public float deployTimer = 0;
        public float projectorBeamTimer = 0;
        public bool isCrafting = false;
        public int activeBuff = 0;
        public int activeBuffTime = 0;

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

        public override void Update()
        {
            if(activeBuff > 0)
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
            if(hasProjector)
            {
                if(deployTimer < 105)
                    deployTimer++;
                if (RadianceUtils.TryGetTileEntityAs(Position.X, Position.Y + 2, out ProjectorTileEntity entity))
                {
                    projector = entity;
                    if (inputItem.type != ItemID.None)
                    {
                        TransmutationRecipe activeRecipe = null;
                        for (int i = 0; i < numRecipes; i++)
                        {
                            if (transmutationRecipe[i] != null && transmutationRecipe[i].inputItem == inputItem.type && UnlockSystem.UnlockMethods.GetValueOrDefault(transmutationRecipe[i].unlock) && transmutationRecipe[i].inputStack <= inputItem.stack)
                            {
                                activeRecipe = transmutationRecipe[i];
                                break;
                            }
                            else if (activeRecipe != null)
                                activeRecipe = null;
                        }

                        if (activeRecipe != null)
                        {
                            isCrafting = true;
                            MaxRadiance = activeRecipe.requiredRadiance;
                            bool flag = false;
                            switch (activeRecipe.specialRequirements)
                            {
                                case SpecialRequirements.None:
                                    flag = true;
                                    break;
                            }
                            CurrentRadiance = projector.CurrentRadiance;
                            projector.MaxRadiance = MaxRadiance;

                            if (activeRecipe != null && //has active recipe
                                (outputItem.type == ItemID.None || activeRecipe.outputItem == outputItem.type) && //output item is empty or same as recipe output  
                                activeRecipe.outputStack <= outputItem.maxStack - outputItem.stack && //output item current stack is less than or equal to the recipe output stack
                                currentRadiance >= activeRecipe.requiredRadiance && //contains enough radiance to craft
                                projector.containedLens != ProjectorTileEntity.LensEnum.None && //projector has lens in it
                                flag //special requirement is met
                                )
                            {
                                glowTime = Math.Min(glowTime + 2, 90);
                                craftingTimer++;
                            }
                            if (craftingTimer >= 120)
                                Craft(activeRecipe);
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
                            CurrentRadiance = 0;
                            MaxRadiance = 0;
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
                CurrentRadiance = MaxRadiance = 0;
                craftingTimer = 0;
            }
            if(craftingTimer == 0 && glowTime > 0) 
                glowTime -= Math.Clamp(glowTime, 0, 2);
            if (projectorBeamTimer > 0)
                projectorBeamTimer--;

            
        }
        public void Craft(TransmutationRecipe activeRecipe)
        {
            for (int i = 0; i < 70; i++)
            {
                Dust d = Dust.NewDustPerfect(RadianceUtils.MultitileCenterWorldCoords(Position.X, Position.Y) + new Vector2(Width * 8, Height * 8), DustID.GoldFlame, Main.rand.NextVector2Circular(5, 5));
                d.noGravity = i % 7 != 0;
                d.scale = 1.2f;
                if (i % 2 == 0)
                {
                    Dust g = Dust.NewDustPerfect(RadianceUtils.MultitileCenterWorldCoords(Position.X, Position.Y) + new Vector2(Width * 8, Height * 32) + Vector2.UnitX * Main.rand.NextFloat(-4, 4), DustID.GoldFlame);
                    g.noGravity = true;
                    g.scale = 1.2f;
                }
            }
            switch(activeRecipe.specialEffects)
            {
                case SpecialEffects.SummonRain:
                    for (int i = 0; i < 60; i++)
                    {
                        Dust d = Dust.NewDustPerfect(RadianceUtils.MultitileCenterWorldCoords(Position.X, Position.Y) + new Vector2(Width * 8, Height * 8), 45, Main.rand.NextVector2Circular(5, 5), 255);
                        d.noGravity = true;
                        d.velocity *= 2;
                        d.fadeIn = 1.2f;
                    }
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Main.StartRain();
                    break;

                case SpecialEffects.RemoveRain:
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < 60; i++)
                        {
                            Dust d = Dust.NewDustPerfect(RadianceUtils.MultitileCenterWorldCoords(Position.X, Position.Y) + new Vector2(Width * 8, Height * 8), 242, Main.rand.NextVector2Circular(5, 5));
                            d.noGravity = true;
                            d.velocity *= 2;
                            d.scale = 1.2f;
                        }
                        Main.StopRain();
                    }
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
            if(activeRecipe.specialEffects != SpecialEffects.PotionDisperse)
                activeBuff = activeBuffTime = 0;

            inputItem.stack -= activeRecipe.inputStack;
            if (inputItem.stack <= 0)
                inputItem.TurnToAir();
            if (outputItem.type == ItemID.None)
                outputItem = new Item(activeRecipe.outputItem, activeRecipe.outputStack);
            else
                outputItem.stack += activeRecipe.outputStack;

            projector.CurrentRadiance = CurrentRadiance = 0;
            projector.MaxRadiance = MaxRadiance = 0;
            craftingTimer = 0;
            projectorBeamTimer = 60;
            SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/ProjectorFire"), new Vector2(Position.X * 16 + Width * 8, Position.Y * 16 + -Height * 8));

            activeRecipe = null;
        }
        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, width, height);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);
            }
            int placedEntity = Place(i, j - 1);
            return placedEntity;
        }

        public override void SaveData(TagCompound tag)
        {
            if (inputItem.type != ItemID.None)
                tag["InputItem"] = inputItem;
            if (outputItem.type != ItemID.None)
                tag["OutputItem"] = outputItem;
            if (activeBuff > 0)
                tag["BuffType"] = activeBuff;
            if (activeBuffTime > 0)
                tag["BuffTime"] = activeBuffTime;
        }

        public override void LoadData(TagCompound tag)
        {
            inputItem = tag.Get<Item>("InputItem");
            outputItem = tag.Get<Item>("OutputItem");
            activeBuff = tag.Get<int>("BuffType");
            activeBuffTime = tag.Get<int>("BuffTime");
        }
    }
}