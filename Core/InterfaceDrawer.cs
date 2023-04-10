using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Tiles.Transmutator;
using Radiance.Core.Systems;
using Radiance.Utilities;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Radiance.Core
{
    public static class InterfaceDrawer
    {
        public static void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            for (int k = 0; k < layers.Count; k++)
            {
                if (layers[k].Name == "Vanilla: Interface Logic 1")
                {
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Radiance I/O Tile Display", DrawRadianceIO, InterfaceScaleType.Game));
                }
                if(layers[k].Name == "Vanilla: Town NPC House Banners")
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Hover UI Data", DrawHoverUIData, InterfaceScaleType.Game));
                if (layers[k].Name == "Vanilla: Emote Bubbles")
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Ray Display", DrawRays, InterfaceScaleType.Game));
                if (layers[k].Name == "Vanilla: Player Chat")
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Incomplete Entry Text", DrawIncompleteText, InterfaceScaleType.UI));
            }
        }
        public static bool DrawHoverUIData()
        {
            RadianceInterfacePlayer mp = Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>();
            foreach (HoverUIData data in mp.activeHoverData)
            {
                foreach (HoverUIElement element in data.elements)
                {
                    element.Update();
                    element.Draw(Main.spriteBatch);
                }
            }
            return true;
        }
        public static bool DrawRays()
        {
            Player player = Main.LocalPlayer;
            if (player.GetModPlayer<RadiancePlayer>().canSeeRays)
            {
                foreach (RadianceRay ray in RadianceTransferSystem.rays)
                {
                    ray.DrawRay();
                }
            }
            return true;
        }
        
        public static bool DrawRadianceIO()
        {
            Player player = Main.LocalPlayer;
            if (player.GetModPlayer<RadiancePlayer>().canSeeRays)
            {
                foreach (RadianceUtilizingTileEntity entity in TileEntity.ByID.Values.Where(x => x as RadianceUtilizingTileEntity != null))
                {
                    if (RadianceUtils.OnScreen(new Rectangle(entity.Position.X * 16, entity.Position.Y * 16, entity.Width * 16, entity.Height * 16)))
                    {
                        int currentPos = 0;
                        for (int x = 0; x < entity.Width * entity.Height; x++)
                        {
                            currentPos++;
                            string type = "";
                            if (entity.inputTiles.Contains(currentPos))
                                type = "Input";
                            else if (entity.outputTiles.Contains(currentPos))
                                type = "Output";

                            if (type != "")
                            {
                                Vector2 pos = new Vector2(entity.Position.X + x % entity.Width, entity.Position.Y + (float)Math.Floor((double)x / entity.Width)) * 16 + new Vector2(8, 8);
                                RadianceDrawing.DrawSoftGlow(pos, type == "Input" ? Color.Blue : Color.Red, Math.Max(0.2f * (float)Math.Abs(RadianceUtils.SineTiming(60)), 0.16f), RadianceDrawing.DrawingMode.Tile);
                                RadianceDrawing.DrawSoftGlow(pos, Color.White, Math.Max(0.15f * (float)Math.Abs(RadianceUtils.SineTiming(60)), 0.10f), RadianceDrawing.DrawingMode.Tile);
                            }
                        }
                    }
                }
            }
            return true;
        }

        public static bool DrawIncompleteText()
        {
            Player player = Main.LocalPlayer;
            string str = player.GetModPlayer<RadianceInterfacePlayer>().incompleteEntryText;
            if (str != string.Empty)
            {
                DynamicSpriteFont font = FontAssets.MouseText.Value;
                Vector2 pos = Main.MouseScreen + Vector2.One * 16;
                pos.X = Math.Min(Main.screenWidth - font.MeasureString(str).X - 6, pos.X);
                Utils.DrawBorderStringFourWay(Main.spriteBatch, font, str, pos.X, pos.Y, Color.White, Color.Black, Vector2.Zero);
            }
            return true;
        }
    }
}