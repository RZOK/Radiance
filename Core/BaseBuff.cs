using Terraria;
using Terraria.ModLoader;

namespace Radiance.Core
{
    public abstract class BaseBuff : ModBuff
    {
        private readonly bool debuff;
        private readonly bool displayTime;

        public BaseBuff(bool debuff, bool displayTime = true)
        {
            this.debuff = debuff;
            this.displayTime = displayTime;
        }

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = debuff;
            Main.buffNoTimeDisplay[Type] = !displayTime;
        }
    }
}