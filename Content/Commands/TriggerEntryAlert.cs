using Radiance.Content.UI.NewEntryAlert;
using Radiance.Core;
using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

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
                    player.GetModPlayer<RadianceInterfacePlayer>().newEntryUnlockedTimer = NewEntryAlertUI.timerMax;
                    SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/EntryUnlock"));
                    for (int i = 0; i < 5; i++)
                    {
                        UnlockSystem.unlockedEntries.Add(new EntryAlertText(EncycloradiaSystem.entries[Main.rand.Next(EncycloradiaSystem.entries.Count)]));
                    }
                }
                else
                {
                    SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/EntryUnlock"));
                    for (int i = 0; i < 5; i++)
                    {
                        UnlockSystem.unlockedEntries.Add(new EntryAlertText(EncycloradiaSystem.entries[Main.rand.Next(EncycloradiaSystem.entries.Count)]));
                    }
                }
            }
        }
    }
}