namespace Radiance.Core.Visuals.Primitives
{
    public class PrimitiveCircle : IDisposable
    {
        private readonly Primitives primitives;

        internal readonly int maxPointCount;

        internal readonly TrailWidthFunction trailWidthFunction;

        internal readonly TrailColorFunction trailColorFunction;

        private readonly BasicEffect baseEffect;
        public int IndexCount => 6 * maxPointCount;
        public int VertexCount => maxPointCount * 2 + 2;

        /// <summary>
        /// Array of positions that define the trail. NOTE: Positions[Positions.Length - 1] is assumed to be the start (e.g. Projectile.Center) and Positions[0] is assumed to be the end.
        /// </summary>
        public Vector2[] Positions
        {
            get => positions;
            set
            {
                if (value.Length != maxPointCount)
                {
                    throw new ArgumentException("Array of positions was a different length than the expected result!");
                }

                positions = value;
            }
        }

        private Vector2[] positions;

        private const float defaultWidth = 16;

        public PrimitiveCircle(int maxPointCount, TrailWidthFunction trailWidthFunction, TrailColorFunction trailColorFunction)
        {
            this.maxPointCount = maxPointCount;
            this.trailWidthFunction = trailWidthFunction;
            this.trailColorFunction = trailColorFunction;

            /* A---B---C
             * |  /|  /|
             * D / E / F
             * |/  |/  |
             * G---H---I
             *
             * Let D, E, F, etc. be the set of n points that define the trail.
             * Since each point generates 2 vertices, there are 2n vertices, plus the tip's count.
             *
             * As for indices - in the region between 2 defining points there are 2 triangles.
             * The amount of regions in the whole trail are given by n - 1, so there are 2(n - 1) triangles for n points.
             * Finally, since each triangle is defined by 3 indices, there are 6(n - 1) indices, plus the tip's count.
             */

            primitives = new Primitives(Main.graphics.GraphicsDevice, VertexCount, IndexCount);
            baseEffect = new BasicEffect(Main.graphics.GraphicsDevice)
            {
                VertexColorEnabled = true,
                TextureEnabled = false
            };
        }

