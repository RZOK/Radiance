//using Radiance.Core.Systems;
//using Steamworks;
//using Terraria.Graphics.Effects;

//namespace Radiance.Content.Particles
//{
//    public class PearlFlow : Particle
//    {
//        private const int POINT_COUNT = 5;

//        private Vector2 endPosition;
//        private List<Vector2> drawPoints;
//        internal PrimitiveTrail TrailDrawer;
//        public override string Texture => "Radiance/Content/Particles/HiddenTextSparkle";

//        public PearlFlow(Vector2 startPosition, Vector2 endPosition)
//        {
//            position = startPosition;
//            this.endPosition = endPosition;
//            timeLeft = maxTime = 30;
//            specialDraw = true;
//            drawPixelated = true;
//            mode = ParticleSystem.DrawingMode.Additive;
//        }

//        public override void Update()
//        {
//            TrailDrawer = TrailDrawer ?? new PrimitiveTrail(50, f =>
//            {
//                return 5f;
//            }, factor =>
//            {
//                return Color.White;
//            }, new NoTip());
//            int pointCount = 32;
            
//            drawPoints = new BezierCurve(position, new Vector2((position.X + endPosition.X) / 2f, position.Y - 160f), endPosition).GetEvenlySpacedPoints(pointCount);
//            for (int i = 1; i < drawPoints.Count - 1f; i++) //don't modify the start or end
//            {
//                int mod = 1 - ((i % 2) * 2);
//                float distance = i - 1f;
//                if (i > (pointCount - 2f) / 2f)
//                    distance = pointCount - 2f - i;
//                distance += 2f;
//                drawPoints[i] -= Vector2.UnitY.RotatedBy(drawPoints[i].AngleTo(drawPoints[i + 1])) * SineTiming(55 + distance * 2f, i * 21f) * mod * (distance * 0.1f);
//            }
//            int pointsRemaining = (int)(pointCount * (1f - Progress));
//            drawPoints.RemoveRange(pointsRemaining, pointCount - pointsRemaining);
//            drawPoints.RemoveRange(0, (int)MathF.Max(0, (16 - pointCount * Progress)));
//            TrailDrawer.SetPositionsSmart(drawPoints, position, SmoothBezierPointRetreivalFunction);
//        }

//        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
//        {
//            Effect effect = Filters.Scene["FadedUVMapStreak"].GetShader().Shader;
//            effect.Parameters["time"].SetValue(0f);
//            effect.Parameters["fadeDistance"].SetValue(0f);
//            effect.Parameters["fadePower"].SetValue(0f);
//            effect.Parameters["trailTexture"].SetValue(ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/BasicTrail").Value);

//            TrailDrawer?.Render(effect, -Main.screenPosition);
//        }
//    }
//}