using Radiance.Content.Items.BaseItems;
using Radiance.Content.Particles;
using Radiance.Content.Tiles.Pedestals;
using Radiance.Core.Systems;


namespace Radiance.Content.Items.PedestalItems
{
    public class OrchestrationCore : BaseContainer, IPedestalItem, ITransmutationRecipe
    {
        public OrchestrationCore() : base(
            null,
            10,
            false)
        { }

        private static readonly Color AOE_CIRCLE_COLOR = new Color(235, 71, 120, 0);
        private static readonly float AOE_CIRCLE_RADIUS = 80;
        private static readonly float MINIMUM_RADIANCE = 0.02f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Orchestration Core");
            Tooltip.SetDefault("Warps nearby items when placed on a Pedestal");
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
            recipe.unlock = UnlockCondition.UnlockedByDefault;
            recipe.otherRequirements = new List<TransmutationRequirement> { TransmutationRequirement.testRequirement };
        }

        public new void UpdatePedestal(PedestalTileEntity pte)
        {
            if (pte.enabled)
            {
                pte.aoeCircleColor = AOE_CIRCLE_COLOR;
                pte.aoeCircleRadius = AOE_CIRCLE_RADIUS;

                if (pte.actionTimer > 0)
                    pte.actionTimer--;

                if (Main.GameUpdateCount % 40 == 0)
                {
                    if (Main.rand.NextBool(3))
                    {
                        int f = Dust.NewDust(pte.FloatingItemCenter(Item), 16, 16, DustID.TeleportationPotion, 0, 0);
                        Main.dust[f].velocity *= 0.3f;
                        Main.dust[f].scale = 0.8f;
                    }
                }
                if (pte.actionTimer == 0 && pte.storedRadiance >= 0.05f)
                {
                    for (int i = 0; i < Main.item.Length; i++)
                    {
                        Item item = Main.item[i];
                        if (item.Distance(pte.TileEntityWorldCenter()) > AOE_CIRCLE_RADIUS || !pte.itemImprintData.ImprintAcceptsItem(item) || item.IsAir || !item.active)
                            continue;

                        List<PedestalTileEntity> alreadyTeleportedTo = new List<PedestalTileEntity>() { pte };
                        PedestalTileEntity destination = pte;

                        while (GetDestination(alreadyTeleportedTo, destination, out destination, item)) { }

                        if (destination != pte)
                        {
                            MoveItem(item, alreadyTeleportedTo);
                            break;
                        }
                    }
                }
            }
        }

        public static bool GetDestination(List<PedestalTileEntity> alreadyTeleportedTo, PedestalTileEntity source, out PedestalTileEntity destination, Item itemBeingTransferred)
        {
            destination = source;
            PedestalTileEntity proposedDestination = GetConnectedPedestal(source.Position + new Point16(1, 0), itemBeingTransferred, alreadyTeleportedTo);
            if (proposedDestination != null)
            {
                destination = proposedDestination;
                alreadyTeleportedTo.Add(destination);
                return true;
            }

            proposedDestination = GetConnectedPedestal(source.Position + new Point16(0, 1), itemBeingTransferred, alreadyTeleportedTo);
            if (proposedDestination != null)
            {
                destination = proposedDestination;
                alreadyTeleportedTo.Add(destination);
                return true;
            }

            return false;
        }

        public static PedestalTileEntity GetConnectedPedestal(Point16 position, Item item, List<PedestalTileEntity> alreadyTeleportedTo)
        {
            if (RadianceRay.FindRay(position, out RadianceRay outputtingRay) &&
                outputtingRay.inputTE is PedestalTileEntity proposedDestination &&
                proposedDestination.enabled &&
                proposedDestination.GetSlot(0).type == ModContent.ItemType<OrchestrationCore>() &&
                proposedDestination.storedRadiance > MINIMUM_RADIANCE &&
                proposedDestination.itemImprintData.ImprintAcceptsItem(item) &&
                !alreadyTeleportedTo.Contains(proposedDestination) &&
                item.noGrabDelay == 0
                )
            {
                return proposedDestination;
            }
            return null;
        }

