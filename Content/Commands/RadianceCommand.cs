using Microsoft.Xna.Framework;
using Radiance.Content.Items.BaseItems;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Radiance.Content.Commands
{
	public class RadianceCommand : ModCommand
	{
		public override CommandType Type
			=> CommandType.Chat;

		public override string Command
			=> "setradiance";

		public override string Description
			=> "Sets a held item's current Radiance to the value";

		public override void Action(CommandCaller caller, string input, string[] args) {
			Player player = Main.LocalPlayer;
			BaseContainer container = player.inventory[player.selectedItem].ModItem as BaseContainer;
			if (container != null) container.CurrentRadiance = float.Parse(args[0]);
		}
	}
}
