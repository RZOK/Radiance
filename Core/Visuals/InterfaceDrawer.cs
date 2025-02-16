using Radiance.Core.Config;
using Radiance.Core.Systems;
using ReLogic.Graphics;
using Terraria.UI;

namespace Radiance.Core.Visuals
{
    public class InterfaceDrawer : ModSystem
    {
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            for (int k = 0; k < layers.Count; k++)
            {
                if (layers[k].Name == "Vanilla: Interface Logic 1")
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Radiance I/O Tile Display", DrawRadianceIO, InterfaceScaleType.Game));
                if (layers[k].Name == "Vanilla: MP Player Names")
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Hover UI Data", DrawHoverUIData, InterfaceScaleType.Game));
                if (layers[k].Name == "Vanilla: Emote Bubbles")
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Ray Display", DrawRays, InterfaceScaleType.Game));
                if (layers[k].Name == "Vanilla: Player Chat")
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Incomplete Entry Text", DrawIncompleteText, InterfaceScaleType.UI));
                if (layers[k].Name == "Vanilla: Interface Logic 4")
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Fake Mouse Text", DrawFakeMouseText, InterfaceScaleType.UI));
            }
        }
        public bool DrawFakeMouseText()
        {
            RadianceInterfacePlayer mp = Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>();
            if(mp.currentFakeHoverText != string.Empty && Main.mouseItem.IsAir)
            {
                DrawFakeItemHover(Main.spriteBatch, mp.currentFakeHoverText.Split("\n"), fancy: mp.fancyHoverTextBackground);
            }
            return true;
        }

        public bool DrawHoverUIData()
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

        public bool DrawRays()
        {
            Player player = Main.LocalPlayer;
            if (player.GetModPlayer<RadiancePlayer>().canSeeRays)
            {
                foreach (RadianceRay ray in RadianceTransferSystem.rays)
                {
                    ray.DrawRay();
                }
                Main.spriteBatch.GetSpritebatchDetails(out SpriteSortMode spriteSortMode, out BlendState blendState, out SamplerState samplerState, out DepthStencilState depthStencilState, out RasterizerState rasterizerState, out Effect effect, out Matrix matrix);
                Main.spriteBatch.End();

                Effect circleEffect = Terraria.Graphics.Effects.Filters.Scene["HorizEdgeSoften"].GetShader().Shader;
                circleEffect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/RayTiling2").Value);
                circleEffect.Parameters["fadeThreshold"].SetValue(0.92f);
                circleEffect.Parameters["color"].SetValue(new Color(247, 136, 125).ToVector4() * 0.5f);

                Main.spriteBatch.Begin(spriteSortMode, BlendState.Additive, samplerState, depthStencilState, rasterizerState, circleEffect, matrix);
                foreach (RadianceRay ray in RadianceTransferSystem.rays)
                {
                    if ((ray.hasIoAtEnds[0] && ray.hasIoAtEnds[1]) || (ray.hasIoAtEnds[2] && ray.hasIoAtEnds[3]))
                        ray.DrawRayOverlay();
                }
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(spriteSortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, matrix);
            }
            return true;
        }
        public enum RadianceIOIndicatorMode
        {
            None,
            Input,
            Output
        }
        public static bool DrawRadianceIO()
        {
            Player player = Main.LocalPlayer;
            if (player.GetModPlayer<RadiancePlayer>().canSeeRays)
            {
                foreach (RadianceUtilizingTileEntity entity in TileEntity.ByID.Values.Where(x => x as RadianceUtilizingTileEntity != null))
                {
                    if (OnScreen(new Rectangle(entity.Position.X * 16, entity.Position.Y * 16, entity.Width * 16, entity.Height * 16)))
                    {
                        int currentPos = 0;
                        for (int x = 0; x < entity.Width * entity.Height; x++)
                        {
                            currentPos++;
                            RadianceIOIndicatorMode type = RadianceIOIndicatorMode.None;
                            if (entity.inputTiles.Contains(currentPos))
                                type = RadianceIOIndicatorMode.Input;
                            else if (entity.outputTiles.Contains(currentPos))
                                type = RadianceIOIndicatorMode.Output;

                            if (type != RadianceIOIndicatorMode.None)
                            {
                                Vector2 pos = new Vector2(entity.Position.X + x % entity.Width, entity.Position.Y + (x - x % entity.Width) / entity.Width) * 16 + Vector2.One * 8;
                                RadianceDrawing.DrawRadianceIOSlot(type, pos);
                            }
                        }
                    }
                }
            }
            return true;
        }

        public bool DrawIncompleteText()
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