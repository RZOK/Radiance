using Radiance.Content.Items.ProjectorLenses;
using Radiance.Content.Items.Tools.Misc;
using System.Collections.Generic;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Content.EncycloradiaEntries.Transmutation
{
    public class LensofPathosEntry : EncycloradiaEntry
    {
        public LensofPathosEntry()
        {
            displayName = "Lens of Pathos";
            tooltip = "Nothing for the other two";
            fastNavInput = "ULRU";
            incomplete = UnlockBoolean.unlockedByDefault;
            unlock = UnlockBoolean.unlockedByDefault;
            category = EntryCategory.Transmutation;
            icon = ModContent.ItemType<LensofPathos>();
            visible = true;

            AddToEntry(this, new TextPage()
            {
                text =
                @"As mentioned in the \b 'Basic Influencing' \r entry, \y Radiance \r functions not only as a resource itself, but may also serve as a carrier for other ethereal matter. The \b Lens of Pathos \r is the first alternate Projector lens that puts this fact to use. | " +
                @"The \b Lens of Pathos \r is similar to an ordinary Flareglass lens, but with a clearer composure to loosen the restrictions on what may pass through. By using a fired \y Radiance \r " +
                @"blast as a hammer of sorts to 'nail' the essence of emotions trapped within the Transmutator, the capabilities of what you can use Transmutation for are significantly increased. Unfortunately, this process requires two additional resources: A \b vessel \r to hold the contents, and the concentrated ephemeral essence of emotions to fill said vessel. | " +
                @"The vessel required is something that can actually feel emotion, but is weak enough to have its life force dragged from its body in order to act as the container. One type of creature fits this perfectly: Critters. | " +
                @"By hovering over the Transmutator while a \b Lens of Pathos \r is inserted into a Projector below will reveal the radius in which an alive full-sized critter (bunnies, squirrels, and birds) will be slain when the crafting process begins in order to supply its life as a vessel for emotions. | " +
                @"The next situation to figure out is how to get the emotional essence required in the first place. | " +
                @"However, this issue is already solvable with your current knowledge. | " +
                @"\b Potion Dispersal \r is your answer to forcibly creating emotions. As it turns out, buff potions not only supply the mental or physical enhancements that they are known to give, but can also manipulate the feelings of a consumer, and this is a fact that remains true when the syrupy liquid is in its gaseous form through Potion Dispersal. The \y Radiance \r fired from the Projector will absorb the emotions from the vapor and carry them into the object upon Transmutation, opening the door for many new objects to be created that required this additional substance. | " +
                @"The emotions carried by potions are less varied than you may think, and are divided into four color categories based on what is given. | " +
                @"\1 Scarlet \r emotions involve desires of delivering harm unto, or reducing harm from, other creatures. | " +
                @"\2 Cerulean \r emotions are associated with calmness, creativity, and vitality. | " +
                @"\3 Verdant \r emotions are those related to cognitive abilities and insightfulness. | " +
                @"Lastly, \4 Mauve \r emotions are based around pride and the ego from posessing abilities that others do not. | You can view what potion effect is aligned with what color by viewing the color of the timer above a Transmutator that is actively dispersing a potion, or by creating an " +
                @"\b Alchemical Lens \r to conveniently view the color aligned to a potion without the need to consume it. | " +
                @"Each recipe involving the \b Lens of Pathos \r has a required emotion color that has to actively be in the Transmutator at the time that the craft goes through, and a required minimum duraton of that effect, as a measure for how much of it will be absorbed into the transmutated object. Any potion that grants the necessary color can be used, but you cannot mix and match potions of identical color. Failure to meet the required duration or color at the time of the craft will not stop it from going through, resulting in lost Radiance and an unchanged item."
            });
            AddToEntry(this, new RecipePage()
            {
                items = new Dictionary<int, int>()
                {
                    { ItemID.CobaltBar, 5 },
                    { ModContent.ItemType<ShimmeringGlass>(), 5 }
                },
                station = GetItem(ItemID.MythrilAnvil),
                result = GetItem(ModContent.ItemType<AlchemicalLens>())
            });
        }
    }
}