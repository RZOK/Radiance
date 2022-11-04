using Microsoft.Xna.Framework;
using Radiance.Core.Systems;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Radiance.Content.Commands
{
	internal class AddInputOutput : ModCommand
	{
		public override CommandType Type
			=> CommandType.Chat;

		public override string Command
			=> "addio";

		public override string Description
			=> "Sets a held item's current Radiance to the value";

		public override void Action(CommandCaller caller, string input, string[] args) {
			Player player = Main.LocalPlayer;
            RadianceTransferSystem.Instance.AddInputOutput(player.Center, player.Center - new Vector2(50, 50));
		}
	}
}
