using Radiance.Core;
using Radiance.Core.Encycloradia;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;

namespace Radiance.Content.Commands
{
    public class ReadEntryCommand : ModCommand
    {
        public override CommandType Type
            => CommandType.Chat;

        public override string Command
            => "readentry";

        public override string Description
            => "Read entry";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            Player player = Main.LocalPlayer;
            if (player.GetModPlayer<RadiancePlayer>().debugMode)
            {
                EncycloradiaEntry entry = EncycloradiaSystem.FindEntry("LensofPathosEntry");
                foreach (EncycloradiaPage page in entry.pages.Where(x => x.GetType() == typeof(TextPage)))
                {
                    foreach (CustomTextSnippet snip in page.text)
                    {
                        Console.WriteLine(page.number + ": " + snip.text); //text does not display properly. output: https://i.imgur.com/JsaNl95.png
                    }
                }
            }
        }
    }
}