using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Tiles;
using Radiance.Core;
using Radiance.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace Radiance.Content.Items.PedestalItems
{
    public class FormationCore : BaseContainer
    {
        public override Texture2D RadianceAdjustingTexture => null;
        public override float MaxRadiance => 10;
        public override ContainerModeEnum ContainerMode => ContainerModeEnum.InputOnly;
        public override ContainerQuirkEnum ContainerQuirk => ContainerQuirkEnum.CantAbsorbNonstandardTooltip;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Formation Core");
            Tooltip.SetDefault("Stores an ample amount of Radiance\nWhen placed atop a Pedestal, nearby items are placed onto adjacent empty Pedestals");
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
        public void PedestalEffect(PedestalTileEntity pte)
        {
            Vector2 pos = RadianceUtils.MultitileCenterWorldCoords(pte.Position.X, pte.Position.Y) + Vector2.UnitX * pte.Width * 8;
            PedestalTileEntity adjacentPTE = TryGetPedestal(pte);
            if (Main.GameUpdateCount % 120 == 0)
            {
                if (Main.rand.NextBool(3))
                {
                    Vector2 vel = (Vector2.UnitX * 2).RotatedByRandom(MathHelper.Pi);
                    for (int i = 0; i < 4; i++)
                    {
                        float rot = MathHelper.PiOver2 * i;
                        Dust f = Dust.NewDustPerfect(pos - new Vector2(0, -5 * RadianceUtils.SineTiming(30) + 2), 89);
                        f.velocity = vel.RotatedBy(rot);
                        f.noGravity = true;
                        f.scale = Main.rand.NextFloat(0.8f, 1.1f);
                    }
                }
            }
            if (pte.CurrentRadiance >= 0.01f && adjacentPTE != null)
            {
                for (int k = 0; k < Main.maxItems; k++)
                {
                    if (Vector2.Distance(Main.item[k].Center, pos) < 75 && Main.item[k].noGrabDelay == 0 && Main.item[k].active && Main.item[k].GetGlobalItem<ModGlobalItem>().formationPickupTimer == 0)
                    {
                        CurrentRadiance -= 0.01f;
                        DustSpawn(Main.item[k]);
                        adjacentPTE.itemPlaced = Main.item[k].Clone();
                        Main.item[k].stack--;
                        if (Main.item[k].stack == 0)
                        {
                            Main.item[k].active = false;
                            Main.item[k].TurnToAir();
                        }
                        pte.actionTimer = 12;
                        break;
                    }
                }
            }
            if (pte.actionTimer > 0)
            {
                if (pte.actionTimer % 4 == 0)
                { 
                    Vector2 vel = (Vector2.UnitX * Main.rand.Next(3, 6)).RotatedByRandom(MathHelper.Pi);
                    for (int d = 0; d < 4; d++)
                    {
                        float rot = MathHelper.PiOver2 * d;
                        Dust f = Dust.NewDustPerfect(pos - new Vector2(0, -5 * RadianceUtils.SineTiming(30) + 2), 89);
                        f.velocity = vel.RotatedBy(rot);
                        f.noGravity = true;
                        f.scale = Main.rand.NextFloat(1, 1.3f);
                    }
                }
                pte.actionTimer--;
            }
        }
        public static PedestalTileEntity TryGetPedestal(PedestalTileEntity pte)
        {
            RadianceUtils.TryGetTileEntityAs(pte.Position.X + 2, pte.Position.Y, out PedestalTileEntity entity);
            if (entity != null && entity.itemPlaced.type == ItemID.None) 
                return entity;
            RadianceUtils.TryGetTileEntityAs(pte.Position.X - 1, pte.Position.Y, out PedestalTileEntity entity2);
            if (entity2 != null && entity2.itemPlaced.type == ItemID.None) 
                return entity2;
            return null;

        }
        public static void DustSpawn(Item item)
        {
            for (int i = 0; i < 30; i++)
            {
                SoundEngine.PlaySound(SoundID.Item8, item.Center);
                int f = Dust.NewDust(item.position, item.width, item.height, 89, 0, 0);
                Main.dust[f].velocity *= 0.5f;
                Main.dust[f].scale = 1.2f;
                Main.dust[f].noGravity = true;
            }
        }
    }
}