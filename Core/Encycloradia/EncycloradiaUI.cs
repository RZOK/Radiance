using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.EncycloradiaEntries;
using Radiance.Core.Systems;
using Radiance.Utilities;
using rail;
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
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

        public override bool Visible => Main.LocalPlayer.chest == -1 && Main.npcShop == 0;

        public Encycloradia encycloradia = new();
        public EncycloradiaOpenButton encycloradiaOpenButton = new();

        public Texture2D mainTexture { get => ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/Encycloradia" + (encycloradia.BookOpen ? "Main" : "Closed")).Value; }

        public bool bookVisible = false;
        public bool bookOpen = false;

        public override void OnInitialize()
        {
            AddCategoryButton("Influencing", RadianceUtils.InfluencingColor, EntryCategory.Influencing, new Vector2(190, 170));
            AddCategoryButton("Transmutation", RadianceUtils.TransmutationColor, EntryCategory.Transmutation, new Vector2(310, 170));
            AddCategoryButton("Apparatuses", RadianceUtils.ApparatusesColor, EntryCategory.Apparatuses, new Vector2(190, 290));
            AddCategoryButton("Instruments", RadianceUtils.InstrumentsColor, EntryCategory.Instruments, new Vector2(310, 290));
            AddCategoryButton("Pedestalworks", RadianceUtils.PedestalworksColor, EntryCategory.Pedestalworks, new Vector2(190, 410));
            AddCategoryButton("Phenomena", RadianceUtils.PhenomenaColor, EntryCategory.Phenomena, new Vector2(310, 410));
            
            foreach(var entry in entries.Where(x => x.visible == true))
            {
                AddEntryButton(entry);
            }

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
            SoundEngine.PlaySound(SoundID.MenuOpen);
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

        public bool backBarTick = false;
        public bool openArrowTick = false;
        public bool pageArrowLeftTick = false;
        public bool pageArrowRightTick = false;

        public override void Update(GameTime gameTime)
        {
            if (BookVisible && Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
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
                    DrawBackBar(spriteBatch, drawPos);
                    DrawPages(spriteBatch, drawPos); //left
                    DrawPages(spriteBatch, drawPos + Vector2.UnitX * 350, true); //right
                    if(currentEntry.pages.Count - 1 > rightPage.number)
                        DrawPageArrows(spriteBatch, drawPos, true);
                    if(leftPage.number > 0)
                        DrawPageArrows(spriteBatch, drawPos, false);
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
            Vector2 arrowPos = drawPos + new Vector2(GetDimensions().Width / 2 + (right ? 320 : -320), GetDimensions().Height - 90);
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
                spriteBatch.Draw(arrowGlowTexture, arrowPos + new Vector2(-2, -2), null, new Color(0, 255, 255), 0, arrowTexture.Size() / 2, 1, right ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    if (right)
                    {
                        leftPage = currentEntry.pages.Find(n => n.number == leftPage.number + 2);
                        rightPage = currentEntry.pages.Find(n => n.number == rightPage.number + 2) == null ? new MiscPage() { number = leftPage.number + 1 } : currentEntry.pages.Find(n => n.number == rightPage.number + 2);
                    }
                    else
                    {
                        leftPage = currentEntry.pages.Find(n => n.number == leftPage.number - 2);
                        rightPage = currentEntry.pages.Find(n => n.number == rightPage.number - 2);
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
                    SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/PageTurn"));
                    if (currentEntry == EncycloradiaSystem.FindEntry("TitleEntry"))
                    {
                        BookOpen = false;
                        return;
                    }
                    if (currentEntry.visible) 
                        currentEntry = EncycloradiaSystem.FindEntry(currentEntry.category.ToString() + "Entry");
                    else
                        currentEntry = EncycloradiaSystem.FindEntry("TitleEntry");
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
                List<EntryButton> matchingEntriesSorted = matchingEntries.OrderBy(x => x.displayName).ToList();
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
                string line = String.Empty;
                if (page.text != null)
                {
                    foreach (CustomTextSnippet ts in page.text)
                    {
                        float gap = 190;
                        string[] words = ts.text.Split();
                        foreach (string word in words)
                        {
                            if (word == "NEWLINE")
                            {
                                gap += 1;
                                xDrawOffset = 0;
                                yDrawOffset += 24;
                                line = default;
                                continue;
                            }
                            line += word;
                            Utils.DrawBorderStringFourWay(spriteBatch, font, word, drawPos.X + xDrawOffset + 61 - (right ? 0 : (yDrawOffset / 23)), drawPos.Y + yDrawOffset + 52, ts.color, ts.backgroundColor, Vector2.Zero, 1);
                            if (font.MeasureString(line).X > gap)
                            {
                                gap += 1;
                                line = default;
                                yDrawOffset += 24;
                                xDrawOffset = 0;
                            }
                            if (line != null)
                                xDrawOffset += font.MeasureString(word + (Array.IndexOf(words, word) == words.Length ? "" : " ")).X;
                        }
                    }
                }
            }
            if(page.GetType() == typeof(RecipePage))
            {
                RecipePage recipePage = page as RecipePage;
                Vector2 pos = drawPos + new Vector2(distanceBetweenPages / 2 + 30, UIParent.mainTexture.Height / 2);
                Texture2D overlayTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/CraftingOverlay").Value;
                Main.instance.LoadItem(recipePage.station.type);
                Main.instance.LoadItem(recipePage.result.type);

                spriteBatch.Draw(overlayTexture, pos, null, Color.White, 0, overlayTexture.Size() / 2, 1, SpriteEffects.None, 0);
                spriteBatch.Draw(TextureAssets.Item[recipePage.station.type].Value, pos - Vector2.UnitY * 95, null, Color.White, 0, TextureAssets.Item[recipePage.station.type].Size() / 2, 1, SpriteEffects.None, 0);
                spriteBatch.Draw(TextureAssets.Item[recipePage.result.type].Value, pos + Vector2.UnitY * 95, null, Color.White, 0, TextureAssets.Item[recipePage.result.type].Size() / 2, 1, SpriteEffects.None, 0);
                float longestItem = 0;
                foreach (int item in recipePage.items.Keys)
                {
                    if ((Item.GetDrawHitbox(item, null).Width + Item.GetDrawHitbox(item, null).Height) / 2 > longestItem)
                        longestItem = (Item.GetDrawHitbox(item, null).Width + Item.GetDrawHitbox(item, null).Height) / 2;
                }
                foreach (int item in recipePage.items.Keys)
                {
                    Main.instance.LoadItem(item);
                    double deg = (float)Main.GameUpdateCount / 5 + 360 / recipePage.items.Keys.Count * recipePage.items.Keys.ToList().IndexOf(item);
                    double rad = MathHelper.ToRadians((float)deg);
                    double dist = longestItem + 24;
                    Vector2 pos2 = pos - new Vector2(-(int)(Math.Cos(rad) * dist), -(int)(Math.Sin(rad) * dist));
                    RadianceDrawing.DrawSoftGlow(Main.screenPosition + pos2 - Vector2.UnitY * 95, new Color(255, 255, 255, 50), (float)(Item.GetDrawHitbox(item, null).Width + Item.GetDrawHitbox(item, null).Height) / 100, Main.UIScaleMatrix);
                    spriteBatch.Draw(TextureAssets.Item[item].Value, pos2 - Vector2.UnitY * 95, new Rectangle?(Item.GetDrawHitbox(item, null)), Color.White, 0, new Vector2(Item.GetDrawHitbox(item, null).Width, Item.GetDrawHitbox(item, null).Height) / 2, 1, SpriteEffects.None, 0);
                    recipePage.items.TryGetValue(item, out int value);
                    if (value > 1)
                        Utils.DrawBorderStringFourWay(Main.spriteBatch, font, value.ToString(), pos2.X - TextureAssets.Item[recipePage.result.type].Size().X / 2, pos2.Y - 95, Color.White, Color.Black, Vector2.Zero);
                }
                if(recipePage.result.stack > 1)
                    Utils.DrawBorderStringFourWay(Main.spriteBatch, font, recipePage.result.stack.ToString(), pos.X - TextureAssets.Item[recipePage.result.type].Size().X / 2, pos.Y + TextureAssets.Item[recipePage.result.type].Size().Y / 4 + 90, Color.White, Color.Black, Vector2.Zero);
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
                    UIParent.encycloradia.currentEntry = EncycloradiaSystem.FindEntry(texture + "Entry");
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
        public string displayName { get => entry.displayName; }
        public enum EntryStatus
        {
            Locked,
            Incomplete,
            Unlocked
        }
        public EntryStatus entryStatus { get => UnlockSystem.UnlockMethods.GetValueOrDefault(entry.unlock) == true ? EntryStatus.Unlocked : UnlockSystem.UnlockMethods.GetValueOrDefault(entry.incomplete) == true ? EntryStatus.Incomplete : EntryStatus.Locked; }
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
            Width.Set(font.MeasureString(displayName).X, 0);
            Height.Set(font.MeasureString(displayName).Y, 0);
            Main.instance.LoadItem(entry.icon);
            Texture2D tex = entryStatus == EntryStatus.Locked ? ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/LockIcon").Value : TextureAssets.Item[entry.icon].Value;
            Color color = entryStatus == EntryStatus.Unlocked ? new Color(255, 255, 255, 255) : entryStatus == EntryStatus.Incomplete ? new Color(180, 180, 180, 255) : new Color(110, 110, 110, 255);
            string text = entryStatus == EntryStatus.Unlocked ? displayName : entryStatus == EntryStatus.Incomplete ? "Incomplete Entry" : "Locked";


            float scale = 1f;
            if (tex.Size().X > 32 || tex.Size().Y > 32)
            {
                if (tex.Size().X > tex.Size().Y)
                    scale = 32f / tex.Size().X;
                else
                    scale = 32f / tex.Size().Y;
            }
            Vector2 scaledTexSized = tex.Size() * scale;

            Rectangle frame = new Rectangle((int)(drawPos.X - scaledTexSized.X / 2), (int)(drawPos.Y - scaledTexSized.Y / 2), (int)scaledTexSized.X + (int)font.MeasureString(text).X + 4, (int)scaledTexSized.Y);
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

                        Vector2 pos = Main.MouseScreen + Vector2.One * 16;
                        pos.X = Math.Min(Main.screenWidth - FontAssets.MouseText.Value.MeasureString("Incomplete Text").X - 6, pos.X);
                        Utils.DrawBorderStringFourWay(spriteBatch, font, "Incomplete Text", pos.X, pos.Y, Color.White, Color.Black, Vector2.Zero); //todo: move this to another layer
                        break;

                }
                if (entryStatus == EntryStatus.Unlocked)
                {
                    if (Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/PageTurn"));
                        UIParent.encycloradia.currentEntry = entry;
                        UIParent.encycloradia.leftPage = entry.pages.Find(n => n.number == 0);
                        UIParent.encycloradia.rightPage = entry.pages.Find(n => n.number == 1);
                    }
                }
            }
            else
                tick = false;

            float timing = RadianceUtils.EaseInOutQuart(Math.Clamp(visualsTimer / (maxVisualTimer * 2) + 0.5f, 0.5f, 1));

            if(visualsTimer > 0)
            {
                RadianceDrawing.DrawSoftGlow(Main.screenPosition + drawPos, Color.White * (visualsTimer / maxVisualTimer), 0.24f, Main.UIScaleMatrix);
                RadianceDrawing.DrawBeam(Main.screenPosition + drawPos , Main.screenPosition + drawPos + Vector2.UnitX * 300, Color.White.ToVector4() * visualsTimer / maxVisualTimer, 0.3f, 24, Main.UIScaleMatrix, true);
            }
            Utils.DrawBorderStringFourWay(spriteBatch, font, text, drawPos.X + scaledTexSized.X / 2 + 4 * Math.Clamp(timing * 2, 1, 2f), drawPos.Y + 4, color, Color.Lerp(Color.Black, RadianceUtils.RadianceColor1, timing - 0.5f), Vector2.UnitY * font.MeasureString(text).Y / 2);
            spriteBatch.Draw(tex, new Vector2(drawPos.X, drawPos.Y), null, entryStatus == EntryStatus.Incomplete ? Color.Black : Color.White, 0, tex.Size() / 2, Math.Clamp(timing + 0.2f, 1, 1.2f), SpriteEffects.None, 0);

            Recalculate();
        }
    }
}