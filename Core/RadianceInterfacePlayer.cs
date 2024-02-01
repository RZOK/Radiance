using Radiance.Content.Items.Tools.Misc;

namespace Radiance.Core
{
    public class RadianceInterfacePlayer : ModPlayer
    {
        public List<HoverUIData> currentHoveredObjects = new List<HoverUIData>();
        public List<HoverUIData> activeHoverData = new List<HoverUIData>();

        public float newEntryUnlockedTimer = 0;
        public int inventoryItemRightClickDelay = 0;
        public string incompleteEntryText = string.Empty;
        public string currentFakeHoverText = string.Empty;
        public bool fancyHoverTextBackground = false;
        public bool hoveringScrollWheelEntity = false;
        public bool canSeeItemImprints => Player.GetPlayerHeldItem().type == ModContent.ItemType<CeramicNeedle>();
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

        public override void ResetEffects()
        {
            incompleteEntryText = string.Empty;
            currentFakeHoverText = string.Empty;
            fancyHoverTextBackground = false;
            hoveringScrollWheelEntity = false;

            if (inventoryItemRightClickDelay > 0)
                inventoryItemRightClickDelay--;
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
                        if (item.timer < HoverUIElement.timerMax)
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
}