﻿using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Content.Items.RadianceCells;
using Radiance.Content.Particles;
using Radiance.Core.Systems;
using ReLogic.Graphics;
using Terraria.Localization;

namespace Radiance.Core.Encycloradia
{
    public abstract class EncycloradiaPage
    {
        protected static DynamicSpriteFont Font => FontAssets.MouseText.Value;
        public int index = 0;
        public string text;
        public LocalizedText[] keys;
        /// <summary>
        /// Draws the page.
        /// </summary>
        /// <param name="encycloradia"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="drawPos"></param>
        /// <param name="rightPage"></param>
        /// <param name="actuallyDrawPage">THIS IS IMPORTANT for processing text page colors that extend from a previous page</param>
        public abstract void DrawPage(Encycloradia encycloradia, SpriteBatch spriteBatch, Vector2 drawPos, bool rightPage, bool actuallyDrawPage);
    }

    public class TextPage : EncycloradiaPage
    {
        public List<Rectangle> hiddenTextRects = new List<Rectangle>();
        public List<HiddenTextSparkle> hiddenTextSparkles = new List<HiddenTextSparkle>();

        public override void DrawPage(Encycloradia encycloradia, SpriteBatch spriteBatch, Vector2 drawPos, bool rightPage, bool actuallyDrawPage)
        {
            string parseBracketsString = string.Empty;
            int colonsLeft = 0;
            float xDrawOffset = 0;
            float yDrawOffset = 0;
            Rectangle hiddenTextRect = default;
            if (text != string.Empty)
            {
                foreach (string word in text.Split(" "))
                {
                    bool colorParseMode = false;
                    foreach (char character in word)
                    {
                        bool shouldDrawCharacter = true;

                        #region Bracket Parsing
                        // if the character is an open bracket, don't draw it and instead start setting the amount of extra text for formatting
                        if (character == '[')
                        {
                            colonsLeft = 2;
                            continue;
                        }
                        if (character == ']')
                        {
                            encycloradia.bracketsParsingMode = 'r';
                            encycloradia.bracketsParsingText = string.Empty;
                            if (hiddenTextRect != default)
                            {
                                hiddenTextRects.Add(hiddenTextRect);
                                hiddenTextRect = default;
                            }
                            continue;
                        }
                        if (colonsLeft > 0)
                        {
                            parseBracketsString += character;
                            if (character == ':')
                                colonsLeft--;

                            if (colonsLeft == 0)
                            {
                                string[] bracketParameters = parseBracketsString.Split(':');
                                encycloradia.bracketsParsingMode = bracketParameters[0][0];
                                encycloradia.bracketsParsingText = bracketParameters[1];
                            }
                            continue;
                        }

                        #endregion Bracket Parsing

                        #region Colorcode Parsing

                        if (character == '\n')
                        {
                            xDrawOffset = 0;
                            yDrawOffset += Font.LineSpacing - 1;
                            if (hiddenTextRect != default)
                            {
                                hiddenTextRects.Add(hiddenTextRect);
                                hiddenTextRect = default;
                            }
                            continue;
                        }
                        // if the char is the parse character, don't draw it or the next char
                        if (character == EncycloradiaUI.PARSE_CHARACTER)
                        {
                            colorParseMode = true;
                            continue;
                        }
                        if (colorParseMode)
                        {
                            DrawPage_ParseColor(encycloradia, character);
                            colorParseMode = false;
                            continue;
                        }

                        #endregion Colorcode Parsing

                        Color drawColor = encycloradia.drawnColor;
                        Color bgColor = encycloradia.drawnBGColor;
                        float drawPosX = drawPos.X + xDrawOffset + 61 - (rightPage ? 0 : (yDrawOffset / 23));
                        float drawPosY = drawPos.Y + yDrawOffset + 56;

                        #region Hidden Text Parsing

                        if (encycloradia.bracketsParsingMode == 'c')
                        {
                            EncycloradiaEntry entry = EncycloradiaSystem.FindEntry(encycloradia.bracketsParsingText);
                            if(entry is null)
                                EncycloradiaSystem.ThrowEncycloradiaError($"Entry {encycloradia.bracketsParsingText} for hidden text not found!", true);
                            else if (entry.unlockedStatus != UnlockedStatus.Unlocked || (encycloradia.drawnColor == Color.White && encycloradia.drawnBGColor == Color.Black))
                            {
                                drawColor = CommonColors.EncycloradiaHiddenColor;
                                bgColor = CommonColors.EncycloradiaHiddenColor.GetDarkColor();
                            }

                            // hidden text rectangle stuff
                            if (entry.unlockedStatus != UnlockedStatus.Unlocked)
                            {
                                Vector2 measurements = Font.MeasureString(character.ToString()) * EncycloradiaUI.LINE_SCALE;
                                if (hiddenTextRect == default)
                                {
                                    hiddenTextRect.X = (int)drawPosX;
                                    hiddenTextRect.Y = (int)drawPosY;
                                    hiddenTextRect.Width = (int)measurements.X;
                                    hiddenTextRect.Height = (int)measurements.Y;
                                }
                                else
                                {
                                    hiddenTextRect.Width += (int)measurements.X;
                                    if (hiddenTextRect.Height < measurements.Y)
                                        hiddenTextRect.Height = (int)measurements.Y;
                                }
                                shouldDrawCharacter = false;
                            }
                        }

                        #endregion Hidden Text Parsing

                        Vector2 lerpedPos = Vector2.Lerp(new Vector2(Main.screenWidth, Main.screenHeight) / 2, new Vector2(drawPosX, drawPosY), EaseOutExponent(encycloradia.bookAlpha, 4));
                        if (actuallyDrawPage && shouldDrawCharacter)
                            Utils.DrawBorderStringFourWay(spriteBatch, Font, character.ToString(), lerpedPos.X, lerpedPos.Y, drawColor * encycloradia.bookAlpha, bgColor * encycloradia.bookAlpha, Vector2.Zero, EncycloradiaUI.LINE_SCALE);

                        xDrawOffset += Font.MeasureString(character.ToString()).X * EncycloradiaUI.LINE_SCALE;
                    }

                    float xIncrease = Font.MeasureString(" ").X * EncycloradiaUI.LINE_SCALE;
                    xDrawOffset += xIncrease;
                    if (hiddenTextRect != default)
                        hiddenTextRect.Width += (int)xIncrease;
                }
                if (hiddenTextRect != default)
                    hiddenTextRects.Add(hiddenTextRect);

                ManageSparkles(spriteBatch, actuallyDrawPage);
            }
        }
        private void DrawPage_ParseColor(Encycloradia encycloradia, char character)
        {
            switch (character)
            {
                case 'y': //radiance yellow
                    encycloradia.drawnColor = CommonColors.RadianceTextColor;
                    encycloradia.drawnBGColor = CommonColors.RadianceTextColor.GetDarkColor();
                    break;

                case 'b': //context blue
                    encycloradia.drawnColor = CommonColors.ContextColor;
                    encycloradia.drawnBGColor = CommonColors.ContextColor.GetDarkColor();
                    break;

                case 'g': //locked gray
                    encycloradia.drawnColor = CommonColors.LockedColor;
                    encycloradia.drawnBGColor = CommonColors.LockedColor.GetDarkColor();
                    break;

                case 'i': //influencing red
                    encycloradia.drawnColor = CommonColors.InfluencingTextColor;
                    encycloradia.drawnBGColor = CommonColors.InfluencingTextColor.GetDarkColor();
                    break;

                case 't': //transmutation lime
                    encycloradia.drawnColor = CommonColors.TransmutationTextColor;
                    encycloradia.drawnBGColor = CommonColors.TransmutationTextColor.GetDarkColor();
                    break;

                case 'a': //apparatuses blue
                    encycloradia.drawnColor = CommonColors.ApparatusesTextColor;
                    encycloradia.drawnBGColor = CommonColors.ApparatusesTextColor.GetDarkColor();
                    break;

                case 's': //instruments orange
                    encycloradia.drawnColor = CommonColors.InstrumentsTextColor;
                    encycloradia.drawnBGColor = CommonColors.InstrumentsTextColor.GetDarkColor();
                    break;

                case 'd': //pedestalworks purple
                    encycloradia.drawnColor = CommonColors.PedestalworksTextColor;
                    encycloradia.drawnBGColor = CommonColors.PedestalworksTextColor.GetDarkColor();
                    break;

                case 'p': //phenomena teal
                    encycloradia.drawnColor = CommonColors.PhenomenaTextColor;
                    encycloradia.drawnBGColor = CommonColors.PhenomenaTextColor.GetDarkColor();
                    break;

                case '1': //scarlet
                    encycloradia.drawnColor = CommonColors.ScarletColor;
                    encycloradia.drawnBGColor = CommonColors.ScarletColor.GetDarkColor();
                    break;

                case '2': //cerulean
                    encycloradia.drawnColor = CommonColors.CeruleanColor;
                    encycloradia.drawnBGColor = CommonColors.CeruleanColor.GetDarkColor();
                    break;

                case '3': //verdant
                    encycloradia.drawnColor = CommonColors.VerdantColor;
                    encycloradia.drawnBGColor = CommonColors.VerdantColor.GetDarkColor();
                    break;

                case '4': //mauve
                    encycloradia.drawnColor = CommonColors.MauveColor;
                    encycloradia.drawnBGColor = CommonColors.MauveColor.GetDarkColor();
                    break;

                case 'r': //reset
                    encycloradia.drawnColor = Color.White;
                    encycloradia.drawnBGColor = Color.Black;
                    break;

                default:
                    encycloradia.drawnColor = Color.Red;
                    encycloradia.drawnBGColor = Color.DarkRed;
                    break;
            }
        }
        public void ManageSparkles(SpriteBatch spriteBatch, bool actuallyDrawPage)
        {
            if (!Main.gamePaused && Main.hasFocus)
            {
                for (int i = 0; i < hiddenTextRects.Count; i++)
                {
                    Rectangle rect = hiddenTextRects[i];
                    rect.Inflate(-6, -rect.Height / 2 + 4);
                    rect.Y += rect.Height / 2;
                    if(Main.LocalPlayer.GetModPlayer<RadiancePlayer>().debugMode)
                        Utils.DrawRect(spriteBatch, new Rectangle(rect.X + (int)Main.screenPosition.X, rect.Y + (int)Main.screenPosition.Y, rect.Width, rect.Height), Color.Red);
                    
                    if (Main.GameUpdateCount % 30 == 0 && Main.rand.NextFloat(4f - rect.Width / EncycloradiaUI.LINE_SCALE / 100) < 1f)
                        hiddenTextSparkles.Add(new HiddenTextSparkle(Main.rand.NextVector2FromRectangle(rect), Vector2.UnitY * Main.rand.NextFloat(-0.05f, -0.025f), Main.rand.Next(360, 450), Main.rand.NextFloat(0.7f, 0.85f)));
                }
            }
            foreach (HiddenTextSparkle sparkle in hiddenTextSparkles)
            {
                if (!Main.gamePaused && Main.hasFocus)
                {
                    sparkle.Update();
                    sparkle.position += sparkle.velocity;
                    sparkle.timeLeft--;
                }

                if (actuallyDrawPage)
                    sparkle.SpecialDraw(spriteBatch, sparkle.position + Vector2.Zero);
            }
            hiddenTextSparkles.RemoveAll(x => x.timeLeft <= 0);
            hiddenTextRects.Clear();
        }
    }

