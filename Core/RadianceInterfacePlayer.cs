using Mono.Cecil.Cil;
using MonoMod.Cil;
using Radiance.Content.Items.Tools.Misc;
using Terraria.UI;

namespace Radiance.Core
{
    public class RadianceInterfacePlayer : ModPlayer
    {
        public List<HoverUIData> currentHoveredObjects = new List<HoverUIData>();
        public List<HoverUIData> activeHoverData = new List<HoverUIData>();

        public bool canSeeRays = false;
        public float newEntryUnlockedTimer = 0;
        public int inventoryItemRightClickDelay = 0;

        public string currentFakeHoverText = string.Empty;
        public Color hoverTextBGColor;
        public Texture2D hoverTextBGTexture;

        public bool hoveringScrollWheelEntity = false;
        public bool canSeeLensItems = false;
        public List<ImprovedTileEntity> visibleTileEntities = new List<ImprovedTileEntity>();
        public Item currentlyActiveUIItem;
        public Item realHoveredItem;
        public int realCursorItemType;
        public bool CanSeeItemImprints => Player.PlayerHeldItem().type == ModContent.ItemType<CeramicNeedle>();
        public override void Load()
        {
            IL_Main.DrawInterface_40_InteractItemIcon += GetRealCursorItem;
            On_Player.ScrollHotbar += DontScrollHotbar;
            On_ItemSlot.MouseHover_ItemArray_int_int += GetRealHoveredItem;
        }

        private void GetRealHoveredItem(On_ItemSlot.orig_MouseHover_ItemArray_int_int orig, Item[] inv, int context, int slot)
        {
            Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().realHoveredItem = inv[slot];
            orig(inv, context, slot);
        }

        public override void Unload()
        {
            IL_Main.DrawInterface_40_InteractItemIcon -= GetRealCursorItem;
            On_Player.ScrollHotbar -= DontScrollHotbar;
            On_ItemSlot.MouseHover_ItemArray_int_int -= GetRealHoveredItem;
        }
        private void DontScrollHotbar(On_Player.orig_ScrollHotbar orig, Player self, int Offset)
        {
            if (self.GetModPlayer<RadianceInterfacePlayer>().hoveringScrollWheelEntity)
                return;

            orig(self, Offset);
        }
        private void GetRealCursorItem(ILContext il)
        {
            // in the case that the cursor item isn't set manually, it picks the player's held item (when in range)
            // however, it doesn't save that value anywhere
            // so we have to grab it manually when it's drawn

            ILCursor cursor = new ILCursor(il);
            cursor.EmitDelegate<Action>(() => Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().realCursorItemType = 0);
            if (!cursor.TryGotoNext(MoveType.After,
               i => i.MatchLdsfld(typeof(Main), nameof(Main.instance)),
               i => i.MatchLdloc1(),
               i => i.MatchCallvirt(typeof(Main), nameof(Main.LoadItem))
               ))
            {
                LogIlError("RadialUIMouseIndicator real cursor item grab", "Couldn't navigate to after instance.LoadItem(num)");
                return;
            }
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.EmitDelegate<Action<int>>((x) => Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().realCursorItemType = x);
        }
        public override void OnEnterWorld()
        {
            visibleTileEntities.Clear();
        }
        public override void ResetEffects()
        {
            realHoveredItem = null;
            canSeeRays = false;

            currentFakeHoverText = string.Empty;
            hoverTextBGTexture = TextureAssets.InventoryBack13.Value;
            hoverTextBGColor = new Color(23, 25, 81, 255) * 0.925f; //vanilla default

            hoveringScrollWheelEntity = false;
            canSeeLensItems = false;

            if (inventoryItemRightClickDelay > 0)
                inventoryItemRightClickDelay--;
        }
        public override void UpdateDead()
        {
            canSeeRays = false;

            currentFakeHoverText = string.Empty;
            hoverTextBGTexture = TextureAssets.InventoryBack13.Value;
            hoverTextBGColor = new Color(23, 25, 81, 255) * 0.925f;

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
        public void SetFakeHoverText(string text, Color? color = null, Texture2D tex = null)
        {
            Main.LocalPlayer.mouseInterface = true;
            currentFakeHoverText = text;
            if(color.HasValue)
                hoverTextBGColor = color.Value;
            if (tex is not null)
                hoverTextBGTexture = tex;
        }
    }
    public static class RadianceInterfacePlayerExtensions
    {
        public static void SetCurrentUIItem(this Player player, ModItem item)
        {
            if (item is not null)
            {
                player.GetModPlayer<RadianceInterfacePlayer>().currentlyActiveUIItem = item.Item;
                ((IPlayerUIItem)item).OnOpen();
            }
            else
                player.GetModPlayer<RadianceInterfacePlayer>().currentlyActiveUIItem = null;
        }

        public static ModItem GetCurrentUIItem(this Player player)
        {
            if (player.GetModPlayer<RadianceInterfacePlayer>().currentlyActiveUIItem is not null)
                return player.GetModPlayer<RadianceInterfacePlayer>().currentlyActiveUIItem.ModItem;
            return null;
        }

        public static bool HasActiveItemUI(this Player player) => player.GetCurrentUIItem() is not null;

        public static void ResetActiveItemUI(this Player player)
        {
            (player.GetCurrentUIItem() as IPlayerUIItem)?.OnClose();
            player.SetCurrentUIItem(null);
        }
        public static void SetFakeHoverText(this Player player, string text, Color? color = null, Texture2D tex = null) => player.GetModPlayer<RadianceInterfacePlayer>().SetFakeHoverText(text, color, tex);
        public static void SetFakeHoverText(this Player player, string[] text, Color? color = null, Texture2D tex = null) => player.GetModPlayer<RadianceInterfacePlayer>().SetFakeHoverText(string.Join('\n', text), color, tex);
    }
}