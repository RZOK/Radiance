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
                            ParticleSystem.AddParticle(new Lightning(new List<Vector2> { pte.FloatingItemCenter(Item), npc.Center }, AOE_CIRCLE_COLOR, 12, 2f, 0.2f));

                            ParticleSystem.AddParticle(new ContaintmentCatalyst_Prison(npc.Center, 60, 128, 8));
                            ParticleSystem.AddParticle(new ContaintmentCatalyst_Prison(npc.Center, 60, 256, 8));

                            //Vector2 position = npc.Center + Vector2.UnitX.RotatedByRandom(TwoPi) * 32f;
                            //Vector2 endPosition = position + position.DirectionTo(npc.Center) * 64f;
                            //ParticleSystem.AddParticle(new ContaintmentCatalyst_Chain(position, endPosition, 60));

                            npc.active = false;
                            storedRadiance -= MINIMUM_RADIANCE;
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

        public ContaintmentCatalyst_Prison(Vector2 position, int maxTime, int radius, int stopRadius)
        {
            this.position = position;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            this.radius = radius;
            this.stopRadius = stopRadius;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Additive;
            color = new Color(189, 106, 43);
            drawPixelated = true;
            pointCount = 50;
        }

        public override void Update()
        {
            circle ??= new PrimitiveCircle(pointCount, TrailWidth, TrailColor);
            circle.SetPositions(position, radius * MathF.Pow(1f - Progress, 5f) + stopRadius);
        }

        private Color TrailColor(float factor)
        {
            return colorToDraw * 0.7f * MathF.Pow(Progress, 1.6f);
        }

        private float TrailWidth(float factor)
        {
            return 2f;
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            colorToDraw = color with { A = 255 };
            for (int i = 0; i < 4; i++)
            {
                circle?.Render(null, -Main.screenPosition + Vector2.UnitX.RotatedBy(TwoPi * (i / 4f)) * 2f);
            }
            colorToDraw = Color.White with { A = 255 };
            circle?.Render(null, -Main.screenPosition);
        }
    }

    public class ContaintmentCatalyst_Chain : Particle
    {
        public override string Texture => "Radiance/Content/Items/PedestalItems/ContainmentCatalyst_Chain";
        public Texture2D tex;
        internal PrimitiveTrail TrailDrawer;

        private List<Vector2> pointCache;
        private Color colorToDraw;
        private int pointCount;
        private Vector2 endPosition;

        public ContaintmentCatalyst_Chain(Vector2 startPosition, Vector2 endPosition, int maxTime)
        {
            this.position = startPosition;
            this.endPosition = endPosition;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Additive;
            color = new Color(189, 106, 43);
            drawPixelated = false;
            pointCount = 2;
            tex = ModContent.Request<Texture2D>(Texture).Value;
            pointCache = new List<Vector2>();
            SetCache();
        }

        public override void Update()
        {
            TrailDrawer ??= new PrimitiveTrail(pointCount, TrailWidth, TrailColor);
            TrailDrawer.SetPositionsSmart(pointCache, position, RigidPointRetreivalFunction);
            TrailDrawer.NextPosition = position + velocity;
        }

        private void SetCache()
        {
            for (int i = 0; i < pointCount; i++)
            {
                pointCache.Add(Vector2.Lerp(position, endPosition, i / (pointCount - 1)));
            }
        }

        private Color TrailColor(float factor)
        {
            return colorToDraw * 0.7f * MathF.Pow(Progress, 1.6f);
        }

        private float TrailWidth(float factor)
        {
            return tex.Height / 2f + (12f * (1 - MathF.Pow(Progress, 0.2f)));
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            colorToDraw = color with { A = 255 };
            //for (int i = 0; i < 4; i++)
            //{
            //    TrailDrawer?.Render(null, -Main.screenPosition + Vector2.UnitX.RotatedBy(TwoPi * (i / 4f)) * 2f);
            //}
            //colorToDraw = Color.White with { A = 255 };
            Effect effect = Filters.Scene["PrimTrailWrap"].GetShader().Shader;
            effect.Parameters["samplerTex"].SetValue(tex);
            effect.Parameters["repeat"].SetValue(position.Distance(endPosition) / tex.Width);

            TrailDrawer?.Render(effect, -Main.screenPosition);
        }
    }
}