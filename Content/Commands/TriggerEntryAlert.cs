using Radiance.Core;
using Radiance.Content.Items.BaseItems;
using Terraria;
using Terraria.ModLoader;
using Radiance.Core.Encycloradia;
using Radiance.Content.UI.NewEntryAlert;
using Terraria.Audio;

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
			//if (player.GetModPlayer<RadiancePlayer>().debugMode)
			//{
				//if (player.GetModPlayer<RadianceInterfacePlayer>().newEntryUnlockedTimer == 0)
				//{
				//	player.GetModPlayer<RadianceInterfacePlayer>().newEntryUnlockedTimer = NewEntryAlertUI.timerMax;
    //                SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/EntryUnlock"));
    //                for (int i = 0; i < 5; i++)
    //                {
    //                    NewEntryAlertUI.Instance.easeTimer = NewEntryAlertUI.easeTimerMax;
    //                    NewEntryAlertUI.unlockedEntries.Add(new EntryAlertText(EncycloradiaSystem.entries[Main.rand.Next(EncycloradiaSystem.entries.Count)]));
    //                }

    //            }
				//else
    //            {
    //                SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/EntryUnlock"));
    //                for (int i = 0; i < 5; i++)
				//	{
				//		NewEntryAlertUI.Instance.easeTimer = NewEntryAlertUI.easeTimerMax;
    //                    unlocksy.Add(new EntryAlertText(EncycloradiaSystem.entries[Main.rand.Next(EncycloradiaSystem.entries.Count)]));
    //                }
				//}
            //}
		}
	}
}
