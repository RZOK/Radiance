using Radiance.Core.Systems;

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
            if (player.GetModPlayer<RadiancePlayer>().debugMode)
            {
                foreach ((var key, var value) in UnlockSystem.UnlockMethods)
                {
                    Main.NewText(key.ToString() + ": " + value.ToString());
                }
            }
        }
    }
}