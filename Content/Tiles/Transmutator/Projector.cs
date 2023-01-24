using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Core;
using Radiance.Content.Items.TileItems;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Utilities;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles.Transmutator
{
    public class Projector : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.Height = 4;
            TileObjectData.newTile.CoordinateHeights = new int[4] { 16, 16, 16, 18 };
            HitSound = SoundID.Item52;
            DustType = -1;

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Projector");
            AddMapEntry(new Color(48, 49, 53), name);

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<ProjectorTileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out ProjectorTileEntity entity))
            {
                Tile tile = Main.tile[i, j];
                if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
                {
                    Color glowColor = Color.Lerp(new Color(255, 50, 50), new Color(0, 255, 255), entity.deployTimer / 35);
                    Color tileColor = Lighting.GetColor(i, j);
                    Color tileColor2 = Lighting.GetColor(i, j - 2);
                    float deployTimer = entity.deployTimer;
                    Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
                    Texture2D baseTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/Transmutator/ProjectorBase").Value;
                    Texture2D holderTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/Transmutator/ProjectorHolder").Value;
                    Texture2D glowTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/Transmutator/ProjectorGlow").Value;
                    Vector2 basePosition = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y + 32) + zero;
                    if (RadianceUtils.TryGetTileEntityAs(i, j - 2, out TransmutatorTileEntity transEntity))
                    {
                        if (transEntity.glowTime > 0)
                            glowColor = Color.Lerp(new Color(0, 255, 255), CommonColors.RadianceColor1, transEntity.glowTime / 90);
                        if(transEntity.craftingTimer > 0)
                        {
                            RadianceDrawing.DrawSoftGlow(RadianceUtils.MultitileCenterWorldCoords(i, j) + zero + new Vector2(entity.Width, entity.Height) * 8, CommonColors.RadianceColor1 * (transEntity.craftingTimer / 120), 0.3f * (transEntity.craftingTimer / 120), Matrix.Identity);
                            RadianceDrawing.DrawSoftGlow(RadianceUtils.MultitileCenterWorldCoords(i, j) + zero + new Vector2(entity.Width, entity.Height) * 8, Color.White * (transEntity.craftingTimer / 120), 0.2f * (transEntity.craftingTimer / 120), Matrix.Identity);
                        }
                        if (transEntity.projectorBeamTimer > 0)
                        {
                            RadianceDrawing.DrawBeam(RadianceUtils.MultitileCenterWorldCoords(i, j) + zero + new Vector2(entity.Width, entity.Height) * 8, RadianceUtils.MultitileCenterWorldCoords(i, j) - Vector2.UnitY + zero + new Vector2(entity.Width * 8, -2), Color.White.ToVector4() * transEntity.projectorBeamTimer / 60, 0.5f, 8, Matrix.Identity);
                            RadianceDrawing.DrawBeam(RadianceUtils.MultitileCenterWorldCoords(i, j) + zero + new Vector2(entity.Width, entity.Height) * 8, RadianceUtils.MultitileCenterWorldCoords(i, j) - Vector2.UnitY + zero + new Vector2(entity.Width * 8, -2), CommonColors.RadianceColor1.ToVector4() * transEntity.projectorBeamTimer / 60, 0.5f, 6, Matrix.Identity);
                        }
                    }
                    if (entity.containedLens != ProjectorTileEntity.LensEnum.None)
                    {
                        string modifier = string.Empty;
                        switch (entity.containedLens)
                        {
                            case ProjectorTileEntity.LensEnum.Flareglass:
                                modifier = "Flareglass";
                                break;
                            case ProjectorTileEntity.LensEnum.Pathos:
                                modifier = "Pathos";
                                break;
                        }
                        Texture2D glassTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/Transmutator/Lens" + modifier).Value;
                        Main.spriteBatch.End();
                        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);

                        Main.spriteBatch.Draw
                        (
                            glassTexture,
                            basePosition - (Vector2.UnitY * 2) - (Vector2.UnitY * (float)(32 * RadianceUtils.EaseInOutQuart(deployTimer / 105))) + (Vector2.UnitX * 12),
                            null,
                            tileColor,
                            0,
                            Vector2.Zero,
                            1,
                            SpriteEffects.None,
                            0
                        );
                        Main.spriteBatch.End();
                        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
                    }
                    //holder
                    Main.spriteBatch.Draw
                    (
                        holderTexture,
                        basePosition - (Vector2.UnitY * 2) - (Vector2.UnitY * (float)(32 * RadianceUtils.EaseInOutQuart(deployTimer / 105))) + (Vector2.UnitX * 2),
                        null,
                        tileColor2,
                        0,
                        Vector2.Zero,
                        1,
                        SpriteEffects.None,
                        0
                    );
                    //base
                    Main.spriteBatch.Draw
                    (
                        baseTexture,
                        basePosition,
                        null,
                        tileColor,
                        0,
                        Vector2.Zero,
                        1,
                        SpriteEffects.None,
                        0
                    );
                    //glow
                    Main.spriteBatch.Draw
                    (
                        glowTexture,
                        basePosition + new Vector2(10, 2),
                        null,
                        glowColor,
                        0,
                        Vector2.Zero,
                        1,
                        SpriteEffects.None,
                        0
                    );

                    //if (deployTimer > 0)
                    //{
                    //    Vector2 pos = new Vector2(i * 16, j * 16) + zero + new Vector2(entity.Width / 2, 0.7f) * 16 + Vector2.UnitX * 8; //tile world coords + half entity width (center of multitiletile) + a bit of increase
                    //    float mult = (float)Math.Clamp(Math.Abs(RadianceUtils.SineTiming(120)), 0.85f, 1f); //color multiplier
                    //    for (int h = 0; h < 2; h++)
                    //        RadianceDrawing.DrawBeam(pos, new Vector2(pos.X, 0), h == 1 ? new Color(255, 255, 255, entity.beamTimer).ToVector4() * mult : new Color(0, 255, 255, entity.beamTimer).ToVector4() * mult, 0.2f, h == 1 ? 10 : 14, Matrix.Identity);
                    //    RadianceDrawing.DrawSoftGlow(pos - Vector2.UnitY * 2, new Color(0, 255, 255, entity.beamTimer) * mult, 0.25f, Matrix.Identity);
                    //}
                }
            }
            return false;
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {

        }
        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            RadianceInterfacePlayer mp = player.GetModPlayer<RadianceInterfacePlayer>();
            if (RadianceUtils.TryGetTileEntityAs(i, j, out ProjectorTileEntity entity))
            {
                if (entity.deployed)
                {
                    if (Main.tile[i, j].TileFrameX <= 18 && Main.tile[i, j].TileFrameY <= 18)
                    {
                        player.noThrow = 2;
                        player.cursorItemIconEnabled = true;
                        player.cursorItemIconID = entity.itemPlaced.type == ItemID.None ? ModContent.ItemType<ShimmeringGlass>() : entity.itemPlaced.type;
                    }
                }
                if (entity.MaxRadiance > 0)
                    mp.radianceContainingTileHoverOverCoords = new Vector2(i, j);
            }
        }
        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (Main.tile[i, j].TileFrameX <= 18 && Main.tile[i, j].TileFrameY <= 18)
            {
                if (RadianceUtils.TryGetTileEntityAs(i, j, out ProjectorTileEntity entity))
                {
                    if (!player.ItemAnimationActive && entity.deployed)
                    {
                        Item selItem = RadianceUtils.GetPlayerHeldItem();
                        if (entity.itemPlaced.type != ItemID.None)
                        {
                            int num = Item.NewItem(new EntitySource_TileEntity(entity), i * 16 + entity.Width * 3, j * 16, 1, 1, entity.itemPlaced.type, 1, false, 0, false, false);
                            Item item = Main.item[num];

                            item.netDefaults(entity.itemPlaced.netID);
                            item.velocity.Y = Main.rand.Next(-20, -5) * 0.2f;
                            item.velocity.X = Main.rand.Next(-20, 21) * 0.2f;
                            item.position.Y -= item.height;
                            item.newAndShiny = false;

                            if (Main.netMode == NetmodeID.MultiplayerClient)
                                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, num, 0f, 0f, 0f, 0, 0, 0);
                            SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/LensPop"), new Vector2(i * 16 + entity.Width * 8, j * 16 + -entity.Height * 8));
                        }
                        if (entity.validLensesAndTheirEnum.ContainsKey(selItem.type))
                        {
                            SpawnLensDust(RadianceUtils.MultitileCenterWorldCoords(i, j) - (Vector2.UnitY * 2) + (Vector2.UnitX * 10), entity.validLensesAndTheirEnum.GetValueOrDefault(selItem.type));
                            SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/LensPop"), new Vector2(i * 16 + entity.Width * 8, j * 16 + -entity.Height * 8));
                            entity.itemPlaced = selItem.Clone();
                            selItem.stack -= 1;
                            if (selItem.stack == 0) selItem.TurnToAir();
                        }
                        else if(entity.containedLens != ProjectorTileEntity.LensEnum.None)
                        {
                            entity.itemPlaced = new Item(0, 1);
                            SpawnLensDust(RadianceUtils.MultitileCenterWorldCoords(i, j) - (Vector2.UnitY * 2) + (Vector2.UnitX * 10), entity.containedLens);
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        public static void SpawnLensDust(Vector2 pos, ProjectorTileEntity.LensEnum lens)
        {
            int type = DustID.GoldFlame;
            switch (lens)
            {
                case ProjectorTileEntity.LensEnum.Flareglass:
                    type = DustID.GoldFlame;
                    break;
                case ProjectorTileEntity.LensEnum.Pathos:
                    type = DustID.CrimsonTorch;
                    break;
            }
            for (int i = 0; i < 20; i++)
            {
                int d = Dust.NewDust(pos, 8, 32, type);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 0.1f;
                Main.dust[d].scale = 1.7f;
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out ProjectorTileEntity entity))
            {
                if (entity.itemPlaced.type != ItemID.None)
                {
                    SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/LensPop"), new Vector2(i * 16 + entity.Width * 8, j * 16 + -entity.Height * 8));
                    int num = Item.NewItem(new EntitySource_TileEntity(entity), i * 16, j * 16, 1, 1, entity.itemPlaced.type, 1, false, 0, false, false);
                    Item item = Main.item[num];

                    //todo: make this work for transferring all globalitem values from placed item to dropped item
                    //Instanced<GlobalItem>[] globalItemsArray = new Instanced<GlobalItem>[0];
                    //globalItemsArray = globalItems
                    //        .Select(g => new Instanced<GlobalItem>(g.Index, g))
                    //        .ToArray();
                    //foreach (var theitem in globalItemsArray)
                    //{
                    //    Console.WriteLine(theitem);
                    //}
                    SpawnLensDust(RadianceUtils.MultitileCenterWorldCoords(i, j) - (Vector2.UnitY * 2) + (Vector2.UnitX * 10), entity.containedLens);
                    item.netDefaults(entity.itemPlaced.netID);
                    item.velocity.Y = Main.rand.NextFloat(-4, -2);
                    item.velocity.X = Main.rand.NextFloat(-2, 2);
                    item.newAndShiny = false;
                    item.stack = entity.itemPlaced.stack;
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, num, 0f, 0f, 0f, 0, 0, 0);
                    }
                }
                Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 16, ModContent.ItemType<ProjectorItem>());
                Point16 origin = RadianceUtils.GetTileOrigin(i, j);
                ModContent.GetInstance<ProjectorTileEntity>().Kill(origin.X, origin.Y);
            }
        }
    }

    public class ProjectorTileEntity : RadianceUtilizingTileEntity
    {
        #region Fields
        private float maxRadiance = 0;
        private float currentRadiance = 0;
        private int width = 2;
        private int height = 4;
        private List<int> inputTiles = new() { 7, 8 };
        private List<int> outputTiles = new() { };
        private int parentTile = ModContent.TileType<Projector>();
        public Item itemPlaced = new(0, 1);
        public float deployTimer = 0;
        public bool hasTransmutator = false;
        public bool deployed = false;
        public enum LensEnum 
        {
            None,
            Flareglass,
            Pathos
        };
        public LensEnum containedLens = LensEnum.None;
        public Dictionary<int, LensEnum> validLensesAndTheirEnum = new() 
        {
            { ModContent.ItemType<ShimmeringGlass>(), LensEnum.Flareglass },
            { ModContent.ItemType<LensofPathos>(), LensEnum.Pathos },
        };

        #endregion Fields

        #region Propeties

        public override float MaxRadiance
        {
            get => maxRadiance;
            set => maxRadiance = value;
        }

        public override float CurrentRadiance
        {
            get => currentRadiance;
            set => currentRadiance = value;
        }

        public override int Width
        {
            get => width;
            set => width = value;
        }

        public override int Height
        {
            get => height;
            set => height = value;
        }

        public override int ParentTile
        {
            get => parentTile;
            set => parentTile = value;
        }

        public override List<int> InputTiles
        {
            get => inputTiles;
            set => inputTiles = value;
        }

        public override List<int> OutputTiles
        {
            get => outputTiles;
            set => outputTiles = value;
        }

        #endregion Propeties

        public override void Update()
        {
            hasTransmutator = Main.tile[Position.X, Position.Y - 1].TileType == ModContent.TileType<Transmutator>() && Main.tile[Position.X, Position.Y - 1].TileFrameX == 0;
            Vector2 position = new Vector2(Position.X, Position.Y) * 16 + new Vector2(Width / 2, 0.7f) * 16 + Vector2.UnitX * 8;
            if (hasTransmutator)
            {
                if (deployTimer < 105)
                {
                    if (deployTimer == 1)
                        SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/ProjectorLift"), position + new Vector2(width * 8, -height * 8));
                    deployTimer++;
                }
                if(RadianceUtils.TryGetTileEntityAs(Position.X, Position.Y - 1, out TransmutatorTileEntity entity))
                    if(!entity.isCrafting)
                        MaxRadiance = CurrentRadiance = 0;
            }
            else
            {
                CurrentRadiance = MaxRadiance = 0;
                if(deployTimer > 0)
                {
                    if (containedLens != LensEnum.None)
                    {
                        int num = Item.NewItem(new EntitySource_TileEntity(this), (int)position.X, (int)position.Y, 1, 1, itemPlaced.type, 1, false, 0, false, false);
                        Item item = Main.item[num];

                        item.netDefaults(itemPlaced.netID);
                        item.velocity.Y = Main.rand.Next(-20, -5) * 0.2f;
                        item.velocity.X = Main.rand.Next(-20, 21) * 0.2f;
                        item.position.Y -= item.height;
                        item.newAndShiny = false;

                        if (Main.netMode == NetmodeID.MultiplayerClient)
                        {
                            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, num, 0f, 0f, 0f, 0, 0, 0);
                        }
                        SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/LensPop"), new Vector2(position.X, position.Y));
                        itemPlaced = new Item(0, 1);
                        Projector.SpawnLensDust(RadianceUtils.MultitileCenterWorldCoords(Position.X, Position.Y) - (Vector2.UnitY * 2) + (Vector2.UnitX * 10), containedLens);
                    }
                    if (deployTimer == 104)
                        SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/ProjectorLift"), position + new Vector2(width * 8, -height * 8));
                    deployTimer--;
                }
            }
                
            if (validLensesAndTheirEnum.ContainsKey(itemPlaced.type))
                containedLens = validLensesAndTheirEnum.GetValueOrDefault(itemPlaced.type);
            else
                containedLens = LensEnum.None;

            deployed = deployTimer == 105;

            //else if (deployTimer > 0)
            //{
            //    pickupTimer = 0;
            //    if (deployTimer == 550)
            //        SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/BeaconLift"), position + new Vector2(width / 2, -height / 2)); //todo: make sound not freeze game for a moment when played for the first time in an instance
            //    deployTimer--;
            //}
            AddToCoordinateList();
            inputsConnected.Clear();
            outputsConnected.Clear();
        }
        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, width, height);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);
            }
            int placedEntity = Place(i - 1, j - 2);
            return placedEntity;
        }
        public override void OnKill()
        {
            RemoveFromCoordinateList();
        }
        public override void SaveData(TagCompound tag)
        {
            if (CurrentRadiance > 0)
                tag["CurrentRadiance"] = CurrentRadiance;
            if (itemPlaced.type != ItemID.None)
                tag["Item"] = itemPlaced;
        }
        public override void LoadData(TagCompound tag)
        {
            CurrentRadiance = tag.Get<float>("CurrentRadiance");
            itemPlaced = tag.Get<Item>("Item");
        }
    }
}