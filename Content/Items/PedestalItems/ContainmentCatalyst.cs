using Radiance.Content.Items.BaseItems;
using Radiance.Content.Particles;
using Radiance.Content.Tiles.Pedestals;
using Radiance.Core.Systems;
using Radiance.Core.Visuals.Primitives;
using Steamworks;
using System.Transactions;
using Terraria.Graphics.Effects;

namespace Radiance.Content.Items.PedestalItems
{
    public class ContainmentCatalyst : BaseContainer, IPedestalItem
    {
        public ContainmentCatalyst() : base(
            null,
            10,
            false)
        { }

        private static readonly Color AOE_CIRCLE_COLOR = new Color(189, 106, 43);
        private static readonly float AOE_CIRCLE_RADIUS = 256;
        private static readonly float MINIMUM_RADIANCE = 0.01f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Containment Catalyst");
            Tooltip.SetDefault("Traps nearby critters in stasis when placed on a Pedestal");
            Item.ResearchUnlockCount = 3;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Green;
        }

        public new void UpdatePedestal(PedestalTileEntity pte)
        {
            if (pte.enabled)
            {
                pte.aoeCircleColor = AOE_CIRCLE_COLOR;
                pte.aoeCircleRadius = AOE_CIRCLE_RADIUS;

                if (pte.actionTimer < 30)
                    pte.actionTimer++;

                Vector2 pos = RadianceUtils.TileEntityWorldCenter(pte);
                if (pte.actionTimer >= 30 && pte.storedRadiance >= MINIMUM_RADIANCE)
                {
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc is null || !npc.active || !npc.CountsAsACritter || npc.catchItem == 0)
                            continue;

                        if (npc.Distance(pos) < AOE_CIRCLE_RADIUS)
                        {
                            Item.NewItem(Item.GetSource_TileInteraction(pte.Position.X, pte.Position.Y, nameof(ContainmentCatalyst)), npc.Center, npc.catchItem);
                            npc.active = false;
                            storedRadiance -= MINIMUM_RADIANCE;

                            ParticleSystem.AddParticle(new Lightning(new List<Vector2> { pte.FloatingItemCenter(Item), npc.Center }, AOE_CIRCLE_COLOR, 12, 2f, 0.2f));
                            ParticleSystem.AddParticle(new ContaintmentCatalyst_Prison(npc.Center, 120, 32, 24));
                            float rotationModifier = Main.rand.NextFloat(TwoPi);
                            int numChains = 3;
                            for (int h = 0; h < numChains; h++)
                            {
                                float rotation = rotationModifier + TwoPi * (h / (float)numChains) + Main.rand.NextFloat(0.7f);
                                ParticleSystem.AddParticle(new ContaintmentCatalyst_Chain(npc.Center + Main.rand.NextVector2Circular(4, 4), 120, rotation));
                            }
                            SoundEngine.PlaySound(SoundID.Item101 with { Volume = 0.7f }, npc.Center);
                            break;
                        }
                    }
                    pte.actionTimer = 0;
                }
            }
        }
    }

    public class ContaintmentCatalyst_Prison : Particle
    {
        public override string Texture => "Radiance/Content/Particles/TestParticle";
        internal PrimitiveCircle circle;

        private Color colorToDraw;
        private int radius;
        private int stopRadius;
        private int pointCount;

        public ContaintmentCatalyst_Prison(Vector2 position, int maxTime, int radius, int minRadius)
        {
            this.position = position;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            this.radius = radius;
            this.stopRadius = minRadius;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Additive;
            color = new Color(189, 106, 43);
            drawPixelated = true;
            pointCount = 50;
        }

        public override void Update()
        {
            circle ??= new PrimitiveCircle(pointCount, TrailWidth, TrailColor);
            circle.SetPositions(position, radius * (1f - EaseOutExponent(Progress, 12f)) + stopRadius);
        }

        private Color TrailColor(float factor)
        {
            return colorToDraw * 1.2f * (1f - MathF.Pow(Progress, 0.7f));
        }

        private float TrailWidth(float factor)
        {
            return 1f;
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            colorToDraw = color * 0.5f;
            for (int i = 0; i < 4; i++)
            {
                circle?.Render(null, -Main.screenPosition + Vector2.UnitX.RotatedBy(PiOver2 + TwoPi * (i / 4f)) * 2f);
            }
            colorToDraw = Color.White * 0.4f;
            circle?.Render(null, -Main.screenPosition);
        }
    }

    public class ContaintmentCatalyst_Chain : Particle
    {
        public override string Texture => "Radiance/Content/Items/PedestalItems/ContainmentCatalyst_Chain";

        public ContaintmentCatalyst_Chain(Vector2 position, int maxTime, float rotation)
        {
            this.position = position;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Additive;
            color = new Color(189, 106, 43);
            this.rotation = rotation;
        }

        public override void Update()
        {
            scale = Lerp(2.5f, 1f, EaseOutExponent(Progress, 12f));
        }
        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Color colorToDraw = color * MathF.Pow(1f - Progress, 0.7f);
            for (int i = 0; i < 4; i++)
            {
                spriteBatch.Draw(tex, drawPos + Vector2.UnitX.RotatedBy(PiOver2 + TwoPi * (i / 4f)) * 2f, null, colorToDraw * 0.5f, rotation, tex.Size() / 2f, scale, 0, 0);
            }
            spriteBatch.Draw(tex, drawPos, null, Color.White * MathF.Pow(1f - Progress, 0.7f) * 0.6f, rotation, tex.Size() / 2f, scale, 0, 0);
        }
    }
}