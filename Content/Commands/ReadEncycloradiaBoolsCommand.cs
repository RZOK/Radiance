
using Radiance.Core;
using Terraria;
using Terraria.ModLoader;
using Radiance.Core.Systems;
using System.Reflection;
using System.Linq;

namespace Radiance.Content.Commands
{
    public class ReadEncycloradiaBoolsCommand : ModCommand
    {
        public override CommandType Type
            => CommandType.Chat;

        public override string Command
            => "readbools";

        public override string Description
            => "Prints unlock bools and their value";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            Player player = Main.LocalPlayer;
            ref bool brain = ref NPC.downedBoss1;
            Main.NewText(UnlockSystem.downedEyeOfCthulhu.unlockBoolValue.Value);
            Main.NewText(brain);
            FieldInfo[] fieldInfo = typeof(UnlockSystem).GetFields(BindingFlags.Static | BindingFlags.Public);
            //if (player.GetModPlayer<RadiancePlayer>().debugMode)
            //{
            //    foreach (var unl in fieldInfo.Where(x => x.FieldType == typeof(UnlockCondition)))
            //    {
            //        UnlockCondition cond = (UnlockCondition)unl.GetValue(typeof(UnlockCondition));
            //        Main.NewText(unl.Name.ToString() + ": " + cond.unlockBoolValue);
            //    }
            //}
        }
    }
}
