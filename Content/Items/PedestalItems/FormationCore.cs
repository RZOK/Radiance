using Radiance.Content.Items.BaseItems;
using Radiance.Content.Particles;
using Radiance.Content.Tiles.Pedestals;
using Radiance.Core.Config;
using Radiance.Core.Systems;


namespace Radiance.Content.Items.PedestalItems
{
    public class FormationCore : BaseContainer, IPedestalItem, ITransmutationRecipe
    {
        public FormationCore() : base(
            null,
            10,
            false)
        { }

        private static readonly Color AOE_CIRCLE_COLOR = new Color(16, 112, 64);
        private static readonly float AOE_CIRCLE_RADIUS = 75;
        private static readonly float MINIMUM_RADIANCE = 0.01f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Formation Core");
            Tooltip.SetDefault("Places nearby items into adjacent inventories when placed on a Pedestal");
            Item.ResearchUnlockCount = 3;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.LightRed;
        }

        public void AddTransmutationRecipe(TransmutationRecipe recipe)
        {
            recipe.inputItems = new int[] { ItemID.CursedFlame, ItemID.Ichor };
            recipe.inputStack = 3;
            recipe.unlock = UnlockCondition.UnlockedByDefault;
        }

        public new void UpdatePedestal(PedestalTileEntity pte)
        {
            if (pte.enabled)
            {
                pte.aoeCircleColor = AOE_CIRCLE_COLOR;
                pte.aoeCircleRadius = AOE_CIRCLE_RADIUS;

                Vector2 pos = MultitileWorldCenter(pte.Position.X, pte.Position.Y);
                if (pte.actionTimer > 0)
                {
                    Vector2 vel = Vector2.UnitX.RotatedByRandom(Pi) * Main.rand.Next(3, 6);
                    for (int d = 0; d < 4; d++)
                    {
                        float rot = PiOver2 * d;
                        Dust f = Dust.NewDustPerfect(pte.FloatingItemCenter(Item) - new Vector2(0, -5 * SineTiming(30) + 2), DustID.GemEmerald);
                        f.velocity = vel.RotatedBy(rot);
                        f.noGravity = true;
                        f.scale = Main.rand.NextFloat(1f, 1.3f);
                    }
                    pte.actionTimer--;
                }
                if (pte.storedRadiance >= MINIMUM_RADIANCE && pte.actionTimer == 0)
                {
                    for (int k = 0; k < Main.maxItems; k++)
                    {
                        Item item = Main.item[k];
                        if (item.IsAir)
                            continue;

                        IInventory adjacentInventory = TryGetAdjacentInentory(pte, item, out ImprovedTileEntity inventoryEntity);
                        if (pte.itemImprintData.ImprintAcceptsItem(item) && adjacentInventory is not null && Vector2.Distance(item.Center, pos) < AOE_CIRCLE_RADIUS && item.noGrabDelay == 0 && item.active && !item.IsAir && item.GetGlobalItem<FormationCoreGlobalItem>().formationPickupTimer == 0)
                        {
                            Item clonedItem = item.Clone();
                            adjacentInventory.InsertItem(item, out _);
                            DustSpawn(item);
                            ParticleSystem.AddParticle(new StarItem(item.Center, inventoryEntity.TileEntityWorldCenter(), 60, Color.PaleGreen, clonedItem, 1f));
                            storedRadiance -= MINIMUM_RADIANCE;
                            pte.actionTimer = 5;
                        }
                    }
                }

                if (Main.GameUpdateCount % 120 == 0)
                {
                    if (Main.rand.NextBool(3))
                    {
                        Vector2 vel = Vector2.UnitX.RotatedByRandom(Pi) * 2;
                        for (int i = 0; i < 4; i++)
                        {
                            float rot = PiOver2 * i;
                            Dust f = Dust.NewDustPerfect(pte.FloatingItemCenter(Item) - new Vector2(0, -5 * SineTiming(30) + 2), 89);
                            f.velocity = vel.RotatedBy(rot);
                            f.noGravity = true;
                            f.scale = Main.rand.NextFloat(0.8f, 1.1f);
                        }
                    }
                }
            }
        }