    public class CategoryPage : EncycloradiaPage
    {
        public EntryCategory category = EntryCategory.None;
        public List<EncycloradiaEntry> entries => EncycloradiaSystem.EntriesByCategory[category];
        public int[] visualTimers = new int[EncycloradiaUI.ENTRIES_PER_CATEGORY_PAGE];
        public bool[] ticks = new bool[EncycloradiaUI.ENTRIES_PER_CATEGORY_PAGE];

        public CategoryPage(EntryCategory category) => this.category = category;

        public override void DrawPage(Encycloradia encycloradia, SpriteBatch spriteBatch, Vector2 drawPos, bool rightPage, bool actuallyDrawPage)
        {
            if (actuallyDrawPage)
            {
                if (entries.Count != 0)
                {
                    List<EncycloradiaEntry> pagesToDraw = entries
                        .Where(x => x.visible == EntryVisibility.Visible || (x.visible == EntryVisibility.NotVisibleUntilUnlocked && x.unlockedStatus == UnlockedStatus.Unlocked))
                        .OrderByDescending(x => x.Unread && x.unlockedStatus == UnlockedStatus.Unlocked)
                        .ToList();

                    int lower = (index - 1) * EncycloradiaUI.ENTRIES_PER_CATEGORY_PAGE;
                    int upper = Math.Min(EncycloradiaUI.ENTRIES_PER_CATEGORY_PAGE, pagesToDraw.Count - (index - 1) * EncycloradiaUI.ENTRIES_PER_CATEGORY_PAGE);
                    pagesToDraw = pagesToDraw.GetRange(lower, upper);

                    int yDistance = 0;
                    Vector2 buttonOffset = new Vector2(74, 64);
                    for (int i = 0; i < pagesToDraw.Count; i++)
                    {
                        EncycloradiaEntry entry = pagesToDraw[i];
                        DrawButton(encycloradia, entry, spriteBatch, drawPos + buttonOffset + Vector2.UnitY * yDistance, i);
                        yDistance += EncycloradiaUI.PIXELS_BETWEEN_ENTRY_BUTTONS;
                    }
                }
            }
        }

