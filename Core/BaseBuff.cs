using Terraria;
using Terraria.ModLoader;

namespace Radiance.Core
{
    public abstract class BaseBuff : ModBuff
    {
        private readonly string name;
        private readonly string toolTip;
        private readonly bool debuff;
        private readonly bool displayTime;

        public BaseBuff(string name, string toolTip, bool debuff, bool displayTime = true)
        {
            this.name = name;
            this.toolTip = toolTip;
            this.debuff = debuff;
            this.displayTime = displayTime;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault(name);
            Description.SetDefault(toolTip);
            Main.debuff[Type] = debuff;
            Main.buffNoTimeDisplay[Type] = !displayTime;
        }
    }
}