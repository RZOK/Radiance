using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Utilities;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Xml.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
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
            AddCategoryButton("Influencing", new Color(255, 0, 103), EntryCategory.Influencing, new Vector2(190, 170));
            AddCategoryButton("Transmutation", new Color(103, 255, 0), EntryCategory.Transmutation, new Vector2(310, 170));
            AddCategoryButton("Apparatuses", new Color(0, 103, 255), EntryCategory.Apparatuses, new Vector2(190, 290));
            AddCategoryButton("Instruments", new Color(255, 103, 0), EntryCategory.Instrument, new Vector2(310, 290));
            AddCategoryButton("Pedestalworks", new Color(103, 0, 255), EntryCategory.Pedestalwork, new Vector2(190, 410));
            AddCategoryButton("Phenomena", new Color(0, 255, 103), EntryCategory.Phenomena, new Vector2(310, 410));

            encycloradiaOpenButton.Left.Set(-85, 0);
            encycloradiaOpenButton.Top.Set(240, 0);
            encycloradiaOpenButton.Width.Set(34, 0);
            encycloradiaOpenButton.Height.Set(34, 0);
            Append(encycloradiaOpenButton);

            encycloradia.Left.Set(0, 0.5f);
            encycloradia.Top.Set(0, 0.5f);
            encycloradia.parentElements = Elements;

            Append(encycloradia);
        }
        public void AddCategoryButton(string texture, Color color, EntryCategory category, Vector2 pos)
        {
            CategoryButton button = new()
            {
                texture = texture,
                color = color,
                category = category,
                pos = pos,
            };
            Append(button);
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

        public EncycloradiaEntry currentEntry = EncycloradiaSystem.FindEntry("TitleEntry");
        public EncycloradiaPage leftPage = new MiscPage();
        public EncycloradiaPage rightPage = new MiscPage();
        public const int distanceBetweenPages = 350;
        public Vector2 LeftPageCenter { get => Vector2.UnitX * GetDimensions().Width / 4; }
        public Vector2 RightPageCenter { get => Vector2.UnitX * GetDimensions().Width * 3 / 4; }
        public bool BookOpen { get => UIParent.bookOpen; set => UIParent.bookOpen = value; }
        public bool BookVisible { get => UIParent.bookVisible; set => UIParent.bookVisible = value; }
        public List<UIElement> parentElements = new();

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
                {
                    leftPage = currentEntry.pages.Find(n => n.number == 0);
                    rightPage = currentEntry.pages.Find(n => n.number == 1);
                    DrawPages(spriteBatch, drawPos); //left
                    DrawPages(spriteBatch, drawPos += Vector2.UnitX * 350, true); //right
                }
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
        protected void DrawPages(SpriteBatch spriteBatch, Vector2 drawPos, bool right = false)
        {
            EncycloradiaPage page = right ? rightPage : leftPage;
            DynamicSpriteFont font = FontAssets.MouseText.Value;

            if (currentEntry == EncycloradiaSystem.FindEntry("TitleEntry"))
            {
                if (right)
                {
                    foreach (CategoryButton x in parentElements.Where(n => n.GetType() == typeof(CategoryButton)))
                        x.DrawStuff(spriteBatch, drawPos);
                }
            }
            if(page.GetType() == typeof(TextPage))
            {
                float xDrawOffset = 0;
                float yDrawOffset = 0;
                string line = String.Empty;
                foreach (CustomTextSnippet ts in page.text)
                {
                    string[] words = ts.text.Split();
                    foreach(string word in words)
                    {
                        line += word;
                        Utils.DrawBorderStringFourWay(spriteBatch, font, word, drawPos.X + xDrawOffset + 68, drawPos.Y + yDrawOffset + 70, ts.color, ts.backgroundColor, Vector2.Zero, 0.9f);
                        if (font.MeasureString(line).X > 180)
                        {
                            line = default;
                            yDrawOffset += 22;
                            xDrawOffset = 0;
                        }
                        if (line != null)
                            xDrawOffset += font.MeasureString(word + " ").X;
                    }
                }
            }
        }
    }
    internal class CategoryButton : UIElement
    {
        public EncycloradiaUI UIParent => Parent as EncycloradiaUI;
        public string texture = "MissingEntry";
        public Color color = Color.White;
        public Color realColor = Color.White;
        public EntryCategory category = EntryCategory.None;
        public Vector2 pos = Vector2.Zero;
        public Vector2 drawPos = Vector2.Zero;
        public float visualsTimer = 0;
        private Vector2 size { get => ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/" + texture + "Symbol").Value.Size(); }
        public override void Draw(SpriteBatch spriteBatch)
        {
            CalculatedStyle dims = UIParent.encycloradia.GetDimensions();
            Left.Set(dims.X + pos.X, 0);
            Top.Set(dims.Y + pos.Y, 0);
            Width.Set(size.X, 0);
            Height.Set(size.Y, 0);

            Recalculate();
        }
        public override void Update(GameTime gameTime)
        {
            if (!UIParent.bookVisible) visualsTimer = 0;
            base.Update(gameTime);
        }
        public void DrawStuff(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            int maxVisualTimer = 10;
            Texture2D tex = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/" + texture + "Symbol").Value;
            drawPos += pos;
            drawPos -= size / 2;
            Rectangle frame = new Rectangle((int)(drawPos.X - size.X / 2), (int)(drawPos.Y - size.Y / 2), (int)size.X, (int)size.Y);
            float timing = RadianceUtils.EaseInOutQuart(Math.Clamp(visualsTimer / (maxVisualTimer * 2) + 0.5f, 0.5f, 1));
            realColor = color * timing;
            spriteBatch.Draw(tex, drawPos, null, realColor, 0, size / 2, Math.Clamp(timing + 0.3f, 1, 1.3f), SpriteEffects.None, 0);
            if (frame.Contains(Main.MouseScreen.ToPoint()))
            {
                if (visualsTimer < maxVisualTimer)
                    visualsTimer++;
            }
            else if (visualsTimer > 0)
                visualsTimer--;
            if(visualsTimer > 0)
            {
                DynamicSpriteFont font = FontAssets.MouseText.Value;
                Utils.DrawBorderStringFourWay(
                    Main.spriteBatch,
                    font,
                    texture,
                    drawPos.X,
                    drawPos.Y,
                    realColor * timing * 2f,
                    Color.Black * timing,
                    font.MeasureString(texture) / 2,
                    timing);
            }
        }
    }
}