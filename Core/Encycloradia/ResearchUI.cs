//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;
//using Radiance.Content.Items.BaseItems;
//using Radiance.Content.Items.ProjectorLenses;
//using Radiance.Content.Items.RadianceCells;
//using Radiance.Core.Systems;
//using Radiance.Utilities;
//using ReLogic.Graphics;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Terraria;
//using Terraria.Audio;
//using Terraria.GameContent;
//using Terraria.ID;
//using Terraria.ModLoader;
//using Terraria.UI;
//using Terraria.UI.Chat;
//using Radiance.Core.Interfaces;
//using static Radiance.Core.Encycloradia.EncycloradiaSystem;
//using static Radiance.Core.Systems.TransmutationRecipeSystem;
//using static Radiance.Core.Encycloradia.ResearchHandler;

//namespace Radiance.Core.Encycloradia
//{
//    internal class ResearchUI : SmartUIState
//    {
//        public static ResearchUI Instance { get; set; }

//        public ResearchUI()
//        {
//            Instance = this;
//        }

//        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
//        public override bool Visible => Main.LocalPlayer.chest == -1 && Main.npcShop == 0;
//        public ResearchOpenButton researchOpenButon = new();
//        public ResearchTable researchTable = new();
//        public Texture2D buttonTexture =>  ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/ResearchButton").Value;
//        public Texture2D mainTexture => ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/ResearchTable").Value;
//        public Texture2D drawerTexture => ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/ResearchTableDrawer").Value;
//        public bool tablevisible = EntryVisibility.NotVisible;

//        public override void OnInitialize()
//        {
//            researchOpenButon.Width.Set(30, 0);
//            researchOpenButon.Height.Set(30, 0);
//            Append(researchOpenButon);

//            Append(researchTable);
//        }
//        public override void Draw(SpriteBatch spriteBatch)
//        {
//            base.Draw(spriteBatch);

//            researchOpenButon.Left.Set(-132, 1);
//            researchOpenButon.Top.Set(104 + AutoUISystem.MapHeight, 0);

//            researchTable.Left.Set(-mainTexture.Width / 2, 0.5f);
//            researchTable.Top.Set(-mainTexture.Height / 2, 0.5f);
//            researchTable.Width.Set(mainTexture.Width, 0);
//            researchTable.Height.Set(mainTexture.Height, 0);

//            Recalculate();
//        }
//    }
//    internal class ResearchOpenButton : UIElement
//    {
//        internal ResearchPlayer researchPlayer => Main.LocalPlayer.GetModPlayer<ResearchPlayer>();
//        public ResearchUI UIParent => Parent as ResearchUI;

//        public override void MouseDown(UIMouseEvent evt)
//        {
//            if (Main.playerInventory)
//            {
//                if (UIParent.Visible)
//                {
//                    Main.playerInventory = false;
//                    UIParent.tableVisible = !UIParent.tableVisible;
//                }
//            }
//        }

//        protected override void DrawSelf(SpriteBatch spriteBatch)
//        {
//            if (Main.playerInventory)
//            {
//                Rectangle dimensions = GetDimensions().ToRectangle();
//                Vector2 drawPos = dimensions.TopLeft();
//                Texture2D alertTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/UnreadAlert").Value;

//                spriteBatch.Draw(UIParent.buttonTexture, drawPos, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

