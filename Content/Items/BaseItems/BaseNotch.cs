using Radiance.Content.Items.Tools.Misc;

namespace Radiance.Content.Items.BaseItems
{
    public abstract class BaseNotch : ModItem
    {
        public readonly Color color;

        public BaseNotch(Color color)
        {
            this.color = color;
        }
        public abstract void MirrorUse();
        public abstract int RadianceCost(LookingGlass lookingGlass, int identicalCount);
    }
}
