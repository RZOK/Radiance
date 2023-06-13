namespace Radiance.Core.Interfaces
{
    public interface IGlowmaskTile
    {
        public Color glowmaskColor { get; }
        public string glowmaskTexture { get; set; }
        public bool ShouldDisplayGlowmask(int i, int j);
    }
}