        public void DrawButton(Encycloradia encycloradia, EncycloradiaEntry entry, SpriteBatch spriteBatch, Vector2 drawPos, int index)
        {
            DynamicSpriteFont font = FontAssets.MouseText.Value;

            Texture2D tex;
            string text;
            Color iconColor = Color.White;
            Color textColor = Color.White;
            Color textBGColor = Color.Black;

            float timing = EaseInOutExponent(Math.Clamp((float)visualTimers[index] / (EncycloradiaUI.ENTRY_BUTTON_MAX_ENTRY_BUTTON_VISUAL_TIMER * 2) + 0.5f, 0.5f, 1), 4);

            switch (entry.unlockedStatus)
            {
                case UnlockedStatus.Unlocked:
                    Main.instance.LoadItem(entry.icon);
                    tex = GetItemTexture(entry.icon);
                    if (Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().unreadEntires.Any(x => entry.internalName == x))
                        tex = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/UnreadAlert").Value;

                    text = Language.GetOrRegister($"{EncycloradiaUI.LOCALIZATION_PREFIX}.Entries.{entry.internalName}.DisplayName").Value;
                    if (entry.visible == EntryVisibility.NotVisibleUntilUnlocked)
                    {
                        textColor = CommonColors.EncycloradiaHiddenColor;
                        textBGColor = Color.Lerp(Color.Black, new Color(212, 63, 182), timing - 0.5f);
                    }
                    else
                        textBGColor = Color.Lerp(Color.Black, CommonColors.RadianceColor1, timing - 0.5f);

                    break;

                case UnlockedStatus.Incomplete:
                    Main.instance.LoadItem(entry.icon);
                    tex = GetItemTexture(entry.icon);
                    text = Language.GetOrRegister($"Mods.{nameof(Radiance)}.CommonStrings.Incomplete").Value;

                    iconColor = Color.Black;
                    textColor = new Color(175, 175, 175, 255);
                    break;

                default:
                    tex = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/LockIcon").Value;
                    text = Language.GetOrRegister($"Mods.{nameof(Radiance)}.CommonStrings.Locked").Value;

                    textColor = new Color(125, 125, 125, 255);
                    break;
            }
            iconColor *= encycloradia.bookAlpha;
            textColor *= encycloradia.bookAlpha;

            float scale = 1f;
            if (tex.Size().X > 32 || tex.Size().Y > 32)
            {
                if (tex.Size().X > tex.Size().Y)
                    scale = 32f / tex.Size().X;
                else
                    scale = 32f / tex.Size().Y;
            }
            Vector2 scaledTexSize = tex.Size() * scale;
            Rectangle frame = new Rectangle((int)(drawPos.X - scaledTexSize.X / 2), (int)(drawPos.Y - scaledTexSize.Y / 2), (int)scaledTexSize.X + (int)Font.MeasureString(text).X + 44, (int)scaledTexSize.Y);

            DrawButton_Hover(encycloradia, entry, frame, index);
            if (visualTimers[index] > 0)
            {
                float drawModifier = ((float)visualTimers[index] / EncycloradiaUI.ENTRY_BUTTON_MAX_ENTRY_BUTTON_VISUAL_TIMER);
                RadianceDrawing.DrawSoftGlow(Main.screenPosition + drawPos, Color.White * drawModifier * encycloradia.bookAlpha, 0.24f);
                RadianceDrawing.DrawSpike(spriteBatch, RadianceDrawing.SpriteBatchData.UIDrawingDataScale, drawPos, drawPos + Vector2.UnitX * 300, Color.White * drawModifier * encycloradia.bookAlpha * 0.7f, 24);
            }
            Utils.DrawBorderStringFourWay(spriteBatch, font, text, drawPos.X + scaledTexSize.X / 2 + 4 * Math.Clamp(timing * 2, 1, 2f), drawPos.Y + 4, textColor, textBGColor, Vector2.UnitY * font.MeasureString(text).Y / 2);
            spriteBatch.Draw(tex, new Vector2(drawPos.X, drawPos.Y), null, iconColor, 0, tex.Size() / 2, scale * Math.Clamp(timing + 0.2f, 1, 1.2f), SpriteEffects.None, 0);
        }

