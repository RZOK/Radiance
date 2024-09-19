using Microsoft.CodeAnalysis.Operations;
using Microsoft.Xna.Framework.Input;
using Radiance.Content.EncycloradiaEntries;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Content.Items.RadianceCells;
using Radiance.Core.Loaders;
using Radiance.Core.Systems;
using ReLogic.Graphics;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;
using static Radiance.Core.Encycloradia.CategoryPage;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.TransmutationRecipeSystem;

namespace Radiance.Core.Encycloradia
{
    public class EncycloradiaUI : SmartUIState
    {
        public static EncycloradiaUI Instance { get; set; }
        public static SoundStyle pageTurnSound;
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        public override bool Visible => true;

        public Encycloradia encycloradia = new();
        public EncycloradiaOpenButton encycloradiaOpenButton = new();
        public Texture2D MainTexture => ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/Encycloradia" + (encycloradia.BookOpen ? "Main" : "Closed")).Value;

        public bool bookVisible = false;
        public bool bookOpen = false;

        public const float ADJUSTMENT_FOR_SLANTED_TEXTURE = 0.33f;
        public const int ENTRY_BUTTON_MAX_ENTRY_BUTTON_VISUAL_TIMER = 10;
        public const char PARSE_CHARACTER = '&';

        public const int MAX_PIXELS_PER_LINE = 300;
        public static float MAX_PIXELS_PER_LINE_ADJUSTED => PIXELS_BETWEEN_LINES / LINE_SCALE;
        public const float PIXELS_BETWEEN_LINES = 24;
        public const int MAX_LINES_PER_PAGE = 15;
        public const float LINE_SCALE = 0.9f;

        public const int ENTRIES_PER_CATEGORY_PAGE = 13;
        public const int PIXELS_BETWEEN_ENTRY_BUTTONS = 28;

        public const int PIXELS_FROM_CENTER_TO_PAGE_ARROWS = 306;
        public const int PIXELS_BETWEEN_PAGES = 350;
        public const int PIXELS_PADDING_FOR_ENTRY_ICON = 12;

        public const string LOCALIZATION_PREFIX = $"Mods.{nameof(Radiance)}.Encycloradia";

        public EncycloradiaUI() => Instance = this;
        public void Load()
        {
            pageTurnSound = new SoundStyle($"{nameof(Radiance)}/Sounds/PageTurn");

            encycloradiaOpenButton.Left.Set(-85, 0);
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

            encycloradia.Left.Set(-MainTexture.Width / 2, 0.5f);
            encycloradia.Top.Set(-MainTexture.Height / 2, 0.5f);
            encycloradia.Width.Set(MainTexture.Width, 0);
            encycloradia.Height.Set(MainTexture.Height, 0);

            Recalculate();
        }
    }

    public class EncycloradiaOpenButton : UIElement
    {
        public EncycloradiaUI UIParent => Parent as EncycloradiaUI;

