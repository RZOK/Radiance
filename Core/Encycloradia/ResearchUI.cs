using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Content.Items.RadianceCells;
using Radiance.Core.Systems;
using Radiance.Utilities;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using Radiance.Core.Interfaces;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.TransmutationRecipeSystem;

namespace Radiance.Core.Encycloradia
{
    internal class ResearchUI : SmartUIState
    {
        public static ResearchUI Instance { get; set; }

        public ResearchUI()
        {
            Instance = this;
        }

        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

        public override bool Visible => Main.LocalPlayer.chest == -1 && Main.npcShop == 0;

        public Encycloradia encycloradia = new();
        public EncycloradiaOpenButton encycloradiaOpenButton = new();
        public Texture2D mainTexture { get => ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/Encycloradia" + (encycloradia.BookOpen ? "Main" : "Closed")).Value; }

        public bool bookVisible = false;
        public bool bookOpen = false;

        public string currentArrowInputs = String.Empty;
        public float arrowTimer = 0;
        public bool arrowHeldDown = false;

        public override void OnInitialize()
        {
            foreach (var entry in entries.Where(x => x.visible == true))
            {
                AddEntryButton(entry);
            }
            AddCategoryButtons();
            encycloradiaOpenButton.Left.Set(-85, 0);
            encycloradiaOpenButton.Top.Set(240, 0);
            encycloradiaOpenButton.Width.Set(34, 0);
            encycloradiaOpenButton.Height.Set(34, 0);
            Append(encycloradiaOpenButton);

            encycloradia.Left.Set(0, 0.5f);
            encycloradia.Top.Set(0, 0.5f);
            encycloradia.parentElements = Elements;

            Append(encycloradia);
            encycloradia.leftPage = encycloradia.currentEntry.pages.Find(n => n.number == 0);
            encycloradia.rightPage = encycloradia.currentEntry.pages.Find(n => n.number == 1);
        }
    }
}