using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
using static Radiance.Core.Encycloradia.EncycloradiaSystem;

namespace Radiance.Core.Encycloradia
{
    internal class EncycloradiaUI : SmartUIState
    {
        public static EncycloradiaUI Instance { get; set; }
        public EncycloradiaUI() 
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
            foreach(var entry in entries.Where(x => x.visible == true))
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
        public void AddCategoryButtons()
        {
            List<CategoryButton> matchingEntries = Elements.Where(x => x as CategoryButton != null).Cast<CategoryButton>().ToList();
            foreach (var button in matchingEntries)
            {
                Elements.Remove(button);
            }
            AddCategoryButton("Influencing", CommonColors.InfluencingColor, EntryCategory.Influencing, new Vector2(190, 178));
            AddCategoryButton("Transmutation", CommonColors.TransmutationColor, EntryCategory.Transmutation, new Vector2(340, 170));
            AddCategoryButton("Apparatuses", CommonColors.ApparatusesColor, EntryCategory.Apparatuses, new Vector2(210, 300));
            AddCategoryButton("Instruments", CommonColors.InstrumentsColor, EntryCategory.Instruments, new Vector2(350, 300));
            AddCategoryButton("Pedestalworks", CommonColors.PedestalworksColor, EntryCategory.Pedestalworks, new Vector2(200, 424));
            AddCategoryButton("Phenomena", CommonColors.PhenomenaColor, EntryCategory.Phenomena, new Vector2(340, 422));
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
        public void AddEntryButton(EncycloradiaEntry entry)
        {
            EntryButton button = new()
            {
                entry = entry
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
            SoundEngine.PlaySound(UIParent.bookOpen ? new SoundStyle($"{nameof(Radiance)}/Sounds/PageTurn"): new SoundStyle($"{nameof(Radiance)}/Sounds/BookClose"));
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
                    DynamicSpriteFont font = FontAssets.MouseText.Value;
                    Texture2D bookGlowTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/InventoryIconGlow").Value;
                    Vector2 pos = Main.MouseScreen + Vector2.One * 16;
                    pos.X = Math.Min(Main.screenWidth - FontAssets.MouseText.Value.MeasureString("Encycloradia").X - 6, pos.X);
                    Utils.DrawBorderStringFourWay(spriteBatch, font, "Encycloradia", pos.X, pos.Y, Color.White, Color.Black, Vector2.Zero);
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
        public bool BookOpen { get => UIParent.bookOpen; set => UIParent.bookOpen = value; }
        public bool BookVisible { get => UIParent.bookVisible; set => UIParent.bookVisible = value; }
        public List<UIElement> parentElements = new();

        public bool backBarTick = false;
        public bool openArrowTick = false;
        public bool pageArrowLeftTick = false;
        public bool pageArrowRightTick = false;
        public void GoToEntry(EncycloradiaEntry entry, bool completed = false)
        {
            currentEntry = entry;
            leftPage = entry.pages.Find(n => n.number == 0);
            rightPage = entry.pages.Find(n => n.number == 1);
            UIParent.arrowTimer = completed ? 30 : 0;
        }
        public Dictionary<Keys, string> keyDictionary = new()
        {
            { Keys.Up, "U" },
            { Keys.Right, "R" },
            { Keys.Down, "D" },
            { Keys.Left, "L" },
        };
        public List<Keys> heldKeys = new();
        public override void Update(GameTime gameTime)
        {
            if (BookVisible && Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
                BookVisible = false;
            if(BookOpen)
            {
                foreach(Keys key in keyDictionary.Keys) 
                {
                    if (Main.keyState.IsKeyDown(key))
                    {
                        if (!heldKeys.Contains(key))
                        {
                            keyDictionary.TryGetValue(key, out string value);
                            if (UIParent.currentArrowInputs.Length >= 4)
                                UIParent.currentArrowInputs = String.Empty;
                            SoundEngine.PlaySound(SoundID.MenuTick);
                            UIParent.currentArrowInputs += value;
                            UIParent.arrowTimer = 300;
                            heldKeys.Add(key);
                        }
                    }
                    else
                        heldKeys.Remove(key);
                }
                UIParent.arrowHeldDown = false;
                if(UIParent.currentArrowInputs.Length >= 4 && UIParent.arrowTimer == 300)
                {
                    EncycloradiaEntry entry = EncycloradiaSystem.FindEntryByFastNavInput(UIParent.currentArrowInputs);
                    if (entry != null)
                    {
                        SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/PageTurn"));
                        GoToEntry(entry, true);
                    }
                    else
                    {
                        UIParent.arrowTimer = 30;
                    }
                }
                if (UIParent.arrowTimer > 0) 
                    UIParent.arrowTimer--;
                if(UIParent.arrowTimer == 0)
                    UIParent.currentArrowInputs = String.Empty;
            }
            if(!BookVisible)
                UIParent.arrowTimer = 0;

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
                    DrawBackBar(spriteBatch, drawPos);

                    if (rightPage != null)
                    {
                        DrawPages(spriteBatch, drawPos + Vector2.UnitX * 346, true);
                        if (currentEntry.pages.Count - 1 > rightPage.number)
                            DrawPageArrows(spriteBatch, drawPos, true);
                    }
                    if (leftPage != null)
                    {
                        DrawPages(spriteBatch, drawPos); 
                        if (leftPage.number > 0)
                            DrawPageArrows(spriteBatch, drawPos, false);
                    }
                    if (UIParent.currentArrowInputs.Length > 0)
                        DrawFastNav(spriteBatch, drawPos);
                }
            }
        }
        protected void DrawFastNav(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D backgroundTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/QuickNavBackground").Value;
            Texture2D arrowTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/QuickNavArrow").Value;
            Texture2D softGlow = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SoftGlow").Value;
            Vector2 realDrawPos = drawPos + UIParent.mainTexture.Size() / 2 - new Vector2(120, 100);
            for (int i = 0; i < UIParent.currentArrowInputs.Length; i++)
            {
                int fadeTime = 30;
                float rotation = UIParent.currentArrowInputs[i] == char.Parse("U") ? 0 : UIParent.currentArrowInputs[i] == char.Parse("R") ? MathHelper.PiOver2 : UIParent.currentArrowInputs[i] == char.Parse("D") ? MathHelper.Pi : MathHelper.PiOver2 * 3;
                
                Main.spriteBatch.Draw(softGlow, realDrawPos + Vector2.UnitX * 80 * i, null, Color.Black * 0.25f * RadianceUtils.EaseOutCirc(Math.Min(UIParent.arrowTimer, fadeTime) / fadeTime), 0, softGlow.Size() / 2, 1.3f, 0, 0);
                spriteBatch.Draw(backgroundTexture, realDrawPos + Vector2.UnitX * 80 * i, null, Color.White * RadianceUtils.EaseOutCirc(Math.Min(UIParent.arrowTimer, fadeTime) / fadeTime), 0, backgroundTexture.Size() / 2, 1, SpriteEffects.None, 0);
                for (int d = 0; d < 2; d++)
                {
                    spriteBatch.Draw(arrowTexture, realDrawPos + (d == 0 ? Vector2.Zero : Vector2.UnitY * -2) + Vector2.UnitX * 80 * i, null, (d == 0 ? Color.Gray : Color.White) * RadianceUtils.EaseOutCirc(Math.Min(UIParent.arrowTimer, fadeTime) / fadeTime), rotation, arrowTexture.Size() / 2, 1, SpriteEffects.None, 0);
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
                if (!openArrowTick)
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    openArrowTick = true;
                }
                Texture2D arrowGlowTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/ArrowGlow").Value;
                spriteBatch.Draw(arrowGlowTexture, arrowPos - new Vector2(2, 2), null, new Color(0, 255, 255), 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/PageTurn"));
                    BookOpen = true;
                }
            }
            else
                openArrowTick = false;
        }
        protected void DrawPageArrows(SpriteBatch spriteBatch, Vector2 drawPos, bool right)
        {
            Texture2D arrowTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/PageArrow").Value;
            Vector2 arrowPos = drawPos + new Vector2(GetDimensions().Width / 2 + (right ? 318 : -318), GetDimensions().Height - 92);
            Rectangle arrowFrame = new Rectangle((int)arrowPos.X - arrowTexture.Width / 2, (int)arrowPos.Y - arrowTexture.Height / 2, arrowTexture.Width, arrowTexture.Height);
            spriteBatch.Draw(arrowTexture, arrowPos, null, Color.White, 0, arrowTexture.Size() / 2, 1, right ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            if (arrowFrame.Contains(Main.MouseScreen.ToPoint()))
            {
                if (right ? !pageArrowRightTick : !pageArrowLeftTick)
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    if (right) pageArrowRightTick = true;
                    else pageArrowLeftTick = true;
                }
                Texture2D arrowGlowTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/ArrowGlow").Value;
                spriteBatch.Draw(arrowGlowTexture, arrowPos, null, new Color(0, 255, 255), 0, arrowGlowTexture.Size() / 2, 1, right ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    if (right)
                    {
                        leftPage = currentEntry.pages.Find(n => n.number == leftPage.number + 2);
                        rightPage = currentEntry.pages.Find(n => n.number == leftPage.number + 1);
                    }
                    else
                    {
                        leftPage = currentEntry.pages.Find(n => n.number == leftPage.number - 2);
                        rightPage = currentEntry.pages.Find(n => n.number == leftPage.number + 1);
                    }
                    SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/PageTurn"));
                }
            }
            else
            {
                if (right) pageArrowRightTick = false;
                else pageArrowLeftTick = false;
            }
        }
        protected void DrawBackBar(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D barGlowTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/EncycloradiaBarGlow").Value;
            Vector2 barPos = drawPos + Vector2.UnitX * (GetDimensions().Width / 2 - 9);
            Rectangle barFrame = new Rectangle((int)barPos.X, (int)barPos.Y, 14, (int)GetDimensions().Height);
            if (barFrame.Contains(Main.MouseScreen.ToPoint()))
            {
                if (!backBarTick)
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    backBarTick = true;
                }
                spriteBatch.Draw(barGlowTexture, barPos - new Vector2(2, 2), null, Main.OurFavoriteColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    if (currentEntry == EncycloradiaSystem.FindEntry("TitleEntry"))
                    {
                        SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/BookClose"));
                        BookOpen = false;
                        return;
                    }
                    else
                        SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/PageTurn"));
                    if (currentEntry.visible)
                        GoToEntry(EncycloradiaSystem.FindEntry(currentEntry.category.ToString() + "Entry"));
                    else
                        GoToEntry(EncycloradiaSystem.FindEntry("TitleEntry"));
                    leftPage = currentEntry.pages.Find(n => n.number == 0);
                    rightPage = currentEntry.pages.Find(n => n.number == 1);
                }
            }
            else
                backBarTick = false;
        }

        protected void DrawPages(SpriteBatch spriteBatch, Vector2 drawPos, bool right = false)
        {
            EncycloradiaPage page = right ? rightPage : leftPage;
            DynamicSpriteFont font = FontAssets.MouseText.Value;

            if(page.GetType() == typeof(CategoryPage))
            {
                CategoryPage categoryPage = page as CategoryPage;
                List<EntryButton> matchingEntries = parentElements.Where(x => x as EntryButton != null).Cast<EntryButton>().ToList();
                matchingEntries.RemoveAll(x => x.entry.category != categoryPage.category);
                List<EntryButton> matchingEntriesSorted = matchingEntries.OrderBy(x => x.entry.displayName).ToList();
                foreach (var entry in matchingEntriesSorted)
                {
                    entry.DrawStuff(spriteBatch, drawPos + new Vector2(74, 64 + 32 * (matchingEntriesSorted.IndexOf(entry))));
                }
            }
            if (page.GetType() == typeof(MiscPage))
            {
                MiscPage miscPage = page as MiscPage;
                switch (miscPage.type)
                {
                    case "Title":
                        leftPage = currentEntry.pages.Find(n => n.number == 0);
                        rightPage = currentEntry.pages.Find(n => n.number == 1);
                        foreach (CategoryButton x in parentElements.Where(n => n.GetType() == typeof(CategoryButton)))
                            x.DrawStuff(spriteBatch, drawPos);
                        break;
                }
            }
            if (page.GetType() == typeof(TextPage))
            {
                float xDrawOffset = 0;
                float yDrawOffset = 0;
                if (page.text != null)
                {
                    for (int h = 0; h < page.text.Length; h++)
                    {
                        CustomTextSnippet ts = page.text[h];
                        string[] words = ts.text.Split(' ');
                        for (int i = 0; i < words.Length; i++)
                        {
                            string word = words[i];
                            if (word == "|")
                            {
                                xDrawOffset = 0;
                                yDrawOffset += 24;
                                continue;
                            }
                            Utils.DrawBorderStringFourWay(spriteBatch, font, word, drawPos.X + xDrawOffset + 61 - (right ? 0 : (yDrawOffset / 23)), drawPos.Y + yDrawOffset + 56, ts.color, ts.backgroundColor, Vector2.Zero, Radiance.encycolradiaLineScale);
                            xDrawOffset += font.MeasureString(word + (i == words.Length - 1 ? "" : " ")).X * Radiance.encycolradiaLineScale;
                        }
                    }
                }
            }
            if(page.GetType() == typeof(RecipePage))
            {
                RecipePage recipePage = page as RecipePage;
                Vector2 pos = drawPos + new Vector2(distanceBetweenPages / 2 + 30, UIParent.mainTexture.Height / 2);
                Texture2D overlayTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/CraftingOverlay").Value;
                Texture2D softGlow = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SoftGlow").Value;

                spriteBatch.Draw(overlayTexture, pos, null, Color.White, 0, overlayTexture.Size() / 2, 1, SpriteEffects.None, 0);

                Main.spriteBatch.Draw(softGlow, pos - Vector2.UnitY * 95, null, Color.Black * 0.3f, 0, softGlow.Size() / 2, (float)(Item.GetDrawHitbox(recipePage.station.type, null).Width + Item.GetDrawHitbox(recipePage.station.type, null).Height) / 100, 0, 0);
                RadianceDrawing.DrawHoverableItem(spriteBatch, recipePage.station.type, pos - Vector2.UnitY * 95, 1); //station

                Main.spriteBatch.Draw(softGlow, pos + Vector2.UnitY * 95, null, Color.Black * 0.3f, 0, softGlow.Size() / 2, (float)(Item.GetDrawHitbox(recipePage.result.Item1.type, null).Width + Item.GetDrawHitbox(recipePage.result.Item1.type, null).Height) / 100, 0, 0);
                RadianceDrawing.DrawHoverableItem(spriteBatch, recipePage.result.Item1.type, pos + Vector2.UnitY * 95, recipePage.result.Item2); //result

                float longestItem = 0;
                foreach (int item in recipePage.items.Keys)
                {
                    if ((Item.GetDrawHitbox(item, null).Width + Item.GetDrawHitbox(item, null).Height) / 2 > longestItem)
                        longestItem = (Item.GetDrawHitbox(item, null).Width + Item.GetDrawHitbox(item, null).Height) / 2;
                }
                foreach (int item in recipePage.items.Keys)
                {
                    double deg = (float)Main.GameUpdateCount / 5 + 360 / recipePage.items.Keys.Count * recipePage.items.Keys.ToList().IndexOf(item);
                    double rad = MathHelper.ToRadians((float)deg);
                    double dist = Math.Min(longestItem / 2 + 40, longestItem + 24);
                    Vector2 pos2 = pos - new Vector2((int)(Math.Cos(rad) * dist), (int)(Math.Sin(rad) * dist)) - Vector2.UnitY * 95;

                    Main.spriteBatch.Draw(softGlow, pos2, null, Color.Black * 0.25f, 0, softGlow.Size() / 2, (float)(Item.GetDrawHitbox(item, null).Width + Item.GetDrawHitbox(item, null).Height) / 100, 0, 0);

                    recipePage.items.TryGetValue(item, out int value); 
                    RadianceDrawing.DrawHoverableItem(spriteBatch, item, pos2, value);
                }
            }
        }
    }

