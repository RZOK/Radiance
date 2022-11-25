using Radiance.Core;
using Radiance.Content.Items.BaseItems;
using Terraria;
using Terraria.ModLoader;

namespace Radiance.Content.Commands
{
	public class ConsumeRadianceCommand : ModCommand
	{
		public override CommandType Type
			=> CommandType.Chat;

		public override string Command
			=> "consumeradiance";

		public override string Description
			=> "Consume's X amount of Radiance from cells in the player's inventory";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			Player player = Main.LocalPlayer;
			if (player.GetModPlayer<RadiancePlayer>().debugMode)
			{
				player.GetModPlayer<RadiancePlayer>().ConsumeRadianceOnHand(float.Parse(args[0]));
            }
		}
	}
}
