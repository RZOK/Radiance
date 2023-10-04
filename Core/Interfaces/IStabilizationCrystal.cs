namespace Radiance.Core.Interfaces
{
    public enum StabilizeType
    {
        Basic
    }
    public interface IStabilizationCrystal
    {
        public string PlacedTexture { get; }
        public int DustID { get; }
        public int StabilizationRange { get; }
        public int StabilizationLevel { get; }
        public StabilizeType StabilizationType { get; }
        public Color CrystalColor { get; }
    }
}
