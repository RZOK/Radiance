using Terraria.ModLoader;
using Radiance.Content.Items.ProjectorLenses;
using static Radiance.Core.Systems.UnlockSystem;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using Radiance.Core;
using Radiance.Utilities;
using Microsoft.Xna.Framework;

namespace Radiance.Content.EncycloradiaEntries
{
    public class LensofPathosEntry : EncycloradiaEntry
    {
        public override void SetDefaults()
        {
            displayName = "Lens of Pathos";
            tooltip = "Nothing for the other two";
            fastNavInput = "ULRU";
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.Transmutation;
            icon = ModContent.ItemType<LensofPathos>();
            visible = true;
        }
        public override void PageAssembly()
        {
            AddToEntry(this, new TextPage() { text = new CustomTextSnippet[] 
            { 
                new CustomTextSnippet("As mentioned in the", Color.White, Color.Black),
                new CustomTextSnippet("'Basic Influencing'", RadianceUtils.ContextColor, RadianceUtils.ContextColorDark),
                new CustomTextSnippet("entry within the", Color.White, Color.Black),
                new CustomTextSnippet("Encycloradia,", RadianceUtils.RadianceColor1, RadianceUtils.RadianceColorDark),
                RadianceUtils.radianceSnippet,
                new CustomTextSnippet("functions not only as a resource itself, but may also serve as a carrier for other ethereal matter. The", Color.White, Color.Black),
                new CustomTextSnippet("Lens of Pathos", RadianceUtils.ContextColor, RadianceUtils.ContextColorDark),
                new CustomTextSnippet("is the first alternate Projector lens that puts this fact to use. NEWLINE NEWLINE", Color.White, Color.Black),
                new CustomTextSnippet("The", Color.White, Color.Black),
                new CustomTextSnippet("Lens of Pathos", RadianceUtils.ContextColor, RadianceUtils.ContextColorDark),
                new CustomTextSnippet("is similar to a standard Flareglass lens, however this one is significantly less restrictive on what it will let shine through. It utilizes", Color.White, Color.Black),
                new CustomTextSnippet("the essence of emotions", RadianceUtils.ContextColor, RadianceUtils.ContextColorDark),
                new CustomTextSnippet("mixed into the", Color.White, Color.Black),
                RadianceUtils.radianceSnippet,
                new CustomTextSnippet("being projected in order to transmutate something that normally would not turn into anything without them. NEWLINE", Color.White, Color.Black),
                new CustomTextSnippet("Unfortunately, this process requires two additional resources: A", Color.White, Color.Black),
                new CustomTextSnippet("vessel to hold the emotions,", RadianceUtils.ContextColor, RadianceUtils.ContextColorDark),
                new CustomTextSnippet("and the actual emotions to", Color.White, Color.Black),
                new CustomTextSnippet("fill", RadianceUtils.ContextColor, RadianceUtils.ContextColorDark),
                new CustomTextSnippet("said vessel. NEWLINE NEWLINE", Color.White, Color.Black),

            } } );
        }
    }
}
