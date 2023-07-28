﻿using Microsoft.Win32.SafeHandles;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Content.Particles;
using Radiance.Core.Systems;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Terraria.Localization;
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
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.CoordinateHeights = new int[3] { 16, 16, 16 };
            Main.tileNoAttach[Type] = true;
            Main.tileSolidTop[Type] = true;
            Main.tileTable[Type] = true;
            HitSound = SoundID.Item52;
            DustType = -1;

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Radiance Transmutator");
            AddMapEntry(new Color(255, 197, 97), name);

            TileObjectData.newTile.AnchorBottom = new AnchorData(Terraria.Enums.AnchorType.AlternateTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorAlternateTiles = new int[1] { ModContent.TileType<Projector>() };

            TileObjectData.newTile.AnchorValidTiles = new int[] {
                ModContent.TileType<Projector>()
            };

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<TransmutatorTileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (TryGetTileEntityAs(i, j, out TransmutatorTileEntity entity))
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
                            Vector2 pos = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + new Vector2(16, 16) + tileDrawingZero;
                            Texture2D texture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/" + texString + "Icon").Value;

                            RadianceDrawing.DrawSoftGlow(pos + Main.screenPosition, new Color(color.R, color.G, color.B, (byte)(15 + 10 * SineTiming(20))), 1.5f);
                            Main.spriteBatch.Draw(texture, pos, null, new Color(color.R, color.G, color.B, (byte)(50 + 20 * SineTiming(20))), 0, texture.Size() / 2, 1.5f + 0.05f * SineTiming(20), SpriteEffects.None, 0);
                        }
                    }
                    Texture2D baseTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/Transmutator/TransmutatorBase").Value;
                    Texture2D glowTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/Transmutator/TransmutatorGlow").Value;

                    Color tileColor = Lighting.GetColor(i, j);
                    Vector2 basePosition = entity.TileEntityWorldCenter() - Main.screenPosition + tileDrawingZero;
                    Main.spriteBatch.Draw(baseTexture, basePosition, null, tileColor, 0, baseTexture.Size() / 2, 1, SpriteEffects.None, 0);

                    if (entity.projectorBeamTimer > 0)
                    {
                        Color glowColor = Color.White * EaseInOutExponent(entity.projectorBeamTimer / 60, 4);
                        Main.spriteBatch.Draw(glowTexture, basePosition, null, glowColor, 0, baseTexture.Size() / 2, 1, SpriteEffects.None, 0);
                    }

                    if (entity.projectorBeamTimer > 0)
                        RadianceDrawing.DrawSoftGlow(basePosition - Vector2.UnitY * 6 + Main.screenPosition, CommonColors.RadianceColor1 * (entity.projectorBeamTimer / 60), 0.5f * (entity.projectorBeamTimer / 60));
                }
            }
            return false;
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            RadianceInterfacePlayer mp = player.GetModPlayer<RadianceInterfacePlayer>();
            if (TryGetTileEntityAs(i, j, out TransmutatorTileEntity entity))
            {
                if (entity.inventory != null)
                {
                    player.noThrow = 2;
                    player.cursorItemIconEnabled = true;
                    player.cursorItemIconID = !entity.GetSlot(1).IsAir ? entity.GetSlot(1).type : !entity.GetSlot(0).IsAir ? entity.GetSlot(0).type : ModContent.ItemType<TransmutatorItem>();
                }
                entity.AddHoverUI();
            }
        }
        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (TryGetTileEntityAs(i, j, out TransmutatorTileEntity entity) && !player.ItemAnimationActive)
            {
                Item selItem = GetPlayerHeldItem();
                bool success = false;
                if (entity.GetSlot(1).IsAir || !selItem.IsAir)
                {
                    if (entity.GetSlot(0).type != selItem.type || entity.GetSlot(0).stack == entity.GetSlot(0).maxStack)
                        entity.DropItem(0, new Vector2(i * 16, j * 16));
                    entity.SafeInsertItemIntoSlot(0, ref selItem, out success);
                    if (success)
                        SoundEngine.PlaySound(SoundID.MenuTick);
                }
                else
                    entity.DropItem(1, new Vector2(i * 16, j * 16));

                if (success)
                    return true;
            }
            return false;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (TryGetTileEntityAs(i, j, out TransmutatorTileEntity entity))
            {
                entity.DropAllItems(new Vector2(i * 16, j * 16));
                Point origin = GetTileOrigin(i, j);
                ModContent.GetInstance<TransmutatorTileEntity>().Kill(origin.X, origin.Y);
            }
        }
    }

    public class TransmutatorTileEntity : RadianceUtilizingTileEntity, IInventory
    {
        public TransmutatorTileEntity() : base(ModContent.TileType<Transmutator>(), 0, new(), new())
        {
        }

        public bool HasProjector => projector != null;
        public ProjectorTileEntity projector;
        public float craftingTimer = 0;
        public float glowTime = 0;
        public float projectorBeamTimer = 0;
        public bool isCrafting = false;
        public int activeBuff = 0;
        public int activeBuffTime = 0;

        public Item[] inventory { get; set; }
        public byte[] inputtableSlots => new byte[] { 0 };
        public byte[] outputtableSlots => new byte[] { 1 };
        protected override HoverUIData ManageHoverUI()
        {
            List<HoverUIElement> data = new List<HoverUIElement>();
            if (inventory != null)
            {
                data.Add(new TransmutatorUIElement("Input", false, new Vector2(-40, 0)));
                data.Add(new TransmutatorUIElement("Output", true, new Vector2(-40, 0)));
            }
            if (maxRadiance > 0)
                data.Add(new RadianceBarUIElement("RadianceBar", currentRadiance, maxRadiance, new Vector2(0, 40)));

            if (projector != null)
            {
                if (projector.lensID == ProjectorLensID.Pathos)
                    data.Add(new CircleUIElement("PathosAoECircle", 600, Color.Red));
            }
            if (activeBuff > 0)
                data.Add(new CircleUIElement("BuffAoECircle", 480, CommonColors.RadianceColor1));

            if (activeBuff > 0 && activeBuffTime > 0)
            {
                //TimeSpan.MaxValue.TotalSeconds
                TimeSpan time = TimeSpan.FromSeconds(activeBuffTime / 60);
                string str = activeBuffTime < 216000 ? time.ToString(@"mm\:ss") : time.ToString(@"hh\:mm\:ss");
                Color color = PotionColors.ScarletPotions.Contains(activeBuff) ? CommonColors.ScarletColor : PotionColors.CeruleanPotions.Contains(activeBuff) ? CommonColors.CeruleanColor : PotionColors.VerdantPotions.Contains(activeBuff) ? CommonColors.VerdantColor : PotionColors.MauvePotions.Contains(activeBuff) ? CommonColors.MauveColor : Color.White;
                data.Add(new TextUIElement("PotionTime", string.Join(" ", Regex.Split(GetBuffName(activeBuff), @"(?<!^)(?=[A-Z])")) + ": " + str, color, new Vector2(0, -40)));
            }
            return new HoverUIData(this, this.TileEntityWorldCenter(), data.ToArray());
        }
        public override void OrderedUpdate()
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
            if (Main.tile[Position.X, Position.Y + 3].TileType == ModContent.TileType<Projector>() && Main.tile[Position.X, Position.Y + 2].TileFrameX == 0)
            {
                if (TryGetTileEntityAs(Position.X, Position.Y + 3, out ProjectorTileEntity entity))
                    projector = entity;
            }
            else
                projector = null;

            if (HasProjector)
            {
                if (!this.GetSlot(0).IsAir)
                {
                    TransmutationRecipe activeRecipe = null;
                    foreach (TransmutationRecipe recipe in transmutationRecipes)
                    {
                        if (recipe.inputItems.Contains(this.GetSlot(0).type) && UnlockSystem.UnlockMethods.GetValueOrDefault(recipe.unlock) && this.GetSlot(0).stack >= recipe.inputStack)
                        {
                            activeRecipe = recipe;
                            break;
                        }
                    }
                    if (activeRecipe != null)
                    {
                        isCrafting = true;
                        bool flag = true;
                        if (activeRecipe.specialRequirements.Length > 0)
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
                        maxRadiance = activeRecipe.requiredRadiance;

                        if ((this.GetSlot(1).IsAir || activeRecipe.outputItem == this.GetSlot(1).type) && //output item is empty or same as recipe output
                            activeRecipe.outputStack <= this.GetSlot(1).maxStack - this.GetSlot(1).stack && //output item current stack is less than or equal to the recipe output stack
                            currentRadiance >= activeRecipe.requiredRadiance && //contains enough radiance to craft
                            projector.lensID != ProjectorLensID.None && //projector has lens in it
                            flag //special requirements are met
                            )
                        {
                            glowTime = Math.Min(glowTime + 2, 90);
                            craftingTimer++;
                            if (craftingTimer >= 120)
                            {
                                Craft(activeRecipe);
                                activeRecipe = null;
                            }
                        }
                    }
                    else
                    {
                        currentRadiance = maxRadiance = 0;
                        isCrafting = false;
                    }
                }
                else
                {
                    isCrafting = false;
                    if (craftingTimer > 0)
                        craftingTimer--;
                    else
                    {
                        currentRadiance = 0;
                        maxRadiance = 0;
                    }
                }
            }
            else
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
            ParticleSystem.AddParticle(new StarFlare(this.TileEntityWorldCenter() - Vector2.UnitY * 4, 12, 50, new Color(255, 220, 138), new Color(255, 220, 138), 0.125f));
            SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/ProjectorFire"), new Vector2(Position.X * 16 + Width * 8, Position.Y * 16 + -Height * 8));

            switch (activeRecipe.specialEffects)
            {
                case SpecialEffects.SummonRain:
                    for (int i = 0; i < 60; i++)
                    {
                        Dust d = Dust.NewDustPerfect(MultitileOriginWorldPosition(Position.X, Position.Y) + new Vector2(Width * 8, Height * 8), 45, Main.rand.NextVector2Circular(5, 5), 255);
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
                        Dust d = Dust.NewDustPerfect(MultitileOriginWorldPosition(Position.X, Position.Y) + new Vector2(Width * 8, Height * 8), 242, Main.rand.NextVector2Circular(5, 5));
                        d.noGravity = true;
                        d.velocity *= 2;
                        d.scale = 1.2f;
                    }
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Main.StopRain();

                    break;

                case SpecialEffects.PotionDisperse:
                    Item item = GetItem((int)activeRecipe.specialEffectValue);
                    if (activeBuff == item.buffType)
                        activeBuffTime += item.buffTime * 4;
                    else
                        activeBuffTime = item.buffTime * 4;

                    activeBuff = item.buffType;
                    break;
            }

            if (activeRecipe.specialEffects == SpecialEffects.MoveToOutput)
            {
                Item item = this.GetSlot(0).Clone();
                this.SetItemInSlot(1, item);
                this.GetSlot(0).TurnToAir();
            }
            else
            {
                this.GetSlot(0).stack -= activeRecipe.inputStack;
                if (this.GetSlot(0).stack <= 0)
                    this.GetSlot(0).TurnToAir();

                if (this.GetSlot(1).IsAir)
                    this.SetItemInSlot(1, new Item(activeRecipe.outputItem, activeRecipe.outputStack));
                else
                    this.GetSlot(1).stack += activeRecipe.outputStack;
            }
            if (this.GetSlot(1).ModItem is IOnTransmutateEffect transmutated)
                transmutated.OnTransmutate();

            craftingTimer = 0;
            projectorBeamTimer = 60;
            projector.ContainerPlaced.currentRadiance -= activeRecipe.requiredRadiance;
        }

        public override void SaveExtraData(TagCompound tag)
        {
            tag["BuffType"] = activeBuff;
            tag["BuffTime"] = activeBuffTime;
            this.SaveInventory(tag);
        }

        public override void LoadExtraData(TagCompound tag)
        {
            activeBuff = tag.Get<int>("BuffType");
            activeBuffTime = tag.Get<int>("BuffTime");
            this.LoadInventory(tag, 2);
        }
    }

    public class TransmutatorUIElement : HoverUIElement
    {
        public bool output = false;

        public TransmutatorUIElement(string name, bool output, Vector2 targetPosition) : base(name)
        {
            this.output = output;
            this.targetPosition = output ? -targetPosition : targetPosition;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            TransmutatorTileEntity entity = parent.entity as TransmutatorTileEntity;
            if (entity != null)
            {
                RadianceDrawing.DrawSoftGlow(elementPosition, (output ? Color.Red : Color.Blue) * timerModifier, Math.Max(0.4f * (float)Math.Abs(SineTiming(100)), 0.35f));
                RadianceDrawing.DrawSoftGlow(elementPosition, Color.White * timerModifier, Math.Max(0.2f * (float)Math.Abs(SineTiming(100)), 0.27f));

                RadianceDrawing.DrawHoverableItem(Main.spriteBatch, output ? entity.GetSlot(1).type : entity.GetSlot(0).type, realDrawPosition, output ? entity.GetSlot(1).stack : entity.GetSlot(0).stack, Color.White * timerModifier);
            }
        }
    }

    public class AssemblableTransmutator : ModTile
    {
        public override string Texture => "Radiance/Content/Tiles/Transmutator/Transmutator";

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.CoordinateHeights = new int[3] { 16, 16, 16 };
            HitSound = SoundID.Item52;
            DustType = -1;

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Radiance Transmutator");
            AddMapEntry(new Color(255, 197, 97), name);

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<AssemblableTransmutatorTileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (TryGetTileEntityAs(i, j, out AssemblableTransmutatorTileEntity entity))
                entity.Draw(spriteBatch, entity.CurrentStage);

            return false;
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            RadianceInterfacePlayer mp = player.GetModPlayer<RadianceInterfacePlayer>();
            if (TryGetTileEntityAs(i, j, out AssemblableTransmutatorTileEntity entity))
                entity.DrawHoverUI();
        }

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (TryGetTileEntityAs(i, j, out AssemblableTransmutatorTileEntity entity) && !player.ItemAnimationActive)
                entity.ConsumeMaterials(player);

            return false;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (TryGetTileEntityAs(i, j, out AssemblableTransmutatorTileEntity entity))
            {
                entity.DropUsedItems();
                Point origin = GetTileOrigin(i, j);
                ModContent.GetInstance<AssemblableTransmutatorTileEntity>().Kill(origin.X, origin.Y);
            }
        }
    }

    public class AssemblableTransmutatorTileEntity : AssemblableTileEntity
    {
        public AssemblableTransmutatorTileEntity() : base(
            ModContent.TileType<AssemblableTransmutator>(),
            ModContent.TileType<Transmutator>(),
            ModContent.GetInstance<TransmutatorTileEntity>(),
            5,
            ModContent.Request<Texture2D>("Radiance/Content/Tiles/Transmutator/AssemblableTransmutator").Value,
            new List<(int, int)>()
            {
                (22, 8),
                (22, 4),
                (21, 8),
                (ModContent.ItemType<ShimmeringGlass>(), 6 ),
            }
            )
        { }

        public override void OnStageIncrease(int stage)
        {
            if (stage == 1)
            {
                for (int i = 0; i < 30; i++)
                {
                    Dust dust = Main.dust[Dust.NewDust(Position.ToVector2() * 16 - Vector2.One * 2 + Vector2.UnitY * 16, Width * 16, (Height - 1) * 16, DustID.Smoke)];
                    dust.velocity *= 0.1f;
                    dust.scale = 1.5f;
                    dust.fadeIn = 1.2f;
                }
                SoundEngine.PlaySound(SoundID.Item52, this.TileEntityWorldCenter());
            }
            else if (stage < StageCount - 1)
            {
                for (int i = 0; i < 30; i++)
                {
                    Dust dust = Main.dust[Dust.NewDust(Position.ToVector2() * 16 - Vector2.One * 2, Width * 16, Height * 16, DustID.Smoke)];
                    dust.velocity *= 0.1f;
                    dust.scale = 1.5f;
                    dust.fadeIn = 1.2f;
                }
                SoundEngine.PlaySound(SoundID.Item52, this.TileEntityWorldCenter());
            }
            else
            {
                for (int i = 0; i < 40; i++)
                {
                    Dust dust = Main.dust[Dust.NewDust(Position.ToVector2() * 16 - Vector2.One * 2, Width * 16, Height * 16, DustID.Smoke)];
                    dust.velocity *= 0.4f;
                    dust.scale = 1.5f;
                    dust.fadeIn = 1.2f;
                }
                SoundEngine.PlaySound(SoundID.Item37, this.TileEntityWorldCenter());
            }
        }
    }

    public class TransmutatorItem : BaseTileItem
    {
        public TransmutatorItem() : base("TransmutatorItem", "Radiance Transmutator", "Uses concentrated Radiance to convert items into other objects\nCan only be placed above a Radiance Projector", "Transmutator", 1, Item.sellPrice(0, 0, 10, 0), ItemRarityID.Green) { }
    }

    public class TransmutatorBlueprint : BaseTileItem
    {
        public override string Texture => "Radiance/Content/ExtraTextures/Blueprint";
        public TransmutatorBlueprint() : base("TransmutatorBlueprint", "Mysterious Blueprint", "Begins the assembly of an arcane machine", "AssemblableTransmutator", 1, Item.sellPrice(0, 0, 5, 0), ItemRarityID.Blue) { }
    }
}