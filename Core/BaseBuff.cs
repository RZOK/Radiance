using Terraria;
using Terraria.ModLoader;

namespace Radiance.Core
{
    public abstract class BaseBuff : ModBuff
    {
        private readonly string name;
        private readonly string tooltip;
        private readonly bool debuff;
        private readonly bool displayTime;
        
        public BaseBuff(string name, string tooltip, bool debuff, bool displayTime = true)
        {
            this.name = name;
            this.tooltip = tooltip;
            this.debuff = debuff;   
            this.displayTime = displayTime;
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault(name);
            Description.SetDefault(tooltip);
            Main.debuff[Type] = debuff;
            Main.buffNoTimeDisplay[Type] = !displayTime;
        }
    }
}
