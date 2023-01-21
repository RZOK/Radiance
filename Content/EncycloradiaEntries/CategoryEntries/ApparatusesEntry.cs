using Microsoft.Xna.Framework;
using Radiance.Core;
using Radiance.Utilities;
using Terraria.ID;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Content.EncycloradiaEntries
{
    public class ApparatusesEntry : EncycloradiaEntry
    {
        public override void SetDefaults()
        {
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.Apparatuses;
            icon = ItemID.ManaCrystal;
            visible = false;
        }

        public override void PageAssembly()
        {
            AddToEntry(this,
            new TextPage()
            {
                text = new CustomTextSnippet[] 
                {
                    new CustomTextSnippet("A", Color.White, Color.Black),
                    new CustomTextSnippet("contraption", CommonColors.ApparatusesColor, CommonColors.ApparatusesColorDark),
                    new CustomTextSnippet("of unknown potential and workings that remains intriguing to those who gaze upon it. |", Color.White, Color.Black),
                    RadianceUtils.apparatusesSnippet,
                    new CustomTextSnippet("are tiles that utilize", Color.White, Color.Black),
                    RadianceUtils.radianceSnippet,
                    new CustomTextSnippet("to perform various actions. |", Color.White, Color.Black),
                    new CustomTextSnippet("Within this section you will find most", Color.White, Color.Black),
                    new CustomTextSnippet("Radiance-utilizing", CommonColors.RadianceColor1, CommonColors.RadianceColorDark),
                    new CustomTextSnippet("machines that you may create.", Color.White, Color.Black),
                }
            });
            AddToEntry(this, new CategoryPage() { category = EntryCategory.Apparatuses });
        }
    }
}