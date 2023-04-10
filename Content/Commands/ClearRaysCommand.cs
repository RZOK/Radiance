using Radiance.Core;
using Radiance.Core.Systems;
using Terraria;
using Terraria.ModLoader;

namespace Radiance.Content.Commands
{
    public class ClearRaysCommand : ModCommand
    {
        public override CommandType Type
            => CommandType.Chat;

        public override string Command
            => "clearrays";

        public override string Description
            => "Clears all currently active Radiance rays";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            Player player = Main.LocalPlayer;
            if (player.GetModPlayer<RadiancePlayer>().debugMode)
            {
                foreach (RadianceRay ray in RadianceTransferSystem.rays)
                {
                    ray.active = false;
                }
            }
        }
    }
}