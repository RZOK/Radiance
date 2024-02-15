using Radiance.Content.UI.NewEntryAlert;
using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;

namespace Radiance.Content.Commands
{
    public class TriggerEntryAlert : ModCommand
    {
        public override CommandType Type
            => CommandType.Chat;

        public override string Command
            => "makealert";

        public override string Description
            => "Triggers the 'New Entries' Encycloradia alert";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            Player player = Main.LocalPlayer;
            if (player.GetModPlayer<RadiancePlayer>().debugMode)
            {
                if (player.GetModPlayer<RadianceInterfacePlayer>().newEntryUnlockedTimer == 0)
                {
                    player.GetModPlayer<RadianceInterfacePlayer>().newEntryUnlockedTimer = NewEntryAlertUI.NEW_ENTRY_ALERT_UI_TIMER_MAX;
                    SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/EntryUnlock"));
                    for (int i = 0; i < 5; i++)
                    {
                        UnlockSystem.unlockedEntries.Add(new EntryAlertText(EncycloradiaSystem.EncycloradiaEntries[Main.rand.Next(EncycloradiaSystem.EncycloradiaEntries.Count)]));
                    }
                }
                else
                {
                    SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/EntryUnlock"));
                    for (int i = 0; i < 5; i++)
                    {
                        UnlockSystem.unlockedEntries.Add(new EntryAlertText(EncycloradiaSystem.EncycloradiaEntries[Main.rand.Next(EncycloradiaSystem.EncycloradiaEntries.Count)]));
                    }
                }
            }
        }
    }
}