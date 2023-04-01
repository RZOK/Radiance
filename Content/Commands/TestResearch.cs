using Microsoft.Xna.Framework;
using Radiance.Content.EncycloradiaEntries;
using Radiance.Core;
using Radiance.Core.Encycloradia;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using static Radiance.Core.Encycloradia.ResearchHandler;

namespace Radiance.Content.Commands
{
    public class TestResearch : ModCommand
    {
        public override CommandType Type
            => CommandType.Chat;

        public override string Command
            => "testresearch";

        public override string Description
            => "Creates a new test research";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            Player player = Main.LocalPlayer;
            if (player.GetModPlayer<RadiancePlayer>().debugMode)
            {
                ResearchPlayer rplayer = player.GetModPlayer<ResearchPlayer>();

                EncycloradiaResearch research = new EncycloradiaResearch(new List<ResearchElement>()
                {
                    new StaticMirror(new Vector2(200, 200), 1),
                    new BeamSpawner(new Vector2(300, 300), 1),
                    new StaticMirror(new Vector2(300, 100), 1)
                },
                EncycloradiaSystem.FindEntry<TitleEntry>());
                
                rplayer.activeResearch = research;

                ResearchBoard board = new ResearchBoard(research);
                rplayer.activeBoard = board;
            }
        }
    }
}