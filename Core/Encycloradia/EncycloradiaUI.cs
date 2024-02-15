using Microsoft.Xna.Framework.Input;
using Radiance.Content.EncycloradiaEntries;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Content.Items.RadianceCells;
using Radiance.Core.Loaders;
using Radiance.Core.Systems;
using ReLogic.Graphics;
using System.Collections.Generic;
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
        public EncycloradiaUI()
        {
            Instance = this;
        }
        public static SoundStyle pageTurnSound;
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        public override bool Visible => Main.LocalPlayer.chest == -1 && Main.npcShop == 0;

        public Encycloradia encycloradia = new();
        public EncycloradiaOpenButton encycloradiaOpenButton = new();
        public Texture2D mainTexture { get => ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/Encycloradia" + (encycloradia.BookOpen ? "Main" : "Closed")).Value; }

        public bool bookVisible = false;
        public bool bookOpen = false;

        public string currentArrowInputs = string.Empty;
        public float arrowTimer = 0;
        public bool arrowHeldDown = false;

        public static readonly float ENCYCLORADIA_ADJUSTMENT_FOR_SLANTED_TEXTURE = 0.33f;
        public static float ENCYCLORADIA_MAX_PIXELS_PER_LINE_ADJUSTED => ENCYCLORADIA_PIXELS_BETWEEN_LINES / EncycloradiaUI.ENCYCLORADIA_LINE_SCALE;
        public static readonly int ENCYCLORADIA_ENTRY_BUTTON_MAX_ENTRY_BUTTON_VISUAL_TIMER = 10;
        public static readonly char ENCYCLORADIA_PARSE_CHARACTER = '&';

        public static readonly int ENCYCLORADIA_MAX_PIXELS_PER_LINE = 300;
        public static readonly float ENCYCLORADIA_PIXELS_BETWEEN_LINES = 24;
        public static readonly int ENCYCLORADIA_MAX_LINES_PER_PAGE = 15;
        public static readonly float ENCYCLORADIA_LINE_SCALE = 0.9f;

        public static readonly int ENCYCLORADIA_ENTRIES_PER_CATEGORY_PAGE = 13;
        public static readonly int ENCYCLORADIA_PIXELS_BETWEEN_ENTRY_BUTTONS = 28;

        public static readonly int ENCYCLORADIA_PIXELS_FROM_CENTER_TO_PAGE_ARROWS = 306;
        public static readonly int ENCYCLORADIA_PIXELS_BETWEEN_PAGES = 350;

        public void Load()
        {
            pageTurnSound = new SoundStyle($"{nameof(Radiance)}/Sounds/PageTurn");

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
        }

        public void AddCategoryButtons()
        {
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
                    {
                        UIParent.encycloradia.currentEntry = FindEntry<TitleEntry>();
                        UIParent.encycloradia.leftPage = UIParent.encycloradia.currentEntry.pages.Find(n => n.index == 0);
                        UIParent.encycloradia.rightPage = UIParent.encycloradia.currentEntry.pages.Find(n => n.index == 1);
                    }
                }
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (Main.playerInventory)
            {
                DynamicSpriteFont font = FontAssets.MouseText.Value;
                Rectangle dimensions = GetDimensions().ToRectangle();
                Vector2 drawPos = dimensions.TopLeft();
                Texture2D bookTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/InventoryIcon").Value;
                Texture2D alertTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/UnreadAlert").Value;

                spriteBatch.Draw(bookTexture, drawPos, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                if (IsMouseHovering)
                {
                    Texture2D bookGlowTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/InventoryIconGlow").Value;
                    Vector2 pos = Main.MouseScreen + Vector2.One * 16;
                    pos.X = Math.Min(Main.screenWidth - FontAssets.MouseText.Value.MeasureString("Encycloradia").X - 6, pos.X);

                    spriteBatch.Draw(bookGlowTexture, drawPos + new Vector2(-2, -2), null, Main.OurFavoriteColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                    Utils.DrawBorderStringFourWay(spriteBatch, font, "Encycloradia", pos.X, pos.Y, Color.White, Color.Black, Vector2.Zero);
                    Main.LocalPlayer.mouseInterface = true;
                }

                if (Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().unreadEntires.Any())
                    spriteBatch.Draw(alertTexture, dimensions.TopRight(), null, Color.White, 0, alertTexture.Size() / 2, 1, SpriteEffects.None, 0);
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
        public EncycloradiaPage leftPage
        {
            get => Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().leftPage;
            set => Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().leftPage = value;
        }
        public EncycloradiaPage rightPage
        {
            get => Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().rightPage;
            set => Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().rightPage = value;
        }

        public float bookAlpha = 0;
        public Vector2 initialOffset = Vector2.Zero;
        public float initialRotation = 0;
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
            leftPage = entry.pages.Find(n => n.index == 0);
            rightPage = entry.pages.Find(n => n.index == 1);
            if(UIParent is not null)
                UIParent.arrowTimer = completed ? 30 : 0;
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

                UIParent.arrowTimer = 0;
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
                        if (UIParent.currentArrowInputs.Length >= 4)
                            UIParent.currentArrowInputs = string.Empty;

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
                EncycloradiaEntry entry = FindEntryByFastNavInput(UIParent.currentArrowInputs);
                if (entry != null)
                {
                    SoundEngine.PlaySound(EncycloradiaUI.pageTurnSound);
                    GoToEntry(entry, true);
                }
                else
                    UIParent.arrowTimer = 30;
            }
            if (UIParent.arrowTimer > 0)
                UIParent.arrowTimer--;
            else
                UIParent.currentArrowInputs = String.Empty;
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
            spriteBatch.Draw(UIParent.mainTexture, drawPos + UIParent.mainTexture.Size() / 2 + pos, null, Color.White * alpha, rotation, UIParent.mainTexture.Size() / 2, scale, SpriteEffects.None, 0);
        }
        private void DrawOpenBookElements(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Rectangle dimensions = GetDimensions().ToRectangle();
            if (leftPage is not null)
            {
                leftPage.DrawPage(this, spriteBatch, drawPos, false);
                if (leftPage.index > 0)
                    DrawPageArrows(spriteBatch, drawPos, false);
            }

            if (rightPage is not null)
            {
                rightPage.DrawPage(this, spriteBatch, drawPos + Vector2.UnitX * 346, true);
                if (currentEntry.pages.Count - 1 > rightPage.index)
                    DrawPageArrows(spriteBatch, drawPos, true);
            }
            if (currentEntry.icon != ItemID.ManaCrystal)
                DrawEntryIcon(spriteBatch, drawPos + new Vector2(dimensions.Width / 4 + 19, 41));

            if (UIParent.currentArrowInputs.Length > 0)
                DrawFastNav(spriteBatch, drawPos);
        }
        private void DrawFastNav(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D backgroundTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/QuickNavBackground").Value;
            Texture2D arrowTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/QuickNavArrow").Value;
            Texture2D softGlow = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SoftGlowNoBG").Value;
            Vector2 realDrawPos = drawPos + UIParent.mainTexture.Size() / 2 - new Vector2(120, 100);
            for (int i = 0; i < UIParent.currentArrowInputs.Length; i++)
            {
                int fadeTime = 30;
                float rotation = PiOver2 * keyDictionary.Values.ToList().IndexOf(UIParent.currentArrowInputs[i]);

                Main.spriteBatch.Draw(softGlow, realDrawPos + Vector2.UnitX * 80 * i, null, Color.Black * 0.25f * EaseOutExponent(Math.Min(UIParent.arrowTimer, fadeTime) / fadeTime, 3), 0, softGlow.Size() / 2, 1.3f, 0, 0);
                spriteBatch.Draw(backgroundTexture, realDrawPos + Vector2.UnitX * 80 * i, null, Color.White * EaseOutExponent(Math.Min(UIParent.arrowTimer, fadeTime) / fadeTime, 3), 0, backgroundTexture.Size() / 2, 1, SpriteEffects.None, 0);
                for (int d = 0; d < 2; d++)
                {
                    spriteBatch.Draw(arrowTexture, realDrawPos + (d == 0 ? Vector2.Zero : Vector2.UnitY * -2) + Vector2.UnitX * 80 * i, null, (d == 0 ? Color.Gray : Color.White) * EaseOutCirc(Math.Min(UIParent.arrowTimer, fadeTime) / fadeTime), rotation, arrowTexture.Size() / 2, 1, SpriteEffects.None, 0);
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
            Vector2 arrowPos = drawPos + new Vector2(GetDimensions().Width / 2 + EncycloradiaUI.ENCYCLORADIA_PIXELS_FROM_CENTER_TO_PAGE_ARROWS * (right ? 1 : -1), GetDimensions().Height - 96);
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
                    {
                        leftPage = currentEntry.pages.Find(n => n.index == leftPage.index + 2);
                        rightPage = currentEntry.pages.Find(n => n.index == leftPage.index + 1);
                    }
                    else
                    {
                        leftPage = currentEntry.pages.Find(n => n.index == leftPage.index - 2);
                        rightPage = currentEntry.pages.Find(n => n.index == leftPage.index + 1);
                    }
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

                    if (currentEntry.category != EntryCategory.None && !currentEntry.GetType().IsSubclassOf(typeof(CategoryEntry)))
                        GoToEntry(FindEntry(currentEntry.category.ToString() + "Entry"));
                    else
                        GoToEntry(FindEntry<TitleEntry>());

                    leftPage = currentEntry.pages.Find(n => n.index == 0);
                    rightPage = currentEntry.pages.Find(n => n.index == 1);
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

            spriteBatch.Draw(iconBGTex, drawPos - Vector2.UnitY, bgRect, Color.White * bookAlpha, 0, new Vector2(bgRect.Width / 2, bgRect.Height), 1, SpriteEffects.None, 0);
            spriteBatch.Draw(iconTex, drawPos, rect, Color.White * bookAlpha, 0, new Vector2(rect.Width / 2, rect.Height), 1, SpriteEffects.None, 0);
            spriteBatch.Draw(softGlow, itemPos - Vector2.UnitY * itemSize.Y / 2, null, Color.Black * 0.25f * bookAlpha, 0, softGlow.Size() / 2, itemSize.Length() / 100, 0, 0);
            spriteBatch.Draw(iconItem, itemPos, null, Color.White * bookAlpha, 0, itemOrigin, 1, SpriteEffects.None, 0);

            Rectangle itemRect = new Rectangle((int)(itemPos.X - itemOrigin.X), (int)(itemPos.Y - itemOrigin.Y), (int)itemSize.X, (int)itemSize.Y);

            if (itemRect.Contains(Main.MouseScreen.ToPoint()))
            {
                Dictionary<string, string> arrows = new Dictionary<string, string>()
                {
                    { "U", "↑" },
                    { "R", "→" },
                    { "D", "↓" },
                    { "L", "←" },
                };
                string fastNavInput = arrows.Aggregate(currentEntry.fastNavInput, (current, value) => current.Replace(value.Key, value.Value));
                string[] iconString =
                {
                    $"[c/FFC042:{currentEntry.displayName}]",
                    "'" + currentEntry.tooltip + "'",
                    "Entry",
                    $"[c/3FDEB1:{fastNavInput}]",
                };
                DrawFakeItemHover(spriteBatch, iconString);
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
        private SoundStyle pageSound = EncycloradiaUI.pageTurnSound;
        public bool HasUnread => Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().unreadEntires.Any(x => FindEntry(x).category == category);
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
            if (!UIParent.bookVisible || UIParent.encycloradia.currentEntry != FindEntry<TitleEntry>())
                visualsTimer = 0;

            base.Update(gameTime);
        }

        public void DrawStuff(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            int maxVisualTimer = 10;
            Texture2D tex = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/" + texture + "Symbol").Value;
            Texture2D alertTex = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/UnreadAlert").Value;
            drawPos += pos;
            drawPos -= size / 2;
            Rectangle frame = new Rectangle((int)(drawPos.X - size.X / 2), (int)(drawPos.Y - size.Y / 2), (int)size.X, (int)size.Y);
            float timing = EaseInOutExponent(Math.Min(visualsTimer / (maxVisualTimer * 2) + 0.5f, 1), 4);
            realColor = color * timing;
            spriteBatch.Draw(tex, drawPos, null, realColor * UIParent.encycloradia.bookAlpha, 0, size / 2, Math.Clamp(timing + 0.3f, 1, 1.3f), SpriteEffects.None, 0);
            if (HasUnread)
                spriteBatch.Draw(alertTex, drawPos + new Vector2(tex.Width, -tex.Height) / 2 - new Vector2(8, -8), null, Color.White * UIParent.encycloradia.bookAlpha * (1 - visualsTimer / maxVisualTimer), 0, alertTex.Size() / 2, Math.Clamp(timing + 0.3f, 1, 1.3f), SpriteEffects.None, 0);
            
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
                    if (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift))
                    {
                        Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().unreadEntires.RemoveAll(x => FindEntry(x).category == category);
                        SoundEngine.PlaySound(SoundID.MenuTick);
                    }
                    else
                    {
                        SoundEngine.PlaySound(pageSound);
                        UIParent.encycloradia.GoToEntry(FindEntry(texture + "Entry"));
                    }
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
                    font, texture, drawPos.X, drawPos.Y, realColor * timing * 2f, realColor.GetDarkColor() * timing, font.MeasureString(texture) / 2, timing);
            }
        }
    }
}