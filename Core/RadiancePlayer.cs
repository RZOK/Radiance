using Microsoft.Xna.Framework;
using Radiance.Content.Items.BaseItems;
using System;
using Terraria;
using Terraria.ModLoader;

namespace Radiance.Core
{
    public class RadiancePlayer : ModPlayer
    {
        public bool debugMode = false;
        public bool canSeeRays = false;

        public float currentRadianceOnHand = 0;
        public float maxRadianceOnHand = 0;

        public override void ResetEffects()
        {
            debugMode = false;
            canSeeRays = false;

            maxRadianceOnHand = 0;
            currentRadianceOnHand = 0;
        }

        public override void PostUpdate()
        {
            for (int i = 0; i < 50; i++)
            {
                BaseContainer cell = Player.inventory[i].ModItem as BaseContainer;
                if (cell != null)
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
                            float minus = Math.Clamp(cell.CurrentRadiance, 0, radianceLeft);
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