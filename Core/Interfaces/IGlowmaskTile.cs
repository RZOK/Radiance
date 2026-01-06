namespace Radiance.Core.Interfaces
{
    public interface IGlowmaskTile
    {
        /// <summary>
        /// Gets the glowmask info based on the tile at the point in the world (i, j).
        /// </summary>
        /// <param name="i">The X coordinate of the tile in tile coords.</param>
        /// <param name="j">The Y coordinate of the tile in tile coords.</param>
        /// <param name="tex">The texture to draw.</param>
        /// <param name="color">The color to draw the texture with.</param>
        /// <returns>Whether to draw the glowmask or not.</returns>
        public bool GlowmaskInfo(int i, int j, out Texture2D tex, out Color color);
    }
}