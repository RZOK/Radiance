using Radiance.Content.EncycloradiaEntries;
using Radiance.Core.Encycloradia;
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
            EncycloradiaSystem.EncycloradiaEntries = new List<EncycloradiaEntry>();
            EncycloradiaSystem.Load();
            EncycloradiaSystem.AssembleEntries();
            EncycloradiaSystem.RebuildCategoryPages();
            Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().currentEntry = EncycloradiaSystem.FindEntry<TitleEntry>();
        }
    }
}