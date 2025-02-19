using Radiance.Content.Items.Tools.Misc;

namespace Radiance.Core
{
    public class RadianceInterfacePlayer : ModPlayer
    {
        public List<HoverUIData> currentHoveredObjects = new List<HoverUIData>();
        public List<HoverUIData> activeHoverData = new List<HoverUIData>();

        public bool canSeeRays = false;
        public float newEntryUnlockedTimer = 0;
        public int inventoryItemRightClickDelay = 0;
        public string incompleteEntryText = string.Empty;
        public string currentFakeHoverText = string.Empty;
        public bool fancyHoverTextBackground = false;
        public bool hoveringScrollWheelEntity = false;
        public bool canSeeLensItems = false;
        public List<ImprovedTileEntity> visibleTileEntities = new List<ImprovedTileEntity>();
        public Item currentlyActiveUIItem;
        public bool canSeeItemImprints => Player.HeldItem.type == ModContent.ItemType<CeramicNeedle>();
        public override void Load()
        {
            On_Player.ScrollHotbar += DontScrollHotbar;
        }
        public override void Unload()
        {
            On_Player.ScrollHotbar -= DontScrollHotbar;
        }
        private void DontScrollHotbar(On_Player.orig_ScrollHotbar orig, Player self, int Offset)
        {
            if (self.GetModPlayer<RadianceInterfacePlayer>().hoveringScrollWheelEntity)
                return;

            orig(self, Offset);
        }
        public override void OnEnterWorld()
        {
            visibleTileEntities.Clear();
        }
        public override void ResetEffects()
        {
            canSeeRays = false;
            incompleteEntryText = string.Empty;
            currentFakeHoverText = string.Empty;
            fancyHoverTextBackground = false;
            hoveringScrollWheelEntity = false;
            canSeeLensItems = false;

            if (inventoryItemRightClickDelay > 0)
                inventoryItemRightClickDelay--;
        }
        public override void UpdateDead()
        {
            canSeeRays = false;
            incompleteEntryText = string.Empty;
            currentFakeHoverText = string.Empty;
            fancyHoverTextBackground = false;
            hoveringScrollWheelEntity = false;
            canSeeLensItems = false;

            if (inventoryItemRightClickDelay > 0)
                inventoryItemRightClickDelay--;
        }
        public override void UpdateEquips()
        {
            foreach (ImprovedTileEntity entity in visibleTileEntities)
            {
                entity.AddHoverUI();
            }
            canSeeRays |= (ModContent.GetInstance<MultifacetedLensBuilderToggle>().Active() && ModContent.GetInstance<MultifacetedLensBuilderToggle>().CurrentState == 1);
        }
        public override void PostUpdate()
        {
            List<HoverUIData> dataToRemove = new List<HoverUIData>();
            foreach (var data in activeHoverData)
            {
                List<HoverUIElement> elementsToRemove = new List<HoverUIElement>();
                foreach (var item in data.elements)
                {
                    if (item.updateTimer)
                    {
                        if (item.timer < HoverUIElement.TIMER_MAX)
                            item.timer++;

                        item.updateTimer = false;
                        continue;
                    }
                    if (item.timer > 0)
                        item.timer--; 
                    else
                        elementsToRemove.Add(item);
                }
                data.elements.RemoveAll(elementsToRemove.Contains);
                if (data.elements.All(x => x.timer == 0))
                    dataToRemove.Add(data);
            }
            activeHoverData.RemoveAll(dataToRemove.Contains);
        }
    }
    public static class RadianceInterfacePlayerExtensions
    {
        public static void SetCurrentlyActivePlayerUIItem(this Player player, ModItem item)
        {
            if (item is not null)
            {
                player.GetModPlayer<RadianceInterfacePlayer>().currentlyActiveUIItem = item.Item;
                ((IPlayerUIItem)item).OnOpen();
            }
            else
                player.GetModPlayer<RadianceInterfacePlayer>().currentlyActiveUIItem = null;
        }

        public static ModItem GetCurrentActivePlayerUIItem(this Player player)
        {
            if (player.GetModPlayer<RadianceInterfacePlayer>().currentlyActiveUIItem is not null)
                return player.GetModPlayer<RadianceInterfacePlayer>().currentlyActiveUIItem.ModItem;
            return null;
        }

        public static bool HasActivePlayerUI(this Player player) => player.GetCurrentActivePlayerUIItem() is not null;

        public static void ResetActivePlayerUI(this Player player)
        {
            (player.GetCurrentActivePlayerUIItem() as IPlayerUIItem)?.OnClose();
            player.SetCurrentlyActivePlayerUIItem(null);

            // V move to onclear baselightarray
            // player.GetModPlayer<LightArrayPlayer>().lightArrayConfigOpen = false;
            // player.GetModPlayer<LightArrayPlayer>().lightArrayUITimer = player.GetModPlayer<LightArrayPlayer>().lightArrayConfigTimer = 0;
        }
    }
}