        public override void LeftMouseDown(UIMouseEvent evt)
        {
            if (Main.playerInventory)
            {
                if (UIParent.Visible)
                {
                    UIParent.bookVisible = !UIParent.bookVisible;
                    UIParent.encycloradia.initialOffset = Vector2.UnitX.RotatedByRandom(0.5f) * 300 * Utils.SelectRandom(Main.rand, new int[] { -1, 1 });
                    UIParent.encycloradia.initialRotation = 0.6f * Utils.SelectRandom(Main.rand, new int[] { -1, 1 });
                    Main.playerInventory = false;
                    SoundEngine.PlaySound(UIParent.bookOpen ? EncycloradiaUI.pageTurnSound : new SoundStyle($"{nameof(Radiance)}/Sounds/BookClose"));
                    if(UIParent.encycloradia.currentEntry is null)
                        UIParent.encycloradia.currentEntry = FindEntry<TitleEntry>();
                    
                }
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (Main.playerInventory)
            {
                DynamicSpriteFont font = FontAssets.MouseText.Value;
                Rectangle dimensions = GetDimensions().ToRectangle();

                Texture2D bookTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/InventoryIcon").Value;
                Vector2 drawPos = dimensions.TopLeft();
                spriteBatch.Draw(bookTexture, drawPos, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                if (IsMouseHovering)
                {
                    string encycloradiaString = Language.GetTextValue($"Mods.{nameof(Radiance)}.CommonStrings.Encycloradia");

                    Texture2D bookGlowTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/InventoryIconGlow").Value;
                    Vector2 pos = Main.MouseScreen + Vector2.One * 16;
                    pos.X = Math.Min(Main.screenWidth - FontAssets.MouseText.Value.MeasureString(encycloradiaString).X - 6, pos.X);

                    spriteBatch.Draw(bookGlowTexture, drawPos + new Vector2(-2, -2), null, Main.OurFavoriteColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                    Utils.DrawBorderStringFourWay(spriteBatch, font, encycloradiaString, pos.X, pos.Y, Color.White, Color.Black, Vector2.Zero);
                    Main.LocalPlayer.mouseInterface = true;
                }

                if (Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().unreadEntires.Any(x => FindEntry(x).unlockedStatus == UnlockedStatus.Unlocked))
                {
                    Texture2D alertTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/UnreadAlert").Value;
                    spriteBatch.Draw(alertTexture, dimensions.TopRight(), null, Color.White, 0, alertTexture.Size() / 2, 1, SpriteEffects.None, 0);
                }
            }
            Recalculate();
        }
    }

    public class Encycloradia : UIElement
    {
        public EncycloradiaUI UIParent => Parent as EncycloradiaUI;

        public EncycloradiaEntry currentEntry
        {
            get => Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().currentEntry;
            set => Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().currentEntry = value;
        }
        public int leftPageIndex
        {
            get => Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().leftPageIndex;
            set => Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().leftPageIndex = value;
        }
        public float bookAlpha = 0;
        public Vector2 initialOffset = Vector2.Zero;
        public float initialRotation = 0;

        public string currentArrowInputs = string.Empty;
        public float arrowTimer = 0;
        public bool arrowHeldDown = false;
        public bool BookOpen { get => UIParent.bookOpen; set => UIParent.bookOpen = value; }
        public bool BookVisible { get => UIParent.bookVisible; set => UIParent.bookVisible = value; }
        public List<(string Entry, int LeftPage)> entryHistory = new List<(string, int)>();

        public bool backBarTick = false;
        public bool openArrowTick = false;
        public bool pageArrowLeftTick = false;
        public bool pageArrowRightTick = false;

        public Color drawnColor = Color.White;
        public Color drawnBGColor = Color.Black;

        /// <summary>
        /// r: default, normal text drawing
        /// <para />
        /// c: hidden text
        /// </summary>
        public char bracketsParsingMode = 'r';
        public string bracketsParsingText = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry">The entry to go to.</param>
        /// <param name="fadeOutArrows">Whether to fade out the quick-nav arrows. This is done when the player goes to an entry through quick-nav.</param>
        public void GoToEntry(EncycloradiaEntry entry, bool fadeOutArrows = false)
        {
            foreach (EncycloradiaPage page in currentEntry.pages)
            {
                if (page is TextPage textPage)
                    textPage.hiddenTextSparkles.Clear();
            }
            currentEntry = entry;
            leftPageIndex = 0;
            arrowTimer = fadeOutArrows ? 30 : 0;
        }

        public override void Update(GameTime gameTime)
        {
            if (BookVisible && Main.keyState.IsKeyDown(Keys.Escape))
                BookVisible = false;

            if (BookOpen)
            {
                HandleFastNav();
                if (bookAlpha < 1)
                    bookAlpha = Math.Min(bookAlpha + 1f / 20, 1);
            }
            else if (BookVisible)
            {
                if (bookAlpha < 1)
                    bookAlpha = Math.Min(bookAlpha + 1f / 30, 1);
            }
            if (!BookVisible)
            {
                if (bookAlpha > 0)
                    bookAlpha = 0;

                arrowTimer = 0;
            }
            base.Update(gameTime);
        }

        public readonly Dictionary<Keys, char> keyDictionary = new()
        {
            { Keys.Up, 'U' },
            { Keys.Right, 'R' },
            { Keys.Down, 'D' },
            { Keys.Left, 'L' },
        };
        public List<Keys> heldKeys = new();

        private void HandleFastNav()
        {
            foreach (Keys key in keyDictionary.Keys)
            {
                if (Main.keyState.IsKeyDown(key))
                {
                    if (!heldKeys.Contains(key))
                    {
                        keyDictionary.TryGetValue(key, out char value);
                        if (currentArrowInputs.Length >= 4)
                            currentArrowInputs = string.Empty;

                        SoundEngine.PlaySound(SoundID.MenuTick);
                        currentArrowInputs += value;
                        arrowTimer = 300;
                        heldKeys.Add(key);
                    }
                }
                else
                    heldKeys.Remove(key);
            }
            arrowHeldDown = false;
            if (currentArrowInputs.Length >= 4 && arrowTimer == 300)
            {
                EncycloradiaEntry entry = FindEntryByFastNavInput(currentArrowInputs);
                if (entry != null)
                {
                    SoundEngine.PlaySound(EncycloradiaUI.pageTurnSound);
                    GoToEntry(entry, true);
                }
                else
                    arrowTimer = 30;
            }
            if (arrowTimer > 0)
                arrowTimer--;
            else
                currentArrowInputs = String.Empty;
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
                    DrawOpenBookElements(spriteBatch, drawPos);
                }
            }
        }
        private void DrawBook(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            float scale = 1;
            float alpha = EaseInSine(Math.Min(bookAlpha * 1.5f, 1));
            float rotation = BookOpen ? 0 : (1 - EaseOutExponent(bookAlpha, 2)) * initialRotation;
            Vector2 pos = (BookOpen ? Vector2.Zero : Vector2.Lerp(Vector2.UnitX * initialOffset, Vector2.Zero, EaseOutExponent(bookAlpha, 2)));
            spriteBatch.Draw(UIParent.MainTexture, drawPos + UIParent.MainTexture.Size() / 2 + pos, null, Color.White * alpha, rotation, UIParent.MainTexture.Size() / 2, scale, SpriteEffects.None, 0);
        }
        private void DrawOpenBookElements(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Rectangle dimensions = GetDimensions().ToRectangle();

            bool didDraw = false;
            foreach (EncycloradiaPage page in currentEntry.pages)
            {
                Vector2 realDrawPos = drawPos;
                bool shouldActuallyDraw = page.index == leftPageIndex || page.index == leftPageIndex + 1;
                bool rightPage = page.index % 2 == 1;
                if (rightPage)
                    realDrawPos += Vector2.UnitX * 346;

                page.DrawPage(this, spriteBatch, realDrawPos, rightPage, shouldActuallyDraw);
                if (shouldActuallyDraw)
                    didDraw = true;
            }
            if(leftPageIndex > 0)
                DrawPageArrows(spriteBatch, drawPos, false);
            if (leftPageIndex + 1 < currentEntry.pages.Count - 1)
                DrawPageArrows(spriteBatch, drawPos, true);

            if (!didDraw)
            {
                Radiance.Instance.Logger.Warn($"While on entry '{currentEntry.internalName}' on left page {leftPageIndex}, an error was encountered with the Encycloradia.");
                Main.NewText($"[c/FF0067:{LanguageManager.Instance.GetOrRegister($"{EncycloradiaUI.LOCALIZATION_PREFIX}.ErrorMessage", () => "Encycloradia Error! Please report this with your log file.")}]");

                GoToEntry(FindEntry<TitleEntry>());
            }
            if (currentEntry.icon != ItemID.ManaCrystal)
                DrawEntryIcon(spriteBatch, drawPos + new Vector2(dimensions.Width / 4 + 19, 41));

            if (currentArrowInputs.Length > 0)
                DrawFastNav(spriteBatch, drawPos);
        }
        private void DrawFastNav(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D backgroundTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/QuickNavBackground").Value;
            Texture2D arrowTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/QuickNavArrow").Value;
            Texture2D softGlow = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SoftGlowNoBG").Value;
            Vector2 realDrawPos = drawPos + UIParent.MainTexture.Size() / 2 - new Vector2(120, 100);
            for (int i = 0; i < currentArrowInputs.Length; i++)
            {
                int fadeTime = 30;
                float rotation = PiOver2 * keyDictionary.Values.ToList().IndexOf(currentArrowInputs[i]);

                Main.spriteBatch.Draw(softGlow, realDrawPos + Vector2.UnitX * 80 * i, null, Color.Black * 0.25f * EaseOutExponent(Math.Min(arrowTimer, fadeTime) / fadeTime, 3), 0, softGlow.Size() / 2, 1.3f, 0, 0);
                spriteBatch.Draw(backgroundTexture, realDrawPos + Vector2.UnitX * 80 * i, null, Color.White * EaseOutExponent(Math.Min(arrowTimer, fadeTime) / fadeTime, 3), 0, backgroundTexture.Size() / 2, 1, SpriteEffects.None, 0);
                for (int d = 0; d < 2; d++)
                {
                    spriteBatch.Draw(arrowTexture, realDrawPos + (d == 0 ? Vector2.Zero : Vector2.UnitY * -2) + Vector2.UnitX * 80 * i, null, (d == 0 ? Color.Gray : Color.White) * EaseOutCirc(Math.Min(arrowTimer, fadeTime) / fadeTime), rotation, arrowTexture.Size() / 2, 1, SpriteEffects.None, 0);
                }
            }
        }
        private void DrawOpenArrow(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D arrowTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/UIArrow").Value;
            Vector2 arrowPos = drawPos + new Vector2(GetDimensions().Width / 2, GetDimensions().Height - 30) - arrowTexture.Size() / 2;
            Rectangle arrowFrame = new Rectangle((int)arrowPos.X, (int)arrowPos.Y, arrowTexture.Width, arrowTexture.Height);
            spriteBatch.Draw(arrowTexture, arrowPos, null, Color.White * EaseInSine(bookAlpha), 0, Vector2.Zero, 1, SpriteEffects.None, 0);

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
                    if (currentEntry == null)
                        GoToEntry(FindEntry<TitleEntry>());

                    BookOpen = true;
                    SoundEngine.PlaySound(EncycloradiaUI.pageTurnSound);
                }
            }
            else
                openArrowTick = false;
        }
        protected void DrawPageArrows(SpriteBatch spriteBatch, Vector2 drawPos, bool right)
        {
            Texture2D arrowTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/PageArrow").Value;
            Vector2 arrowPos = drawPos + new Vector2(GetDimensions().Width / 2 + EncycloradiaUI.PIXELS_FROM_CENTER_TO_PAGE_ARROWS * (right ? 1 : -1), GetDimensions().Height - 96);
            Rectangle arrowFrame = new Rectangle((int)arrowPos.X - arrowTexture.Width / 2, (int)arrowPos.Y - arrowTexture.Height / 2, arrowTexture.Width, arrowTexture.Height);
            spriteBatch.Draw(arrowTexture, arrowPos, null, Color.White, 0, arrowTexture.Size() / 2, 1, right ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

            if (arrowFrame.Contains(Main.MouseScreen.ToPoint()))
            {
                if (right ? !pageArrowRightTick : !pageArrowLeftTick)
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    if (right)
                        pageArrowRightTick = true;
                    else
                        pageArrowLeftTick = true;
                }
                Texture2D arrowGlowTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/PageArrowGlow").Value;
                spriteBatch.Draw(arrowGlowTexture, arrowPos, null, CommonColors.EncycloradiaHoverColor, 0, arrowGlowTexture.Size() / 2, 1, right ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    if (right)
                        leftPageIndex += 2;
                    else
                        leftPageIndex -= 2;

                    SoundEngine.PlaySound(EncycloradiaUI.pageTurnSound);
                }
            }
            else
            {
                if (right)
                    pageArrowRightTick = false;
                else
                    pageArrowLeftTick = false;
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
                spriteBatch.Draw(barGlowTexture, barPos - new Vector2(2, 2), null, CommonColors.EncycloradiaHoverColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    if (currentEntry == FindEntry<TitleEntry>())
                    {
                        BookOpen = false;
                        SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/BookClose"));
                        return;
                    }
                    if(entryHistory.Any())
                    {
                        (string entry, int leftPage) lastEntry = entryHistory.Last();
                        GoToEntry(FindEntry(lastEntry.entry));
                        leftPageIndex = lastEntry.leftPage;
                        entryHistory.Remove(lastEntry);

                        SoundEngine.PlaySound(EncycloradiaUI.pageTurnSound);
                        return;
                    }

                    if (currentEntry.category != EntryCategory.None && !currentEntry.GetType().IsSubclassOf(typeof(CategoryEntry)))
                        GoToEntry(FindEntry(currentEntry.category.ToString() + "Entry"));
                    else
                        GoToEntry(FindEntry<TitleEntry>());

                    leftPageIndex = 0;
                    SoundEngine.PlaySound(EncycloradiaUI.pageTurnSound);
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
            Texture2D softGlow = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SoftGlowNoBG").Value;
            Vector2 itemSize = new Vector2(iconItem.Width, iconItem.Height);

            int width = iconItem.Width + EncycloradiaUI.PIXELS_PADDING_FOR_ENTRY_ICON;
            int height = iconItem.Height + EncycloradiaUI.PIXELS_PADDING_FOR_ENTRY_ICON - 4;
            Rectangle rect = new Rectangle(iconTex.Width / 2 - (int)itemSize.X - EncycloradiaUI.PIXELS_PADDING_FOR_ENTRY_ICON, iconTex.Height - (int)itemSize.Y - EncycloradiaUI.PIXELS_PADDING_FOR_ENTRY_ICON, width, height);
            Rectangle bgRect = new Rectangle(iconTex.Width / 2 - (int)itemSize.X - EncycloradiaUI.PIXELS_PADDING_FOR_ENTRY_ICON - 4, iconTex.Height - (int)itemSize.Y - EncycloradiaUI.PIXELS_PADDING_FOR_ENTRY_ICON, width + 4, height + 1);
            rect.X = (iconTex.Width - rect.Width) / 2;
            rect.Y = iconTex.Height - rect.Height;
            bgRect.X = (iconBGTex.Width - bgRect.Width) / 2;
            bgRect.Y = iconBGTex.Height - bgRect.Height;
            Vector2 itemOrigin = new Vector2(itemSize.X / 2, itemSize.Y);
            Vector2 itemPos = drawPos - Vector2.UnitY * 4;

            spriteBatch.Draw(iconBGTex, drawPos - Vector2.UnitY, bgRect, Color.White * bookAlpha, 0, new Vector2(bgRect.Width / 2, bgRect.Height), 1, SpriteEffects.None, 0);
            spriteBatch.Draw(iconTex, drawPos, rect, Color.White * bookAlpha, 0, new Vector2(rect.Width / 2, rect.Height), 1, SpriteEffects.None, 0);
            spriteBatch.Draw(softGlow, itemPos - Vector2.UnitY * itemSize.Y / 2, null, Color.Black * 0.25f * bookAlpha, 0, softGlow.Size() / 2, itemSize.Length() / 100, 0, 0);
            spriteBatch.Draw(iconItem, itemPos, null, Color.White * bookAlpha, 0, itemOrigin, 1, SpriteEffects.None, 0);

            Rectangle itemRect = new Rectangle((int)(itemPos.X - itemOrigin.X), (int)(itemPos.Y - itemOrigin.Y), (int)itemSize.X, (int)itemSize.Y);

            if (itemRect.Contains(Main.MouseScreen.ToPoint()))
                DrawEntryIcon_Hover(spriteBatch, drawPos);
        }
        private void DrawEntryIcon_Hover(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Dictionary<string, string> arrows = new Dictionary<string, string>()
                {
                    { "U", "↑" },
                    { "R", "→" },
                    { "D", "↓" },
                    { "L", "←" },
                };

            string name = Language.GetTextValue($"Mods.{currentEntry.mod.Name}.Encycloradia.Entries.{currentEntry.internalName}.DisplayName");
            string tooltip = Language.GetTextValue($"Mods.{currentEntry.mod.Name}.Encycloradia.Entries.{currentEntry.internalName}.Tooltip");
            string entryString = LanguageManager.Instance.GetOrRegister($"Mods.{nameof(Radiance)}.Encycloradia.EntryString").Value;

            string fastNavInput = arrows.Aggregate(currentEntry.fastNavInput, (current, value) => current.Replace(value.Key, value.Value));
            string[] iconString =
                {
                    $"[c/FFC042:{name}]",
                    $"'{tooltip}'",
                    $"{entryString}",
                    $"[c/3FDEB1:{fastNavInput}]",
                };

            DrawFakeItemHover(spriteBatch, iconString);
        }
    }
}