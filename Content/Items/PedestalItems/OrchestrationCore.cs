using Radiance.Content.Items.BaseItems;
using Radiance.Content.Tiles;
using Radiance.Core.Systems;
using System.Collections.Generic;

namespace Radiance.Content.Items.PedestalItems
{
    public class OrchestrationCore : BaseContainer, IPedestalItem, ITransmutationRecipe
    {
        public OrchestrationCore() : base(
            null,
            null,
            10,
            ContainerMode.InputOnly,
            ContainerQuirk.CantAbsorbNonstandardTooltip)
        { }

        public new Color aoeCircleColor => new Color(235, 71, 120, 0);
        public new float aoeCircleRadius => 100;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Orchestration Core");
            Tooltip.SetDefault("Warps nearby items when placed on a Pedestal\nItems will be teleported to Pedestals linked with outputting rays that also have Orchestration Cores atop them");
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
            recipe.inputItems = new int[] { ItemID.SoulofLight };
            recipe.inputStack = 3;
            recipe.unlock = UnlockSystem.UnlockBoolean.hardmode;
        }

        public new void PedestalEffect(PedestalTileEntity pte)
        {
            base.PedestalEffect(pte);

            Vector2 pos = MultitileWorldCenter(pte.Position.X, pte.Position.Y);
            if (pte.actionTimer > 0)
                pte.actionTimer--;
            if (Main.GameUpdateCount % 40 == 0)
            {
                if (Main.rand.NextBool(3))
                {
                    int f = Dust.NewDust(pos - new Vector2(0, -5 * SineTiming(30) + 2) - new Vector2(8, 8), 16, 16, DustID.TeleportationPotion, 0, 0);
                    Main.dust[f].velocity *= 0.3f;
                    Main.dust[f].scale = 0.8f;
                }
            }
            if (pte.actionTimer == 0 && pte.currentRadiance >= 0.05f)
            {
                List<PedestalTileEntity> list = new List<PedestalTileEntity>() { pte };
                PedestalTileEntity entity = pte;
                int count = 0;
                if (pte != null)
                {
                    while (GetOutput(entity, list, out PedestalTileEntity newEntity) && count < 100)
                    {
                        count++;
                        entity = newEntity;
                        list.Add(newEntity);
                    }
                    if (entity != pte)
                    {
                        for (int k = 0; k < Main.maxItems; k++)
                        {
                            if (Vector2.Distance(Main.item[k].Center, pos) < aoeCircleRadius && Main.item[k].noGrabDelay == 0 && Main.item[k].active)
                            {
                                for (int i = 0; i < 5; i++)
                                {
                                    int f = Dust.NewDust(pos - Vector2.UnitY * (-5 * SineTiming(30) + 2) - new Vector2(8, 8), 16, 16, DustID.TeleportationPotion, 0, 0);
                                    Main.dust[f].velocity *= 0.3f;
                                    Main.dust[f].scale = Main.rand.NextFloat(1.3f, 1.7f);
                                }
                                foreach (PedestalTileEntity pte2 in list)
                                {
                                    pte2.ContainerPlaced.currentRadiance -= 0.05f;
                                    pte2.actionTimer = 45;
                                }
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

        public static bool GetOutput(PedestalTileEntity pte, List<PedestalTileEntity> locations, out PedestalTileEntity entity)
        {
            entity = null;
            if (pte != null)
            {
                if (RadianceRay.FindRay(pte.Position.ToVector2() * 16 + new Vector2(24, 8), out RadianceRay ray))
                {
                    entity = ray.inputTE as PedestalTileEntity;
                    if (entity != null && !locations.Contains(entity) && entity.GetSlot(0).type == ModContent.ItemType<OrchestrationCore>() && entity.ContainerPlaced.currentRadiance >= 0.05f)
                    {
                        return true;
                    }
                }
                if (RadianceRay.FindRay(pte.Position.ToVector2() * 16 + new Vector2(8, 24), out RadianceRay ray2))
                {
                    entity = ray2.inputTE as PedestalTileEntity;
                    if (entity != null && !locations.Contains(entity) && entity.GetSlot(0).type == ModContent.ItemType<OrchestrationCore>() && entity.ContainerPlaced.currentRadiance >= 0.05f)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void DustSpawn(Item item)
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