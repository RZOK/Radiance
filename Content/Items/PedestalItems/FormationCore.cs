using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Tiles;
using Radiance.Core;
using Radiance.Utils;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.PedestalItems
{
    public class FormationCore : BaseContainer
    {
        #region Fields

        private float maxRadiance = 10;
        private ContainerModeEnum containerMode = ContainerModeEnum.InputOnly;
        private ContainerQuirkEnum containerQuirk = ContainerQuirkEnum.CantAbsorbNonstandardTooltip;

        public Texture2D radianceAdjustingTexture = null;

        #endregion

        #region Properties

#nullable enable
        public override Texture2D? RadianceAdjustingTexture
        {
            get => radianceAdjustingTexture;
            set => radianceAdjustingTexture = value;
        }
#nullable disable

        public override float MaxRadiance
        {
            get => maxRadiance;
            set => maxRadiance = value;
        }
        public override ContainerModeEnum ContainerMode
        {
            get => containerMode;
            set => containerMode = value;
        }
        public override ContainerQuirkEnum ContainerQuirk
        {
            get => containerQuirk;
            set => containerQuirk = value;
        }

        #endregion

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Formation Core");
            Tooltip.SetDefault("Stores an ample amount of Radiance\nWarps nearby items when placed on a Pedestal\nItems will be teleported to Pedestals linked with outputting rays that also have Formation Cores atop them");
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
            Vector2 pos = MathUtils.MultitileCenterWorldCoords(pte.Position.X, pte.Position.Y) + Vector2.UnitX * pte.Width * 8;
            if (pte.actionTimer > 0)
                pte.actionTimer--;
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
                }
                if (entity != null)
                {
                    if (entity.itemPlaced.type == ModContent.ItemType<FormationCore>())
                    {
                        for (int k = 0; k < Main.maxItems; k++)
                        {
                            if (Vector2.Distance(Main.item[k].Center, pos) < 100 && Main.item[k].noGrabDelay == 0 && Main.item[k].active)
                            {
                                CurrentRadiance -= 0.05f;
                                entity.actionTimer = 60;
                                pte.actionTimer = 60;
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
        }
        public void DustSpawn(Item item)
        {
            for (int i = 0; i < 30; i++)
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