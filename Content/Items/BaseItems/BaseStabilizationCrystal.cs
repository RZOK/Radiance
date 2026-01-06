namespace Radiance.Content.Items.BaseItems
{
    public abstract class BaseStabilizationCrystal : ModItem
    {
        public enum StabilizeType
        {
            Basic
        }

        public readonly string placedTexture;
        public readonly int dustID;
        public readonly int stabilizationRange;
        public readonly int stabilizationLevel;
        public readonly StabilizeType stabilizationType;
        public readonly Color crystalColor;

        public BaseStabilizationCrystal(string placedTexture, int dustID, int stabilizationRange, int stabilizationLevel, StabilizeType stabilizationType, Color crystalColor)
        {
            this.placedTexture = placedTexture;
            this.dustID = dustID;
            this.stabilizationRange = stabilizationRange;
            this.stabilizationLevel = stabilizationLevel;
            this.stabilizationType = stabilizationType;
            this.crystalColor = crystalColor;
        }
    }
}