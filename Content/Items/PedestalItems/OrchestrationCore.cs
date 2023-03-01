using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Tiles;
using Radiance.Core;
using Radiance.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Radiance.Core.Interfaces;

namespace Radiance.Content.Items.PedestalItems
{
    public class OrchestrationCore : BaseContainer, IPedestalItem
    {
        public override Texture2D RadianceAdjustingTexture => null;
        public override float MaxRadiance => 10;
        public override ContainerModeEnum ContainerMode => ContainerModeEnum.InputOnly;
        public override ContainerQuirkEnum ContainerQuirk => ContainerQuirkEnum.CantAbsorbNonstandardTooltip;
        public new Color aoeCircleColor => new Color(235, 71, 120, 0);
        public new float aoeCircleRadius => 100;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Orchestration Core");
            Tooltip.SetDefault("Stores an ample amount of Radiance\nWarps nearby items when placed on a Pedestal\nItems will be teleported to Pedestals linked with outputting rays that also have Formation Cores atop them");
            SacrificeTotal = 3;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.LightRed;
        }

        public new void PedestalEffect(PedestalTileEntity pte)
        {
            base.PedestalEffect(pte);
            Vector2 pos = RadianceUtils.MultitileCenterWorldCoords(pte.Position.X, pte.Position.Y) + Vector2.UnitX * pte.Width * 8;
            if (pte.actionTimer > 0)
                pte.actionTimer--;
            if (Main.GameUpdateCount % 40 == 0)
            {
                if (Main.rand.NextBool(3))
                {
                    int f = Dust.NewDust(pos - new Vector2(0, -5 * RadianceUtils.SineTiming(30) + 2) - new Vector2(8, 8), 16, 16, DustID.TeleportationPotion, 0, 0);
                    Main.dust[f].velocity *= 0.3f;
                    Main.dust[f].scale = 0.8f;
                }
            }
            if (pte.actionTimer == 0 && pte.CurrentRadiance >= 0.05f)
            {
                PedestalTileEntity entity = null;
                for (int i = 0; i < pte.outputsConnected.Count; i++)
                {
                    RadianceRay ray = pte.outputsConnected[i];
                    if (ray.interferred)
                        continue;
                    if (ray.GetIO(ray.endPos).Item1 != pte && ray.GetIO(ray.endPos).Item2 == RadianceRay.IOEnum.Input && ray.GetIO(ray.endPos).Item1 as PedestalTileEntity != null)
                        entity = ray.GetIO(ray.endPos).Item1 as PedestalTileEntity;
                    else if (ray.GetIO(ray.startPos).Item1 != pte && ray.GetIO(ray.startPos).Item2 == RadianceRay.IOEnum.Input && ray.GetIO(ray.startPos).Item1 as PedestalTileEntity != null)
                        entity = ray.GetIO(ray.startPos).Item1 as PedestalTileEntity;

                    if (entity != null && entity.GetSlot(0).type == ModContent.ItemType<OrchestrationCore>() && entity.CurrentRadiance >= 0.05f) 
                        break;
                    else 
                        entity = null;
                }

                if (entity != null)
                {
                    for (int k = 0; k < Main.maxItems; k++)
                    {
                        if (Vector2.Distance(Main.item[k].Center, pos) < aoeCircleRadius && Main.item[k].noGrabDelay == 0 && Main.item[k].active)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                int f = Dust.NewDust(pos - Vector2.UnitY * (-5 * RadianceUtils.SineTiming(30) + 2) - new Vector2(8, 8), 16, 16, DustID.TeleportationPotion, 0, 0);
                                Main.dust[f].velocity *= 0.3f;
                                Main.dust[f].scale = Main.rand.NextFloat(1.3f, 1.7f);
                            }
                            CurrentRadiance -= 0.05f;
                            entity.containerPlaced.CurrentRadiance -= 0.05f;
                            entity.actionTimer = 60;
                            pte.actionTimer = 30;
                            DustSpawn(Main.item[k]);
                            Main.item[k].Center = entity.Position.ToVector2() * 16 + new Vector2(16, -Main.item[k].height / 4);
                            DustSpawn(Main.item[k]);
                            Main.item[k].velocity.X = Main.rand.NextFloat(-3, 3);
                            Main.item[k].velocity.Y = Main.rand.NextFloat(-3, -5);
                            Main.item[k].noGrabDelay = 30;
                            break;
                        }
                    }
                }
            }
        }

        public void DustSpawn(Item item)
        {
            Rectangle rec = Item.GetDrawHitbox(item.type, null);
            for (int i = 0; i < (rec.Width + rec.Height) / 2; i++)
            {
                SoundEngine.PlaySound(SoundID.Item8, item.Center);
                Dust d = Dust.NewDustPerfect(item.Center, 164, Main.rand.NextVector2Circular(4, 4));
                int f = Dust.NewDust(item.position, item.width, item.height, DustID.TeleportationPotion, 0, 0);
                Main.dust[f].velocity *= 0.5f;
                Main.dust[f].scale = 1.2f;
            }
        }
    }
}