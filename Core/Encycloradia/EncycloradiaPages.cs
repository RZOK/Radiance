using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Content.Items.RadianceCells;
using Radiance.Core.Systems;
using ReLogic.Graphics;
using static Radiance.Core.Systems.TransmutationRecipeSystem;

namespace Radiance.Core.Encycloradia
{
    public abstract class EncycloradiaPage
    {
        protected static DynamicSpriteFont Font => FontAssets.MouseText.Value;
        public int index = 0;
        public string text;

        public abstract void DrawPage(Encycloradia encycloradia, SpriteBatch spriteBatch, Vector2 drawPos, bool rightPage);
    }

    public class TextPage : EncycloradiaPage
    {
        public override void DrawPage(Encycloradia encycloradia, SpriteBatch spriteBatch, Vector2 drawPos, bool rightPage)
        {
            float xDrawOffset = 0;
            float yDrawOffset = 0;
            if (text != string.Empty)
            {
                foreach (string word in text.Split())
                {
                    bool parseMode = false;
                    foreach (char character in word)
                    {
                        if (character == EncycloradiaUI.ENCYCLORADIA_PARSE_CHARACTER)
                        {
                            parseMode = true;
                            continue;
                        }
                        if (parseMode)
                        {
                            switch (character)
                            {
                                case 'n': //newline
                                    xDrawOffset = 0;
                                    yDrawOffset += EncycloradiaUI.ENCYCLORADIA_PIXELS_BETWEEN_LINES;
                                    break;

                                case 'y': //radiance yellow
                                    encycloradia.drawnColor = CommonColors.RadianceColor1;
                                    encycloradia.drawnBGColor = CommonColors.RadianceColor1.GetDarkColor();
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
                                    encycloradia.drawnColor = CommonColors.InfluencingColor;
                                    encycloradia.drawnBGColor = CommonColors.InfluencingColor.GetDarkColor();
                                    break;

                                case 't': //transmutation lime
                                    encycloradia.drawnColor = CommonColors.TransmutationColor;
                                    encycloradia.drawnBGColor = CommonColors.TransmutationColor.GetDarkColor();
                                    break;

                                case 'a': //apparatuses blue
                                    encycloradia.drawnColor = CommonColors.ApparatusesColor;
                                    encycloradia.drawnBGColor = CommonColors.ApparatusesColor.GetDarkColor();
                                    break;

                                case 's': //instruments orange
                                    encycloradia.drawnColor = CommonColors.InstrumentsColor;
                                    encycloradia.drawnBGColor = CommonColors.InstrumentsColor.GetDarkColor();
                                    break;

                                case 'd': //pedestalworks purple
                                    encycloradia.drawnColor = CommonColors.PedestalworksColor;
                                    encycloradia.drawnBGColor = CommonColors.PedestalworksColor.GetDarkColor();
                                    break;

                                case 'h': //phenomena teal
                                    encycloradia.drawnColor = CommonColors.PhenomenaColor;
                                    encycloradia.drawnBGColor = CommonColors.PhenomenaColor.GetDarkColor();
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
                            }
                            parseMode = false;
                            continue;
                        }
                        float drawPosX = drawPos.X + xDrawOffset + 61 - (rightPage ? 0 : (yDrawOffset / 23));
                        float drawPosY = drawPos.Y + yDrawOffset + 56;
                        Vector2 lerpedPos = Vector2.Lerp(new Vector2(Main.screenWidth, Main.screenHeight) / 2, new Vector2(drawPosX, drawPosY), EaseOutExponent(encycloradia.bookAlpha, 4));
                        Utils.DrawBorderStringFourWay(spriteBatch, Font, character.ToString(), lerpedPos.X, lerpedPos.Y, encycloradia.drawnColor * encycloradia.bookAlpha, encycloradia.drawnBGColor * encycloradia.bookAlpha, Vector2.Zero, EncycloradiaUI.ENCYCLORADIA_LINE_SCALE);
                        xDrawOffset += Font.MeasureString(character.ToString()).X * EncycloradiaUI.ENCYCLORADIA_LINE_SCALE;
                    }
                    xDrawOffset += Font.MeasureString(" ").X * EncycloradiaUI.ENCYCLORADIA_LINE_SCALE;
                }
            }
        }
    }

