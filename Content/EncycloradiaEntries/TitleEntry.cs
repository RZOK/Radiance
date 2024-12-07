using Microsoft.Xna.Framework.Input;
using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;
using ReLogic.Graphics;
using Terraria.Localization;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;

namespace Radiance.Content.EncycloradiaEntries
{
    public class TitleEntry : EncycloradiaEntry
    {
        public const int TIP_COUNT = 7;
        private int selectedTip = Main.rand.Next(TIP_COUNT);
        public TitleEntry()
        {
            incomplete = UnlockCondition.UnlockedByDefault;
            unlock = UnlockCondition.UnlockedByDefault;
            category = EntryCategory.None;
            visible = EntryVisibility.NotVisible;
            for (int i = 0; i < TIP_COUNT; i++)
            {
                LanguageManager.Instance.GetOrRegister($"Mods.{mod.Name}.Encycloradia.Entries.{GetUninitializedEntryName(this)}.Tip_{i}");
            }
            pages = [
                new TextPage() { keys = new LocalizedText[] 
                { 
                    LanguageManager.Instance.GetOrRegister($"Mods.{mod.Name}.Encycloradia.Entries.{GetUninitializedEntryName(this)}.TextPage_0"), 
                    LanguageManager.Instance.GetOrRegister($"Mods.{mod.Name}.Encycloradia.Entries.{GetUninitializedEntryName(this)}.Tip_{selectedTip}") 
                } },
                new TitlePage()
            ];
        }
    }
    public class TitlePage : EncycloradiaPage
    {
        public int[] visualTimers = new int[6];
        public bool[] ticks = new bool[6];
        public const int VISUAL_TIMER_MAX = 10;
        public const int BUTTON_HORIZONTAL_PADDING = 140;
        public const int BUTTON_VERTICAL_PADDING = 126;
        public const int BUTTON_VERTICAL_OFFSET = 116;
        public static readonly Color[] categoryColors = new Color[] 
        { 
            CommonColors.InfluencingColor,
            CommonColors.TransmutationColor,
            CommonColors.ApparatusesColor,
            CommonColors.InstrumentsColor,
            CommonColors.PedestalworksColor,
            CommonColors.PhenomenaColor 
        };
        public override void DrawPage(Encycloradia encycloradia, SpriteBatch spriteBatch, Vector2 drawPos, bool rightPage, bool actuallyDrawPage)
        {
            if (!actuallyDrawPage)
                return;

            for (int i = 0; i < Enum.GetNames(typeof(EntryCategory)).Length - 1; i++)
            {
                DrawButton(encycloradia, spriteBatch, drawPos, (EntryCategory)(i + 1), categoryColors[i], i);
            }

            if (DateTime.Today.Month == 3 && DateTime.Today.Day == 31)
            {
                Vector2 flagDrawPos = drawPos - new Vector2(300, -400);
                Texture2D tex = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/flalg").Value;
                Texture2D shadowTex = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/flalgShadow").Value;
                spriteBatch.Draw(shadowTex, flagDrawPos + new Vector2(-2, 4), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                spriteBatch.Draw(tex, flagDrawPos, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                Rectangle frame = new Rectangle((int)flagDrawPos.X, (int)flagDrawPos.Y, tex.Width, tex.Height);
                if (frame.Contains(Main.MouseScreen.ToPoint()))
                {
                    Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().currentFakeHoverText = "[c/FC92E5:Happy Transgender Day of Visibility!]";
                    Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().fancyHoverTextBackground = true;
                }
            }
        }

        public void DrawButton(Encycloradia encycloradia, SpriteBatch spriteBatch, Vector2 drawPos, EntryCategory category, Color color, int index)
        {
            drawPos += new Vector2(BUTTON_HORIZONTAL_PADDING * (index % 2 + 1), BUTTON_VERTICAL_OFFSET + BUTTON_VERTICAL_PADDING * (index / 2));

            bool HasUnread = Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().unreadEntires.Any(x => IsUnread(x, category));
            float timing = EaseInOutExponent(Math.Min((float)visualTimers[index] / (VISUAL_TIMER_MAX * 2) + 0.5f, 1), 4);

            string textureString = category.ToString();
            Texture2D tex = ModContent.Request<Texture2D>($"Radiance/Core/Encycloradia/Assets/{textureString}Symbol").Value;
            Rectangle frame = new Rectangle((int)(drawPos.X - tex.Width / 2), (int)(drawPos.Y - tex.Height / 2), tex.Width, tex.Height);
            Color realColor = color * timing;
            spriteBatch.Draw(tex, drawPos, null, realColor * encycloradia.bookAlpha, 0, tex.Size() / 2f, Math.Clamp(timing + 0.3f, 1, 1.3f), SpriteEffects.None, 0);

            if (HasUnread)
            {
                Texture2D alertTex = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/UnreadAlert").Value;
                spriteBatch.Draw(alertTex, drawPos + new Vector2(tex.Width, -tex.Height) / 2 - new Vector2(8, -8), null, Color.White * encycloradia.bookAlpha * (1f - (float)visualTimers[index] / VISUAL_TIMER_MAX), 0, alertTex.Size() / 2, Math.Clamp(timing + 0.3f, 1, 1.3f), SpriteEffects.None, 0);
            }
            if (frame.Contains(Main.MouseScreen.ToPoint()))
            {
                if (!ticks[index])
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    ticks[index] = true;
                }
                if (visualTimers[index] < VISUAL_TIMER_MAX)
                    visualTimers[index]++;

                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    if (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift))
                    {
                        Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().unreadEntires.RemoveAll(x => FindEntry(x).category == category);
                        SoundEngine.PlaySound(SoundID.MenuTick);
                    }
                    else
                    {
                        visualTimers = new int[6];
                        ticks = new bool[6];
                        encycloradia.GoToEntry(FindEntry(textureString + "Entry"));
                        SoundEngine.PlaySound(EncycloradiaUI.pageTurnSound);
                    }
                }
            }
            else
            {
                ticks[index] = false;
                if (visualTimers[index] > 0)
                    visualTimers[index]--;
            }
            if (visualTimers[index] > 0)
            {
                DynamicSpriteFont font = FontAssets.MouseText.Value;
                string categoryString = LanguageManager.Instance.GetOrRegister($"Mods.Radiance.CommonStrings.Categories.{textureString}", () => textureString).Value;
                Utils.DrawBorderStringFourWay(Main.spriteBatch, font, categoryString, drawPos.X, drawPos.Y, realColor * timing * 2f, realColor.GetDarkColor() * timing, font.MeasureString(categoryString) / 2, timing);
            }
        }
        internal static bool IsUnread(string name, EntryCategory category)
        {
            EncycloradiaEntry entry = FindEntry(name);
            return entry.unlockedStatus == UnlockedStatus.Unlocked && entry.category == category;
        }
    }
}
