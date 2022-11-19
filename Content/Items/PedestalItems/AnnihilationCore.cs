using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Tiles;
using Radiance.Utils;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace Radiance.Content.Items.PedestalItems
{
    public class AnnihilationCore : BaseContainer
    {
        #region Fields

        private float maxRadiance = 10;
        private ContainerModeEnum containerMode = ContainerModeEnum.InputOutput;
        private ContainerQuirkEnum containerQuirk = ContainerQuirkEnum.CantAbsorbNonstandardTooltip;

        public Texture2D radianceAdjustingTexture = null;

        #endregion Fields

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

        #endregion Properties

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Annihilation Core");
            Tooltip.SetDefault("Stores an ample amount of Radiance\nDestroys nearby items when supplied with Radiance atop a Pedestal\nOnly common items can be disintegrated");
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
            if (pte.actionTimer == 0 && pte.CurrentRadiance >= 0.01f)
            {
                for (int k = 0; k < Main.maxItems; k++)
                {
                    if (Vector2.Distance(Main.item[k].Center, pos) < 75 && Main.item[k].noGrabDelay == 0 && Main.item[k].active && Main.item[k].rare >= ItemRarityID.Gray && Main.item[k].rare <= ItemRarityID.Blue)
                    {
                        CurrentRadiance -= 0.01f;
                        pte.actionTimer = 60;
                        DustSpawn(Main.item[k]);
                        Main.item[k].TurnToAir();
                        Main.item[k].active = false;
                        break;
                    }
                }
            }
        }

        public void DustSpawn(Item item)
        {
            for (int i = 0; i < item.width + item.height; i++)
            {
                SoundEngine.PlaySound(SoundID.Item74, item.Center);
                int f = Dust.NewDust(item.position, item.width, item.height, 70, 0, 0);
                Main.dust[f].velocity *= 0.5f;
                Main.dust[f].velocity.Y = Main.rand.NextFloat(-1, -4);
                Main.dust[f].velocity.Y *= Main.rand.NextFloat(1, 4);
                Main.dust[f].noGravity = true;
                Main.dust[f].scale = Main.rand.NextFloat(1.3f, 1.7f);
            }
        }
    }
}