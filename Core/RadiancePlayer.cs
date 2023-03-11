using Radiance.Content.Items.BaseItems;
using Radiance.Utilities;
using System;
using Terraria;
using Terraria.ModLoader;

namespace Radiance.Core
{
    public class RadiancePlayer : ModPlayer
    {
        public bool debugMode = false;
        public bool canSeeRays = false;
        public bool alchemicalLens = false;

        public float currentRadianceOnHand;
        public float maxRadianceOnHand;
        public float discount;

        public override void ResetEffects()
        {
            debugMode = false;
            canSeeRays = false;
            alchemicalLens = false;
            discount = 0;
        }
        public override void UpdateDead()
        {
            debugMode = false;
            canSeeRays = false;
            alchemicalLens = false;
            discount = 0;
        }
        public override void PostUpdate()
        {
            maxRadianceOnHand = 0;
            currentRadianceOnHand = 0;
            
            for (int i = 0; i < 50; i++)
            {
                BaseContainer cell = Player.inventory[i].ModItem as BaseContainer;
                if (cell != null && cell.ContainerMode != BaseContainer.ContainerModeEnum.InputOnly)
                {
                    maxRadianceOnHand += cell.MaxRadiance;
                    currentRadianceOnHand += cell.CurrentRadiance;
                }
            }
        }
        public void ConsumeRadianceOnHand(float consumedAmount)
        {
            float radianceLeft = consumedAmount;
            if (maxRadianceOnHand > 0 && currentRadianceOnHand >= consumedAmount)
            {
                for (int i = 0; i < 50; i++)
                {
                    BaseContainer cell = Player.inventory[i].ModItem as BaseContainer;
                    if (cell != null)
                    {
                        if (cell.CurrentRadiance > 0)
                        {
                            float minus = Math.Clamp(cell.CurrentRadiance, 0, radianceLeft) * Player.GetRadianceDiscount();
                            cell.CurrentRadiance -= minus;
                            radianceLeft -= minus;
                        }
                    }
                    if (radianceLeft == 0) return;
                }
            }
        }
    }
}