using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Content.Particles;
using Radiance.Core.Config;
using Radiance.Core.Loaders;
using Radiance.Core.Systems;
using Radiance.Core.Systems.ParticleSystems;
using System.Text.RegularExpressions;
using Terraria.Localization;
using Terraria.ObjectData;
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
                    if (entity.activeBuff > 0 && entity.activeBuffTime > 0 && entity.projector != null && ProjectorLensData.loadedData[entity.projector.LensPlaced.type].id == nameof(LensofPathos))
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

                    entity.SafeInsertSlot(0, selItem, out success, true, true);
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
        public const int DISPERSAL_BUFF_RADIUS = 640;
        public const float RADIANCE_DISCOUNT_MINIMUM = 0.1f;

        public float radianceModifier = 1;
        public bool HasProjector => projector != null;
        public ProjectorTileEntity projector;

        public float glowTime = 0;
        public float projectorBeamTimer = 0;

        public float craftingTimer = 0;
        public bool isCrafting = false;

        public int activeBuff = 0;
        public int activeBuffTime = 0;

        public EnvironmentalEffect activeEffect = null;
        public int activeEffectTime = 0;

        public Item[] inventory { get; set; }
        public int inventorySize { get; set; }
        public byte[] inputtableSlots => new byte[] { 0 };
        public byte[] outputtableSlots => new byte[] { 1 };

        // Return true to have the output item become the result of the recipe. Return false if you're setting it manually.
        public delegate bool PreTransmutateItemDelegate(TransmutatorTileEntity transmutator, TransmutationRecipe recipe);

        public delegate void PostTransmutateItemDelegate(TransmutatorTileEntity transmutator, TransmutationRecipe recipe);

        public static event PreTransmutateItemDelegate PreTransmutateItemEvent;

        public static event PostTransmutateItemDelegate PostTransmutateItemEvent;

        public List<float> queuedDiscounts = new List<float>();
        private List<float> activeDiscounts = new List<float>();

        public TransmutatorTileEntity() : base(ModContent.TileType<Transmutator>(), 0, new(), new(), usesItemImprints: true)
        {
            inventorySize = 2;
            this.ConstructInventory();
        }

        protected override HoverUIData GetHoverUI()
        {
            List<HoverUIElement> data = new List<HoverUIElement>();
            if (maxRadiance > 0)
                data.Add(new RadianceBarUIElement("RadianceBar", storedRadiance, maxRadiance, new Vector2(0, 40)));

            if (inventory is not null)
            {
                data.Add(new TransmutatorUIElement("Input", false, new Vector2(-40, 0)));
                data.Add(new TransmutatorUIElement("Output", true, new Vector2(-40, 0)));
            }
            if (projector is not null)
            {
                if (projector.LensPlaced is not null && ProjectorLensData.loadedData[projector.LensPlaced.type].id == nameof(LensofPathos))
                    data.Add(new CircleUIElement("PathosAoECircle", 600, Color.Red));
            }

            float yGap = -32;
            if (radianceModifier != 1)
            {
                string str = (radianceModifier).ToString() + "x";
                data.Add(new TextUIElement("RadianceModifier", str, CommonColors.RadianceColor1, Vector2.UnitY * yGap));
                yGap -= 20;
            }
            if (activeBuff > 0 && activeBuffTime > 0)
            {
                data.Add(new CircleUIElement("BuffAoECircle", DISPERSAL_BUFF_RADIUS, CommonColors.RadianceColor1));

                //TimeSpan.MaxValue.TotalSeconds
                TimeSpan time = TimeSpan.FromSeconds(activeBuffTime / 60);
                string str = activeBuffTime < 216000 ? time.ToString(@"mm\:ss") : time.ToString(@"hh\:mm\:ss");
                Color color = PotionColors.ScarletPotions.Contains(activeBuff) ? CommonColors.ScarletColor : PotionColors.CeruleanPotions.Contains(activeBuff) ? CommonColors.CeruleanColor : PotionColors.VerdantPotions.Contains(activeBuff) ? CommonColors.VerdantColor : PotionColors.MauvePotions.Contains(activeBuff) ? CommonColors.MauveColor : Color.White;
                data.Add(new TextUIElement("PotionTime", string.Join(" ", Regex.Split(GetBuffName(activeBuff), @"(?<!^)(?=[A-Z])")) + ": " + str, color, new Vector2(0, yGap)));
                yGap -= 20;
            }
            if (activeEffect is not null && activeEffectTime > 0)
            {
                data.AddRange(activeEffect.GetHoverUI(this));

                //TimeSpan.MaxValue.TotalSeconds
                TimeSpan time = TimeSpan.FromSeconds(activeEffectTime / 60);
                string str = activeEffectTime < 216000 ? time.ToString(@"mm\:ss") : time.ToString(@"hh\:mm\:ss");
                data.Add(new TextUIElement("EnvironmentalEffectTime", activeEffect.localizationKey.Value + ": " + str, activeEffect.color, new Vector2(0, yGap)));
            }

            return new HoverUIData(this, this.TileEntityWorldCenter(), data.ToArray());
        }

        public bool CanInsertSlot(Item item, byte slot, bool overrideValidInputs, bool ignoreItemImprint)
        {
            if ((!ignoreItemImprint && !itemImprintData.ImprintAcceptsItem(item)) || (!overrideValidInputs && !inputtableSlots.Contains(slot)))
                return false;

            return true;
        }

        public override void PreOrderedUpdate()
        {
            radianceModifier = 1;
            if (projector is not null && projector.LensPlaced is not null && ProjectorLensData.loadedData[projector.LensPlaced.type].preOrderedUpdate is not null)
                ProjectorLensData.loadedData[projector.LensPlaced.type].preOrderedUpdate(projector);

            activeEffect?.PreOrderedUpdate(this);
        }

        public override void OrderedUpdate()
        {
            if (activeEffect is not null)
            {
                activeEffect.OrderedUpdate(this);

                activeEffectTime--;
                if (activeEffectTime <= 0)
                    activeEffect = null;
            }
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
            if (projector is not null && projector.LensPlaced is not null && ProjectorLensData.loadedData[projector.LensPlaced.type].orderedUpdate is not null)
                ProjectorLensData.loadedData[projector.LensPlaced.type].orderedUpdate(projector);

            if (projectorBeamTimer > 0)
                projectorBeamTimer--;

            if (Main.tile[Position.X, Position.Y + 3].TileType == ModContent.TileType<Projector>() && Main.tile[Position.X, Position.Y + 2].TileFrameX == 0)
            {
                if (TryGetTileEntityAs(Position.X, Position.Y + 3, out ProjectorTileEntity entity))
                    projector = entity;
            }
            else
                projector = null;

            activeDiscounts.Clear();
            activeDiscounts.AddRange(queuedDiscounts);
            foreach (float discount in activeDiscounts)
            {
                radianceModifier -= discount;
                if (radianceModifier <= RADIANCE_DISCOUNT_MINIMUM)
                    radianceModifier = RADIANCE_DISCOUNT_MINIMUM;
            }
            queuedDiscounts.Clear();

            if (HasProjector && (projector.LensPlaced is not null && !projector.LensPlaced.IsAir) && (projector.ContainerPlaced is not null && !projector.ContainerPlaced.Item.IsAir) && !this.GetSlot(0).IsAir)
            {
                TransmutationRecipe activeRecipe = null;
                if (TransmutationRecipeSystem.byInputItem.TryGetValue(this.GetSlot(0).type, out TransmutationRecipe recipe) && recipe.unlock.condition() && this.GetSlot(0).stack >= recipe.inputStack)
                    activeRecipe = recipe;

                if (activeRecipe != null)
                {
                    isCrafting = true;
                    bool specialRequirementsMet = true;
                    foreach (TransmutationRequirement req in activeRecipe.transmutationRequirements)
                    {
                        specialRequirementsMet &= req.condition(this);
                    }
                    storedRadiance = projector.storedRadiance;
                    maxRadiance = activeRecipe.requiredRadiance * radianceModifier;

                    if ((this.GetSlot(1).IsAir || activeRecipe.outputItem == this.GetSlot(1).type) && //output item is empty or same as recipe output
                        (activeRecipe.outputStack <= this.GetSlot(1).maxStack - this.GetSlot(1).stack || this.GetSlot(1).IsAir) && //output item current stack is less than or equal to the recipe output stack
                        !projector.LensPlaced.IsAir //projector has lens in it
                        && storedRadiance >= maxRadiance //contains enough radiance to craft
                        && specialRequirementsMet //special requirements are met
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
                glowTime = Math.Max(glowTime - 2, 0);
        }

        private void Craft(TransmutationRecipe activeRecipe)
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
                    this.SetSlot(1, new Item(activeRecipe.outputItem, activeRecipe.outputStack));
                else
                    this.GetSlot(1).stack += activeRecipe.outputStack;
            }
            PostTransmutateItemEvent?.Invoke(this, activeRecipe);

            craftingTimer = 0;
            projectorBeamTimer = 60;
            projector.ContainerPlaced.storedRadiance -= activeRecipe.requiredRadiance * radianceModifier;

            WorldParticleSystem.system.AddParticle(new StarFlare(this.TileEntityWorldCenter() - Vector2.UnitY * 4, 8, new Color(255, 220, 138) * 0.8f, new Color(255, 220, 138) * 0.8f, 0.125f));
            SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/ProjectorFire"), new Vector2(Position.X * 16 + Width * 8, Position.Y * 16 + -Height * 8));
        }

        public override void SaveExtraExtraData(TagCompound tag)
        {
            tag[nameof(activeBuff)] = activeBuff;
            tag[nameof(activeBuffTime)] = activeBuffTime;
            tag[nameof(activeEffect)] = activeEffect.GetID();
            tag[nameof(activeEffectTime)] = activeEffectTime;
            this.SaveInventory(tag);
        }

        public override void LoadExtraExtraData(TagCompound tag)
        {
            activeBuff = tag.GetInt(nameof(activeBuff));
            activeBuffTime = tag.GetInt(nameof(activeBuffTime));
            activeEffect = EnvironmentalEffect.GetEnvironmentalEffect(tag.GetString(nameof(activeEffect)));
            activeEffectTime = tag.GetInt(nameof(activeEffectTime));
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
            if (entity is not null)
            {
                Color color = output ? ModContent.GetInstance<AccessibilityConfig>().radianceOutputColor : ModContent.GetInstance<AccessibilityConfig>().radianceInputColor;
                RadianceDrawing.DrawSoftGlow(elementPosition, color * timerModifier, Math.Max(0.4f * (float)Math.Abs(SineTiming(100)), 0.35f));
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

            TileObjectData.newTile.AnchorBottom = new AnchorData(Terraria.Enums.AnchorType.AlternateTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorAlternateTiles = new int[1] { ModContent.TileType<Projector>() };

            TileObjectData.newTile.AnchorValidTiles = new int[] {
                ModContent.TileType<Projector>()
            };

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<AssemblableTransmutatorTileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (TryGetTileEntityAs(i, j, out AssemblableTransmutatorTileEntity entity))
                entity.Draw(spriteBatch, entity.stage);

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
                1,
                UnlockCondition.UnlockedByDefault));
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

    public abstract class EnvironmentalEffect : ILoadable
    {
        private static Dictionary<string, EnvironmentalEffect> environmentalEffects = new Dictionary<string, EnvironmentalEffect>();
        public string name;
        public Mod mod;
        public Color color;
        public LocalizedText localizationKey;

        public EnvironmentalEffect(string name, Mod mod, Color color)
        {
            this.name = name;
            this.mod = mod;
            this.color = color;
            localizationKey = Language.GetOrRegister($"Mods.{mod.Name}.Transmutation.EnvironmentalEffects.{name}");

            environmentalEffects.Add(GetID(), this);
        }

        public virtual void PreOrderedUpdate(TransmutatorTileEntity transmutator)
        { }

        public virtual void OrderedUpdate(TransmutatorTileEntity transmutator)
        { }

        public virtual List<HoverUIElement> GetHoverUI(TransmutatorTileEntity transmutator)
        { return null; }

        public string GetID() => $"{mod.Name}.{name}";

        public static EnvironmentalEffect GetEnvironmentalEffect<T>() where T : EnvironmentalEffect => environmentalEffects.Values.FirstOrDefault(x => x.GetType() == typeof(T));

        public static EnvironmentalEffect GetEnvironmentalEffect(string id)
        {
            if (id == string.Empty)
                return null;

            return environmentalEffects[id];
        }

        public void Load(Mod mod)
        { }

        public void Unload()
        { }
    }
}