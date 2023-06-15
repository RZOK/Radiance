using Radiance.Core.Systems;
using ReLogic.Graphics;
using System.Collections.Generic;
using Terraria.UI;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;

namespace Radiance.Content.UI.NewEntryAlert
{
    internal class NewEntryAlertUI : SmartUIState
    {
        public static NewEntryAlertUI Instance { get; set; }

        public static List<EntryAlertText> unlockedEntries
        {
            get => UnlockSystem.unlockedEntries;
            set => UnlockSystem.unlockedEntries = value;
        }

        public NewEntryAlertUI()
        {
            Instance = this;
        }

        public float Timer
        {
            get => Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().newEntryUnlockedTimer;
            set => Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().newEntryUnlockedTimer = value;
        }

        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));

        public override bool Visible => Timer > 0;

        public override void OnInitialize()
        {
            //foreach(var entry in entries.Where(x => x.visible == true))
            //{
            //    AddEntryButton(entry);
            //}
            //AddCategoryButtons();
            //encycloradiaOpenButton.Left.Set(-85, 0);
            //encycloradiaOpenButton.Top.Set(240, 0);
            //encycloradiaOpenButton.Width.Set(34, 0);
            //encycloradiaOpenButton.Height.Set(34, 0);
            //Append(encycloradiaOpenButton);

            //encycloradia.Left.Set(0, 0.5f);
            //encycloradia.Top.Set(0, 0.5f);
            //encycloradia.parentElements = Elements;

            //Append(encycloradia);
            //encycloradia.leftPage = encycloradia.currentEntry.pages.Find(n => n.number == 0);
            //encycloradia.rightPage = encycloradia.currentEntry.pages.Find(n => n.number == 1);
        }

        public override void Update(GameTime gameTime)
        {
            if (!Main.gamePaused && Main.hasFocus && Visible)
            {
                if (oldLength < unlockedEntries.Count)
                {
                    easeTimer = 30;
                    oldLength = unlockedEntries.Count;
                }
                if (easeTimer > 0)
                    easeTimer--;
                if (Timer > 0)
                    Timer--;
            }
            if (!Visible)
            {
                overflowAlpha = 0;
                oldPos = 0;
                easeTimer = 30;
                unlockedEntries.Clear();
                oldLength = 0;
            }
            base.Update(gameTime);
        }

        public const int timerMax = 600;
        public const int fadeIn = 45;
        public const int fadeOut = 45;

        public float easeTimer = 30;
        public const int easeTimerMax = 30;
        public float oldPos = 0;
        public float overflowAlpha = 0;
        public int oldLength = 0;

        public override void Draw(SpriteBatch spriteBatch)
        {
            DynamicSpriteFont font = FontAssets.MouseText.Value;
            Texture2D outerTopTexture = ModContent.Request<Texture2D>("Radiance/Content/UI/NewEntryAlert/NewEntryAlertOuterTop").Value;
            Texture2D outerBottomTexture = ModContent.Request<Texture2D>("Radiance/Content/UI/NewEntryAlert/NewEntryAlertOuterBottom").Value;
            Texture2D innerTopTexture = ModContent.Request<Texture2D>("Radiance/Content/UI/NewEntryAlert/NewEntryAlertInnerTop").Value;
            Texture2D innerBottomTexture = ModContent.Request<Texture2D>("Radiance/Content/UI/NewEntryAlert/NewEntryAlertInnerBottom").Value;

            Vector2 drawModifier = Vector2.UnitX * outerTopTexture.Width;
            if (Timer > timerMax - fadeIn)
                drawModifier *= EaseInCirc(Math.Min(fadeIn, Timer - timerMax + fadeIn) / fadeIn);
            else
                drawModifier *= EaseInOutCirc(1 - Math.Min(fadeOut, Timer) / fadeOut);
            Vector2 drawPos = new Vector2(Main.screenWidth, Main.screenHeight - 110) + drawModifier;
            bool hasOneExtraAccessorySlot = Main.LocalPlayer.CanDemonHeartAccessoryBeShown() || Main.LocalPlayer.CanMasterModeAccessoryBeShown();
            bool hasTwoExtraAccessorySlots = Main.LocalPlayer.CanDemonHeartAccessoryBeShown() && Main.LocalPlayer.CanMasterModeAccessoryBeShown();
            if (hasOneExtraAccessorySlot && Main.playerInventory)
                drawPos += Vector2.UnitY * (hasTwoExtraAccessorySlots ? 36 : 46); //shift down if extra slots are drawn

            const int distBetweenEntries = 25;
            const int startingDistance = 52;
            float topOffset = -startingDistance - Math.Min(unlockedEntries.Count, 11) * distBetweenEntries;
            if (easeTimer <= 0 || Timer == timerMax - 1)
            {
                if (Timer == timerMax - 1)
                    SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/EntryUnlock"));

                oldPos = topOffset;
            }
            float lerpedPos = MathHelper.Lerp(oldPos, topOffset, EaseInOutCirc(1 - (easeTimer / easeTimerMax)));
            if (Main.playerInventory)
                lerpedPos = -startingDistance - Math.Min(unlockedEntries.Count * 25, distBetweenEntries);
            for (int i = 0; i < 2; i++)
            {
                spriteBatch.Draw(
                    i == 0 ? outerTopTexture : innerTopTexture,
                    drawPos + new Vector2(i == 0 ? -outerTopTexture.Width / 2 : 18 - innerTopTexture.Width / 2, lerpedPos + (hasTwoExtraAccessorySlots && Main.playerInventory ? 60 : 0)),
                    null,
                    i == 0 ? Color.White : Color.Lerp(CommonColors.RadianceColor1, CommonColors.RadianceColor2, SineTiming(30)),
                    0,
                    i == 0 ? outerTopTexture.Size() / 2 : new Vector2(outerTopTexture.Width, innerTopTexture.Height) / 2,
                    1,
                    SpriteEffects.None, 0);

                spriteBatch.Draw(
                    i == 0 ? outerBottomTexture : innerBottomTexture,
                    drawPos + new Vector2(i == 0 ? -outerBottomTexture.Width / 2 : 18 - innerBottomTexture.Width / 2, 16),
                    null,
                    i == 0 ? Color.White : Color.Lerp(CommonColors.RadianceColor1, CommonColors.RadianceColor2, SineTiming(30)),
                    0,
                    i == 0 ? outerBottomTexture.Size() / 2 : new Vector2(outerBottomTexture.Width, innerBottomTexture.Height) / 2,
                    1,
                    SpriteEffects.None, 0);
            }
            if (Timer != timerMax - 1)
            {
                bool visible = true;
                if (hasTwoExtraAccessorySlots && Main.playerInventory)
                    visible = false;
                if (visible)
                {
                    RadianceDrawing.DrawSoftGlow(Main.screenPosition + drawPos + new Vector2(-lerpedPos / 2, (lerpedPos + 16) / 2), Color.Lerp(CommonColors.RadianceColor1, CommonColors.RadianceColor2, SineTiming(60)) * 0.8f, lerpedPos / 50, RadianceDrawing.SpriteBatchData.UIDrawingDataScale);
                    for (int i = 0; i < 2; i++)
                    {
                        Texture2D texture = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/InventoryIcon").Value;
                        spriteBatch.Draw(texture, drawPos + new Vector2(i == 0 ? -240 : -20, lerpedPos + 34), null, Color.White, 0, texture.Size() / 2, 1, SpriteEffects.None, 0);
                    }
                    Utils.DrawBorderStringFourWay(spriteBatch, font, "New Entries Unlocked!", drawPos.X - 220, drawPos.Y + lerpedPos + 24, CommonColors.RadianceColor1, Color.Black, Vector2.Zero, 1);
                }
            }
            for (int i = 0; i < unlockedEntries.Count; i++)
            {
                EntryAlertText alert = unlockedEntries[i];
                if (i == 10)
                {
                    if (overflowAlpha < 255)
                        overflowAlpha = 255 * ((overflowAlpha + 0.15f * overflowAlpha + 0.15f) / 255);
                    if (!Main.playerInventory)
                    {
                        string overflowString = "...plus " + (unlockedEntries.Count - 10) + " more.";
                        Utils.DrawBorderStringFourWay(spriteBatch, font, overflowString, drawPos.X - 200, drawPos.Y, Color.Gray * (overflowAlpha / 255), Color.Black * (overflowAlpha / 255), Vector2.UnitY * font.MeasureString(overflowString).Y, 1);
                    }
                    break;
                }
                Color color = Color.White;
                switch (alert.entry.category)
                {
                    case EntryCategory.Influencing:
                        color = CommonColors.InfluencingColor;
                        break;

                    case EntryCategory.Transmutation:
                        color = CommonColors.TransmutationColor;
                        break;

                    case EntryCategory.Apparatuses:
                        color = CommonColors.ApparatusesColor;
                        break;

                    case EntryCategory.Instruments:
                        color = CommonColors.InstrumentsColor;
                        break;

                    case EntryCategory.Pedestalworks:
                        color = CommonColors.PedestalworksColor;
                        break;

                    case EntryCategory.Phenomena:
                        color = CommonColors.PhenomenaColor;
                        break;
                }
                alert.pos.X = drawPos.X - 230;
                alert.pos.Y = drawPos.Y + lerpedPos + startingDistance + distBetweenEntries * (i + 1);
                if (alert.alpha < 255)
                    alert.alpha = 255 * ((alert.alpha + 0.15f * alert.alpha + 0.15f) / 255);
                if (!Main.playerInventory)
                    Utils.DrawBorderStringFourWay(spriteBatch, font, alert.entry.displayName, alert.pos.X, alert.pos.Y, color * (alert.alpha / 255), color.GetDarkColor() * (alert.alpha / 255), Vector2.UnitY * font.MeasureString(alert.entry.displayName).Y, 1);
            }
            if (unlockedEntries.Count > 0 && Main.playerInventory && !hasTwoExtraAccessorySlots)
            {
                string overflowString = unlockedEntries.Count + (unlockedEntries.Count == 1 ? " entry" : " entries") + " unlocked.";
                Utils.DrawBorderStringFourWay(spriteBatch, font, overflowString, drawPos.X - 230, drawPos.Y, Color.DarkGray, Color.Black, Vector2.UnitY * font.MeasureString(overflowString).Y, 1);
            }
            Recalculate();
            base.Draw(spriteBatch);
        }
    }

    public class EntryAlertText
    {
        public float alpha = 0;
        public EncycloradiaEntry entry;
        public Vector2 pos;

        public EntryAlertText(EncycloradiaEntry assignedEntry)
        {
            entry = assignedEntry;
        }
    }
}