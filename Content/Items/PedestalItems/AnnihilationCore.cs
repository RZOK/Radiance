using Radiance.Content.Items.BaseItems;
using Radiance.Content.Particles;
using Radiance.Content.Tiles.Pedestals;
using Radiance.Core.Systems;

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

        public void AddTransmutationRecipe(TransmutationRecipe recipe)
        {
            recipe.inputItems = new int[] { ItemID.SoulofNight };
            recipe.inputStack = 3;
            recipe.unlock = UnlockSystem.UnlockBoolean.hardmode;
        }

        public new void PedestalEffect(PedestalTileEntity pte)
        {
            base.PedestalEffect(pte);
            Vector2 pos = RadianceUtils.TileEntityWorldCenter(pte);
            if (Main.GameUpdateCount % 120 == 0)
            {
                int f = Dust.NewDust(pos - new Vector2(0, -5 * SineTiming(30) + 2) - new Vector2(8, 8), 16, 16, DustID.PurpleCrystalShard, 0, 0);
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
                    Item item = Main.item[k];
                    if (Vector2.Distance(item.Center, pos) < 75 && item.noGrabDelay == 0 && item.active && item.rare >= ItemRarityID.Gray && item.rare <= ItemRarityID.Blue && pte.itemImprintData.IsItemValid(item))
                    {
                        //for (int i = 0; i < 5; i++)
                        //{
                        //    int f = Dust.NewDust(pos - new Vector2(0, -5 * SineTiming(30) + 2) - new Vector2(8, 8), 16, 16, DustID.PurpleCrystalShard, 0, 0);
                        //    Main.dust[f].velocity *= 0.3f;
                        //    Main.dust[f].noGravity = true;
                        //    Main.dust[f].scale = Main.rand.NextFloat(1.3f, 1.7f);
                        //}
                        currentRadiance -= 0.01f;
                        pte.actionTimer = 60;
                        for (int i = 0; i < 20; i++)
                        {
                            Vector2 ashPos = item.Center + Main.rand.NextVector2Circular(item.width, item.height);
                            Vector2 velocity = item.Center.DirectionTo(ashPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1, 2);
                            ParticleSystem.AddParticle(new Ash(ashPos, velocity, 120, 0, Color.DarkGray));
                        }
                        item.TurnToAir();
                        item.active = false;
                        break;
                    }
                }
            }
        }
    }
}