    public class CategoryPage : EncycloradiaPage
    {
        public EntryCategory category = EntryCategory.None;
        public List<EncycloradiaEntry> entries => EncycloradiaSystem.EntriesByCategory[category];
        public int[] visualTimers = new int[EncycloradiaUI.ENCYCLORADIA_ENTRIES_PER_CATEGORY_PAGE];
        public bool[] ticks = new bool[EncycloradiaUI.ENCYCLORADIA_ENTRIES_PER_CATEGORY_PAGE];
        public CategoryPage(EntryCategory category) => this.category = category;
        public override void DrawPage(Encycloradia encycloradia, SpriteBatch spriteBatch, Vector2 drawPos, bool rightPage)
        {
            if (entries.Any())
            {
                List<EncycloradiaEntry> pagesToDraw = entries
                    .Where(x => x.visible == EntryVisibility.Visible || (x.visible == EntryVisibility.NotVisibleUntilUnlocked && x.unlockedStatus == UnlockedStatus.Unlocked))
                    .OrderByDescending(x => x.unread)
                    .ToList();

                int lower = (index - 1) * EncycloradiaUI.ENCYCLORADIA_ENTRIES_PER_CATEGORY_PAGE;
                int upper = Math.Min(EncycloradiaUI.ENCYCLORADIA_ENTRIES_PER_CATEGORY_PAGE, pagesToDraw.Count - (index - 1) * EncycloradiaUI.ENCYCLORADIA_ENTRIES_PER_CATEGORY_PAGE);
                pagesToDraw = pagesToDraw.GetRange(lower, upper);


                int yDistance = 0;
                Vector2 buttonOffset = new Vector2(74, 64);
                for (int i = 0; i < pagesToDraw.Count; i++)
                {
                    EncycloradiaEntry entry = pagesToDraw[i];
                    DrawButton(encycloradia, entry, spriteBatch, drawPos + buttonOffset + Vector2.UnitY * yDistance, i);
                    yDistance += EncycloradiaUI.ENCYCLORADIA_PIXELS_BETWEEN_ENTRY_BUTTONS;
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

            float timing = EaseInOutExponent(Math.Clamp((float)visualTimers[index] / (EncycloradiaUI.ENCYCLORADIA_ENTRY_BUTTON_MAX_ENTRY_BUTTON_VISUAL_TIMER * 2) + 0.5f, 0.5f, 1), 4);

            switch (entry.unlockedStatus)
            {
                case UnlockedStatus.Unlocked:
                    Main.instance.LoadItem(entry.icon);
                    tex = GetItemTexture(entry.icon);
                    if (Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().unreadEntires.Any(x => entry.name == x))
                        tex = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/UnreadAlert").Value;

                    text = entry.displayName;
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
                    text = "Incomplete Entry";

                    iconColor = Color.Black;
                    textColor = new Color(200, 200, 200, 255);
                    break;

                default:
                    tex = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/LockIcon").Value;
                    text = "Locked";

                    textColor = new Color(150, 150, 150, 255);
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

            HandleButtonHover(encycloradia, entry, frame, index);
            if (visualTimers[index] > 0)
            {
                RadianceDrawing.DrawSoftGlow(Main.screenPosition + drawPos, Color.White * (visualTimers[index] / EncycloradiaUI.ENCYCLORADIA_ENTRY_BUTTON_MAX_ENTRY_BUTTON_VISUAL_TIMER) * encycloradia.bookAlpha, 0.24f);
                //drawspike here
                //RadianceDrawing.DrawBeam(Main.screenPosition + drawPos, Main.screenPosition + drawPos + Vector2.UnitX * 300, Color.White.ToVector4() * visualsTimer / maxVisualTimer * UIParent.encycloradia.bookAlpha, 24, true);
            }
            Utils.DrawBorderStringFourWay(spriteBatch, font, text, drawPos.X + scaledTexSize.X / 2 + 4 * Math.Clamp(timing * 2, 1, 2f), drawPos.Y + 4, textColor, textBGColor, Vector2.UnitY * font.MeasureString(text).Y / 2);
            spriteBatch.Draw(tex, new Vector2(drawPos.X, drawPos.Y), null, iconColor, 0, tex.Size() / 2, scale * Math.Clamp(timing + 0.2f, 1, 1.2f), SpriteEffects.None, 0);
        }

        public void HandleButtonHover(Encycloradia encycloradia, EncycloradiaEntry entry, Rectangle frame, int index)
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
                        if (visualTimers[index] < EncycloradiaUI.ENCYCLORADIA_ENTRY_BUTTON_MAX_ENTRY_BUTTON_VISUAL_TIMER)
                            visualTimers[index]++;

                        if (Main.mouseLeft && Main.mouseLeftRelease)
                        {
                            if (Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().unreadEntires.Any(x => entry.name == x))
                                Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().unreadEntires.Remove(entry.name);

                            encycloradia.GoToEntry(entry);
                            visualTimers[index] = 0;
                            ticks[index] = false;

                            Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().unreadEntires.RemoveAll(x => x == entry.name);
                            SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/PageTurn"));
                        }
                        break;

                    case UnlockedStatus.Incomplete:
                        Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().currentFakeHoverText = $"Unlock this entry by {entry.unlock.unlockCondition}.";
                        break;
                }
            }
            else
            {
                if (visualTimers[index] > 0)
                    visualTimers[index]--;

                ticks[index] = false;
            }
        }
    }

