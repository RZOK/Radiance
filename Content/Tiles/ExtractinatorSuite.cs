using Radiance.Content.Items.BaseItems;
using System.Reflection;
using Terraria.Localization;
using Terraria.ObjectData;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Radiance.Content.Particles;
using Terraria.ID;
using Radiance.Core.Systems;

namespace Radiance.Content.Tiles
{
    public class ExtractinatorSuite : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = false;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
            TileObjectData.newTile.Width = 3;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.CoordinateHeights = new int[1] { 20 };
            HitSound = SoundID.Item52;
            DustType = -1;

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Extractinator Suite");
            AddMapEntry(new Color(219, 33, 0), name);

            TileObjectData.newTile.AnchorBottom = new AnchorData(Terraria.Enums.AnchorType.AlternateTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorAlternateTiles = new int[1] { TileID.Extractinator };

            TileObjectData.newTile.AnchorValidTiles = new int[] {
                TileID.Extractinator
            };
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<ExtractinatorSuiteTileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }

        public override void HitWire(int i, int j)
        {
            ToggleTileEntity(i, j);
        }

        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
        {
            offsetY = 4;
        }

        public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (TryGetTileEntityAs(i, j, out ExtractinatorSuiteTileEntity entity))
            {
                Texture2D mainTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/ExtractinatorSuiteArms").Value;
                Color tileColor = Lighting.GetColor(i, j);
                Vector2 mainPosition = entity.Position.ToWorldCoordinates(20, 22) + tileDrawingZero - Main.screenPosition;

                Main.spriteBatch.Draw(mainTexture, mainPosition, null, tileColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            }
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            if (drawData.tileFrameX == 0 && drawData.tileFrameY == 0)
                Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
        }

        public override bool RightClick(int i, int j)
        {
            if (TryGetTileEntityAs(i, j, out ExtractinatorSuiteTileEntity entity) && !Main.LocalPlayer.ItemAnimationActive)
            {
                Item item = GetPlayerHeldItem();
                bool success = false;

                if (entity.GetSlotsWithItems().Any() && (item.IsAir || item.favorited || !entity.TryInsertItemIntoSlot(item, entity.GetSlotsWithItems().Last())))
                {
                    byte lastSlot = entity.GetSlotsWithItems().Last();
                    entity.DropItem(lastSlot, entity.TileEntityWorldCenter(), out success);
                }

                if (ExtractinatorSuiteTileEntity.CanExtractinator(item.type))
                    entity.SafeInsertItemIntoInventory(item, out success);

                if (success)
                    SoundEngine.PlaySound(SoundID.MenuTick);

                return true;
            }
            return false;
        }

        public override void MouseOver(int i, int j)
        {
            if (TryGetTileEntityAs(i, j, out ExtractinatorSuiteTileEntity entity))
            {
                Main.LocalPlayer.SetCursorItem(ItemID.Hellstone);
                entity.AddHoverUI();
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (TryGetTileEntityAs(i, j, out ExtractinatorSuiteTileEntity entity))
                entity.DropAllItems(entity.TileEntityWorldCenter());

            ModContent.GetInstance<ExtractinatorSuiteTileEntity>().Kill(i, j);
        }
    }

    public class ExtractinatorSuiteTileEntity : RadianceUtilizingTileEntity, IInventory
    {
        public ExtractinatorSuiteTileEntity() : base(ModContent.TileType<ExtractinatorSuite>(), 100, new List<int>() { 0 }, new List<int>(), usesStability: true)
        {
            idealStability = 23;
        }
        public Item[] inventory { get; set; }
        public byte[] inputtableSlots => new byte[3] { 0, 1, 2 };
        public byte[] outputtableSlots => Array.Empty<byte>();
        private Player extractinatorPlayer = new Player();