//                if (IsMouseHovering)
//                {
//                    DynamicSpriteFont font = FontAssets.MouseText.Value;
//                    Texture2D buttonGlowTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/ResearchButtonGlow").Value;
//                    Vector2 pos = Main.MouseScreen + Vector2.One * 16;
//                    pos.X = Math.Min(Main.screenWidth - FontAssets.MouseText.Value.MeasureString("Encycloradia").X - 6, pos.X);
//                    Utils.DrawBorderStringFourWay(spriteBatch, font, "Research", pos.X, pos.Y, Color.White, Color.Black, Vector2.Zero);
//                    Main.LocalPlayer.mouseInterface = true;
//                    spriteBatch.Draw(buttonGlowTexture, drawPos + new Vector2(-2, -2), null, Main.OurFavoriteColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
//                }
//            }
//            Recalculate();
//        }
//    }
//    internal class ResearchTable : UIElement
//    {
//        internal ResearchPlayer researchPlayer => Main.LocalPlayer.GetModPlayer<ResearchPlayer>();
//        private bool TableVisible { get => UIParent.tableVisible; set => UIParent.tableVisible = value; }
//        private static int padding = 14;
//        public ResearchUI UIParent => Parent as ResearchUI;
//        public override void Update(GameTime gameTime)
//        {
//            if (TableVisible && Main.keyState.IsKeyDown(Keys.Escape))
//                Tablevisible = EntryVisibility.NotVisible;
//        }
//        protected override void DrawSelf(SpriteBatch spriteBatch)
//        {
//            if (TableVisible)
//            {
//                Rectangle dimensions = GetDimensions().ToRectangle();
//                Vector2 drawPos = dimensions.TopLeft();
//                researchPlayer.frameOffset = drawPos + Vector2.One * padding;
//                if (ContainsPoint(Main.MouseScreen))
//                    Main.LocalPlayer.mouseInterface = true;

//                DrawTable(spriteBatch, drawPos);

//                if (researchPlayer.activeBoard != null)
//                {
//                    if (dimensions.Contains(Main.MouseScreen.ToPoint()))
//                        researchPlayer.mouseFrame = Main.MouseScreen - researchPlayer.frameOffset;

//                    DrawResearchItems(spriteBatch, drawPos + Vector2.One * padding);
//                }
//            }
//        }
//        protected void DrawTable(SpriteBatch spriteBatch, Vector2 drawPos)
//        {
//            //float scale = 1;
//            //float alpha = RadianceUtils.EaseInSine(Math.Min(bookAlpha * 1.5f, 1));
//            //float rotation = BookOpen ? 0 : (1 - RadianceUtils.EaseOutExponent(bookAlpha, 2)) * initialRotation;
//            //spriteBatch.Draw(UIParent.mainTexture, drawPos + UIParent.mainTexture.Size() / 2 + (table ? Vector2.Zero : Vector2.Lerp(Vector2.UnitX * initialOffset, Vector2.Zero, RadianceUtils.EaseOutExponent(bookAlpha, 3))), null, Color.White * alpha, rotation, UIParent.mainTexture.Size() / 2, scale, SpriteEffects.None, 0);
//            spriteBatch.Draw(UIParent.mainTexture, drawPos + UIParent.mainTexture.Size() / 2, null, Color.White, 0, UIParent.mainTexture.Size() / 2, 1, SpriteEffects.None, 0);
//            if (researchPlayer.activeResearch == null)
//            {
//                spriteBatch.Draw(ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/NotBlank").Value, drawPos, null, Color.Black * 0.6f, 0, -Vector2.One * padding / UIParent.mainTexture.Size(), UIParent.mainTexture.Size() - Vector2.One * (padding * 2 - 2), SpriteEffects.None, 0);
//                DynamicSpriteFont font = FontAssets.MouseText.Value;
//                string text = "You have no active research!";
//                string text2 = "Start research by clicking an unlocked, unresearched entry in the Encycloradia.";
//                Utils.DrawBorderStringFourWay(spriteBatch, font, text, drawPos.X + UIParent.mainTexture.Width / 2, drawPos.Y + UIParent.mainTexture.Height / 2 - 20, CommonColors.RadianceColor1, CommonColors.GetDarkColor(CommonColors.RadianceColor1, 8), font.MeasureString(text) / 2, 1.1f);
//                Utils.DrawBorderStringFourWay(spriteBatch, font, text2, drawPos.X + UIParent.mainTexture.Width / 2, drawPos.Y + UIParent.mainTexture.Height / 2 + 20, Color.White, Color.Black, font.MeasureString(text2) / 2, 0.9f);
//            }
//        }
//        protected void DrawResearchItems(SpriteBatch spriteBatch, Vector2 drawPos)
//        {
//            for (int i = 0; i < researchPlayer.activeBoard.elements.Count; i++)
//            {
//                ResearchElement element = researchPlayer.activeBoard.elements[i];
//                element.Update();
//                element.Draw(spriteBatch, drawPos);
//            }
//        }
//    }
//}