    public class ImagePage : EncycloradiaPage
    {
        public Texture2D texture;

        public override void DrawPage(Encycloradia encycloradia, SpriteBatch spriteBatch, Vector2 drawPos, bool rightPage)
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

        public override void DrawPage(Encycloradia encycloradia, SpriteBatch spriteBatch, Vector2 drawPos, bool rightPage)
        {
            Vector2 pos = drawPos + new Vector2(EncycloradiaUI.ENCYCLORADIA_PIXELS_BETWEEN_PAGES / 2 + 36, encycloradia.UIParent.mainTexture.Height / 2 - 24);
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

    public class TransmutationPage : EncycloradiaPage
    {
        public TransmutationRecipe recipe = new TransmutationRecipe();
        public int currentItemIndex = 0;

        public override void DrawPage(Encycloradia encycloradia, SpriteBatch spriteBatch, Vector2 drawPos, bool rightPage)
        {
            Vector2 pos = drawPos + new Vector2(EncycloradiaUI.ENCYCLORADIA_PIXELS_BETWEEN_PAGES / 2 + 30, encycloradia.UIParent.mainTexture.Height / 2 - 20);
            Texture2D overlayTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/TransmutationOverlay").Value;
            Texture2D softGlow = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SoftGlowNoBG").Value;

            spriteBatch.Draw(overlayTexture, pos, null, Color.White * encycloradia.bookAlpha, 0, overlayTexture.Size() / 2, 1, SpriteEffects.None, 0);

            int currentItem = recipe.inputItems[(int)(Main.GameUpdateCount / 70) % recipe.inputItems.Length];

            Vector2 itemPos = pos - new Vector2(40, 81);
            Main.spriteBatch.Draw(softGlow, itemPos, null, Color.Black * 0.3f, 0, softGlow.Size() / 2, (float)(Item.GetDrawHitbox(currentItem, null).Width + Item.GetDrawHitbox(currentItem, null).Height) / 100, 0, 0);
            RadianceDrawing.DrawHoverableItem(spriteBatch, currentItem, itemPos, recipe.inputStack, Color.White * encycloradia.bookAlpha); //station

            Vector2 resultPos = pos + new Vector2(-40, 109);
            Main.spriteBatch.Draw(softGlow, resultPos, null, Color.Black * 0.3f, 0, softGlow.Size() / 2, (float)(Item.GetDrawHitbox(recipe.outputItem, null).Width + Item.GetDrawHitbox(recipe.outputItem, null).Height) / 100, 0, 0);
            RadianceDrawing.DrawHoverableItem(spriteBatch, recipe.outputItem, resultPos, recipe.outputStack, Color.White * encycloradia.bookAlpha); //result

            int cell = ModContent.ItemType<StandardRadianceCell>();
            if (recipe.requiredRadiance > 4000)
                cell = ModContent.ItemType<StandardRadianceCell>();

            BaseContainer cellContainer = new Item(cell).ModItem as BaseContainer;

            Vector2 cellPos = pos + new Vector2(58, 52);
            RadianceDrawing.DrawHoverableItem(spriteBatch, cell, cellPos, 1); //cell

            #region Required Radiance

            float maxRadiance = cellContainer.maxRadiance;
            float storedRadiance = recipe.requiredRadiance;

            Texture2D barTexture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/TransmutationOverlayBar").Value;

            float radianceCharge = Math.Min(storedRadiance, maxRadiance);
            float fill = radianceCharge / maxRadiance;

            Vector2 barPos = pos + new Vector2(58, -74);

            Main.spriteBatch.Draw(
                barTexture,
                barPos,
                new Rectangle(0, 0, barTexture.Width, (int)(Math.Max(0.05f, Math.Round(fill, 2)) * barTexture.Height)),
                CommonColors.RadianceColor1,
                Pi,
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
                Utils.DrawBorderStringFourWay(spriteBatch, Font, str, textPos.X, textPos.Y, Color.White * encycloradia.bookAlpha, Color.Black * encycloradia.bookAlpha, Vector2.Zero);
            }

            #endregion Required Radiance

            #region Requirements

            int conditionCount = 0;
            if (recipe.specialRequirements != null)
            {
                foreach (SpecialRequirements req in recipe.specialRequirements)
                {
                    conditionCount++;
                }
            }
            Vector2 conditionPos = pos + new Vector2(58, 143);
            Utils.DrawBorderStringFourWay(spriteBatch, Font, conditionCount.ToString(), conditionPos.X, conditionPos.Y, Color.White * encycloradia.bookAlpha, Color.Black * encycloradia.bookAlpha, Font.MeasureString(conditionCount.ToString()) / 2);

            const int padding = 8;
            Rectangle conditionRect = new Rectangle((int)(conditionPos.X - (Font.MeasureString(conditionCount.ToString()).X + padding) / 2), (int)(conditionPos.Y - (Font.MeasureString(conditionCount.ToString()).Y + padding) / 2), (int)Font.MeasureString(conditionCount.ToString()).X + padding, (int)Font.MeasureString(conditionCount.ToString()).Y + padding);
            if (conditionRect.Contains(Main.MouseScreen.ToPoint()))
            {
                Vector2 textPos = Main.MouseScreen + Vector2.One * 16;
                string str = "This recipe has " + (conditionCount == 0 ? "no special requirements" : (conditionCount + " special requirement" + (conditionCount != 1 ? " " : "s ") + "that must be met"));
                textPos.X = Math.Min(Main.screenWidth - FontAssets.MouseText.Value.MeasureString(str).X - 6, textPos.X);
                Utils.DrawBorderStringFourWay(spriteBatch, Font, str, textPos.X, textPos.Y, Color.White * encycloradia.bookAlpha, Color.Black * encycloradia.bookAlpha, Vector2.Zero);
                if (conditionCount > 0)
                {
                    foreach (SpecialRequirements req in recipe.specialRequirements)
                    {
                        const int distance = 24;
                        textPos.Y += distance;
                        textPos.X = Math.Min(Main.screenWidth - FontAssets.MouseText.Value.MeasureString("— " + reqStrings[req]).X - 6, textPos.X);
                        Utils.DrawBorderStringFourWay(spriteBatch, Font, "— " + reqStrings[req], textPos.X, textPos.Y, Color.White * encycloradia.bookAlpha, Color.Black * encycloradia.bookAlpha, Vector2.Zero);
                    }
                }
            }

            #endregion Requirements

            #region Lens

            int lens = ModContent.ItemType<ShimmeringGlass>();
            switch (recipe.lensRequired)
            {
                case ProjectorLensID.Pathos:
                    lens = ModContent.ItemType<LensofPathos>();
                    break;
            }
            Vector2 lensPos = pos - new Vector2(40, -10);
            RadianceDrawing.DrawHoverableItem(spriteBatch, lens, lensPos, 1, Color.White * encycloradia.bookAlpha); //lens

            #endregion Lens
        }
    }
}