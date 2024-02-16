using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;
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
        public override void DrawPage(Encycloradia encycloradia, SpriteBatch spriteBatch, Vector2 drawPos, bool rightPage, bool doDraw)
        {
            encycloradia.leftPage = encycloradia.currentEntry.pages.Find(n => n.index == 0);
            encycloradia.rightPage = encycloradia.currentEntry.pages.Find(n => n.index == 1);
            foreach (CategoryButton x in encycloradia.parentElements.Where(n => n is CategoryButton))
            {
                x.DrawStuff(spriteBatch, drawPos);
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
    }
}