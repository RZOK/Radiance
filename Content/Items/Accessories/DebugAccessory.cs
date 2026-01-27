using Microsoft.CodeAnalysis.CSharp.Syntax;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Content.Particles;
using Radiance.Core.Systems;

using System.Collections.ObjectModel;

namespace Radiance.Items.Accessories
{
    public class DebugAccessory : ModItem, ITransmutationRecipe
    {
        public override string Texture => "Radiance/Content/ExtraTextures/Debug";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Debug Accessory");
            Tooltip.SetDefault("Enables various debug features and information when equipped");
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.value = 0;
            Item.rare = ItemRarityID.Blue;
            Item.useTime = Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.accessory = true;
        }
        
        public override bool? UseItem(Player player)
        {
            ParticleSystem.AddParticle(new PearlFlow(player.Center, player.Center + Vector2.UnitX * 128f, 60));

            return true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<RadiancePlayer>().debugMode = true;
            player.GetModPlayer<RadianceInterfacePlayer>().canSeeRays = true;
            if (TileEntitySystem.orderedEntities is not null)
            {
                foreach (ImprovedTileEntity ite in TileEntitySystem.orderedEntities)
                {
                    if (ite.TileEntityWorldCenter().Distance(player.Center) < 100)
                        ite.AddHoverUI();
                }
            }
        }

        public void AddTransmutationRecipe(TransmutationRecipe recipe)
        {
            recipe.otherRequirements.Add(LivingLens.GetLivingLensRequirememt(PotionColors.InfluentialColor.Scarlet, 6000));
            recipe.requiredRadiance = 100;
            recipe.outputItem = ModContent.ItemType<DebugAccessory>();
            recipe.inputItems = new int[1] { ModContent.ItemType<DebugAccessory>() };
        }
    }
}