        public static IInventory TryGetAdjacentInentory(PedestalTileEntity pte, Item item, out ImprovedTileEntity entity)
        {
            TryGetTileEntityAs(pte.Position.X + 2, pte.Position.Y, out entity);
            if (entity is not null && entity is IInventory inventory)
            {
                IInventory correctInventory = inventory.GetInventory();
                if (correctInventory.CanInsertItem(item))
                    return correctInventory;
            }

            TryGetTileEntityAs(pte.Position.X - 1, pte.Position.Y, out entity);
            if (entity is not null && entity is IInventory inventory2)
            {
                IInventory correctInventory = inventory2.GetInventory();
                if (correctInventory.CanInsertItem(item))
                    return correctInventory;
            }
            return null;
        }

        public static void DustSpawn(Item item)
        {
            for (int i = 0; i < 30; i++)
            {
                SoundEngine.PlaySound(SoundID.Item8, item.Center);
                int f = Dust.NewDust(item.position, item.width, item.height, DustID.GemEmerald, 0, 0);
                Main.dust[f].velocity *= 0.5f;
                Main.dust[f].scale = 1.2f;
                Main.dust[f].noGravity = true;
            }
        }
    }

    public class FormationCoreGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public int formationPickupTimer = 0;

        public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
        {
            if (formationPickupTimer > 0)
                formationPickupTimer--;
        }
    }
    public class StarItem : Particle
    {
        public override string Texture => "Radiance/Content/Particles/StarMedium";
     
        private readonly Item item;
        private readonly Vector2 idealPosition;
        private readonly Vector2 initialPosition;
        private readonly float initialScale;
        private readonly Vector2 curveOffset;
        private float Distance => Math.Max(64, initialPosition.Distance(idealPosition));
        private Vector2 CurvePoint => ((idealPosition - initialPosition) / 2f) + curveOffset;
        private float ScaleMod => Math.Min(2f, Item.GetDrawHitbox(item.type, null).Width / 32f);

        public StarItem(Vector2 position, Vector2 idealPosition, int maxTime, Color color, Item item, float scale)
        {
            this.position = initialPosition = position;
            this.idealPosition = idealPosition;
            this.maxTime = timeLeft = maxTime;
            this.color = color;
            initialScale = scale * Max(1f, ScaleMod);
            this.scale = initialScale;
            mode = ParticleSystem.DrawingMode.Additive;

            this.item = item;
            curveOffset = Main.rand.NextVector2CircularEdge(Distance, Distance);
        }

        public override void Update()
        {
            float scaleStart = 0.75f;
            if (Progress >= scaleStart)
                scale = Lerp(initialScale, 0f, EaseOutExponent((Progress - scaleStart) / (1f - scaleStart), 2f));

            if (timeLeft % 10 == 0)
                ParticleSystem.DelayedAddParticle(new LingeringStar(position, Main.rand.NextVector2CircularEdge(1f, 1f), 30, CommonColors.RadianceColor1, scale * 0.8f)); //dont want huge scale increase so i'll just replace thinstarflare with new pixeled star

            position = Vector2.Hermite(initialPosition, CurvePoint, idealPosition, -CurvePoint, EaseInOutExponent(Progress, 4f));
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D itemTex = GetItemTexture(item.type);
            float colorMod = Min(1f, Progress * 10f);
            bool adaptiveColoring = ModContent.GetInstance<RadianceConfig>().FormationCoreAdaptiveColoring;

            if (adaptiveColoring)
            {
                Main.instance.GraphicsDevice.Textures[1] = itemTex;
                Effect getColorEffect = Terraria.Graphics.Effects.Filters.Scene["StarColoring"].GetShader().Shader;
                getColorEffect.Parameters["alpha"].SetValue((Color.White * colorMod).ToVector4());

                spriteBatch.End();
                RadianceDrawing.SpriteBatchData.AdditiveParticleDrawing.BeginSpriteBatchFromTemplate(effect: getColorEffect);
            }

            Main.spriteBatch.Draw(texture, drawPos, null, color * colorMod, rotation, texture.Size() / 2, scale, SpriteEffects.None, 0);

            if (adaptiveColoring)
            {
                spriteBatch.End();
                RadianceDrawing.SpriteBatchData.AdditiveParticleDrawing.BeginSpriteBatchFromTemplate();
            }

            Main.spriteBatch.Draw(texture, drawPos, null, Color.White * colorMod, rotation, texture.Size() / 2, scale * 0.8f, SpriteEffects.None, 0);
        }
    }
}