        public void MoveItem(Item item, List<PedestalTileEntity> pedestalTileEntities)
        {
            PedestalTileEntity source = pedestalTileEntities.First();
            PedestalTileEntity destination = pedestalTileEntities.Last();
            for (int i = 0; i < 5; i++)
            {
                int f = Dust.NewDust(source.FloatingItemCenter(Item) - Vector2.UnitY * (-5 * SineTiming(30) + 2) - new Vector2(8, 8), 16, 16, DustID.TeleportationPotion, 0, 0);
                Main.dust[f].velocity *= 0.3f;
                Main.dust[f].scale = Main.rand.NextFloat(1.3f, 1.7f);
            }
            for (int i = 0; i < pedestalTileEntities.Count; i++)
            {
                PedestalTileEntity pte = pedestalTileEntities[i];
                Vector2 floatingItemCenter = pte.FloatingItemCenter(Item);

                ParticleSystem.AddParticle(new StarFlare(floatingItemCenter, 10, new Color(255, 100, 150), new Color(235, 71, 120), 0.035f));
                pte.ContainerPlaced.storedRadiance -= MINIMUM_RADIANCE;
                pte.actionTimer = 15;

                if (pte == pedestalTileEntities.Last())
                    break;

                PedestalTileEntity currentDest = pedestalTileEntities[i + 1];
                float amount = 0;
                Vector2 currentDestItem = currentDest.FloatingItemCenter(Item);
                Vector2 direction = floatingItemCenter.DirectionTo(currentDestItem);
                float distance = floatingItemCenter.Distance(currentDestItem);
                int trailLength = (int)(distance / 5f) + 60;
                ParticleSystem.AddParticle(new SpeedLine(currentDestItem, Vector2.Zero, 20, new Color(255, 100, 150), floatingItemCenter.AngleTo(pedestalTileEntities[i + 1].FloatingItemCenter(Item)), distance));

                while (amount < 1f)
                {
                    float offset = Main.rand.NextFloat();
                    Vector2 offsetPosition = Vector2.Lerp(Vector2.Zero + direction * trailLength, direction * distance, offset);
                    ParticleSystem.AddParticle(new SpeedLine(floatingItemCenter + Main.rand.NextVector2Circular(16, 16) + offsetPosition, direction * (distance / 144) * (1f - offset + 0.1f), 20, new Color(255, 100, 150), floatingItemCenter.AngleTo(pedestalTileEntities[i + 1].FloatingItemCenter(Item)), trailLength));
                    if (Main.rand.NextBool())
                    {
                        Dust dust = Dust.NewDustPerfect(floatingItemCenter + Main.rand.NextVector2Circular(24, 24) + offsetPosition, DustID.TeleportationPotion, direction * (distance / 72) * (1f - offset + 0.1f));
                        dust.noLight = true;
                    }
                    amount += 1f / (distance / 100f);
                }
            }
            SoundEngine.PlaySound(SoundID.Item8, item.Center);
            ParticleSystem.AddParticle(new StarFlare(item.Center, 10, new Color(255, 100, 150), new Color(235, 71, 120), 0.025f));
            item.Center = destination.FloatingItemCenter(Item);
            item.velocity.X = Main.rand.NextFloat(-2.5f, 2.5f);
            item.velocity.Y = Main.rand.NextFloat(-3, -5);
            item.noGrabDelay = 30;
            SoundEngine.PlaySound(SoundID.Item8, item.Center);
        }

        public static bool GetOutput(PedestalTileEntity pte, List<PedestalTileEntity> locations, Item item, out PedestalTileEntity entity)
        {
            entity = null;
            if (pte != null)
            {
                if (RadianceRay.FindRay(pte.Position + new Point16(1, 0), out RadianceRay ray))
                {
                    entity = ray.inputTE as PedestalTileEntity;
                    if (entity != null && !locations.Contains(entity) && entity.GetSlot(0).type == ModContent.ItemType<OrchestrationCore>() && entity.ContainerPlaced.storedRadiance >= 0.05f && entity.itemImprintData.ImprintAcceptsItem(item))
                        return true;
                }
                if (RadianceRay.FindRay(pte.Position + new Point16(0, 1), out RadianceRay ray2))
                {
                    entity = ray2.inputTE as PedestalTileEntity;
                    if (entity != null && !locations.Contains(entity) && entity.GetSlot(0).type == ModContent.ItemType<OrchestrationCore>() && entity.ContainerPlaced.storedRadiance >= 0.05f && entity.itemImprintData.ImprintAcceptsItem(item))
                        return true;
                }
            }
            return false;
        }
    }
}