using Radiance.Content.Items.BaseItems;
using Radiance.Content.Tiles.Pedestals;
using Radiance.Core.Systems;

namespace Radiance.Content.Items.PedestalItems
{
    public class FormationCore : BaseContainer, IPedestalItem, ITransmutationRecipe
    {
        public FormationCore() : base(
            null,
            null,
            10,
            ContainerMode.InputOnly,
            ContainerQuirk.CantAbsorbNonstandardTooltip)
        { }

        public new Color aoeCircleColor => new Color(16, 112, 64, 0);
        public new float aoeCircleRadius => 75;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Formation Core");
            Tooltip.SetDefault("When placed atop a Pedestal, nearby items are placed onto adjacent empty Pedestals");
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
            recipe.inputItems = new int[] { ItemID.CursedFlame, ItemID.Ichor };
            recipe.inputStack = 3;
            recipe.unlock = UnlockSystem.UnlockBoolean.hardmode;
        }

        public new void PedestalEffect(PedestalTileEntity pte)
        {
            base.PedestalEffect(pte);
            Vector2 pos = MultitileWorldCenter(pte.Position.X, pte.Position.Y); 
            PedestalTileEntity adjacentPTE = TryGetPedestal(pte);
            if (Main.GameUpdateCount % 120 == 0)
            {
                if (Main.rand.NextBool(3))
                {
                    Vector2 vel = (Vector2.UnitX * 2).RotatedByRandom(Pi);
                    for (int i = 0; i < 4; i++)
                    {
                        float rot = PiOver2 * i;
                        Dust f = Dust.NewDustPerfect(pos - new Vector2(0, -5 * SineTiming(30) + 2), 89);
                        f.velocity = vel.RotatedBy(rot);
                        f.noGravity = true;
                        f.scale = Main.rand.NextFloat(0.8f, 1.1f);
                    }
                }
            }
            if (pte.currentRadiance >= 0.01f && adjacentPTE != null)
            {
                for (int k = 0; k < Main.maxItems; k++)
                {
                    if (Vector2.Distance(Main.item[k].Center, pos) < aoeCircleRadius && Main.item[k].noGrabDelay == 0 && Main.item[k].active && Main.item[k].GetGlobalItem<RadianceGlobalItem>().formationPickupTimer == 0)
                    {
                        currentRadiance -= 0.01f;
                        DustSpawn(Main.item[k]);
                        adjacentPTE.SetItemInSlot(0, Main.item[k].Clone()); //todo: overhaul this
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
                    Vector2 vel = (Vector2.UnitX * Main.rand.Next(3, 6)).RotatedByRandom(Pi);
                    for (int d = 0; d < 4; d++)
                    {
                        float rot = PiOver2 * d;
                        Dust f = Dust.NewDustPerfect(pos - new Vector2(0, -5 * SineTiming(30) + 2), 89);
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
            TryGetTileEntityAs(pte.Position.X + 2, pte.Position.Y, out PedestalTileEntity entity);
            if (entity != null && !entity.GetSlot(0).IsAir)
                return entity;
            TryGetTileEntityAs(pte.Position.X - 1, pte.Position.Y, out PedestalTileEntity entity2);
            if (entity2 != null && !entity2.GetSlot(0).IsAir)
                return entity2;
            return null;
        }

        public static void DustSpawn(Item item)
        {
            for (int i = 0; i < 30; i++)
            {
                SoundEngine.PlaySound(SoundID.Item8, item.Center);
                int f = Dust.NewDust(item.position, item.width, item.height, DustID.GemEmerald, 0, 0);
                Main.dust[f].velocity *= 0.5f;
                Main.dust[f].scale = 1.2f;
                Main.dust[f].noGravity = true;
            }
        }
    }
}