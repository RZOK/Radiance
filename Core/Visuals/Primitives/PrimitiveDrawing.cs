namespace Radiance.Core.Visuals.Primitives
{
    public class Primitives : IDisposable //Credit goes to Oli!!!!
    {
        public bool IsDisposed { get; private set; }

        private DynamicVertexBuffer vertexBuffer;
        private DynamicIndexBuffer indexBuffer;

        private readonly GraphicsDevice device;

        public Primitives(GraphicsDevice device, int maxVertices, int maxIndices)
        {
            this.device = device;

            if (device != null)
            {
                Main.QueueMainThreadAction(() =>
                {
                    vertexBuffer = new DynamicVertexBuffer(device, typeof(VertexPositionColorTexture), maxVertices, BufferUsage.None);
                    indexBuffer = new DynamicIndexBuffer(device, IndexElementSize.SixteenBits, maxIndices, BufferUsage.None);
                });
            }
        }

        public void Render(Effect effect, Matrix translation, Matrix view)
        {
            if (vertexBuffer is null || indexBuffer is null)
                return;

            device.SetVertexBuffer(vertexBuffer);
            device.Indices = indexBuffer;

            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            if (RenderTargetsManager.NoViewMatrixPrims)
                view = Matrix.Identity;

            if (effect is BasicEffect baseEffect)
            {
                baseEffect.View = view;
                baseEffect.Projection = projection;
                baseEffect.World = translation;
            }
            else
            {
                effect.Parameters["uWorldViewProjection"].SetValue(translation * view * projection);
            }

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3);
            }
        }

        public void Render(Effect effect, Matrix translation)
        {
            if (vertexBuffer is null || indexBuffer is null)
                return;

            Render(effect, translation, Main.GameViewMatrix.TransformationMatrix);
        }

        public void SetVertices(VertexPositionColorTexture[] vertices)
        {
            vertexBuffer?.SetData(0, vertices, 0, vertices.Length, VertexPositionColorTexture.VertexDeclaration.VertexStride, SetDataOptions.Discard);
        }

        public void SetIndices(short[] indices)
        {
            indexBuffer?.SetData(0, indices, 0, indices.Length, SetDataOptions.Discard);
        }

        public void Dispose()
        {
            IsDisposed = true;

            vertexBuffer?.Dispose();
            indexBuffer?.Dispose();
        }
    }
}
namespace Radiance.Utilities
{
    #region Point retrieval functions

    public static partial class RadianceUtils
    {
        public delegate List<Vector2> TrailPointRetrievalFunction(IEnumerable<Vector2> originalPositions, int totalTrailPoints);

        public static List<Vector2> RigidPointRetreivalFunction(IEnumerable<Vector2> originalPositions, int totalTrailPoints)
        {
            List<Vector2> basePoints = originalPositions.Where(originalPosition => originalPosition != Vector2.Zero).ToList();
            List<Vector2> endPoints = new List<Vector2>();

            if (basePoints.Count < 2)
            {
                return basePoints;
            }

            float totalLenght = 0f;
            for (int i = 1; i < originalPositions.Count(); i++)
                totalLenght += (originalPositions.ElementAt(i) - originalPositions.ElementAt(i - 1)).Length();

            float stepDistance = totalLenght / (float)totalTrailPoints;
            float distanceToTravel = 0f;
            float distanceTravelled = 0f;
            float currentIndexDistance = 0f;
            int currentIndex = 0;

            while (endPoints.Count() < totalTrailPoints - 1)
            {
                float distanceToNext = (originalPositions.ElementAt(currentIndex) - originalPositions.ElementAt(currentIndex + 1)).Length();
                float nextIndexDistance = currentIndexDistance + distanceToNext;

                while (distanceTravelled + distanceToTravel > nextIndexDistance)
                {
                    currentIndex++;
                    currentIndexDistance += distanceToNext;

                    distanceToTravel -= distanceToNext;
                    distanceTravelled += distanceToNext;

                    distanceToNext = (originalPositions.ElementAt(currentIndex) - originalPositions.ElementAt(currentIndex + 1)).Length();
                    nextIndexDistance = currentIndexDistance + distanceToNext;
                }

                distanceTravelled += distanceToTravel;

                float percentOfTheWayTillTheNextPoint = (distanceTravelled - currentIndexDistance) / distanceToNext;
                endPoints.Add(Vector2.Lerp(originalPositions.ElementAt(currentIndex), originalPositions.ElementAt(currentIndex + 1), percentOfTheWayTillTheNextPoint));

                distanceToTravel = stepDistance;
            }

            endPoints.Add(originalPositions.Last());

            return endPoints;
        }

        // NOTE: Beziers can be laggy when a lot of control points are used, since our implementation
        // uses a recursive Lerp that gets more computationally expensive the more original indices.
        // n(n + 1)/2 linear interpolations to be precise, where n is the amount of original indices.
        public static List<Vector2> SmoothBezierPointRetreivalFunction(IEnumerable<Vector2> originalPositions, int totalTrailPoints)
        {
            List<Vector2> controlPoints = new List<Vector2>();
            for (int i = 0; i < originalPositions.Count(); i++)
            {
                // Don't incorporate points that are zeroed out.
                // They are almost certainly a result of incomplete oldPos arrays.
                if (originalPositions.ElementAt(i) == Vector2.Zero)
                    continue;
                controlPoints.Add(originalPositions.ElementAt(i));
            }

            BezierCurve bezierCurve = new BezierCurve(controlPoints.ToArray());
            return controlPoints.Count <= 1 ? controlPoints : bezierCurve.GetEvenlySpacedPoints(totalTrailPoints);
        }
    }
}
#endregion Point retrieval functions