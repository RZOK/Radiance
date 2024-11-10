using Newtonsoft.Json.Serialization;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Particles;
using Radiance.Content.Tiles.Pedestals;
using Radiance.Core.Systems;
using Terraria.GameContent.Bestiary;

namespace Radiance.Content.Items.PedestalItems
{
    public class AnnihilationCore : BaseContainer, IPedestalItem, ITransmutationRecipe
    {
        public AnnihilationCore() : base(
            null,
            10,
            false)
        { }

        public static readonly new Color AOE_CIRCLE_COLOR = new Color(158, 98, 234);
        public static readonly new float AOE_CIRCLE_RADIUS = 64;
        public static readonly float MINIMUM_RADIANCE = 0.01f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Annihilation Core");
            Tooltip.SetDefault("Destroys nearby items when placed atop a Pedestal");
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
            recipe.unlock = UnlockCondition.unlockedByDefault;
        }

        public new void UpdatePedestal(PedestalTileEntity pte)
        {
            base.UpdatePedestal(pte);
            if (pte.enabled)
            {
                pte.aoeCircleColor = AOE_CIRCLE_COLOR;
                pte.aoeCircleRadius = AOE_CIRCLE_RADIUS;

                if (pte.actionTimer > 0)
                    pte.actionTimer--;

                Vector2 pos = RadianceUtils.TileEntityWorldCenter(pte);
                if (pte.actionTimer == 0 && pte.storedRadiance >= MINIMUM_RADIANCE)
                {
                    for (int k = 0; k < Main.maxItems; k++)
                    {
                        Item item = Main.item[k];
                        if (Vector2.Distance(item.Center, pos) < 75 && item.noGrabDelay == 0 && item.active && pte.itemImprintData.IsItemValid(item))
                        {
                            item.TurnToAir();
                            item.active = false;
                            storedRadiance -= MINIMUM_RADIANCE;
                            pte.actionTimer = 60;

                            ParticleSystem.AddParticle(new StarFlare(pte.GetFloatingItemCenter(Item), 10, 0, new Color(212, 160, 232), new Color(139, 56, 255), 0.025f));
                            ParticleSystem.AddParticle(new MiniLightning(pte.GetFloatingItemCenter(Item), item.Center, new Color(139, 56, 255), 12));
                            ParticleSystem.AddParticle(new DisintegratingItem(item.Center, new Vector2(1, -2), 90, (item.Center.X - pos.X).NonZeroSign(), item.Clone(), GetItemTexture(item.type))); //todo: doesn't work on laptop ???

                            SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/LightningZap") with { PitchVariance = 0.5f, Volume = 0.8f }, pos);

                            Vector2 itemSize = GetItemTexture(item.type).Size();
                            for (int i = 0; i < 3; i++)
                            {
                                ParticleSystem.AddParticle(new SpeedLine(item.Center + Main.rand.NextVector2Circular(itemSize.X - 4f, itemSize.Y - 4f) / 2f - Vector2.UnitY * 24, -Vector2.UnitY * Main.rand.NextFloat(2.5f, 4f), 15, new Color(139, 56, 255), -PiOver2, Main.rand.Next(40, 88)));
                            }
                            break;
                        }
                    }
                }
                if (Main.GameUpdateCount % 120 == 0)
                {
                    int f = Dust.NewDust(pos - new Vector2(0, -5 * SineTiming(30) + 2) - new Vector2(8, 8), 16, 16, DustID.PurpleCrystalShard, 0, 0);
                    Main.dust[f].velocity *= 0.1f;
                    Main.dust[f].noGravity = true;
                    Main.dust[f].scale = Main.rand.NextFloat(1.2f, 1.4f);
                }
            }
        }
    }
}