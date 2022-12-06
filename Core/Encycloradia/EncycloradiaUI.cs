using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;

namespace Radiance.Core.Encycloradia
{
    internal class EncycloradiaUI : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

        public override bool Visible => Main.LocalPlayer.chest == -1 && Main.npcShop == 0;

        public Encycloradia encycloradia = new();
        public EncycloradiaOpenButton encycloradiaOpenButton = new();

        public Texture2D mainTexture { get => ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/Encycloradia" + (encycloradia.BookOpen ? "Main" : "Closed")).Value; }
        //public Texture2D pageArrowTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/PageArrow").Value;

        public bool bookVisible = false;
        public bool bookOpen = false;

        public override void OnInitialize()
        {
            encycloradiaOpenButton.Left.Set(-85, 1);
            encycloradiaOpenButton.Top.Set(240, 0);
            encycloradiaOpenButton.Width.Set(34, 0);
            encycloradiaOpenButton.Height.Set(34, 0);
            Append(encycloradiaOpenButton);

            encycloradia.Left.Set(0, 0.5f);
            encycloradia.Top.Set(0, 0.5f);

            Append(encycloradia);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            encycloradiaOpenButton.Left.Set(-85, 1);
            encycloradiaOpenButton.Top.Set(100 + AutoUISystem.MapHeight, 0);

            encycloradia.Left.Set(-mainTexture.Width / 2, 0.5f);
            encycloradia.Top.Set(-mainTexture.Height / 2, 0.5f);
            encycloradia.Width.Set(mainTexture.Width, 0);
            encycloradia.Height.Set(mainTexture.Height, 0);

            Recalculate();
        }
    }

    internal class EncycloradiaOpenButton : UIElement
    {
        public EncycloradiaUI UIParent => Parent as EncycloradiaUI;

        public override void MouseDown(UIMouseEvent evt)
        {
            UIParent.bookVisible = !UIParent.bookVisible;
            Main.playerInventory = false;
            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (Main.playerInventory)
            {
                Rectangle dimensions = GetDimensions().ToRectangle();
                Vector2 drawPos = dimensions.TopLeft();
                Texture2D bookTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/InventoryIcon").Value;

                spriteBatch.Draw(bookTexture, drawPos, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                if (IsMouseHovering)
                {
                    Texture2D bookGlowTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/InventoryIconGlow").Value;
                    Vector2 pos = Main.MouseScreen + Vector2.One * 16;
                    pos.X = Math.Min(Main.screenWidth - FontAssets.MouseText.Value.MeasureString("Encycloradia").X - 6, pos.X);
                    Utils.DrawBorderString(spriteBatch, "Encycloradia", pos, Color.White);
                    Main.LocalPlayer.mouseInterface = true;
                    spriteBatch.Draw(bookGlowTexture, drawPos + new Vector2(-2, -2), null, Main.OurFavoriteColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                }
            }
            Recalculate();
        }
    }

    internal class Encycloradia : UIElement
    {
        public EncycloradiaUI UIParent => Parent as EncycloradiaUI;

        public EncycloradiaEntry currentEntry = EncycloradiaSystem.FindEntry("TestEntry");
        public EncycloradiaPage leftPage = new MiscPage();
        public EncycloradiaPage rightPage = new MiscPage();

        public Vector2 LeftPageCenter { get => Vector2.UnitX * GetDimensions().Width / 4; }
        public Vector2 RightPageCenter { get => Vector2.UnitX * GetDimensions().Width * 3 / 4; }
        public bool BookOpen { get => UIParent.bookOpen; set => UIParent.bookOpen = value; }
        public bool BookVisible { get => UIParent.bookVisible; set => UIParent.bookVisible = value; }

        public override void Update(GameTime gameTime)
        {
            if (BookVisible && Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                BookOpen = false;
                BookVisible = false;
            }
            base.Update(gameTime);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (BookVisible)
            {
                if (ContainsPoint(Main.MouseScreen))
                    Main.LocalPlayer.mouseInterface = true;

                Rectangle dimensions = GetDimensions().ToRectangle();
                Vector2 drawPos = dimensions.TopLeft();
                DrawBook(spriteBatch, drawPos);
                if (!BookOpen)
                    DrawOpenArrow(spriteBatch, drawPos);
                else
                    DrawBookElements(spriteBatch, drawPos);
            }
        }
        protected void DrawBook(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            spriteBatch.Draw(UIParent.mainTexture, drawPos, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        }
        protected void DrawOpenArrow(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D arrowTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/UIArrow").Value;
            Vector2 arrowPos = drawPos + new Vector2(GetDimensions().Width / 2, GetDimensions().Height - 30) - arrowTexture.Size() / 2;
            Rectangle arrowFrame = new Rectangle((int)arrowPos.X, (int)arrowPos.Y, arrowTexture.Width, arrowTexture.Height);
            spriteBatch.Draw(arrowTexture, arrowPos, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            if (arrowFrame.Contains(Main.MouseScreen.ToPoint()))
            {
                Texture2D arrowGlowTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/ArrowGlow").Value;
                spriteBatch.Draw(arrowGlowTexture, arrowPos - new Vector2(2, 2), null, Main.OurFavoriteColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    BookOpen = true;
                }
            }
        }
        protected void DrawBookElements(SpriteBatch spriteBatch, Vector2 drawPos)
        {

        }
    }
    internal class EntryButton : UIElement
    {
        public Asset<Texture2D> texture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/MissingEntryIcon");
        public Color color;
        public 
    }
}