    internal class CategoryButton : UIElement
    {
        public EncycloradiaUI UIParent => Parent as EncycloradiaUI;
        public string texture = "MissingCategory";
        public Color color = Color.White;
        public Color realColor = Color.White;
        public EntryCategory category = EntryCategory.None;
        public Vector2 pos = Vector2.Zero;
        public Vector2 drawPos = Vector2.Zero;
        public float visualsTimer = 0;
        public bool tick = false;
        private Vector2 size { get => ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/" + texture + "Symbol").Size(); }

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
            if (!UIParent.bookVisible || UIParent.encycloradia.currentEntry != EncycloradiaSystem.FindEntry("TitleEntry")) visualsTimer = 0;
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
                if(!tick)
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    tick = true;
                }
                if (visualsTimer < maxVisualTimer)
                    visualsTimer++;
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/PageTurn"));
                    UIParent.encycloradia.GoToEntry(EncycloradiaSystem.FindEntry(texture + "Entry"));
                }
            }
            else
            {
                tick = false;
                if (visualsTimer > 0)
                    visualsTimer--;
            }
            if (visualsTimer > 0)
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
    internal class EntryButton : UIElement
    {
        public EncycloradiaUI UIParent => Parent as EncycloradiaUI;
        public EncycloradiaEntry entry = EncycloradiaSystem.FindEntry("TitleEntry");
        public Vector2 pos = Vector2.Zero;
        public float visualsTimer = 0;
        public bool hovering = false;
        public bool tick = false;
        public enum EntryStatus
        {
            Locked,
            Incomplete,
            Unlocked
        }
        public EntryStatus entryStatus { get => UnlockSystem.UnlockMethods.GetValueOrDefault(entry.unlock) ? EntryStatus.Unlocked : UnlockSystem.UnlockMethods.GetValueOrDefault(entry.incomplete) ? EntryStatus.Incomplete : EntryStatus.Locked; }
        public override void Update(GameTime gameTime)
        {
            if (visualsTimer > 0 && !hovering)
                visualsTimer--;
            hovering = false;
        }
        public void DrawStuff(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            DynamicSpriteFont font = FontAssets.MouseText.Value;
            int maxVisualTimer = 10;

            Left.Set(drawPos.X, 0);
            Top.Set(drawPos.Y, 0);
            Width.Set(font.MeasureString(entry.displayName).X, 0);
            Height.Set(font.MeasureString(entry.displayName).Y, 0);
            Main.instance.LoadItem(entry.icon);
            Texture2D tex = entryStatus == EntryStatus.Locked ? ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/LockIcon").Value : TextureAssets.Item[entry.icon].Value;
            Color color = entryStatus == EntryStatus.Unlocked ? new Color(255, 255, 255, 255) : entryStatus == EntryStatus.Incomplete ? new Color(180, 180, 180, 255) : new Color(110, 110, 110, 255);
            string text = entryStatus == EntryStatus.Unlocked ? entry.displayName : entryStatus == EntryStatus.Incomplete ? "Incomplete Entry" : "Locked";

            float scale = 1f;
            if (tex.Size().X > 32 || tex.Size().Y > 32)
            {
                if (tex.Size().X > tex.Size().Y)
                    scale = 32f / tex.Size().X;
                else
                    scale = 32f / tex.Size().Y;
            }
            Vector2 scaledTexSized = tex.Size() * scale;

            Rectangle frame = new Rectangle((int)(drawPos.X - scaledTexSized.X / 2), (int)(drawPos.Y - scaledTexSized.Y / 2), (int)scaledTexSized.X + (int)font.MeasureString(text).X + 44, (int)scaledTexSized.Y);
            if (frame.Contains(Main.MouseScreen.ToPoint()))
            {
                hovering = true;
                switch (entryStatus)
                {
                    case EntryStatus.Unlocked:
                        if (!tick)
                        {
                            SoundEngine.PlaySound(SoundID.MenuTick);
                            tick = true;
                        }
                        if (visualsTimer < maxVisualTimer)
                            visualsTimer++;
                        break;
                    case EntryStatus.Incomplete:
                        Player player = Main.LocalPlayer;
                        if(!UnlockSystem.IncompleteText.ContainsKey(entry.unlock))
                        {
                            player.GetModPlayer<RadianceInterfacePlayer>().incompleteEntryText = "You should not be seeing this text!";
                            break;
                        }
                        UnlockSystem.IncompleteText.TryGetValue(entry.unlock, out string value);
                        player.GetModPlayer<RadianceInterfacePlayer>().incompleteEntryText = "Unlock this entry by " + value;
                        break;

                }
                if (entryStatus == EntryStatus.Unlocked)
                {
                    if (Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/PageTurn"));
                        UIParent.encycloradia.GoToEntry(entry);
                    }
                }
            }
            else
                tick = false;

            float timing = RadianceUtils.EaseInOutQuart(Math.Clamp(visualsTimer / (maxVisualTimer * 2) + 0.5f, 0.5f, 1));

            if(visualsTimer > 0)
            {
                RadianceDrawing.DrawSoftGlow(Main.screenPosition + drawPos, Color.White * (visualsTimer / maxVisualTimer), 0.24f, RadianceDrawing.DrawingMode.UI);
                RadianceDrawing.DrawBeam(Main.screenPosition + drawPos , Main.screenPosition + drawPos + Vector2.UnitX * 300, Color.White.ToVector4() * visualsTimer / maxVisualTimer, 0.3f, 24, RadianceDrawing.DrawingMode.UI, true);
            }
            Utils.DrawBorderStringFourWay(spriteBatch, font, text, drawPos.X + scaledTexSized.X / 2 + 4 * Math.Clamp(timing * 2, 1, 2f), drawPos.Y + 4, color, Color.Lerp(Color.Black, CommonColors.RadianceColor1, timing - 0.5f), Vector2.UnitY * font.MeasureString(text).Y / 2);
            spriteBatch.Draw(tex, new Vector2(drawPos.X, drawPos.Y), null, entryStatus == EntryStatus.Incomplete ? Color.Black : Color.White, 0, tex.Size() / 2, Math.Clamp(timing + 0.2f, 1, 1.2f), SpriteEffects.None, 0);

            Recalculate();
        }
    }
}