        public void DrawButton_Hover(Encycloradia encycloradia, EncycloradiaEntry entry, Rectangle frame, int index)
        {
            if (frame.Contains(Main.MouseScreen.ToPoint()))
            {
                switch (entry.unlockedStatus)
                {
                    case UnlockedStatus.Unlocked:
                        if (!ticks[index])
                        {
                            SoundEngine.PlaySound(SoundID.MenuTick);
                            ticks[index] = true;
                        }
                        if (visualTimers[index] < EncycloradiaUI.ENTRY_BUTTON_MAX_ENTRY_BUTTON_VISUAL_TIMER)
                            visualTimers[index]++;

                        if (Main.mouseLeft && Main.mouseLeftRelease)
                        {
                            Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().unreadEntires.Remove(entry.internalName);

                            encycloradia.GoToEntry(entry);
                            visualTimers[index] = 0;
                            ticks[index] = false;

                            SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/PageTurn"));
                        }
                        break;

                    case UnlockedStatus.Incomplete:
                        LocalizedText unlockMethod = LanguageManager.Instance.GetOrRegister($"{EncycloradiaUI.LOCALIZATION_PREFIX}.UnlockBy", () => "Unlock this entry by ");
                        Main.LocalPlayer.SetFakeHoverText($"{unlockMethod.Value} {entry.unlock.tooltip.Value}"); 
                        break;
                }
            }
            else
            {
                ticks[index] = false;
                if (visualTimers[index] > 0)
                    visualTimers[index]--;
            }
        }
    }

