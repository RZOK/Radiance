using Radiance.Core.Systems;
using Terraria.ObjectData;
using Radiance.Content.Tiles.Pedestals;
using Radiance.Content.Items;
using MonoMod.RuntimeDetour;
using Radiance.Content.Items.Tools.Misc;

namespace Radiance.Core.TileEntities
{
    /// <summary>
    /// An 'improved' abstract ModTileEntity that comes with necessary placement methods, size properties, stability support, Hover UI support, and ordered updating.
    /// <para />
    /// All tile entities in the mod should inherit from this as opposed to ModTileEntity.
    /// </summary>
    public abstract class ImprovedTileEntity : ModTileEntity
    {
        /// <summary>
        /// The Tile ID that this Tile Entity's 'parent' will be.
        /// </summary>
        public readonly int ParentTile;
        public bool IsStabilized => idealStability > 0 && Math.Abs(1 - stability / idealStability) <= 0.1f;
        public float idealStability;
        public float stability;
        /// <summary>
        /// Whether the tile entity is wire-enabled or not.
        /// </summary>
        public bool enabled = true;

        /// <summary>
        /// Whether the tile entiy can hold Item Imprint data
        /// </summary>
        public bool usesItemImprints = false;
        public bool HasImprint => itemImprintData.imprintedItems.AnyAndExists();
        public ItemImprintData itemImprintData;

        /// <summary>
        /// The priority for updating in <see cref="TileEntitySystem.orderedEntities"/>. Higher numbers will go first.
        /// </summary>
        public float updateOrder;
        public int Width => TileObjectData.GetTileData(ParentTile, 0).Width;
        public int Height => TileObjectData.GetTileData(ParentTile, 0).Height;

        public ImprovedTileEntity(int parentTile, float updateOrder = 1, bool usesItemImprints = false)
        {
            ParentTile = parentTile;
            this.updateOrder = updateOrder;
            this.usesItemImprints = usesItemImprints;
        }

        #region Item Imprint Detour

        private static Hook RightClickDetour;
        public override void Load()
        {
            RightClickDetour ??= new Hook(typeof(TileLoader).GetMethod("RightClick"), ApplyItemImprintOrLens);

            if (!RightClickDetour.IsApplied)
                RightClickDetour.Apply();
        }
        public override void Unload()
        {
            if (RightClickDetour.IsApplied)
                RightClickDetour.Undo();
        }
        private static bool ApplyItemImprintOrLens(Func<int, int, bool> orig, int i, int j)
        {
            if (TryGetTileEntityAs(i, j, out ImprovedTileEntity entity) && !Main.LocalPlayer.ItemAnimationActive)
            {
                if(Main.LocalPlayer.HeldItem.type == ModContent.ItemType<MultifacetedLens>())
                {
                    RadianceInterfacePlayer riPlayer = Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>();
                    if (!riPlayer.visibleTileEntities.Remove(entity))
                        riPlayer.visibleTileEntities.Add(entity);

                    SoundEngine.PlaySound(SoundID.MenuTick);
                    return true;
                }

                if(entity.ItemImprintRightClick())
                    return true;
            }
            return orig(i, j);
        }

        #endregion

