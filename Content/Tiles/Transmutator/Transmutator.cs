using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Content.Particles;
using Radiance.Core.Loaders;
using Radiance.Core.Systems;
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
                    if (entity.activeBuff > 0 && entity.activeBuffTime > 0 && entity.projector != null && RadianceSets.ProjectorLensID[entity.projector.LensPlaced.type] == (int)ProjectorLensID.Pathos)
                    {
                        Color color = PotionColors.ScarletPotions.Contains(entity.activeBuff) ? CommonColors.ScarletColor : PotionColors.CeruleanPotions.Contains(entity.activeBuff) ? CommonColors.CeruleanColor : PotionColors.VerdantPotions.Contains(entity.activeBuff) ? CommonColors.VerdantColor : PotionColors.MauvePotions.Contains(entity.activeBuff) ? CommonColors.MauveColor : Color.White;
                        string texString = PotionColors.ScarletPotions.Contains(entity.activeBuff) ? "Scarlet" : PotionColors.CeruleanPotions.Contains(entity.activeBuff) ? "Cerulean" : PotionColors.VerdantPotions.Contains(entity.activeBuff) ? "Verdant" : PotionColors.MauvePotions.Contains(entity.activeBuff) ? "Mauve" : string.Empty;
                        if (texString != string.Empty)
                        {
                            Vector2 pos = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + new Vector2(16, 16) + TileDrawingZero;
                            Texture2D texture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/" + texString + "Icon").Value;

                            RadianceDrawing.DrawSoftGlow(pos + Main.screenPosition, new Color(color.R, color.G, color.B, (byte)(15 + 10 * SineTiming(20))), 1.5f);
                            Main.spriteBatch.Draw(texture, pos, null, new Color(color.R, color.G, color.B, (byte)(50 + 20 * SineTiming(20))), 0, texture.Size() / 2, 1.5f + 0.05f * SineTiming(20), SpriteEffects.None, 0);
                        }
                    }
                    Texture2D baseTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/Transmutator/TransmutatorBase").Value;
                    Texture2D glowTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/Transmutator/TransmutatorGlow").Value;

                    Color tileColor = Lighting.GetColor(i, j);
                    Vector2 basePosition = entity.TileEntityWorldCenter() - Main.screenPosition + TileDrawingZero;
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
                bool dropSuccess = false;
                if (entity.GetSlot(1).IsAir || !selItem.IsAir)
                {
                    if (entity.GetSlot(0).type != selItem.type || entity.GetSlot(0).stack == entity.GetSlot(0).maxStack)
                        entity.DropItem(0, entity.TileEntityWorldCenter(), out dropSuccess);

                    entity.SafeInsertItemIntoSlot(0, selItem, out success, true, true);
                }
                else
                    entity.DropItem(1, entity.TileEntityWorldCenter(), out dropSuccess);

                success |= dropSuccess;
                if (success)
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    return true;
                }
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
        public TransmutatorTileEntity() : base(ModContent.TileType<Transmutator>(), 0, new(), new(), usesItemImprints: true)
        {
            inventorySize = 2;
            this.ConstructInventory();
        }

        public bool HasProjector => projector != null;
        public ProjectorTileEntity projector;
        public float craftingTimer = 0;
        public float glowTime = 0;
        public float projectorBeamTimer = 0;
        public bool isCrafting = false;
        public int activeBuff = 0;
        public int activeBuffTime = 0;
        public float radianceModifier = 1;
        public Item[] inventory { get; set; }
        public int inventorySize { get; set; }
        public byte[] inputtableSlots => new byte[] { 0 };
        public byte[] outputtableSlots => new byte[] { 1 };

        public delegate bool PreTransmutateItemDelegate(TransmutatorTileEntity transmutator, TransmutationRecipe recipe);

        public delegate void PostTransmutateItemDelegate(TransmutatorTileEntity transmutator, TransmutationRecipe recipe);

        public static event PreTransmutateItemDelegate PreTransmutateItemEvent;

        public static event PostTransmutateItemDelegate PostTransmutateItemEvent;

        public const int DISPERSAL_BUFF_RADIUS = 640;

        protected override HoverUIData ManageHoverUI()
        {
            List<HoverUIElement> data = new List<HoverUIElement>();
            if (inventory != null)
            {
                data.Add(new TransmutatorUIElement("Input", false, new Vector2(-40, 0)));
                data.Add(new TransmutatorUIElement("Output", true, new Vector2(-40, 0)));
            }
            if (maxRadiance > 0)
                data.Add(new RadianceBarUIElement("RadianceBar", storedRadiance, maxRadiance, new Vector2(0, 40)));

            if (projector != null)
            {
                if (projector.LensPlaced is not null && RadianceSets.ProjectorLensID[projector.LensPlaced.type] == (int)ProjectorLensID.Pathos)
                    data.Add(new CircleUIElement("PathosAoECircle", 600, Color.Red));
            }
            if (activeBuff > 0)
                data.Add(new CircleUIElement("BuffAoECircle", DISPERSAL_BUFF_RADIUS, CommonColors.RadianceColor1));
            float yGap = -32;
            if (radianceModifier != 1)
            {
                string str = (radianceModifier).ToString() + "x";
                data.Add(new TextUIElement("RadianceModifier", str, CommonColors.RadianceColor1, new Vector2(SineTiming(33), yGap + SineTiming(50))));
                yGap -= 16;
            }
            if (activeBuff > 0 && activeBuffTime > 0)
            {
                //TimeSpan.MaxValue.TotalSeconds
                TimeSpan time = TimeSpan.FromSeconds(activeBuffTime / 60);
                string str = activeBuffTime < 216000 ? time.ToString(@"mm\:ss") : time.ToString(@"hh\:mm\:ss");
                Color color = PotionColors.ScarletPotions.Contains(activeBuff) ? CommonColors.ScarletColor : PotionColors.CeruleanPotions.Contains(activeBuff) ? CommonColors.CeruleanColor : PotionColors.VerdantPotions.Contains(activeBuff) ? CommonColors.VerdantColor : PotionColors.MauvePotions.Contains(activeBuff) ? CommonColors.MauveColor : Color.White;
                data.Add(new TextUIElement("PotionTime", string.Join(" ", Regex.Split(GetBuffName(activeBuff), @"(?<!^)(?=[A-Z])")) + ": " + str, color, new Vector2(0, yGap)));
            }
            return new HoverUIData(this, this.TileEntityWorldCenter(), data.ToArray());
        }

        public bool TryInsertItemIntoSlot(Item item, byte slot, bool overrideValidInputs, bool ignoreItemImprint)
        {
            if ((!ignoreItemImprint && !itemImprintData.IsItemValid(item)) || (!overrideValidInputs && !inputtableSlots.Contains(slot)))
                return false;

            return true;
        }

        public override void PreOrderedUpdate()
        {
            radianceModifier = 1;
            if (projector is not null && projector.LensPlaced is not null)
                RadianceSets.ProjectorLensPreOrderedUpdateFunction[projector.LensPlaced.type]?.Invoke(projector);
        }

        public override void OrderedUpdate()
        {
            if (projector is not null && projector.LensPlaced is not null)
                RadianceSets.ProjectorLensOrderedUpdateFunction[projector.LensPlaced.type]?.Invoke(projector);

            if (projectorBeamTimer > 0)
                projectorBeamTimer--;

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
                            if (player.active && !player.ghost && player.Distance(this.TileEntityWorldCenter()) < DISPERSAL_BUFF_RADIUS)
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

            if (HasProjector && (projector.LensPlaced is not null && !projector.LensPlaced.IsAir) && (projector.ContainerPlaced is not null && !projector.ContainerPlaced.Item.IsAir) && !this.GetSlot(0).IsAir)
            {
                TransmutationRecipe activeRecipe = null;
                foreach (TransmutationRecipe recipe in transmutationRecipes)
                {
                    if (recipe.inputItems.Contains(this.GetSlot(0).type) && recipe.unlock.condition() && this.GetSlot(0).stack >= recipe.inputStack)
                    {
                        activeRecipe = recipe;
                        break;
                    }
                }
                if (activeRecipe != null)
                {
                    isCrafting = true;
                    bool flag = true;
                    foreach (TransmutationRequirement req in activeRecipe.transmutationRequirements)
                    {
                        flag &= req.condition(this);
                    }
                    storedRadiance = projector.storedRadiance;
                    maxRadiance = activeRecipe.requiredRadiance * radianceModifier;

                    if ((this.GetSlot(1).IsAir || activeRecipe.outputItem == this.GetSlot(1).type) && //output item is empty or same as recipe output
                        (activeRecipe.outputStack <= this.GetSlot(1).maxStack - this.GetSlot(1).stack || this.GetSlot(1).IsAir) && //output item current stack is less than or equal to the recipe output stack
                        RadianceSets.ProjectorLensID[projector.LensPlaced.type] != (int)ProjectorLensID.None //projector has lens in it
                        && storedRadiance >= maxRadiance //contains enough radiance to craft
                        && flag //special requirements are met
                        )
                    {
                        glowTime = Math.Min(glowTime + 2, 90);
                        craftingTimer++;
                        if (craftingTimer >= 120)
                            Craft(activeRecipe);
                    }
                    return;
                }
            }
            isCrafting = false;
            if (craftingTimer > 0)
                craftingTimer--;

            storedRadiance = maxRadiance = 0;

            if (craftingTimer == 0 && glowTime > 0)
                glowTime -= Math.Clamp(glowTime, 0, 2);
        }

        public void Craft(TransmutationRecipe activeRecipe)
        {
            bool result = true;
            if (PreTransmutateItemEvent is not null)
            {
                foreach (PreTransmutateItemDelegate del in PreTransmutateItemEvent.GetInvocationList())
                {
                    result &= del(this, activeRecipe);
                }
            }
            if (result)
            {
                this.GetSlot(0).stack -= activeRecipe.inputStack;
                if (this.GetSlot(0).stack <= 0)
                    this.GetSlot(0).TurnToAir();

                if (this.GetSlot(1).IsAir)
                    this.SetItemInSlot(1, new Item(activeRecipe.outputItem, activeRecipe.outputStack));
                else
                    this.GetSlot(1).stack += activeRecipe.outputStack;
            }
            PostTransmutateItemEvent?.Invoke(this, activeRecipe);

            craftingTimer = 0;
            projectorBeamTimer = 60;
            projector.ContainerPlaced.storedRadiance -= activeRecipe.requiredRadiance * radianceModifier;

            ParticleSystem.AddParticle(new StarFlare(this.TileEntityWorldCenter() - Vector2.UnitY * 4, 8, 50, new Color(255, 220, 138), new Color(255, 220, 138), 0.125f));
            SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/ProjectorFire"), new Vector2(Position.X * 16 + Width * 8, Position.Y * 16 + -Height * 8));
        }

        public override void SaveExtraExtraData(TagCompound tag)
        {
            tag["BuffType"] = activeBuff;
            tag["BuffTime"] = activeBuffTime;
            this.SaveInventory(tag);
        }

        public override void LoadExtraExtraData(TagCompound tag)
        {
            activeBuff = tag.Get<int>("BuffType");
            activeBuffTime = tag.Get<int>("BuffTime");
            this.LoadInventory(tag);
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
            if (TryGetTileEntityAs(i, j, out AssemblableTransmutatorTileEntity entity))
                entity.DrawHoverUIAndMouseItem();
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
            ModContent.GetInstance<TransmutatorTileEntity>(),
            ModContent.Request<Texture2D>("Radiance/Content/Tiles/Transmutator/AssemblableTransmutator").Value,
            new()
            {
                (CommonItemGroups.IronBars, 4),
                (CommonItemGroups.IronBars, 8),
                (CommonItemGroups.IronBars, 4),
                (CommonItemGroups.SilverBars, 8),
                ([ModContent.ItemType<ShimmeringGlass>()], 6 ),
            }
            )
        { }

        public override void Load()
        {
            BlueprintLoader.AddBlueprint(() => (
                nameof(Transmutator) + "Blueprint",
                ModContent.ItemType<TransmutatorItem>(),
                ModContent.GetInstance<AssemblableTransmutatorTileEntity>(),
                new Color(255, 168, 180),
                1));
        }

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
        public TransmutatorItem() : base("TransmutatorItem", "Radiance Transmutator", "Uses concentrated Radiance to convert items into other objects\nCan only be placed above a Radiance Projector", "Transmutator", 1, Item.sellPrice(0, 0, 10, 0), ItemRarityID.Green)
        {
        }
    }

    //public class TransmutatorBlueprint : BaseTileItem
    //{
    //    public override string Texture => "Radiance/Content/ExtraTextures/Blueprint";

    //    public TransmutatorBlueprint() : base("TransmutatorBlueprint", "Mysterious Blueprint", "Begins the assembly of an arcane machine", "AssemblableTransmutator", 1, Item.sellPrice(0, 0, 5, 0), ItemRarityID.Blue)
    //    {
    //    }
    //}
}