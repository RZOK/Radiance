using Radiance.Content.Items.BaseItems;
using Radiance.Core.Systems;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ObjectData;
using Terraria.UI;

namespace Radiance.Content.Tiles
{
    public class LodestarEffigy : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = false;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.StyleHorizontal = true;
            HitSound = SoundID.Item52;
            DustType = -1;

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Lodestar Effigy");
            AddMapEntry(new Color(94, 81, 81), name);

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<LodestarEffigy_TileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            if (drawData.tileFrameX == 0 && drawData.tileFrameY == 0)
                Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
        }

        public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
        {
            
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            
        }

        public override void MouseOver(int i, int j)
        {
            
        }

        public override bool RightClick(int i, int j)
        {
            return false;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (TryGetTileEntityAs(i, j, out LodestarEffigy_TileEntity _))
            {
                Main.LocalPlayer.GetModPlayer<LodestarEffigy_Player>().updateDisplayedStars = true;
                ModContent.GetInstance<LodestarEffigy_TileEntity>().Kill(i, j);
            }
        }
    }
    public class LodestarEffigy_TileEntity : ImprovedTileEntity
    {
        public LodestarEffigy_TileEntity() : base(ModContent.TileType<LodestarEffigy>()) { }
        internal const float EFFECT_RADIUS = 6400f;


        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            if (Main.LocalPlayer.whoAmI == Main.myPlayer)
                Main.LocalPlayer.GetModPlayer<LodestarEffigy_Player>().updateDisplayedStars = true;

            return base.Hook_AfterPlacement(i, j, type, style, direction, alternate);
        }
    }
    public class LodestarEffigy_Item : BaseTileItem
    {
        public LodestarEffigy_Item() : base(nameof(LodestarEffigy_Item), "Lodestar Effigy", "Placeholder Tooltip", nameof(LodestarEffigy), 1, Item.sellPrice(0, 0, 50, 0), ItemRarityID.Green)
        {

        }
    }
    public class LodestarEffigy_Player : ModPlayer
    {
        public List<Item> displayedStars = new List<Item>();
        public bool updateDisplayedStars = false;

        public override void Load()
        {
            On_Item.NewItem_Inner += UpdateLodestarStars;
            On_Item.UpdateItem += GetItemOldPosition;
        }

        private void GetItemOldPosition(On_Item.orig_UpdateItem orig, Item self, int i)
        {
            if (Main.LocalPlayer.whoAmI == Main.myPlayer && self.type == ItemID.FallenStar && self.active)
            {
                self.oldPosition = self.position;
                orig(self, i);
                if (self.oldPosition != self.position)
                {
                    Main.LocalPlayer.GetModPlayer<LodestarEffigy_Player>().updateDisplayedStars = true;
                }
            }
            else
                orig(self, i);
        }

        private int UpdateLodestarStars(On_Item.orig_NewItem_Inner orig, IEntitySource source, int X, int Y, int Width, int Height, Item itemToClone, int Type, int Stack, bool noBroadcast, int pfix, bool noGrabDelay, bool reverseLookup)
        {
            if (Type == ItemID.FallenStar && Main.myPlayer == Main.LocalPlayer.whoAmI)
                Main.LocalPlayer.GetModPlayer<LodestarEffigy_Player>().updateDisplayedStars = true;

            return orig(source, X, Y, Width, Height, itemToClone, Type, Stack, noBroadcast, pfix, noGrabDelay, reverseLookup);
        }

        public override void PostUpdate()
        {
            if(updateDisplayedStars && Player.whoAmI == Main.myPlayer)
            {
                displayedStars.Clear();

                var lodestars = TileEntitySystem.orderedEntities.Where(x => x is LodestarEffigy_TileEntity);
                for (int i = 0; i < Main.maxItems; i++)
                {
                    Item item = Main.item[i];
                    if (!item.active || item.type != ItemID.FallenStar)
                        continue;

                    foreach (LodestarEffigy_TileEntity lodestar in lodestars)
                    {
                        if (lodestar.TileEntityWorldCenter().Distance(item.Center) < LodestarEffigy_TileEntity.EFFECT_RADIUS)
                        {
                            displayedStars.Add(item);
                            break;
                        }
                    }
                }
                updateDisplayedStars = false;
            }
        }
    }

    public class LodestarEffigy_GlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.type == ItemID.FallenStar;
    }


    public class LodestarEffigy_System : ModSystem
    {

    }
    public class LodestarEffigy_MapLayer : ModMapLayer
    {
        private Texture2D tex;

        public override void Load()
        {
            tex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/Tiles/LodestarEffigy_Star", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        }

        public override void Draw(ref MapOverlayDrawContext context, ref string text)
        {
            foreach (Item star in Main.LocalPlayer.GetModPlayer<LodestarEffigy_Player>().displayedStars)
            {
                if (!star.active)
                    continue;

                if (context.Draw(tex, star.Center / 16f, Color.White, new SpriteFrame(1, 1, 0, 0), 1f, 1f, Alignment.Center).IsMouseOver)
                { 
                    text = "Fallen Star";
                }
            }
        }
    }
}