using Radiance.Content.Items.BaseItems;
using rail;
using ReLogic.Graphics;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles
{
    public class CeaselessSundial : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.Height = 4;
            TileObjectData.newTile.CoordinateHeights = new int[4] { 16, 16, 16, 18 };
            HitSound = SoundID.Dig;
            DustType = -1;

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Ceaseless Stardial");
            AddMapEntry(new Color(0, 188, 255), name);

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<CeaselessSundialTileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }

        public override void HitWire(int i, int j)
        {
            ToggleTileEntity(i, j);
        }

        public override bool RightClick(int i, int j)
        {
            if (TryGetTileEntityAs(i, j, out CeaselessSundialTileEntity entity))
            {
                entity.triggerCount *= 2;
                if (entity.triggerCount > 32)
                    entity.triggerCount = 1;

                SoundEngine.PlaySound(SoundID.MenuTick);
            }
            return true;
        }

        public override void MouseOver(int i, int j)
        {
            if (TryGetTileEntityAs(i, j, out CeaselessSundialTileEntity entity))
            {
                Main.LocalPlayer.SetCursorItem(ModContent.ItemType<CeaselessSundialItem>());
                entity.AddHoverUI();
                Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().hoveringScrollWheelEntity = true;
                if (PlayerInput.ScrollWheelDelta > 0 && entity.triggerCount < 32)
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    entity.triggerCount *= 2;
                    entity.SetNextTrigger();
                }
                else if (PlayerInput.ScrollWheelDelta < 0 && entity.triggerCount > 1)
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    entity.triggerCount /= 2;
                    entity.SetNextTrigger();
                }
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            ModContent.GetInstance<CeaselessSundialTileEntity>().Kill(i, j);
        }
    }

    public class CeaselessSundialTileEntity : ImprovedTileEntity
    {
        public CeaselessSundialTileEntity() : base(ModContent.TileType<CeaselessSundial>(), 1)
        {
        }

        public int triggerCount = 1;
        public int nextTrigger = 0;
        internal double maxTime = Main.dayLength + Main.nightLength;

        public override void OrderedUpdate()
        {
            int time = (int)Main.time;
            if (!Main.dayTime)
                time += (int)Main.dayLength;

            if (nextTrigger == 0)
                SetNextTrigger();

            if (time >= nextTrigger || time < nextTrigger - maxTime / triggerCount)
            {
                if (enabled)
                    Wiring.TripWire(Position.X, Position.Y, Width, Height);

                SetNextTrigger();
            }
        }

        internal void SetNextTrigger()
        {
            int time = (int)Main.time;
            if (!Main.dayTime)
                time += (int)Main.dayLength;

            nextTrigger = (int)(time - time % (maxTime / triggerCount) + maxTime / triggerCount);
            if (nextTrigger > maxTime)
                nextTrigger = (int)maxTime / triggerCount;
        }

        protected override HoverUIData ManageHoverUI()
        {
            List<HoverUIElement> data = new List<HoverUIElement>()
            {
                new CeaselessSundialUIElement("TriggerCount", Vector2.UnitY * -36)
            };

            return new HoverUIData(this, this.TileEntityWorldCenter(), data.ToArray());
        }

        public override void SaveExtraData(TagCompound tag)
        {
            tag[nameof(triggerCount)] = triggerCount;
        }

        public override void LoadExtraData(TagCompound tag)
        {
            triggerCount = tag.GetInt(nameof(triggerCount));
        }
    }

    public class CeaselessSundialUIElement : HoverUIElement
    {
        public List<Item> items;
        public float rotationAmount;
        public float distance;

        public CeaselessSundialUIElement(string name, Vector2 targetPosition) : base(name)
        {
            this.name = name;
            this.targetPosition = targetPosition;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (parent.entity is CeaselessSundialTileEntity entity)
            {
                DynamicSpriteFont font = FontAssets.MouseText.Value;
                Texture2D tex = ModContent.Request<Texture2D>("Radiance/Content/Tiles/CeaselessSundial_Clock").Value;
                Texture2D handTex = ModContent.Request<Texture2D>("Radiance/Content/Tiles/CeaselessSundial_ClockHand").Value;
                Vector2 origin = tex.Size() / 2;
                float scale = Math.Clamp(timerModifier + 0.5f, 0.5f, 1);
                string str = entity.triggerCount.ToString();

                int time = (int)Main.time;
                if (!Main.dayTime)
                    time += (int)Main.dayLength;

                int steps = 48;
                float progress = TwoPi * (float)Utils.GetLerpValue(entity.nextTrigger - entity.maxTime / entity.triggerCount, entity.nextTrigger, time);
                progress -= progress % (TwoPi / steps) + PiOver2;

                Utils.DrawBorderStringFourWay(spriteBatch, font, str, realDrawPosition.X, realDrawPosition.Y - 24f, Color.White * timerModifier * 0.9f, Color.Black * timerModifier, font.MeasureString(str) / 2, scale);
                spriteBatch.Draw(tex, realDrawPosition, null, Color.White * timerModifier, 0, origin, scale, SpriteEffects.None, 0);
                spriteBatch.Draw(handTex, realDrawPosition, null, Color.White * timerModifier, progress, new Vector2(4f, handTex.Height / 2f), scale, SpriteEffects.None, 0);
            }
        }
    }
        public class CeaselessSundialItem : BaseTileItem
        {
            public CeaselessSundialItem() : base("CeaselessSundialItem", "Ceaseless Stardial", "Emits a wire signal a configurable number of times every day", "CeaselessSundial", 1, Item.sellPrice(0, 0, 20, 0), ItemRarityID.LightRed)
            {
            }
        }
    }