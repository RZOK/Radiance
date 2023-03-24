using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        /// <summary>
        /// Do NOT try to consume Radiance by changing currentRadianceOnHand directly. Use ConsumeRadianceOnHand(float consumedAmount) from RadiancePlayer.cs instead.
        /// </summary>
        public float currentRadianceOnHand { get; private set; }
        public float maxRadianceOnHand { get; private set; }
        private float privateDiscount;
        public float RadianceDiscount { get => 1 - Math.Min(privateDiscount, 0.9f); set => privateDiscount = value; }

        public override void ResetEffects()
        {
            debugMode = false;
            canSeeRays = false;
            alchemicalLens = false;
            privateDiscount = 0;
        }

        public override void UpdateDead()
        {
            debugMode = false;
            canSeeRays = false;
            alchemicalLens = false;
            privateDiscount = 0;
        }

        public override void PreUpdate()
        {
            Main.NewText(privateDiscount);
            maxRadianceOnHand = 0;
            currentRadianceOnHand = 0;

            for (int i = 0; i < 58; i++)
            {
                if (Player.inventory[i].ModItem is BaseContainer cell && cell.ContainerMode != BaseContainer.ContainerModeEnum.InputOnly)
                {
                    maxRadianceOnHand += cell.MaxRadiance;
                    currentRadianceOnHand += cell.CurrentRadiance;
                }
            }
        }

        public bool ConsumeRadianceOnHand(float consumedAmount)
        {
            float radianceLeft = consumedAmount * RadianceDiscount;
            if (currentRadianceOnHand >= consumedAmount)
            {
                for (int i = 0; i < 58; i++)
                {
                    if (Player.inventory[i].ModItem is BaseContainer cell && cell.CurrentRadiance > 0)
                    {
                        float minus = Math.Clamp(cell.CurrentRadiance, 0, radianceLeft);
                        cell.CurrentRadiance -= minus;
                        radianceLeft -= minus;
                    }
                    if (radianceLeft == 0)
                        return true;
                }
            }
            return false;
        }
    }
}