        /// <summary>
        /// Adds HoverUIData to the localplayer's list of displayed data depending.
        /// <para />
        /// Create a list of HoverUIElements. Construct then add any that should be displayed. Return a new HoverUIData with the tile entity, the position the data will originate from, and the list as parameters.
        /// <para />
        /// Example: <see cref="PedestalTileEntity.GetHoverData"/>
        /// </summary>
        /// <returns></returns>
        protected virtual HoverUIData GetHoverData() => null;
        public void AddHoverUI()
        {
            if (Main.LocalPlayer.mouseInterface && !Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().visibleTileEntities.Contains(this))
                return;

            HoverUIData data = GetHoverData();
            if (Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().canSeeItemImprints && HasImprint)
                data = GetItemImprintHoverUI();

            data.elements.ForEach(x => x.updateTimer = true);
            var dataInList = Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().activeHoverData.FirstOrDefault(x => x.entity == this);
            if (dataInList != null)
            {
                foreach (var element in data.elements)
                {
                    var contained = dataInList.elements.FirstOrDefault(x => x.name == element.name);
                    if (contained != null)
                    {
                        HoverUIElement elementToAdd = (HoverUIElement)element.Clone();
                        elementToAdd.timer = contained.timer;
                        dataInList.elements.Add(elementToAdd);
                        dataInList.elements.Remove(contained);
                    }
                    else
                        dataInList.elements.Add(element);
                }
            }
            else
                Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().activeHoverData.Add(data);
        }
        public HoverUIData GetItemImprintHoverUI() => new HoverUIData(this, this.TileEntityWorldCenter(), new HoverUIElement[] { new ItemImprintUIElement("ItemImprint", itemImprintData, -Vector2.UnitY * Height * 8) });
        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ParentTile;
        }
        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            Point origin = GetTileOrigin(i, j);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, origin.X, origin.Y, Width, Height);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, origin.X, origin.Y, Type);
            }
            int placedEntity = Place(origin.X, origin.Y);
            if(idealStability > 0 || this is StabilizerTileEntity)
                TileEntitySystem.shouldUpdateStability = true;
            return placedEntity;
        }
        public override void OnNetPlace()
        {
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
        }
        public override void OnKill()
        {
            foreach (Player player in Main.ActivePlayers)
            {
                player.GetModPlayer<RadianceInterfacePlayer>().visibleTileEntities.Remove(this);
            }
            if(idealStability > 0 || this is StabilizerTileEntity)
                TileEntitySystem.shouldUpdateStability = true;
        }
        public bool ItemImprintRightClick()
        {
            if (usesItemImprints)
            {
                Item item = Main.LocalPlayer.GetPlayerHeldItem();
                if (item.type == ModContent.ItemType<ItemImprint>() && item.ModItem is ItemImprint itemImprint)
                {
                    itemImprintData = itemImprint.imprintData;
                    SoundEngine.PlaySound(SoundID.Grab);
                    item.TurnToAir();
                    return true;
                }
                else if(item.type == ModContent.ItemType<CeramicNeedle>())
                {
                    itemImprintData = default;
                    SoundEngine.PlaySound(SoundID.Grab);
                    return true;
                }
            }
            return false;
        }
        public virtual void SaveExtraData(TagCompound tag) { }
        public sealed override void SaveData(TagCompound tag)
        {
            tag[nameof(enabled)] = enabled;
            if (usesItemImprints)
                tag[nameof(ItemImprintData)] = itemImprintData;
            
            SaveExtraData(tag);
        }
        public virtual void LoadExtraData(TagCompound tag) { }
        public sealed override void LoadData(TagCompound tag)
        {
            enabled = tag.GetBool(nameof(enabled));
            if (usesItemImprints)
                itemImprintData = tag.Get<ItemImprintData>(nameof(ItemImprintData));

            LoadExtraData(tag);
        }
        /// <summary>
        /// Please set ideal stability in here for tile entities that set their ideal stability every tick instead of it being set in the constructor!  
        /// </summary>
        public virtual void SetIdealStability() { }
        public override sealed void Update() { }

        /// <summary>
        /// Updates the tile entity in priority according to its <see cref="updateOrder"/>. Anything intended to run every tick should be done so here.
        /// <para />
        /// Standard ModTileEntity Update() is sealed. Use this instead.
        /// </summary>
        public virtual void OrderedUpdate() { }

        /// <summary>
        /// Runs on every ImprovedTileEntity in the same order as OrderedUpdate, but will do so in PreUpdateWorld as opposed to PostUpdateWorld. 
        /// <para />
        /// The most common use case is resetting variables on lower-update-priority tile entities that then get set in <see cref="OrderedUpdate"/>
        /// <para />
        /// Example: <see cref="PedestalTileEntity.PreOrderedUpdate"/>
        /// </summary>
        public virtual void PreOrderedUpdate() { }
    }
}