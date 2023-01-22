﻿using Terraria.ModLoader;
using Radiance.Content.Items.ProjectorLenses;
using static Radiance.Core.Systems.UnlockSystem;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using Radiance.Core;
using Radiance.Utilities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.ID;
using IL.Terraria.GameContent.UI.BigProgressBar;
using Radiance.Content.Items.Tools.Misc;
using Terraria;
using System;

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
                new CustomTextSnippet("'Basic Influencing'", CommonColors.ContextColor, CommonColors.ContextColorDark),
                new CustomTextSnippet("entry within the", Color.White, Color.Black),
                new CustomTextSnippet("Encycloradia,", CommonColors.RadianceColor1, CommonColors.RadianceColorDark),
                RadianceUtils.radianceSnippet,
                new CustomTextSnippet("functions not only as a resource itself, but may also serve as a carrier for other ethereal matter. The", Color.White, Color.Black),
                new CustomTextSnippet("Lens of Pathos", CommonColors.ContextColor, CommonColors.ContextColorDark),
                new CustomTextSnippet("is the first alternate Projector lens that puts this fact to use. |", Color.White, Color.Black),
                new CustomTextSnippet("The", Color.White, Color.Black),
                new CustomTextSnippet("Lens of Pathos", CommonColors.ContextColor, CommonColors.ContextColorDark),
                new CustomTextSnippet("is similar to an ordinary Flareglass lens, but with a clearer composure to loosen the restrictions on what may pass through. By using a fired", Color.White, Color.Black),
                RadianceUtils.radianceSnippet,
                new CustomTextSnippet("blast as a hammer of sorts to 'nail' the essence of emotions trapped within the Transmutator, the capabilities of what you can use Transmutation for are significantly increased. Unfortunately, this process requires two additional resources: A", Color.White, Color.Black),
                new CustomTextSnippet("vessel", CommonColors.ContextColor, CommonColors.ContextColorDark),
                new CustomTextSnippet("to hold the contents, and the concentrated ephemeral essence of emotions to fill said vessel. | The vessel required is something that can actually feel emotion, but is weak enough to have its life force dragged from its body in order to act as the container. One type of creature fits this perfectly: Critters. | By hovering over the Transmutator while a", Color.White, Color.Black),
                new CustomTextSnippet("Lens of Pathos", CommonColors.ContextColor, CommonColors.ContextColorDark),
                new CustomTextSnippet("is inserted into a Projector below will reveal the radius in which an alive full-sized critter (bunnies, squirrels, and birds) will be slain when the crafting process begins in order to supply its life as a vessel for emotions. | The next situation to figure out is how to get the emotional essence required in the first place. | However, this issue is already solvable with your current knowledge. |", Color.White, Color.Black),
                new CustomTextSnippet("Potion Dispersal", CommonColors.ContextColor, CommonColors.ContextColorDark),
                new CustomTextSnippet("is your answer to forcibly creating emotions. As it turns out, buff potions not only supply the mental or physical enhancements that they are known to give, but can also manipulate the feelings of a consumer, and this is a fact that remains true when the syrupy liquid is in its gaseous form through Potion Dispersal. The", Color.White, Color.Black),
                RadianceUtils.radianceSnippet,
                new CustomTextSnippet("fired from the Projector will absorb the emotions from the vapor and carry them into the object upon Transmutation, opening the door for many new objects to be created that required this additional substance. | The emotions carried by potions are less varied than you may think, and are divided into four color categories based on what is given. |", Color.White, Color.Black),
                new CustomTextSnippet("Scarlet", CommonColors.ScarletColor, CommonColors.ScarletColorDark),
                new CustomTextSnippet("emotions involve desires of delivering harm unto, or reducing harm from, other creatures. |", Color.White, Color.Black),
                new CustomTextSnippet("Cerulean", CommonColors.CeruleanColor, CommonColors.CeruleanColorDark),
                new CustomTextSnippet("emotions are associated with calmness, creativity, and vitality. |", Color.White, Color.Black),
                new CustomTextSnippet("Verdant", CommonColors.VerdantColor, CommonColors.VerdantColorDark),
                new CustomTextSnippet("emotions are those related to cognitive abilities and insightfulness. | Lastly,", Color.White, Color.Black),
                new CustomTextSnippet("Mauve", CommonColors.MauveColor, CommonColors.MauveColorDark),
                new CustomTextSnippet("emotions are based around pride and the ego from posessing abilities that others do not. | You can view what potion effect is aligned with what color by viewing the color of the timer above a Transmutator that is actively dispersing a potion, or by creating an", Color.White, Color.Black),
                new CustomTextSnippet("Alchemical Lens", CommonColors.ContextColor, CommonColors.ContextColorDark),
                new CustomTextSnippet("to conveniently view the color aligned to a potion without the need to consume it. | Each recipe involving the", Color.White, Color.Black),
                new CustomTextSnippet("Lens of Pathos", CommonColors.ContextColor, CommonColors.ContextColorDark),
                new CustomTextSnippet("has a required emotion color that has to actively be in the Transmutator at the time that the craft goes through, and a required minimum duraton of that effect, as a measure for how much of it will be absorbed into the transmutated object. Any potion that grants the necessary color can be used, but you cannot mix and match potions of identical color. Failure to meet the required duration or color at the time of the craft will not stop it from going through, resulting in lost Radiance and an unchanged item.", Color.White, Color.Black),
            } } );
            Console.WriteLine(ModContent.ItemType<AlchemicalLens>());
            AddToEntry(this, new RecipePage()
            {
                items = new Dictionary<int, int>()
                {
                    { ItemID.CobaltBar, 5 },
                    { ModContent.ItemType<ShimmeringGlass>(), 5 }
                },
                station = new Item(ItemID.MythrilAnvil),
                result = RadianceUtils.GetItem(ModContent.ItemType<AlchemicalLens>())
            });
        }
    }
}