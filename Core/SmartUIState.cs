using Terraria.UI;

namespace Radiance.Core
{
    //robbed from slr
    public abstract class SmartUIState : UIState
    {
        public abstract int InsertionIndex(List<GameInterfaceLayer> layers);

        public virtual bool Visible { get; set; } = false;

        public virtual InterfaceScaleType Scale { get; set; } = InterfaceScaleType.UI;

        public virtual void Unload()
        { }
    }
}