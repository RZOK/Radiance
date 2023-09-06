using Radiance.Content.Items.BaseItems;
using Radiance.Content.Particles;
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

        public new Color aoeCircleColor => new Color(16, 112, 64);
        public new float aoeCircleRadius => 75;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Formation Core");
            Tooltip.SetDefault("Places nearby items into adjacent inventories when placed atop a Pedestal");
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

        public static readonly float FORMATION_CORE_MINIMUM_RADIANCE = 0.01f;

        public new void PedestalEffect(PedestalTileEntity pte)
        {
            base.PedestalEffect(pte);
            Vector2 pos = MultitileWorldCenter(pte.Position.X, pte.Position.Y);

            if (pte.currentRadiance >= FORMATION_CORE_MINIMUM_RADIANCE && pte.actionTimer == 0)
            {
                for (int k = 0; k < Main.maxItems; k++)
                {
                    Item item = Main.item[k];
                    if (item.IsAir)
                        continue;

                    IInventory adjacentInventory = TryGetAdjacentInentory(pte, item, out ImprovedTileEntity inventoryEntity);
                    if (pte.itemImprintData.IsItemValid(item) && adjacentInventory is not null && Vector2.Distance(item.Center, pos) < aoeCircleRadius && item.noGrabDelay == 0 && item.active && !item.IsAir && item.GetGlobalItem<RadianceGlobalItem>().formationPickupTimer == 0)
                    {
                        //Item visualItem = item.Clone();
                        //Chest.VisualizeChestTransfer(visualItem.Center + new Vector2(visualItem.width, visualItem.height) / 2, entity.TileEntityWorldCenter(), visualItem, visualItem.stack);
                        Item clonedItem = item.Clone();
                        currentRadiance -= FORMATION_CORE_MINIMUM_RADIANCE;
                        DustSpawn(item);
                        adjacentInventory.SafeInsertItemIntoInventory(item, out _);
                        pte.actionTimer = 4;
                        Rectangle drawBox = Item.GetDrawHitbox(item.type, null);
                        ParticleSystem.AddParticle(new StarItem(item.Center, inventoryEntity.TileEntityWorldCenter(), 60, Color.PaleGreen, clonedItem, 0.05f));
                        for (int i = 0; i < 3; i++)
                        {
                            Vector2 littleParticleSpawnOffset = Main.rand.NextVector2Circular(drawBox.Width * item.scale, drawBox.Height * item.scale);
                            //ParticleSystem.AddParticle(new StarItem(item.Center + littleParticleSpawnOffset, inventoryEntity.TileEntityWorldCenter(), (int)(90 * Main.rand.NextFloat(0.8f, 1.2f)), Color.PaleGoldenrod, clonedItem, 0.025f - 0.005f * i));
                        }
                        break;
                    }
                }
            }
            if (pte.actionTimer > 0)
            {
                Vector2 vel = (Vector2.UnitX * Main.rand.Next(3, 6)).RotatedByRandom(Pi);
                for (int d = 0; d < 4; d++)
                {
                    float rot = PiOver2 * d;
                    Dust f = Dust.NewDustPerfect(pte.GetFloatingItemCenter(Item) - new Vector2(0, -5 * SineTiming(30) + 2), 89);
                    f.velocity = vel.RotatedBy(rot);
                    f.noGravity = true;
                    f.scale = Main.rand.NextFloat(1, 1.3f);
                }

                pte.actionTimer--;
            }

            if (Main.GameUpdateCount % 120 == 0)
            {
                if (Main.rand.NextBool(3))
                {
                    Vector2 vel = (Vector2.UnitX * 2).RotatedByRandom(Pi);
                    for (int i = 0; i < 4; i++)
                    {
                        float rot = PiOver2 * i;
                        Dust f = Dust.NewDustPerfect(pte.GetFloatingItemCenter(Item) - new Vector2(0, -5 * SineTiming(30) + 2), 89);
                        f.velocity = vel.RotatedBy(rot);
                        f.noGravity = true;
                        f.scale = Main.rand.NextFloat(0.8f, 1.1f);
                    }
                }
            }
        }

        public static IInventory TryGetAdjacentInentory(PedestalTileEntity pte, Item item, out ImprovedTileEntity entity)
        {
            TryGetTileEntityAs(pte.Position.X + 2, pte.Position.Y, out entity);
            if (entity is not null && entity is IInventory inventory)
            {
                IInventory correctInventory = inventory.GetCorrectInventory();
                if (correctInventory.CanInsertItemIntoInventory(item))
                    return correctInventory;
            }

            TryGetTileEntityAs(pte.Position.X - 1, pte.Position.Y, out entity);
            if (entity is not null && entity is IInventory inventory2)
            {
                IInventory correctInventory = inventory2.GetCorrectInventory();
                if (correctInventory.CanInsertItemIntoInventory(item))
                    return correctInventory;
            }
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