    public class ImagePage : EncycloradiaPage
    {
        public Texture2D texture;

        public override void DrawPage(Encycloradia encycloradia, SpriteBatch spriteBatch, Vector2 drawPos, bool rightPage, bool actuallyDrawPage)
        {
            throw new NotImplementedException();
        }
    }

    public class RecipePage : EncycloradiaPage
    {
        public Dictionary<int, int> items;
        public Item station;
        public Item result;
        public string extras = string.Empty;

        public override void DrawPage(Encycloradia encycloradia, SpriteBatch spriteBatch, Vector2 drawPos, bool rightPage, bool actuallyDrawPage)
        {
            if (actuallyDrawPage)
            {
                Vector2 pos = drawPos + new Vector2(EncycloradiaUI.PIXELS_BETWEEN_PAGES / 2 + 36, encycloradia.UIParent.MainTexture.Height / 2 - 24);
                Texture2D overlayTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/CraftingOverlay").Value;
                Texture2D softGlow = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SoftGlowNoBG").Value;

                spriteBatch.Draw(overlayTexture, pos, null, Color.White * encycloradia.bookAlpha, 0, overlayTexture.Size() / 2, 1, SpriteEffects.None, 0);

                Vector2 stationPos = pos - Vector2.UnitY * 81;

                if (!station.IsAir)
                {
                    Main.spriteBatch.Draw(softGlow, stationPos, null, Color.Black * 0.3f * encycloradia.bookAlpha, 0, softGlow.Size() / 2, (float)(Item.GetDrawHitbox(station.type, null).Width + Item.GetDrawHitbox(station.type, null).Height) / 100, 0, 0);
                    RadianceDrawing.DrawHoverableItem(spriteBatch, station.type, stationPos, 1); //station
                }

                Vector2 resultPos = pos + Vector2.UnitY * 109;
                Main.spriteBatch.Draw(softGlow, resultPos, null, Color.Black * 0.3f * encycloradia.bookAlpha, 0, softGlow.Size() / 2, (float)(Item.GetDrawHitbox(result.type, null).Width + Item.GetDrawHitbox(result.type, null).Height) / 100, 0, 0);
                RadianceDrawing.DrawHoverableItem(spriteBatch, result.type, resultPos, result.stack); //result

                float longestItem = 0;
                foreach (int item in items.Keys)
                {
                    if ((Item.GetDrawHitbox(item, null).Width + Item.GetDrawHitbox(item, null).Height) / 2 > longestItem)
                        longestItem = (Item.GetDrawHitbox(item, null).Width + Item.GetDrawHitbox(item, null).Height) / 2;
                }
                foreach (int item in items.Keys)
                {
                    float deg = (float)Main.GameUpdateCount / 5 + 360 / items.Keys.Count * items.Keys.ToList().IndexOf(item);
                    Vector2 pos2 = stationPos + (Vector2.UnitX * Math.Min(longestItem / 2 + 40, longestItem + 24)).RotatedBy(ToRadians(deg));

                    Main.spriteBatch.Draw(softGlow, pos2, null, Color.Black * 0.25f * encycloradia.bookAlpha, 0, softGlow.Size() / 2, (float)(Item.GetDrawHitbox(item, null).Width + Item.GetDrawHitbox(item, null).Height) / 100, 0, 0);

                    items.TryGetValue(item, out int value);
                    RadianceDrawing.DrawHoverableItem(spriteBatch, item, pos2, value, Color.White * encycloradia.bookAlpha);
                }
            }
        }
    }

