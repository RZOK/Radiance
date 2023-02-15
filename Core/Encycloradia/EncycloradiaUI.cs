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
using System.Linq.Expressions;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.TransmutationRecipeSystem;

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
            SoundEngine.PlaySound(UIParent.bookOpen ? new SoundStyle($"{nameof(Radiance)}/Sounds/PageTurn") : new SoundStyle($"{nameof(Radiance)}/Sounds/BookClose"));
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

        public Color drawnColor = Color.White;
        public Color drawnBGColor = Color.Black;

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
            if (BookOpen)
            {
                foreach (Keys key in keyDictionary.Keys)
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
                if (UIParent.currentArrowInputs.Length >= 4 && UIParent.arrowTimer == 300)
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
                if (UIParent.arrowTimer == 0)
                    UIParent.currentArrowInputs = String.Empty;
            }
            if (!BookVisible)
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

                    if (currentEntry.visible)
                        DrawEntryIcon(spriteBatch, drawPos + new Vector2(dimensions.Width / 4 + 19, 41));
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

        protected void DrawEntryIcon(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D iconItem = TextureAssets.Item[currentEntry.icon].Value;
            Texture2D iconTex = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/EntryIcon").Value;
            Texture2D iconBGTex = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/EntryIconBackground").Value;
            Texture2D softGlow = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SoftGlow").Value;
            Vector2 itemSize = new Vector2(iconItem.Width, iconItem.Height);
            Vector2 iconSize = new Vector2(iconTex.Width, iconTex.Height);
            Vector2 bgSize = new Vector2(iconBGTex.Width, iconBGTex.Height);

            const int padding = 12;
            int width = iconItem.Width + padding;
            int height = iconItem.Height + padding - 4;
            Rectangle rect = new Rectangle(iconTex.Width / 2 - (int)itemSize.X - padding, iconTex.Height - (int)itemSize.Y - padding, width, height);
            Rectangle bgRect = new Rectangle(iconTex.Width / 2 - (int)itemSize.X - padding - 4, iconTex.Height - (int)itemSize.Y - padding, width + 4, height + 1);
            rect.X = (iconTex.Width - rect.Width) / 2;
            rect.Y = iconTex.Height - rect.Height;
            bgRect.X = (iconBGTex.Width - bgRect.Width) / 2;
            bgRect.Y = iconBGTex.Height - bgRect.Height;
            Vector2 itemOrigin = new Vector2(itemSize.X / 2, itemSize.Y);
            Vector2 itemPos = drawPos - Vector2.UnitY * 4;

            spriteBatch.Draw(iconBGTex, drawPos - Vector2.UnitY, bgRect, Color.White, 0, new Vector2(bgRect.Width / 2, bgRect.Height), 1, SpriteEffects.None, 0);
            spriteBatch.Draw(iconTex, drawPos, rect, Color.White, 0, new Vector2(rect.Width / 2, rect.Height), 1, SpriteEffects.None, 0);
            spriteBatch.Draw(softGlow, itemPos - Vector2.UnitY * itemSize.Y / 2, null, Color.Black * 0.25f, 0, softGlow.Size() / 2, itemSize.Length() / 100, 0, 0);
            spriteBatch.Draw(iconItem, itemPos, null, Color.White, 0, itemOrigin, 1, SpriteEffects.None, 0);

            Rectangle itemRect = new Rectangle((int)(itemPos.X - itemOrigin.X), (int)(itemPos.Y - itemOrigin.Y), (int)itemSize.X, (int)itemSize.Y);

            if (itemRect.Contains(Main.MouseScreen.ToPoint()))
            {
                float boxWidth;
                float boxHeight = -16;
                Vector2 pos = Main.MouseScreen + new Vector2(4, 28);
                Dictionary<string, string> arrows = new Dictionary<string, string>()
                {
                    { "U", "↑" },
                    { "R", "→" },
                    { "D", "↓" },
                    { "L", "←" },
                };
                var font = FontAssets.MouseText.Value;
                string fastNavInput = arrows.Aggregate(currentEntry.fastNavInput, (current, value) => current.Replace(value.Key, value.Value));
                string[] iconString =
                {
                    "[c/FFC042:" + currentEntry.displayName + "]",
                    "'" + currentEntry.tooltip + "'",
                    "Entry",
                    fastNavInput,
                };
                if (Main.MouseScreen.X < Main.screenWidth / 2)
                {
                    var widest = iconString.OrderBy(n => ChatManager.GetStringSize(font, n, Vector2.One).X).Last();
                    boxWidth = ChatManager.GetStringSize(font, widest, Vector2.One).X;
                    pos += new Vector2(30, 0);
                }
                else
                {
                    var widest = iconString.OrderBy(n => ChatManager.GetStringSize(font, n, Vector2.One).X).Last();
                    boxWidth = ChatManager.GetStringSize(font, widest, Vector2.One).X;
                    pos -= new Vector2(30, 0);
                }

                string widest2 = iconString.OrderBy(n => ChatManager.GetStringSize(font, n, Vector2.One).X).Last();
                boxWidth = ChatManager.GetStringSize(font, widest2, Vector2.One).X + 20;

                foreach (string str in iconString)
                {
                    boxHeight += ChatManager.GetStringSize(font, str, Vector2.One).Y;
                }

                Utils.DrawInvBG(spriteBatch, new Rectangle((int)pos.X - 14, (int)pos.Y - 10, (int)boxWidth + 6, (int)boxHeight + 28), new Color(23, 25, 81, 255) * 0.925f);

                foreach (string str in iconString)
                {
                    if (arrows.ContainsValue(str[0].ToString()) && str.Length == 4)
                        Utils.DrawBorderStringFourWay(spriteBatch, font, str, pos.X, pos.Y, new Color(63, 222, 177), new Color(21, 90, 121), Vector2.Zero);
                    else
                        Utils.DrawBorderString(spriteBatch, str, pos, Color.White);
                    pos.Y += ChatManager.GetStringSize(font, str, Vector2.One).Y;
                }
            }
        }

        protected void DrawPages(SpriteBatch spriteBatch, Vector2 drawPos, bool right = false)
        {
            EncycloradiaPage page = right ? rightPage : leftPage;
            DynamicSpriteFont font = FontAssets.MouseText.Value;

            if (page.GetType() == typeof(CategoryPage))
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
                    foreach (string word in page.text.Split())
                    {
                        if (word == "|")
                        {
                            xDrawOffset = 0;
                            yDrawOffset += 24;
                            continue;
                        }
                        if(word.StartsWith(@"\"))
                        {
                            switch(word[1].ToString().ToLower())
                            {
                                case "y": //radiance yellow
                                    drawnColor = CommonColors.RadianceColor1;
                                    drawnBGColor = CommonColors.RadianceColor1.GetDarkColor();
                                    break;
                                case "b": //context blue
                                    drawnColor = CommonColors.ContextColor;
                                    drawnBGColor = CommonColors.ContextColor.GetDarkColor();
                                    break;
                                case "g": //locked gray
                                    drawnColor = CommonColors.LockedColor;
                                    drawnBGColor = CommonColors.LockedColor.GetDarkColor();
                                    break;
                                case "i": //influencing red
                                    drawnColor = CommonColors.InfluencingColor;
                                    drawnBGColor = CommonColors.InfluencingColor.GetDarkColor();
                                    break;
                                case "t": //transmutation lime
                                    drawnColor = CommonColors.TransmutationColor;
                                    drawnBGColor = CommonColors.TransmutationColor.GetDarkColor();
                                    break;
                                case "a": //apparatuses blue
                                    drawnColor = CommonColors.ApparatusesColor;
                                    drawnBGColor = CommonColors.ApparatusesColor.GetDarkColor();
                                    break;
                                case "n": //instruments orange
                                    drawnColor = CommonColors.InstrumentsColor;
                                    drawnBGColor = CommonColors.InstrumentsColor.GetDarkColor();
                                    break;
                                case "d": //pedestalworks purple
                                    drawnColor = CommonColors.PedestalworksColor;
                                    drawnBGColor = CommonColors.PedestalworksColor.GetDarkColor();
                                    break;
                                case "h": //phenomena teal
                                    drawnColor = CommonColors.PhenomenaColor;
                                    drawnBGColor = CommonColors.PhenomenaColor.GetDarkColor();
                                    break;
                                case "1": //scarlet
                                    drawnColor = CommonColors.ScarletColor;
                                    drawnBGColor = CommonColors.ScarletColor.GetDarkColor();
                                    break;
                                case "2": //cerulean
                                    drawnColor = CommonColors.CeruleanColor;
                                    drawnBGColor = CommonColors.CeruleanColor.GetDarkColor();
                                    break;
                                case "3": //verdant
                                    drawnColor = CommonColors.VerdantColor;
                                    drawnBGColor = CommonColors.VerdantColor.GetDarkColor();
                                    break;
                                case "4": //mauve
                                    drawnColor = CommonColors.MauveColor;
                                    drawnBGColor = CommonColors.MauveColor.GetDarkColor();
                                    break;
                                case "r": //reset
                                    drawnColor = Color.White;
                                    drawnBGColor = Color.Black;
                                    break;
                            }
                            continue;
                        }
                        Utils.DrawBorderStringFourWay(spriteBatch, font, word, drawPos.X + xDrawOffset + 61 - (right ? 0 : (yDrawOffset / 23)), drawPos.Y + yDrawOffset + 56, drawnColor, drawnBGColor, Vector2.Zero, Radiance.encycolradiaLineScale);
                        xDrawOffset += font.MeasureString(word + " ").X * Radiance.encycolradiaLineScale;
                    }
                }
            }
            if (page.GetType() == typeof(RecipePage))
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
                    float deg = (float)Main.GameUpdateCount / 5 + 360 / recipePage.items.Keys.Count * recipePage.items.Keys.ToList().IndexOf(item);
                    Vector2 pos2 = pos - Vector2.UnitY * 95 + (Vector2.UnitX * Math.Min(longestItem / 2 + 40, longestItem + 24)).RotatedBy(MathHelper.ToRadians(deg));

                    Main.spriteBatch.Draw(softGlow, pos2, null, Color.Black * 0.25f, 0, softGlow.Size() / 2, (float)(Item.GetDrawHitbox(item, null).Width + Item.GetDrawHitbox(item, null).Height) / 100, 0, 0);

                    recipePage.items.TryGetValue(item, out int value);
                    RadianceDrawing.DrawHoverableItem(spriteBatch, item, pos2, value);
                }
            }
            if (page.GetType() == typeof(TransmutationPage))
            {
                TransmutationPage transmutationPage = page as TransmutationPage;
                Vector2 pos = drawPos + new Vector2(distanceBetweenPages / 2 + 30, UIParent.mainTexture.Height / 2 - 20);
                Texture2D overlayTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/TransmutationOverlay").Value;
                Texture2D softGlow = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SoftGlow").Value;

                spriteBatch.Draw(overlayTexture, pos, null, Color.White, 0, overlayTexture.Size() / 2, 1, SpriteEffects.None, 0);

                List<int> items = new List<int>() { transmutationPage.recipe.inputItem };
                if(transmutationPage.recipe.id.EndsWith("_0"))
                {
                    int counter = 1;
                    do
                    {
                        items.Add(TransmutationRecipeSystem.FindRecipe(transmutationPage.recipe.id.TrimEnd('0') + counter.ToString()).inputItem);
                        counter++;
                    }
                    while (TransmutationRecipeSystem.FindRecipe(transmutationPage.recipe.id.TrimEnd('0') + counter.ToString()) != null);
                }
                int currentItem = items[transmutationPage.currentItemIndex];
                if (Main.GameUpdateCount % 90 == 0)
                {
                    transmutationPage.currentItemIndex++;
                    if(transmutationPage.currentItemIndex >= items.Count)
                    {
                        transmutationPage.currentItemIndex = 0;
                    }
                }

                Vector2 itemPos = pos - new Vector2(42, 82);
                Main.spriteBatch.Draw(softGlow, itemPos, null, Color.Black * 0.3f, 0, softGlow.Size() / 2, (float)(Item.GetDrawHitbox(currentItem, null).Width + Item.GetDrawHitbox(currentItem, null).Height) / 100, 0, 0);
                RadianceDrawing.DrawHoverableItem(spriteBatch, currentItem, itemPos, transmutationPage.recipe.inputStack); //station

                Vector2 resultPos = pos + new Vector2(-42, 108);
                Main.spriteBatch.Draw(softGlow, resultPos, null, Color.Black * 0.3f, 0, softGlow.Size() / 2, (float)(Item.GetDrawHitbox(transmutationPage.recipe.outputItem, null).Width + Item.GetDrawHitbox(transmutationPage.recipe.outputItem, null).Height) / 100, 0, 0);
                RadianceDrawing.DrawHoverableItem(spriteBatch, transmutationPage.recipe.outputItem, resultPos, transmutationPage.recipe.outputStack); //result

                int cell = ModContent.ItemType<StandardRadianceCell>();
                if (transmutationPage.recipe.requiredRadiance > 4000)
                {
                    cell = ModContent.ItemType<StandardRadianceCell>();
                }
                BaseContainer cellContainer = new Item(cell).ModItem as BaseContainer;

                Vector2 cellPos = pos + new Vector2(55, 51);
                RadianceDrawing.DrawHoverableItem(spriteBatch, cell, cellPos, 1); //cell

                #region Required Radiance

                float maxRadiance = cellContainer.MaxRadiance;
                float currentRadiance = transmutationPage.recipe.requiredRadiance;

                Texture2D barTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/TransmutationOverlayBar").Value;

                float radianceCharge = Math.Min(currentRadiance, maxRadiance);
                float fill = radianceCharge / maxRadiance;

                Vector2 barPos = pos + new Vector2(56, -75);

                Main.spriteBatch.Draw(
                    barTexture,
                    barPos,
                    new Rectangle(0, 0, barTexture.Width, (int)(Math.Max(0.05f, Math.Round(fill, 2)) * barTexture.Height)),
                    CommonColors.RadianceColor1,
                    MathHelper.Pi,
                    new Vector2(barTexture.Width / 2, barTexture.Height / 2),
                    1,
                    SpriteEffects.None,
                    0);

                Rectangle rect = new Rectangle((int)barPos.X - barTexture.Width / 2, (int)barPos.Y - barTexture.Height / 2, barTexture.Width, barTexture.Height);
                if (rect.Contains(Main.MouseScreen.ToPoint()))
                {
                    Vector2 textPos = Main.MouseScreen + Vector2.One * 16;
                    string str = "This recipe uses Radiance worth " + (fill < 0.005 ? "less than 0.5% " : ("about " + fill * 100 + "% ")) + "of the listed cell's total capacity";
                    textPos.X = Math.Min(Main.screenWidth - FontAssets.MouseText.Value.MeasureString(str).X - 6, textPos.X);
                    Utils.DrawBorderStringFourWay(spriteBatch, font, str, textPos.X, textPos.Y, Color.White, Color.Black, Vector2.Zero);
                }
                #endregion Required Radiance



                #region Requirements

                int conditionCount = 0;
                if (transmutationPage.recipe.specialRequirements != null)
                {
                    foreach (SpecialRequirements req in transmutationPage.recipe.specialRequirements)
                    {
                        conditionCount++;
                    }
                }
                Vector2 conditionPos = pos + new Vector2(56, 142);
                Utils.DrawBorderStringFourWay(spriteBatch, font, conditionCount.ToString(), conditionPos.X, conditionPos.Y, Color.White, Color.Black, font.MeasureString(conditionCount.ToString()) / 2);

                const int padding = 8;
                Rectangle conditionRect = new Rectangle((int)(conditionPos.X - (font.MeasureString(conditionCount.ToString()).X + padding) / 2), (int)(conditionPos.Y - (font.MeasureString(conditionCount.ToString()).Y + padding) / 2), (int)font.MeasureString(conditionCount.ToString()).X + padding, (int)font.MeasureString(conditionCount.ToString()).Y + padding);
                if (conditionRect.Contains(Main.MouseScreen.ToPoint()))
                {
                    Vector2 textPos = Main.MouseScreen + Vector2.One * 16;
                    string str = "This recipe has " + (conditionCount == 0 ? "no special requirements" : (conditionCount + " special requirement" + (conditionCount != 1 ? "" : " ") + "that must be met"));
                    textPos.X = Math.Min(Main.screenWidth - FontAssets.MouseText.Value.MeasureString(str).X - 6, textPos.X);
                    Utils.DrawBorderStringFourWay(spriteBatch, font, str, textPos.X, textPos.Y, Color.White, Color.Black, Vector2.Zero);
                    if (conditionCount > 0)
                    {
                        foreach (SpecialRequirements req in transmutationPage.recipe.specialRequirements)
                        {
                            const int distance = 24;
                            textPos.Y += distance;
                            textPos.X = Math.Min(Main.screenWidth - FontAssets.MouseText.Value.MeasureString("— " + TransmutationRecipeSystem.reqStrings[req]).X - 6, textPos.X);
                            Utils.DrawBorderStringFourWay(spriteBatch, font, "— " + TransmutationRecipeSystem.reqStrings[req], textPos.X, textPos.Y, Color.White, Color.Black, Vector2.Zero);
                        }
                    }
                }

                #endregion Requirements

                #region Lens

                int lens = ModContent.ItemType<ShimmeringGlass>();
                switch (transmutationPage.recipe.lensRequired)
                {
                    case ProjectorLensID.Pathos:
                        lens = ModContent.ItemType<LensofPathos>();
                        break;
                }
                Vector2 lensPos = pos - new Vector2(42, -9);
                RadianceDrawing.DrawHoverableItem(spriteBatch, lens, lensPos, 1); //lens

                #endregion Lens
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
        private Vector2 size => ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/" + texture + "Symbol").Size();

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
                if (!tick)
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
                        if (!UnlockSystem.IncompleteText.ContainsKey(entry.unlock))
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

            if (visualsTimer > 0)
            {
                RadianceDrawing.DrawSoftGlow(Main.screenPosition + drawPos, Color.White * (visualsTimer / maxVisualTimer), 0.24f, RadianceDrawing.DrawingMode.UI);
                RadianceDrawing.DrawBeam(Main.screenPosition + drawPos, Main.screenPosition + drawPos + Vector2.UnitX * 300, Color.White.ToVector4() * visualsTimer / maxVisualTimer, 0.3f, 24, RadianceDrawing.DrawingMode.UI, true);
            }
            Utils.DrawBorderStringFourWay(spriteBatch, font, text, drawPos.X + scaledTexSized.X / 2 + 4 * Math.Clamp(timing * 2, 1, 2f), drawPos.Y + 4, color, Color.Lerp(Color.Black, CommonColors.RadianceColor1, timing - 0.5f), Vector2.UnitY * font.MeasureString(text).Y / 2);
            spriteBatch.Draw(tex, new Vector2(drawPos.X, drawPos.Y), null, entryStatus == EntryStatus.Incomplete ? Color.Black : Color.White, 0, tex.Size() / 2, scale * Math.Clamp(timing + 0.2f, 1, 1.2f), SpriteEffects.None, 0);

            Recalculate();
        }
    }
}