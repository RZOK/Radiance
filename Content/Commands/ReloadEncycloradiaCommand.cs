using Radiance.Core.Encycloradia;

namespace Radiance.Content.Commands
{
    public class ReloadEncycloradiaCommand : ModCommand
    {
        public override string Command => "reloadencyclo";
        public override string Description => "Reloads the Encycloradia and its entries.";
        public override CommandType Type => CommandType.Chat;

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (Main.LocalPlayer.GetModPlayer<RadiancePlayer>().debugMode)
                EncycloradiaSystem.ReloadEncycloradia();
        }
    }
}