using Radiance.Content.Items.BaseItems;
using System.Reflection;
using Terraria.Localization;
using Terraria.ObjectData;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Radiance.Content.Particles;
using Terraria.ID;
using Radiance.Core.Systems;
using Radiance.Content.Items.Materials;
using Humanizer;
using Steamworks;
using log4net.Appender;
using Radiance.Core.Systems.ParticleSystems;

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
                Color tileColor = Lighting.GetColor(i, j);
                Vector2 mainPosition = entity.Position.ToWorldCoordinates() + TileDrawingZero - Main.screenPosition;
                int extractinatorFrame = Main.tileFrame[TileID.Extractinator];
                int height = extractinatorFrame < 5 ? extractinatorFrame * 2 : 20 - extractinatorFrame * 2;

                Texture2D pistonTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/ExtractinatorSuitePiston").Value;
                Vector2 pistonPosition = mainPosition + new Vector2(6, height);
                Rectangle pistonFrame = new Rectangle(0, extractinatorFrame * 18, 20, 18);
                Main.spriteBatch.Draw(pistonTexture, pistonPosition, pistonFrame, tileColor, 0, new Vector2(pistonFrame.Width / 2, 0), 1, SpriteEffects.None, 0);

                Texture2D armTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/ExtractinatorSuiteArms").Value;
                Vector2 armPosition = mainPosition + new Vector2(12, 14);
                Main.spriteBatch.Draw(armTexture, armPosition, null, tileColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                Texture2D orbGlowTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/ExtractinatorSuiteOrbGlow").Value;
                Vector2 orbGlowPosition = mainPosition + new Vector2(2, 2);
                Main.spriteBatch.Draw(orbGlowTexture, orbGlowPosition, null, Color.White * entity.glowModifier, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                Texture2D crystalMeterTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/ExtractinatorSuiteCrystalGlow").Value;
                Vector2 crystalMeterPosition = mainPosition + new Vector2(16, 4);
                float filledRatio = entity.crystalCharge / ExtractinatorSuiteTileEntity.CRYSTAL_CHARGE_MAX;
                int filledPixels = (int)MathF.Ceiling(crystalMeterTexture.Width * filledRatio - crystalMeterTexture.Width * filledRatio % 2 + 2) / 2;
                float baseColorModifier = 1f;
                for (int k = 0; k < filledPixels; k++)
                {
                    float colorModifier = MathF.Min(filledRatio * 4 - k, 1f);
                    Main.spriteBatch.Draw(crystalMeterTexture, crystalMeterPosition + Vector2.UnitX * 2 * k, new Rectangle(2 * k, 0, 2, crystalMeterTexture.Height), Color.White * MathF.Min(baseColorModifier * colorModifier, 1f), 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                }
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

                if (item.IsAir || item.favorited || !entity.CanInsertItemIntoInventory(item))
                {
                    if (entity.GetSlotsWithItems(end: 3).Count != 0)
                    {
                        byte lastSlot = entity.GetSlotsWithItems(end: 3).Last();
                        entity.DropItem(lastSlot, entity.TileEntityWorldCenter(), out success);
                    }
                    else if (!entity.GetSlot(3).IsAir)
                        entity.DropItem(3, entity.TileEntityWorldCenter(), out success);
                }
                if(!item.IsAir && !item.favorited)
                    entity.SafeInsertItemIntoInventory(item, out success, true, true);

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
        public ExtractinatorSuiteTileEntity() : base(ModContent.TileType<ExtractinatorSuite>(), 100, new List<int>() { 1 }, new List<int>(), usesItemImprints: true)
        {
            inventorySize = 4;
            idealStability = 23;
            this.ConstructInventory();

            ExtractinatorUse ??= (Action<int, int>)Delegate.CreateDelegate(typeof(Action<int, int>), extractinatorPlayer, typeof(Player).GetMethod("ExtractinatorUse", BindingFlags.Instance | BindingFlags.NonPublic));
            DropItemFromExtractinator ??= (Action<int, int>)Delegate.CreateDelegate(typeof(Action<int, int>), extractinatorPlayer, typeof(Player).GetMethod("DropItemFromExtractinator", BindingFlags.Instance | BindingFlags.NonPublic));
        }
        public Item[] inventory { get; set; }
        public byte[] inputtableSlots => new byte[4] { 0, 1, 2, 3 };
        public byte[] outputtableSlots => Array.Empty<byte>();
        public int inventorySize { get; set; }
        public Tile ExtractinatorBelow => Framing.GetTileSafely(Position.X, Position.Y + 1);


        public Action<int, int> ExtractinatorUse;
        public Action<int, int> DropItemFromExtractinator;

        public float extractinateTimer = 0;
        public float crystalCharge = 0;
        public float glowModifier = 0;
        private readonly Player extractinatorPlayer = new Player();

        public const float ORB_GLOW_TIME_MAX = 60;
        public const float CRYSTAL_CHARGE_MAX = 1200;
        public const float REQUIRED_RADIANCE = 0.007f;

        public delegate int ExtractinateDelegate(int inputItem, out int outputStack);
        public static event ExtractinateDelegate ExtractinatorSuiteExtrasEvent;
        public override void Load()
        {
            IL_Player.ExtractinatorUse += DropItemsInCorrectPlace;
            ExtractinatorSuiteExtrasEvent += ProcessSands;
        }
        public override void Unload()
        {
            ExtractinatorSuiteExtrasEvent -= ProcessSands;
        }
        public static bool CanExtractinator(int type) => ItemID.Sets.ExtractinatorMode[type] > -1 || ExtraExtractinatorables.Contains(type);
        private Vector2 ExtractinatorCenter => this.TileEntityWorldCenter() + Vector2.UnitY * 32 + new Vector2(Main.rand.NextFloat(-16, 16), Main.rand.NextFloat(-16, 16));

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
        private static bool PlayerIsExtractinatorPlayer(Player player) => player.GetModPlayer<RadiancePlayer>().fakePlayerType == RadiancePlayer.FakePlayerType.ExtractinatorSuite;
        private static void ProperlyExtractinate(Player player, int itemType, int itemStack)
        {
            Vector2 pos = player.Center;
            int number = Item.NewItem(new EntitySource_Misc("ExtractinatorSuite"), (int)pos.X, (int)pos.Y, 1, 1, itemType, itemStack, noBroadcast: false, -1);
            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, number, 1f);
        }

        public bool TryInsertItemIntoSlot(Item item, byte slot, bool overrideValidInputs, bool ignoreItemImprint)
        {
            if (slot == 3)
                return item.type == ModContent.ItemType<PetrifiedCrystal>();

            if ((!ignoreItemImprint && !itemImprintData.IsItemValid(item)) || (!overrideValidInputs && !inputtableSlots.Contains(slot)))
                return false;
            
            return CanExtractinator(item.type);
        }
        /// <summary>
        /// If you want an item to be processable by the suite but not by normal extractinators, add it to this list and add an event to <see cref="ExtractinatorSuiteExtrasEvent"/> for it
        /// </summary>
        public static List<int> ExtraExtractinatorables = new List<int>()
        {
            ItemID.SandBlock,
            ItemID.EbonsandBlock,
            ItemID.CrimsandBlock,
            ItemID.PearlsandBlock,
        };

        private int ProcessSands(int inputItem, out int outputStack)
        {
            outputStack = 0;
            List<int> sands = new List<int>()
            {
                ItemID.SandBlock,
                ItemID.EbonsandBlock,
                ItemID.CrimsandBlock,
                ItemID.PearlsandBlock
            };

            if (sands.Contains(inputItem))
            {
                int random = Main.rand.Next(200);
                if (random < 190)
                    return ItemID.None;

                if (random < 195)
                {
                    outputStack = 1;
                    return ItemID.WhitePearl;
                }
                if (random < 199)
                {
                    outputStack = 1;
                    return ItemID.BlackPearl;
                }
                outputStack = 1;
                return ItemID.PinkPearl;
            }
            return 0;
        }

        public override void OrderedUpdate()
        {
            extractinatorPlayer.Center = ExtractinatorCenter;
            extractinatorPlayer.GetModPlayer<RadiancePlayer>().fakePlayerType = RadiancePlayer.FakePlayerType.ExtractinatorSuite;
            
            List<byte> slotsWithExtractinatableItems = this.GetSlotsWithItems(end: 3);
            if (enabled && storedRadiance >= REQUIRED_RADIANCE && slotsWithExtractinatableItems.Count != 0)
            {
                Item itemToProcess = this.GetSlot(slotsWithExtractinatableItems.Last());
                if (!itemToProcess.IsAir && CanExtractinator(itemToProcess.type))
                {
                    // if there's no petrified crystal charge, consume one and set charge to 20 seconds worth
                    if (crystalCharge <= 0)
                    {
                        Item crystalItem = this.GetSlot(3);
                        if (!crystalItem.IsAir)
                        {
                            crystalItem.stack--;
                            if (crystalItem.stack <= 0)
                                crystalItem.TurnToAir();

                            crystalCharge = CRYSTAL_CHARGE_MAX;
                        }
                    }
                    // if there is charge, function as normal. not an else so that both can happen in the same tick
                    if (crystalCharge > 0)
                    {
                        ProcessItem(itemToProcess);
                        return;
                    }
                }
            }
            if (glowModifier > 0)
                glowModifier -= 1f / ORB_GLOW_TIME_MAX;
        }
        private void ProcessItem(Item item)
        {
            extractinateTimer += IsStabilized ? 1.5f : 1;

            if (extractinateTimer % 12 == 0)
            {
                Item clonedItem = item.Clone();
                Main.instance.LoadItem(clonedItem.type);
                WorldParticleSystem.system.AddParticle(new ExtractinatorDust(this.TileEntityWorldCenter() + Vector2.UnitX * (8 + Main.rand.NextFloat(4)), 20, GetItemTexture(clonedItem.type), Main.rand.NextFloat(0.8f, 1f)));
            }
            if (extractinateTimer >= 300)
            {
                if (ExtraExtractinatorables.Contains(item.type))
                {
                    int outputItem = 0;
                    int outputStack = 0;
                    foreach (ExtractinateDelegate del in ExtractinatorSuiteExtrasEvent.GetInvocationList())
                    {
                        int eventOutputItem = del(item.type, out int eventOutputItemStack);
                        if (eventOutputItem != ItemID.None)
                        {
                            outputItem = eventOutputItem;
                            outputStack = eventOutputItemStack;
                            break;
                        }
                    }
                    if (outputItem != ItemID.None)
                    {
                        int number = Item.NewItem(new EntitySource_TileEntity(this), (int)extractinatorPlayer.Center.X, (int)extractinatorPlayer.Center.Y, 1, 1, outputItem, outputStack, noBroadcast: false, -1);
                        if (Main.netMode == NetmodeID.MultiplayerClient)
                            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, number, 1f);
                    }
                }
                else
                    ExtractinatorUse(ItemID.Sets.ExtractinatorMode[item.type], TileID.Extractinator);

                item.stack--;
                if (item.stack <= 0)
                    item.TurnToAir();

                SoundEngine.PlaySound(SoundID.CoinPickup, this.TileEntityWorldCenter());
                extractinateTimer = 0;
            }
            if (glowModifier < 1f)
                glowModifier += 1f / ORB_GLOW_TIME_MAX;

            crystalCharge--;
            storedRadiance -= REQUIRED_RADIANCE;
        }

        protected override HoverUIData ManageHoverUI()
        {
            List<HoverUIElement> data = new List<HoverUIElement>()
            {
                new RadianceBarUIElement("RadianceBar", storedRadiance, maxRadiance, Vector2.UnitY * 40),
                new StabilityBarElement("StabilityBar", stability, idealStability, new Vector2(-48, -32))
            };
            List<byte> slotsWithItems = this.GetSlotsWithItems(0, 3);
            for (int i = 0; i < slotsWithItems.Count; i++)
            {
                byte slot = slotsWithItems[i];
                data.Add(new ItemUIElement("ItemSlot" + i, this.GetSlot(slot).type, new Vector2(40 * i, -22f), this.GetSlot(slot).stack));
            }

            if (!this.GetSlot(3).IsAir)
            {
                float height = 22;
                if (slotsWithItems.Count != 0)
                    height = 52;

                data.Add(new ItemUIElement("PetrifiedCrystalCount", this.GetSlot(3).type, Vector2.UnitY * - height, this.GetSlot(3).stack));
            }
            return new HoverUIData(this, this.TileEntityWorldCenter(), data.ToArray());
        }

        public override void SaveExtraExtraData(TagCompound tag) => this.SaveInventory(tag);

        public override void LoadExtraExtraData(TagCompound tag) => this.LoadInventory(tag);
    }

    public class ExtractinatorSuiteItem : BaseTileItem
    {
        public ExtractinatorSuiteItem() : base("ExtractinatorSuiteItem", "Extractinator Suite", "Automatically processes items when placed above an Extractinator", "ExtractinatorSuite", 1, Item.sellPrice(0, 0, 50, 0), ItemRarityID.Orange) { }
    }
    public class ExtractinatorSuiteUIElement : HoverUIElement
    {
        public List<Item> items;
        public float rotationAmount;
        public float distance;
        public ExtractinatorSuiteUIElement(string name, List<Item> items, float rotationAmount, float distance) : base(name) 
        {
            this.items = items;
            this.rotationAmount = rotationAmount;
            this.distance = distance;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < items.Count; i++)
            {
                Item item = items[i];

                Texture2D softGlow = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SoftGlowNoBG").Value;
                Rectangle drawBox = Item.GetDrawHitbox(item.type, null);
                Vector2 itemSize = new Vector2(drawBox.Width, drawBox.Height);

                float scale = Math.Min(timerModifier + 0.5f, 1f);
                Vector2 offset = new Vector2(0 + 40 * i, -distance - 6f);

                spriteBatch.Draw(softGlow, realDrawPosition + offset * timerModifier, null, Color.Black * 0.25f, 0, softGlow.Size() / 2, itemSize.Length() / 80, 0, 0);
                RadianceDrawing.DrawHoverableItem(spriteBatch, item.type, realDrawPosition + offset * timerModifier, item.stack, scale: scale, hoverable: false);
            }
        }
    }
}