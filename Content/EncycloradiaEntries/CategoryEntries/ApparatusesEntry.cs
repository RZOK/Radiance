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
            visible = false;
        }

        public override void PageAssembly()
        {
            AddToEntry(this,
            new TextPage()
            {
                text = new CustomTextSnippet[] 
                {
                    "A".BWSnippet(),
                    "contraption".DarkColorSnippet(CommonColors.ApparatusesColor),
                    "of unknown potential and workings that remains intriguing to those who gaze upon it. |".BWSnippet(),
                    CommonSnippets.apparatusesSnippet,
                    "are tiles that utilize".BWSnippet(),
                    CommonSnippets.radianceSnippet,
                    "to perform various actions. |".BWSnippet(),
                    "Within this section you will find most".BWSnippet(),
                    "Radiance-utilizing".DarkColorSnippet(CommonColors.RadianceColor1),
                    "machines that you may create.".BWSnippet(),
                }
            });
            AddToEntry(this, new CategoryPage() { category = EntryCategory.Apparatuses });
        }
    }
}