using MonoMod.RuntimeDetour;
using Radiance.Content.Items;
using Radiance.Content.Items.Tools.Misc;
using Radiance.Content.Tiles.Pedestals;
using Radiance.Core.RenderTargets;
using Radiance.Core.Systems;
using ReLogic.Content;
using Steamworks;
using Terraria.ObjectData;

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

        public string mapIcon;

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

        public static TileEntityIconRenderer mapIconRenderer = new TileEntityIconRenderer();
        public ImprovedTileEntity(int parentTile, string mapIcon = null, float updateOrder = 1, bool usesItemImprints = false)
        {
            ParentTile = parentTile;
            this.mapIcon = mapIcon;
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
                if (Main.LocalPlayer.HeldItem.type == ModContent.ItemType<MultifacetedLens>())
                {
                    RadianceInterfacePlayer riPlayer = Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>();
                    if (!riPlayer.visibleTileEntities.Remove(entity))
                        riPlayer.visibleTileEntities.Add(entity);

                    SoundEngine.PlaySound(SoundID.MenuTick);
                    return true;
                }

                if (entity.ItemImprintRightClick())
                    return true;
            }
            return orig(i, j);
        }

        #endregion Item Imprint Detour

        /// <summary>
        /// Adds HoverUIData to the localplayer's list of displayed data depending.
        /// <para />
        /// Create a list of HoverUIElements. Construct then add any that should be displayed. Return a new HoverUIData with the tile entity, the position the data will originate from, and the list as parameters.
        /// <para />
        /// Example: <see cref="PedestalTileEntity.GetHoverUI"/>
        /// </summary>
        /// <returns></returns>
        protected virtual HoverUIData GetHoverUI() => null;

        /// <summary>
        /// Draws map-based UI.
        /// </summary>
        public virtual void DrawMapUI(SpriteBatch spriteBatch, Vector2 position, float scale) { }

        public void AddHoverUI()
        {
            if (Main.LocalPlayer.mouseInterface && !Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().visibleTileEntities.Contains(this))
                return;

            HoverUIData data = GetHoverUI();
            if (data is null)
                return;

            if (Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().visibleTileEntities.Contains(this))
            {
                MultifacetedLensHoverElement element = new MultifacetedLensHoverElement();
                element.parent = data;
                data.elements.Add(element);
            }

            if (Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().CanSeeItemImprints && HasImprint)
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
            if (idealStability > 0 || this is StabilizerTileEntity)
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
            if (idealStability > 0 || this is StabilizerTileEntity)
                TileEntitySystem.shouldUpdateStability = true;
        }

        public bool ItemImprintRightClick()
        {
            if (usesItemImprints)
            {
                Item item = Main.LocalPlayer.PlayerHeldItem();
                if (item.type == ModContent.ItemType<ItemImprint>() && item.ModItem is ItemImprint itemImprint)
                {
                    itemImprintData = itemImprint.imprintData;
                    SoundEngine.PlaySound(SoundID.Grab);
                    item.TurnToAir();
                    return true;
                }
                else if (item.type == ModContent.ItemType<CeramicNeedle>())
                {
                    itemImprintData = default;
                    SoundEngine.PlaySound(SoundID.Grab);
                    return true;
                }
            }
            return false;
        }

        public virtual void SaveExtraData(TagCompound tag)
        { }

        public override sealed void SaveData(TagCompound tag)
        {
            tag[nameof(enabled)] = enabled;
            if (usesItemImprints)
                tag[nameof(ItemImprintData)] = itemImprintData;

            SaveExtraData(tag);
        }

        public virtual void LoadExtraData(TagCompound tag)
        { }

        public override sealed void LoadData(TagCompound tag)
        {
            enabled = tag.GetBool(nameof(enabled));
            if (usesItemImprints)
                itemImprintData = tag.Get<ItemImprintData>(nameof(ItemImprintData));

            LoadExtraData(tag);
        }

        /// <summary>
        /// Please set ideal stability in here for tile entities that set their ideal stability every tick instead of it being set in the constructor!
        /// </summary>
        public virtual void SetIdealStability()
        { }

        public override sealed void Update()
        { }

        /// <summary>
        /// Updates the tile entity in priority according to its <see cref="updateOrder"/>. Anything intended to run every tick should be done so here.
        /// <para />
        /// Standard ModTileEntity Update() is sealed. Use this instead.
        /// </summary>
        public virtual void OrderedUpdate()
        { }

        /// <summary>
        /// Runs on every ImprovedTileEntity in the same order as OrderedUpdate, but will do so in PreUpdateWorld as opposed to PostUpdateWorld.
        /// <para />
        /// The most common use case is resetting variables on lower-update-priority tile entities that then get set in <see cref="OrderedUpdate"/>
        /// <para />
        /// Example: <see cref="PedestalTileEntity.PreOrderedUpdate"/>
        /// </summary>
        public virtual void PreOrderedUpdate()
        { }

        /// <summary>
        /// Searches for each ImprovedTileEntity with a maximum distance of <paramref name="range"/> from the tile entity's center, making sure each corner is within the bounds as well.
        /// <para />
        /// If the width/height of the tile entity is even, it will expand the range east/south respectively by 1, meaning the 'center' is technically between two tiles.
        /// <para />
        /// Example: <see cref="CinderCrucibleTileEntity.OrderedUpdate"/>
        /// </summary>
        /// <param name="range">The distance from 'start' that a tile can be.</param>
        /// <returns>A list of each ImprovedTileEntity in range.</returns>
        public List<ImprovedTileEntity> TileEntitySearchHard(int range, Point16? offset = null)
        {
            List<ImprovedTileEntity> tileEntitiesInRange = new List<ImprovedTileEntity>();
            Point16 position = Position + new Point16((Width - 1) / 2, (Height - 1) / 2);
            if (offset.HasValue)
                position += offset.Value;

            foreach (ImprovedTileEntity tileEntity in TileEntitySystem.orderedEntities)
            {
                if (tileEntity == this)
                    continue;

                int horizontalRange = range;
                int verticalRange = range;
                if (Width % 2 == 0 && tileEntity.Position.X > position.X)
                    horizontalRange++;
                if (Height % 2 == 0 && tileEntity.Position.Y > position.Y)
                    verticalRange++;

                if(Math.Abs(position.X - tileEntity.Position.X) <= horizontalRange && Math.Abs(position.Y - tileEntity.Position.Y) <= verticalRange &&
                   Math.Abs(position.X - (tileEntity.Position.X + tileEntity.Width - 1)) <= horizontalRange && Math.Abs(position.Y - (tileEntity.Position.Y + tileEntity.Height - 1)) <= verticalRange)
                    tileEntitiesInRange.Add(tileEntity);
            }
            return tileEntitiesInRange; 
        }
    }
    
    public class TileEntityIconRenderer : INeedRenderTargetContent
    {
        private Dictionary<int,TileEntityIconRenderTargetContent> tileEntityToOutlinedIcon;

        public bool IsReady => false;

        public TileEntityIconRenderer()
        {
            Main.ContentThatNeedsRenderTargets.Add(this);
            Reset();
        }

        public void Reset()
        {
            tileEntityToOutlinedIcon = new Dictionary<int, TileEntityIconRenderTargetContent>();
        }

        public void DrawWithOutlines(ImprovedTileEntity tileEntity, Vector2 position, Color color, float rotation, float scale, SpriteEffects effects, out bool remove)
        {
            remove = false;
            int item = GetItemTypeForTileType(tileEntity.ParentTile);
            if (item == 0)
                return;

            if (!tileEntityToOutlinedIcon.ContainsKey(tileEntity.type))
            {
                tileEntityToOutlinedIcon[tileEntity.Type] = new TileEntityIconRenderTargetContent();
                tileEntityToOutlinedIcon[tileEntity.Type].SetTexture(GetItemTexture(GetItemTypeForTileType(tileEntity.ParentTile)));
                
            }
            TileEntityIconRenderTargetContent tileEntityIconRenderTargetContent = tileEntityToOutlinedIcon[tileEntity.Type];
            if (tileEntityIconRenderTargetContent.IsReady)
            {
                if (!Main.mapFullscreen && Main.mapStyle == 1)
                {
                    Rectangle mapRect = new Rectangle(Main.miniMapX, Main.miniMapY, Main.miniMapWidth, Main.miniMapHeight);
                    if (!mapRect.Contains(new Point((int)position.X, (int)position.Y)))
                        return;
                }
                RenderTarget2D target = tileEntityIconRenderTargetContent.GetTarget();
                Rectangle collisionRect = new Rectangle((int)(position.X - target.Width / 2f), (int)(position.Y - target.Height / 2f), target.Width, target.Height);
                float scaleModifier = 1f;
                if(collisionRect.Contains(Main.MouseScreen.ToPoint()))
                {
                    scaleModifier = 1.5f;
                    Main.instance.MouseText(ItemLoader.GetItem(item).DisplayName.Value);
                    if (Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        remove = true;
                        SoundEngine.PlaySound(SoundID.MenuTick);
                    }
                }


                Main.spriteBatch.Draw(target, position, null, color, rotation, target.Size() / 2f, scale * scaleModifier, effects, 0f);
            }
            else
            {
                tileEntityIconRenderTargetContent.Request();
            }
        }

        public void PrepareRenderTarget(GraphicsDevice device, SpriteBatch spriteBatch)
        {
            foreach (var renderTargetContent in tileEntityToOutlinedIcon.Values)
            {
                if (renderTargetContent!= null && !renderTargetContent.IsReady)
                {
                    renderTargetContent.PrepareRenderTarget(device, spriteBatch);
                }
            }
        }
    }
    public class TileEntityIconRenderTargetContent : RadianceOutlinedDrawRenderTargetContent
    {
        private Texture2D texture;

        public void SetTexture(Texture2D texture)
        {
            if (this.texture != texture)
            {
                this.texture = texture;
                _wasPrepared = false;
                width = texture.Width + 8;
                height = texture.Height + 8;
            }
        }

        public override void DrawTheContent(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, new Vector2(4f, 4f), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
    }
}