        private void GenerateMesh(out VertexPositionColorTexture[] vertices, out short[] indices)
        {
            VertexPositionColorTexture[] verticesTemp = new VertexPositionColorTexture[maxPointCount * 2 + 2];

            short[] indicesTemp = new short[maxPointCount * 6];

            // k = 0 indicates starting at the end of the trail (furthest from the origin of it).
            for (int k = 0; k <= Positions.Length; k++)
            {
                // 1 at k = Positions.Length - 1 (start) and 0 at k = 0 (end).
                float factorAlongTrail = (float)k / (Positions.Length);

                // Uses the trail width function to decide the width of the trail at this point (if no function, use
                float width = trailWidthFunction?.Invoke(factorAlongTrail) ?? defaultWidth;

                Vector2 current = Positions[k % positions.Length];
                Vector2 next = Positions[(k + 1) % positions.Length];

                Vector2 normalToNext = (next - current).SafeNormalize(Vector2.Zero);
                Vector2 normalPerp = normalToNext.RotatedBy(PiOver2);

                /* A
                 * |
                 * B---D
                 * |
                 * C
                 *
                 * Let B be the current point and D be the next one.
                 * A and C are calculated based on the perpendicular vector to the normal from B to D, scaled by the desired width calculated earlier.
                 */

                Vector2 a = current + (normalPerp * width);
                Vector2 c = current - (normalPerp * width);

                /* Texture coordinates are calculated such that the top-left is (0, 0) and the bottom-right is (1, 1).
                 * To achieve this, we consider the Y-coordinate of A to be 0 and that of C to be 1, while the X-coordinate is just the factor along the trail.
                 * This results in the point last in the trail having an X-coordinate of 0, and the first one having a Y-coordinate of 1.
                 */
                Vector2 texCoordA = new Vector2(factorAlongTrail, 0);
                Vector2 texCoordC = new Vector2(factorAlongTrail, 1);

                // Calculates the color for each vertex based on its texture coordinates. This acts like a very simple shader (for more complex effects you can use the actual shader).
                Color colorA = trailColorFunction?.Invoke(factorAlongTrail) ?? Color.White;
                Color colorC = trailColorFunction?.Invoke(factorAlongTrail) ?? Color.White;

                /* 0---1---2
                 * |  /|  /|
                 * A / B / C
                 * |/  |/  |
                 * 3---4---5
                 *
                 * Assuming we want vertices to be indexed in this format, where A, B, C, etc. are defining points and numbers are indices of mesh points:
                 * For a given point that is k positions along the chain, we want to find its indices.
                 * These indices are given by k for the above point and k + n for the below point.
                 */

                verticesTemp[k] = new VertexPositionColorTexture(a.Vec3(), colorA, texCoordA);
                verticesTemp[k + maxPointCount + 1] = new VertexPositionColorTexture(c.Vec3(), colorC, texCoordC);
            }

            /* Now, we have to loop through the indices to generate triangles.
             * Looping to maxPointCount - 1 brings us halfway to the end; it covers the top row (excluding the last point on the top row).
             */
            for (short k = 0; k < maxPointCount - 1; k++)
            {
                /* 0---1
                 * |  /|
                 * A / B
                 * |/  |
                 * 2---3
                 *
                 * This illustration is the most basic set of points (where n = 2).
                 * In this, we want to make triangles (2, 3, 1) and (1, 0, 2).
                 * Generalising this, if we consider A to be k = 0 and B to be k = 1, then the indices we want are going to be (k + n, k + n + 1, k + 1) and (k + 1, k, k + n)
                 */

                indicesTemp[k * 6] = (short)(k + maxPointCount + 1);
                indicesTemp[k * 6 + 1] = (short)(k + maxPointCount + 2);
                indicesTemp[k * 6 + 2] = (short)(k + 1);
                indicesTemp[k * 6 + 3] = (short)(k + 1);
                indicesTemp[k * 6 + 4] = k;
                indicesTemp[k * 6 + 5] = (short)(k + maxPointCount + 1);
            }

            //Set the final triangles to loop the strip
            indicesTemp[(maxPointCount - 1) * 6] = (short)(maxPointCount * 2); //maxPointCount - 1 + maxPointCount + 1
            indicesTemp[(maxPointCount - 1) * 6 + 1] = (short)(maxPointCount * 2 + 1); //maxPointCount - 1 + maxPointCount + 2
            indicesTemp[(maxPointCount - 1) * 6 + 2] = (short)0;
            indicesTemp[(maxPointCount - 1) * 6 + 3] = (short)0;
            indicesTemp[(maxPointCount - 1) * 6 + 4] = (short)(maxPointCount - 1);
            indicesTemp[(maxPointCount - 1) * 6 + 5] = (short)(maxPointCount * 2);//maxPointCount - 1 + maxPointCount + 1

            vertices = verticesTemp;

            // Maybe we could use an array instead of a list for the indices, if someone figures out how to add indices to an array properly.
            indices = indicesTemp;
        }

        private void SetupMeshes()
        {
            GenerateMesh(out VertexPositionColorTexture[] mainVertices, out short[] mainIndices);
            primitives.SetVertices(mainVertices);
            primitives.SetIndices(mainIndices);
        }

        public void Render(Effect effect = null, Vector2? offset = null)
        {
            Vector2 offset_ = offset.GetValueOrDefault();
            Render(effect, Matrix.CreateTranslation(offset_.Vec3()));
        }

        public void Render(Effect effect = null, Matrix? translation = null)
        {
            if (Positions == null && !(primitives?.IsDisposed ?? true))
            {
                return;
            }

            Main.instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            SetupMeshes();
            if (!translation.HasValue)
                translation = Matrix.CreateTranslation(-Main.screenPosition.Vec3());

            effect ??= baseEffect;
            primitives.Render(effect, translation.Value);
        }

        public void Dispose()
        {
            primitives?.Dispose();
        }

        public void SetPositions(Vector2 center, float radius, float offset = 0f)
        {
            Vector2[] circlePositions = new Vector2[maxPointCount];
            for (int i = 0; i < maxPointCount; i++)
            {
                circlePositions[i] = center + (i / (float)maxPointCount * TwoPi + offset).ToRotationVector2() * radius;
            }
            positions = circlePositions;
        }
    }
}
