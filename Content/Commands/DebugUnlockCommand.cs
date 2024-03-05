using Radiance.Content.UI.NewEntryAlert;
using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.Commands
{
    public class DebugUnlockCommand : ModCommand
    {
        public override CommandType Type
            => CommandType.Chat;

        public override string Command
            => "debugunlock";

        public override string Description
            => "Toggles the debug unlock cundition.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            UnlockSystem.debugCondition = !UnlockSystem.debugCondition;
        }
    }
}