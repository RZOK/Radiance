using Microsoft.Xna.Framework;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Tiles;
using Radiance.Core;
using Radiance.Core.Interfaces;
using Radiance.Core.Systems;
using Radiance.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace Radiance.Content.Items.PedestalItems
{
    public class AnnihilationCore : BaseContainer, IPedestalItem, ITransmutationRecipe
    {
        public AnnihilationCore() : base(
            null,
            null,
            10,
            ContainerMode.InputOnly,
            ContainerQuirk.CantAbsorbNonstandardTooltip)
        { }

        public new Color aoeCircleColor => new Color(158, 98, 234);
        public new float aoeCircleRadius => 75;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Annihilation Core");
            Tooltip.SetDefault("Destroys nearby items when atop a Pedestal\nOnly common items can be disintegrated");
            Item.ResearchUnlockCount = 3;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.LightRed;
        }

        public void AddTransmutationRecipe()
        {
            TransmutationRecipe recipe = new TransmutationRecipe();
            recipe.inputItems = new int[] { ItemID.SoulofNight };
            recipe.inputStack = 3;
            recipe.outputItem = Item.type;
            recipe.unlock = UnlockSystem.UnlockBoolean.hardmode;
            TransmutationRecipeSystem.AddRecipe(recipe);
        }

        public new void PedestalEffect(PedestalTileEntity pte)
        {
            base.PedestalEffect(pte);
            Vector2 pos = RadianceUtils.TileEntityWorldCenter(pte);
            if (Main.GameUpdateCount % 120 == 0)
            {
                int f = Dust.NewDust(pos - new Vector2(0, -5 * RadianceUtils.SineTiming(30) + 2) - new Vector2(8, 8), 16, 16, DustID.PurpleCrystalShard, 0, 0);
                Main.dust[f].velocity *= 0.1f;
                Main.dust[f].noGravity = true;
                Main.dust[f].scale = Main.rand.NextFloat(1.2f, 1.4f);
            }
            if (pte.actionTimer > 0)
                pte.actionTimer--;

            if (pte.actionTimer == 0 && pte.currentRadiance >= 0.01f)
            {
                for (int k = 0; k < Main.maxItems; k++)
                {
                    if (Vector2.Distance(Main.item[k].Center, pos) < 75 && Main.item[k].noGrabDelay == 0 && Main.item[k].active && Main.item[k].rare >= ItemRarityID.Gray && Main.item[k].rare <= ItemRarityID.Blue)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            int f = Dust.NewDust(pos - new Vector2(0, -5 * RadianceUtils.SineTiming(30) + 2) - new Vector2(8, 8), 16, 16, DustID.PurpleCrystalShard, 0, 0);
                            Main.dust[f].velocity *= 0.3f;
                            Main.dust[f].noGravity = true;
                            Main.dust[f].scale = Main.rand.NextFloat(1.3f, 1.7f);
                        }
                        currentRadiance -= 0.01f;
                        pte.actionTimer = 60;
                        DustSpawn(Main.item[k]);
                        Main.item[k].TurnToAir();
                        Main.item[k].active = false;
                        break;
                    }
                }
            }
        }

        public static void DustSpawn(Item item)
        {
            Rectangle rec = Item.GetDrawHitbox(item.type, null);
            for (int i = 0; i < rec.Width + rec.Height; i++)
            {
                SoundEngine.PlaySound(SoundID.Item74, item.Center);
                Dust f = Dust.NewDustPerfect(item.Center + new Vector2(Main.rand.NextFloat(-rec.Width, rec.Width), Main.rand.NextFloat(-rec.Height, rec.Height + 16)) / 2, 70);
                f.velocity *= 0.5f;
                f.velocity.Y = Main.rand.NextFloat(-1, -4) * Main.rand.NextFloat(1, 4);
                f.noGravity = true;
                f.scale = Main.rand.NextFloat(1.3f, 1.7f);
            }
        }
    }
}