using Radiance.Core.Encycloradia;

namespace Radiance.Content.Commands
{
    public class MakeCategoryButtons : ModCommand
    {
        public override CommandType Type
            => CommandType.Chat;

        public override string Command
            => "makecat";

        public override string Description
            => "Recreates the Encycloradia category buttons";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            Player player = Main.LocalPlayer;
            if (player.GetModPlayer<RadiancePlayer>().debugMode)
            {
                EncycloradiaUI.Instance.AddCategoryButtons();
            }
        }
    }
}