    public class TransmutationPage : EncycloradiaPage
    {
        public TransmutationRecipe recipe = new TransmutationRecipe();

        public override void DrawPage(Encycloradia encycloradia, SpriteBatch spriteBatch, Vector2 drawPos, bool rightPage, bool actuallyDrawPage)
        {
            if (actuallyDrawPage)
            {
                Texture2D overlayTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/TransmutationOverlay").Value;
                Vector2 pos = drawPos + new Vector2(EncycloradiaUI.PIXELS_BETWEEN_PAGES / 2 + 30, encycloradia.UIParent.MainTexture.Height / 2 - 20);

                spriteBatch.Draw(overlayTexture, pos, null, Color.White * encycloradia.bookAlpha, 0, overlayTexture.Size() / 2, 1, SpriteEffects.None, 0);

                BaseContainer cell = DrawPage_GetCellForRecipe();
                DrawPage_Items(encycloradia, spriteBatch, pos - new Vector2(40, 81));
                DrawPage_Cell(encycloradia, spriteBatch, pos + new Vector2(58, 52), cell);
                DrawPage_RequiredRadiance(encycloradia, spriteBatch, pos + new Vector2(58, -74), cell);
                DrawPage_Requirements(encycloradia, spriteBatch, pos + new Vector2(58, 143));
                DrawPage_Lens(encycloradia, spriteBatch, pos - new Vector2(40, -10));
            }
        }
        private BaseContainer DrawPage_GetCellForRecipe()
        {
            int cell = ModContent.ItemType<StandardRadianceCell>();
            //if (recipe.requiredRadiance > 4000)
            //    cell = ModContent.ItemType<StandardRadianceCell>(); //todo: replace with bigger cell

            return new Item(cell).ModItem as BaseContainer;
        }
        private void DrawPage_Items(Encycloradia encycloradia, SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D softGlow = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SoftGlowNoBG").Value;
            int currentItem = recipe.inputItems[Main.GameUpdateCount / 75 % recipe.inputItems.Length];

            Main.spriteBatch.Draw(softGlow, drawPos, null, Color.Black * 0.3f, 0, softGlow.Size() / 2, (float)(Item.GetDrawHitbox(currentItem, null).Width + Item.GetDrawHitbox(currentItem, null).Height) / 100, 0, 0);
            RadianceDrawing.DrawHoverableItem(spriteBatch, currentItem, drawPos, recipe.inputStack, Color.White * encycloradia.bookAlpha, encycloradia: true); // input

            Vector2 resultPos = drawPos + new Vector2(0, 190);
            Main.spriteBatch.Draw(softGlow, resultPos, null, Color.Black * 0.3f, 0, softGlow.Size() / 2, (float)(Item.GetDrawHitbox(recipe.outputItem, null).Width + Item.GetDrawHitbox(recipe.outputItem, null).Height) / 100, 0, 0);
            RadianceDrawing.DrawHoverableItem(spriteBatch, recipe.outputItem, resultPos, recipe.outputStack, Color.White * encycloradia.bookAlpha, encycloradia: true); // output
        }
        private void DrawPage_Cell(Encycloradia encycloradia, SpriteBatch spriteBatch, Vector2 drawPos, BaseContainer cell)
        {
            RadianceDrawing.DrawHoverableItem(spriteBatch, cell.Type, drawPos, 1, encycloradia: true); 
        }
        private void DrawPage_RequiredRadiance(Encycloradia encycloradia, SpriteBatch spriteBatch, Vector2 drawPos, BaseContainer cell)
        {
            Texture2D barTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/TransmutationOverlayBar").Value;

            float maxRadiance = cell.maxRadiance;
            float storedRadiance = recipe.requiredRadiance;
            float radianceCharge = Math.Min(storedRadiance, maxRadiance);
            float fill = radianceCharge / maxRadiance;

            Main.spriteBatch.Draw(
                barTexture,
                drawPos,
                new Rectangle(0, 0, barTexture.Width, (int)(Math.Max(0.05f, Math.Round(fill, 2)) * barTexture.Height)),
                CommonColors.RadianceColor1,
                Pi,
                new Vector2(barTexture.Width / 2, barTexture.Height / 2),
                1,
                SpriteEffects.None,
                0);

            Rectangle rect = new Rectangle((int)drawPos.X - barTexture.Width / 2, (int)drawPos.Y - barTexture.Height / 2, barTexture.Width, barTexture.Height);
            if (rect.Contains(Main.MouseScreen.ToPoint()))
            {
                Vector2 textPos = Main.MouseScreen + Vector2.One * 16;
                LocalizedText radianceRequiredString;
                if (fill < 0.005f)
                    radianceRequiredString = LanguageManager.Instance.GetOrRegister($"{EncycloradiaUI.LOCALIZATION_PREFIX}.{nameof(TransmutationPage)}.LowRequiredRadiance");
                else
                    radianceRequiredString = LanguageManager.Instance.GetOrRegister($"{EncycloradiaUI.LOCALIZATION_PREFIX}.{nameof(TransmutationPage)}.RequiredRadiance").WithFormatArgs(fill * 100);

                textPos.X = Math.Min(Main.screenWidth - FontAssets.MouseText.Value.MeasureString(radianceRequiredString.Value).X - 6, textPos.X);
                Utils.DrawBorderStringFourWay(spriteBatch, Font, radianceRequiredString.Value, textPos.X, textPos.Y, Color.White * encycloradia.bookAlpha, Color.Black * encycloradia.bookAlpha, Vector2.Zero);
            }
        }
        private void DrawPage_Requirements(Encycloradia encycloradia, SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Utils.DrawBorderStringFourWay(spriteBatch, Font, recipe.transmutationRequirements.Count.ToString(), drawPos.X, drawPos.Y, Color.White * encycloradia.bookAlpha, Color.Black * encycloradia.bookAlpha, Font.MeasureString(recipe.transmutationRequirements.Count.ToString()) / 2);

            const int padding = 8; // i hate this
            Rectangle conditionRect = new Rectangle((int)(drawPos.X - (Font.MeasureString(recipe.transmutationRequirements.Count.ToString()).X + padding) / 2), (int)(drawPos.Y - (Font.MeasureString(recipe.transmutationRequirements.Count.ToString()).Y + padding) / 2), (int)Font.MeasureString(recipe.transmutationRequirements.Count.ToString()).X + padding, (int)Font.MeasureString(recipe.transmutationRequirements.Count.ToString()).Y + padding);
            if (conditionRect.Contains(Main.MouseScreen.ToPoint()))
            {
                Vector2 textPos = Main.MouseScreen + Vector2.One * 16;
                LocalizedText conditionString;
                if (recipe.transmutationRequirements.Count == 0)
                    conditionString = LanguageManager.Instance.GetOrRegister($"{EncycloradiaUI.LOCALIZATION_PREFIX}.NoSpecialRequirements");
                else
                    conditionString = LanguageManager.Instance.GetOrRegister($"{EncycloradiaUI.LOCALIZATION_PREFIX}.SpecialRequirements").WithFormatArgs(recipe.transmutationRequirements.Count);

                textPos.X = Math.Min(Main.screenWidth - FontAssets.MouseText.Value.MeasureString(conditionString.Value).X - 6, textPos.X);
                
                List<string> requirementTooltips = new List<string>() { conditionString.Value };
                recipe.transmutationRequirements.Select(x => "- " + x.tooltip.Value).ToList().ForEach(requirementTooltips.Add);
                Main.LocalPlayer.SetFakeHoverText(string.Join('\n', requirementTooltips));
            }
        }
        private void DrawPage_Lens(Encycloradia encycloradia, SpriteBatch spriteBatch, Vector2 drawPos)
        {
            int lens = ModContent.ItemType<ShimmeringGlass>();
            switch (recipe.lensRequired)
            {
                case ProjectorLensID.Pathos:
                    lens = ModContent.ItemType<LensofPathos>();
                    break;
            }
            RadianceDrawing.DrawHoverableItem(spriteBatch, lens, drawPos, 1, Color.White * encycloradia.bookAlpha, encycloradia: true);
        }
    }
}