        public Action<int, int> ExtractinatorUse;
        public override void Load()
        {
            IL_Player.ExtractinatorUse += DropItemsInCorrectPlace;
        }
        /// <summary>
        /// Vanilla forces the location of the items dropped by an extractinator to be the mouse cursor, so we need to direct it towards our own method instead for dropping if the player performing the extractinatoring is a fake player of the proper type.
        /// </summary>
        private void DropItemsInCorrectPlace(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            cursor.Index = cursor.Instrs.Count;

            if (!cursor.TryGotoPrev(MoveType.Before,
                i => i.MatchLdarg0(),
                i => i.MatchLdloc(7),
                i => i.MatchLdloc(8),
                i => i.MatchCall(out _)))
            {
                LogIlError("Extractinator Suite ExtractinatorUse", "Couldn't navigate to before item check");
                return;
            }
            ILLabel label = cursor.DefineLabel(); // Label before the DropItemFromExtractinator call so we can jump to it if player is not a fake player

            cursor.Emit(OpCodes.Ldarg_0); // Load player instance
            cursor.EmitDelegate(PlayerIsExtractinatorPlayer); // Check if player is an extractinator player
            cursor.Emit(OpCodes.Brfalse, label); // If the player is not fake, go to the label (skipping the rest of the IL that is being inserted by us)
            cursor.Emit(OpCodes.Ldarg_0); // Load the player instance
            cursor.Emit(OpCodes.Ldloc, 7); // Load the item type to be dropped
            cursor.Emit(OpCodes.Ldloc, 8); // Load the item stack to be dropped
            cursor.EmitDelegate(ProperlyExtractinate); // Perform the properly-positioned extractination
            cursor.Emit(OpCodes.Ret); // Return as to not run the vanilla extractinatoruse code
            cursor.MarkLabel(label); // Set the position of the earlier label
        }
        private static bool PlayerIsExtractinatorPlayer(Player player) => player.GetModPlayer<RadiancePlayer>().fakePlayerType == RadiancePlayer.FakePlayerType.Extractinator;
        private static void ProperlyExtractinate(Player player, int itemType, int itemStack)
        {
            Vector2 pos = player.Center;
            int number = Item.NewItem(new EntitySource_Misc("ExtractinatorSuite"), (int)pos.X, (int)pos.Y, 1, 1, itemType, itemStack, noBroadcast: false, -1);
            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, number, 1f);
        }

        public bool TryInsertItemIntoSlot(Item item, byte slot) => CanExtractinator(item.type) && itemImprintData.IsItemValid(item);
        
        public static List<int> ExtraExtractinatorables = new List<int>()
        {
            ItemID.SandBlock,
            ItemID.EbonsandBlock,
            ItemID.CrimsandBlock,
            ItemID.PearlsandBlock,
        };
        public static bool CanExtractinator(int type) => ItemID.Sets.ExtractinatorMode[type] > -1 || ExtraExtractinatorables.Contains(type);
        
        public float extractinateTimer = 0;
        public override void OrderedUpdate()
        {
            this.ConstructInventory(3);
            extractinatorPlayer.Center = this.TileEntityWorldCenter() + Vector2.UnitY * 32 + new Vector2(Main.rand.NextFloat(-16, 16), Main.rand.NextFloat(-16, 16));
            extractinatorPlayer.GetModPlayer<RadiancePlayer>().fakePlayerType = RadiancePlayer.FakePlayerType.Extractinator;
            ExtractinatorUse ??= (Action<int, int>)Delegate.CreateDelegate(typeof(Action<int, int>), extractinatorPlayer, typeof(Player).ReflectionGetMethodFromType("ExtractinatorUse", BindingFlags.Instance | BindingFlags.NonPublic));

            List<byte> slotsWithItems = this.GetSlotsWithItems();
            if (slotsWithItems.Any())
            {
                Item item = this.GetSlot(slotsWithItems.Last());
                if (enabled && !item.IsAir && CanExtractinator(item.type))
                {
                    float speed = 1;
                    if (!IsStabilized)
                        speed = 0.25f;

                    extractinateTimer += speed;

                    if(extractinateTimer % 1 == 0)
                        ParticleSystem.AddParticle(new ExtractinatorDust(this.TileEntityWorldCenter() + Vector2.UnitX * (8 + Main.rand.NextFloat(4)), 35, GetItemTexture(item.Clone().type), 1f));

                    if (extractinateTimer > 60)
                    {
                        ExtractinatorUse(ItemID.Sets.ExtractinatorMode[item.type], TileID.Extractinator);
                        SoundEngine.PlaySound(SoundID.CoinPickup, this.TileEntityWorldCenter());

                        item.stack--;
                        if (item.stack <= 0)
                            item.TurnToAir();

                        extractinateTimer = 0;
                    }
                }
            }
        }

        protected override HoverUIData ManageHoverUI()
        {
            List<HoverUIElement> data = new List<HoverUIElement>()
            {
                new StabilityBarElement("StabilityBar", stability, idealStability, Vector2.UnitX * -48)
            };
            for (int i = 0; i < 3; i++)
            {
                data.Add(new ItemUIElement("ExtractableCount", this.GetSlot((byte)i).type, new Vector2(0, -24).RotatedBy(1.6f * (i - 1)), this.GetSlot((byte)i).stack));
            }

            return new HoverUIData(this, this.TileEntityWorldCenter() - Vector2.UnitY * 8, data.ToArray());
        }

        public override void SaveExtraExtraData(TagCompound tag)
        {
            this.SaveInventory(tag);
        }

        public override void LoadExtraExtraData(TagCompound tag)
        {
            this.LoadInventory(tag, 3);
        }
    }

    public class ExtractinatorSuiteItem : BaseTileItem
    {
        public ExtractinatorSuiteItem() : base("ExtractinatorSuiteItem", "Extractinator Suite", "Automatically processes items when placed above an Extractinator", "ExtractinatorSuite", 1, Item.sellPrice(0, 0, 50, 0), ItemRarityID.Orange)
        {
        }
    }
}