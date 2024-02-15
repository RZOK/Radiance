using Radiance.Content.Items.ProjectorLenses;
using Radiance.Content.Items.Tools.Misc;
using Radiance.Core.Encycloradia;
using Radiance.Core.Systems;
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
            incomplete = UnlockCondition.unlockedByDefault;
            unlock = UnlockCondition.unlockedByDefault;
            category = EntryCategory.Transmutation;
            icon = ModContent.ItemType<LensofPathos>();
            visible = EntryVisibility.Visible;

            AddPageToEntry(new TextPage()
            {
                text =
                "As mentioned in the &b'Basic Influencing' &rentry, &yRadiance&r functions not only as a resource itself, but may also serve as a carrier for other ethereal matter. The &bLens of Pathos&r is the first alternate Projector lens that puts this fact to use.&n&n" +
                "The &bLens of Pathos&r is similar to an ordinary Flareglass lens, but with a clearer composure to loosen the restrictions on what may pass through. By using a fired &yRadiance&r " +
                "blast as a hammer of sorts to 'nail' the essence of emotions trapped within the Transmutator, the capabilities of what you can use Transmutation for are significantly increased. Unfortunately, this process requires two additional resources: A &bvessel&r to hold the contents, and the concentrated ephemeral essence of emotions to fill said vessel.&n&n" +
                "The vessel required is something that can actually feel emotion, but is weak enough to have its life force dragged from its body in order to act as the container. One type of creature fits this perfectly: Critters.&n&n" +
                "By hovering over the Transmutator while a &bLens of Pathos &ris inserted into a Projector below will reveal the radius in which an alive full-sized critter (bunnies, squirrels, and birds) will be slain when the crafting process begins in order to supply its life as a vessel for emotions.&n&n" +
                "The next situation to figure out is how to get the emotional essence required in the first place.&n&n" +
                "However, this issue is already solvable with your current knowledge.&n&n" +
                "&bPotion Dispersal&r is your answer to forcibly creating emotions. As it turns out, buff potions not only supply the mental or physical enhancements that they are known to give, but can also manipulate the feelings of a consumer, and this is a fact that remains true when the syrupy liquid is in its gaseous form through Potion Dispersal. The &yRadiance &rfired from the Projector will absorb the emotions from the vapor and carry them into the object upon Transmutation, opening the door for many new objects to be created that required this additional substance.&n&n" +
                "The emotions carried by potions are less varied than you may think, and are divided into four color categories based on what is given.&n&n" +
                "&1Scarlet&r emotions involve desires of delivering harm unto, or reducing harm from, other creatures.&n&n" +
                "&2Cerulean&r emotions are associated with calmness, creativity, and vitality.&n&n" +
                "&3Verdant&r emotions are those related to cognitive abilities and insightfulness.&n&n" +
                "Lastly, &4Mauve&r emotions are based around pride and the ego from posessing abilities that others do not.&n&nYou can view what potion effect is aligned with what color by viewing the color of the timer above a Transmutator that is actively dispersing a potion, or by creating an " +
                "&bAlchemical Lens&r to conveniently view the color aligned to a potion without the need to consume it.&n&n" +
                "Each recipe involving the &bLens of Pathos &rhas a required emotion color that has to actively be in the Transmutator at the time that the craft goes through, and a required minimum duraton of that effect, as a measure for how much of it will be absorbed into the transmutated object. Any potion that grants the necessary color can be used, but you cannot mix and match potions of identical color. Failure to meet the required duration or color at the time of the craft will not stop it from going through, resulting in lost Radiance and an unchanged item."
            });
            AddPageToEntry(new RecipePage()
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