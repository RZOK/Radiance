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
                    CommonSnippets.BWSnippet("A"),
                    new CustomTextSnippet("contraption", CommonColors.ApparatusesColor, CommonColors.ApparatusesColorDark),
                    CommonSnippets.BWSnippet("of unknown potential and workings that remains intriguing to those who gaze upon it. |"),
                    CommonSnippets.apparatusesSnippet,
                    CommonSnippets.BWSnippet("are tiles that utilize"),
                    CommonSnippets.radianceSnippet,
                    CommonSnippets.BWSnippet("to perform various actions. |"),
                    CommonSnippets.BWSnippet("Within this section you will find most"),
                    new CustomTextSnippet("Radiance-utilizing", CommonColors.RadianceColor1, CommonColors.RadianceColorDark),
                    CommonSnippets.BWSnippet("machines that you may create."),
                }
            });
            AddToEntry(this, new CategoryPage() { category = EntryCategory.Apparatuses });
        }
    }
}