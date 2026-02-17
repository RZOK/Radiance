using Radiance.Content.Items.BaseItems;
using Radiance.Content.Particles;
using Radiance.Content.Tiles.Pedestals;
using Radiance.Core.Systems;
using Terraria.UI;


namespace Radiance.Content.Items.PedestalItems
{
    public class AnnihilationCore : BaseContainer, IPedestalItem, ITransmutationRecipe
    {
        public AnnihilationCore() : base(
            null,
            10,
            false)
        { }

        private static readonly Color AOE_CIRCLE_COLOR = new Color(158, 98, 234);
        private static readonly float AOE_CIRCLE_RADIUS = 64;
        private static readonly float MINIMUM_RADIANCE = 0.01f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Annihilation Core");
            Tooltip.SetDefault("Destroys nearby items when placed on a Pedestal");
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
            recipe.inputItems = new int[] { ItemID.SoulofNight };
            recipe.inputStack = 3;
            recipe.unlock = UnlockCondition.UnlockedByDefault;
        }

        public new void UpdatePedestal(PedestalTileEntity pte)
        {
            if (pte.enabled)
            {
                pte.aoeCircleColor = AOE_CIRCLE_COLOR;
                pte.aoeCircleRadius = AOE_CIRCLE_RADIUS;

                if (pte.actionTimer > 0)
                    pte.actionTimer--;

                Vector2 pos = RadianceUtils.TileEntityWorldCenter(pte);
                if (pte.actionTimer == 0 && pte.storedRadiance >= MINIMUM_RADIANCE)
                {
                    for (int k = 0; k < Main.maxItems; k++)
                    {
                        Item item = Main.item[k];
                        if (Vector2.Distance(item.Center, pos) < 75 && item.noGrabDelay == 0 && item.active && pte.itemImprintData.ImprintAcceptsItem(item))
                        {
                            ParticleSystem.AddParticle(new StarFlare(pte.FloatingItemCenter(Item), 10, new Color(212, 160, 232), new Color(139, 56, 255), 0.025f));
                            ParticleSystem.AddParticle(new Lightning(new List<Vector2> { pte.FloatingItemCenter(Item), item.Center }, new Color(139, 56, 255), 12, 1.5f));
                            ParticleSystem.AddParticle(new DisintegratingItem(item.Center, new Vector2(1, -2), 90, (item.Center.X - pos.X).NonZeroSign(), item.Clone(), GetItemTexture(item.type)));
                            SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/LightningZap") with { PitchVariance = 0.5f, Volume = 0.8f }, pos);

                            item.TurnToAir();
                            item.active = false;
                            storedRadiance -= MINIMUM_RADIANCE;
                            pte.actionTimer = 60;

                            Vector2 itemSize = GetItemTexture(item.type).Size();
                            for (int i = 0; i < 3; i++)
                            {
                                ParticleSystem.AddParticle(new SpeedLine(item.Center + Main.rand.NextVector2Circular(itemSize.X - 4f, itemSize.Y - 4f) / 2f - Vector2.UnitY * 24, -Vector2.UnitY * Main.rand.NextFloat(2.5f, 4f), 15, new Color(139, 56, 255), -PiOver2, Main.rand.Next(40, 88)));
                            }
                            break;
                        }
                    }
                }
                if (Main.GameUpdateCount % 120 == 0)
                {
                    int f = Dust.NewDust(pos - new Vector2(0, -5 * SineTiming(30) + 2) - new Vector2(8, 8), 16, 16, DustID.PurpleCrystalShard, 0, 0);
                    Main.dust[f].velocity *= 0.1f;
                    Main.dust[f].noGravity = true;
                    Main.dust[f].scale = Main.rand.NextFloat(1.2f, 1.4f);
                }
            }
        }
    }
    public class DisintegratingItem : Particle
    {
        public override string Texture => "Radiance/Content/ExtraTextures/Blank";
        public Texture2D itemTexture;
        public Item item;
        public int direction;
        public float alpha;

        public DisintegratingItem(Vector2 position, Vector2 velocity, int maxTime, int direction, Item item, Texture2D tex)
        {
            this.direction = direction;
            this.position = position;
            this.velocity = velocity;
            this.velocity.X *= direction;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            
            mode = ParticleSystem.DrawingMode.Regular;
            itemTexture = tex;
            this.item = item;
        }

        public override void Update()
        {
            if (timeLeft > maxTime / 1.7f)
            {
                velocity.X += Lerp(0.1f, 0, EaseInOutExponent(Progress * 0.5f + 0.5f, 3f)) * direction;
                rotation += (velocity.X / 50f) * (1f - Progress);
            }
            else
            {
                if (timeLeft < maxTime / 2f)
                    alpha = (timeLeft / (float)(maxTime / 2f));

                Rectangle rect = Item.GetDrawHitbox(item.type, null);
                Rectangle ashRectangle = new Rectangle((int)position.X - rect.Width / 2, (int)position.Y - rect.Height / 2, rect.Width, rect.Height);
                if (Main.GameUpdateCount % 2 == 0)
                    ParticleSystem.DelayedAddParticle(new Ash(Main.rand.NextVector2FromRectangle(ashRectangle), 45, 1.2f));
            }
            velocity *= 0.92f;
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Point tileCoords = position.ToTileCoordinates();
            Color color = Lighting.GetColor(tileCoords);
            Color alphaColor = item.GetAlpha(color);
            float scale = 1f;
            ItemSlot.GetItemLight(ref alphaColor, ref scale, item.type);

            Effect effect = Terraria.Graphics.Effects.Filters.Scene["Disintegration"].GetShader().Shader;

            effect.Parameters["sampleTexture"].SetValue(itemTexture);
            effect.Parameters["color1"].SetValue(alphaColor.ToVector4());
            effect.Parameters["color2"].SetValue(alphaColor.MultiplyRGB(new Color(116, 75, 173)).ToVector4() * 0.2f);
            effect.Parameters["time"].SetValue(MathF.Pow(Progress, 2f));

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, effect, Main.GameViewMatrix.TransformationMatrix);

            ItemSlot.DrawItemIcon(item, 0, spriteBatch, drawPos, scale, 256, alphaColor);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}