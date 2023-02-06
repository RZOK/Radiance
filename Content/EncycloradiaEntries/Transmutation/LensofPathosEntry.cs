using Terraria.ModLoader;
using Radiance.Content.Items.ProjectorLenses;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using Radiance.Core;
using Radiance.Utilities;
using System.Collections.Generic;
using Terraria.ID;
using Radiance.Content.Items.Tools.Misc;
using Terraria;
using System;
using Radiance.Core.Systems;
using static Radiance.Core.Systems.UnlockSystem;
using Microsoft.Xna.Framework.Graphics;

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
                "As mentioned in the ".BWSnippet(),
                "'Basic Influencing' ".DarkColorSnippet(CommonColors.ContextColor),
                "entry, ".BWSnippet(),
                "Radiance ".DarkColorSnippet(CommonColors.RadianceColor1),
                "functions not only as a resource itself, but may also serve as a carrier for other ethereal matter. The ".BWSnippet(),
                "Lens of Pathos ".DarkColorSnippet(CommonColors.ContextColor),
                "is the first alternate Projector lens that puts this fact to use. |".BWSnippet(),
                "The ".BWSnippet(),
                "Lens of Pathos ".DarkColorSnippet(CommonColors.ContextColor),
                "is similar to an ordinary Flareglass lens, but with a clearer composure to loosen the restrictions on what may pass through. By using a fired ".BWSnippet(),
                "Radiance ".DarkColorSnippet(CommonColors.RadianceColor1),
                "blast as a hammer of sorts to 'nail' the essence of emotions trapped within the Transmutator, the capabilities of what you can use Transmutation for are significantly increased. Unfortunately, this process requires two additional resources: A ".BWSnippet(),
                "vessel ".DarkColorSnippet(CommonColors.ContextColor),
                "to hold the contents, and the concentrated ephemeral essence of emotions to fill said vessel. | The vessel required is something that can actually feel emotion, but is weak enough to have its life force dragged from its body in order to act as the container. One type of creature fits this perfectly: Critters. | By hovering over the Transmutator while a ".BWSnippet(),
                "Lens of Pathos ".DarkColorSnippet(CommonColors.ContextColor),
                "is inserted into a Projector below will reveal the radius in which an alive full-sized critter (bunnies, squirrels, and birds) will be slain when the crafting process begins in order to supply its life as a vessel for emotions. | The next situation to figure out is how to get the emotional essence required in the first place. | However, this issue is already solvable with your current knowledge. |".BWSnippet(),
                "Potion Dispersal ".DarkColorSnippet(CommonColors.ContextColor),
                "is your answer to forcibly creating emotions. As it turns out, buff potions not only supply the mental or physical enhancements that they are known to give, but can also manipulate the feelings of a consumer, and this is a fact that remains true when the syrupy liquid is in its gaseous form through Potion Dispersal. The ".BWSnippet(),
                "Radiance ".DarkColorSnippet(CommonColors.RadianceColor1),
                "fired from the Projector will absorb the emotions from the vapor and carry them into the object upon Transmutation, opening the door for many new objects to be created that required this additional substance. | The emotions carried by potions are less varied than you may think, and are divided into four color categories based on what is given. |".BWSnippet(),
                "Scarlet ".DarkColorSnippet(CommonColors.ScarletColor),
                "emotions involve desires of delivering harm unto, or reducing harm from, other creatures. |".BWSnippet(),
                "Cerulean ".DarkColorSnippet(CommonColors.CeruleanColor),
                "emotions are associated with calmness, creativity, and vitality. |".BWSnippet(),
                "Verdant ".DarkColorSnippet(CommonColors.VerdantColor),
                "emotions are those related to cognitive abilities and insightfulness. | Lastly, ".BWSnippet(),
                "Mauve ".DarkColorSnippet(CommonColors.MauveColor),
                "emotions are based around pride and the ego from posessing abilities that others do not. | You can view what potion effect is aligned with what color by viewing the color of the timer above a Transmutator that is actively dispersing a potion, or by creating an ".BWSnippet(),
                "Alchemical Lens ".DarkColorSnippet(CommonColors.ContextColor),
                "to conveniently view the color aligned to a potion without the need to consume it. | Each recipe involving the ".BWSnippet(),
                "Lens of Pathos ".DarkColorSnippet(CommonColors.ContextColor),
                "has a required emotion color that has to actively be in the Transmutator at the time that the craft goes through, and a required minimum duraton of that effect, as a measure for how much of it will be absorbed into the transmutated object. Any potion that grants the necessary color can be used, but you cannot mix and match potions of identical color. Failure to meet the required duration or color at the time of the craft will not stop it from going through, resulting in lost Radiance and an unchanged item.".BWSnippet(),
            } } );
            AddToEntry(this, new RecipePage()
            {
                items = new Dictionary<int, int>()
                {
                    { ItemID.CobaltBar, 5 },
                    { ModContent.ItemType<ShimmeringGlass>(), 5 }
                },
                station = RadianceUtils.GetItem(ItemID.MythrilAnvil),
                result = (RadianceUtils.GetItem(ModContent.ItemType<AlchemicalLens>()), 1)
            });
        }
    }
}
