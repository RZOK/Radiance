using Microsoft.Xna.Framework.Input;
using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;
using ReLogic.Graphics;
using Steamworks;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Content.EncycloradiaEntries
{
    public class TitleEntry : EncycloradiaEntry
    {
        public TitleEntry()
        {
            displayName = "Title";
            incomplete = UnlockCondition.unlockedByDefault;
            unlock = UnlockCondition.unlockedByDefault;
            category = EntryCategory.None;
            visible = EntryVisibility.NotVisible;

            AddPageToEntry(new TextPage()
            {
                text =
                "Welcome to the &yEncycloradia.&r&n&n" +
                "Click on a category to the right in order to view its associated entries.&n&n" +
                "If an entry is &glocked,&r you will be unable to view it until it is unlocked.&n&n" +
                "&bTip of the Day:&r&n" +
                Main.rand.Next(Tips)
            });
            AddPageToEntry(new TitlePage());
        }

        private string[] Tips = {
            //useful tips
            "If two rays intersect, they will both glow red and have their transfer rate significantly reduced. Plan around this!",
            "Most &aApparatuses&r will cease to function if powered wire is running through the top left tile portion of them.",
            "Hovering your mouse over an incomplete entry will reveal to you the method of unlocking it.",
            "Holding SHIFT while clicking a category will automatically mark all unread entries in it as read.",
            "Holding SHIFT while hovering over an &aApparatus&r with an area of effect will pause the breathing of the indicator circle.",

            //real life fact tips
            @"The speed of light in a vacuum is 299,792,458 meters per second.",
            @"Blue light is said to help people relax.",
        };
    }

    public class TitlePage : EncycloradiaPage
    {
        public int[] visualTimers = new int[6];
        public bool[] ticks = new bool[6];
        public static readonly int VISUAL_TIMER_MAX = 10;
        public override void DrawPage(Encycloradia encycloradia, SpriteBatch spriteBatch, Vector2 drawPos, bool rightPage, bool doDraw)
        {
            encycloradia.leftPage = encycloradia.currentEntry.pages.Find(n => n.index == 0);
            encycloradia.rightPage = encycloradia.currentEntry.pages.Find(n => n.index == 1);
            DrawButton(encycloradia, spriteBatch, drawPos, EntryCategory.Influencing, CommonColors.InfluencingColor, 0);
            DrawButton(encycloradia, spriteBatch, drawPos, EntryCategory.Transmutation, CommonColors.TransmutationColor, 1);
            DrawButton(encycloradia, spriteBatch, drawPos, EntryCategory.Apparatuses, CommonColors.ApparatusesColor, 2);
            DrawButton(encycloradia, spriteBatch, drawPos, EntryCategory.Instruments, CommonColors.InstrumentsColor, 3);
            DrawButton(encycloradia, spriteBatch, drawPos, EntryCategory.Pedestalworks, CommonColors.PedestalworksColor, 4);
            DrawButton(encycloradia, spriteBatch, drawPos, EntryCategory.Phenomena, CommonColors.PhenomenaColor, 5);

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
            int horizontalPadding = 140;
            int verticalPadding = 126;
            drawPos += new Vector2(horizontalPadding * (index % 2 + 1), 116 + verticalPadding * (index / 2));
            string textureString = category.ToString();
            bool HasUnread = Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().unreadEntires.Any(x => FindEntry(x).category == category);
            Texture2D tex = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/" + textureString + "Symbol").Value;
            Texture2D alertTex = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/UnreadAlert").Value;
            Rectangle frame = new Rectangle((int)(drawPos.X - tex.Width / 2), (int)(drawPos.Y - tex.Height / 2), tex.Width, tex.Height);
            float timing = EaseInOutExponent(Math.Min((float)visualTimers[index] / (VISUAL_TIMER_MAX * 2) + 0.5f, 1), 4);
            Color realColor = color * timing;

            spriteBatch.Draw(tex, drawPos, null, realColor * encycloradia.bookAlpha, 0, tex.Size() / 2f, Math.Clamp(timing + 0.3f, 1, 1.3f), SpriteEffects.None, 0);

            if (HasUnread)
                spriteBatch.Draw(alertTex, drawPos + new Vector2(tex.Width, -tex.Height) / 2 - new Vector2(8, -8), null, Color.White * encycloradia.bookAlpha * (1 - visualTimers[index] / VISUAL_TIMER_MAX), 0, alertTex.Size() / 2, Math.Clamp(timing + 0.3f, 1, 1.3f), SpriteEffects.None, 0);

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
                Utils.DrawBorderStringFourWay(Main.spriteBatch, font, textureString, drawPos.X, drawPos.Y, realColor * timing * 2f, realColor.GetDarkColor() * timing, font.MeasureString(textureString) / 2, timing);
            }
        }
    }
}