using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Common;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.PedestalItems;
using Radiance.Content.Items.TileItems;
using Radiance.Core;
using Radiance.Utils;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles
{
    public class Pedestal : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileContainer[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.CoordinateHeights = new int[2] { 16, 18 };

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Pedestal");
            AddMapEntry(new Color(43, 56, 61), name);

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<PedestalTileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }

        internal static readonly IList<GlobalItem> globalItems = new List<GlobalItem>();

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (TileUtils.TryGetTileEntityAs(i, j, out PedestalTileEntity entity))
            {
                if (!player.ItemAnimationActive)
                {
                    Item selItem = MiscUtils.GetPlayerHeldItem();
                    if (entity.itemPlaced.type != ItemID.None)
                    {
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
                        item.netDefaults(entity.itemPlaced.netID);
                        item.Prefix(entity.itemPlaced.prefix);
                        item.velocity.Y = Main.rand.Next(-20, -5) * 0.2f;
                        item.velocity.X = Main.rand.Next(-20, 21) * 0.2f;
                        item.position.Y -= item.height;
                        item.newAndShiny = false;
#nullable enable
                        BaseContainer? newContainer = item.ModItem as BaseContainer;
#nullable disable
                        if (entity.containerPlaced != null && newContainer != null)
                        {
                            newContainer.CurrentRadiance = entity.containerPlaced.CurrentRadiance;
                        }
                        entity.GetRadianceFromItem(null);
                        if (Main.netMode == NetmodeID.MultiplayerClient)
                        {
                            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, num, 0f, 0f, 0f, 0, 0, 0);
                        }
                    }
                    entity.itemPlaced = selItem.Clone();
                    selItem.stack -= 1;
                    if (selItem.stack == 0) selItem.TurnToAir();
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    return true;
                }
            }
            return false;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (TileUtils.TryGetTileEntityAs(i, j, out PedestalTileEntity entity))
            {
                Tile tile = Main.tile[i, j];
                Vector2 centerOffset = new Vector2(-2, -2) / 2 * 16;
                if (entity.itemPlaced.type != ItemID.None && tile.TileFrameX == 0 && tile.TileFrameY == 0)
                {
                    Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
                    Texture2D texture = TextureAssets.Item[entity.itemPlaced.type].Value;
                    int yCenteringOffset = -texture.Height / 2 - 10;
                    Vector2 position = new Vector2(i * 16 - (int)Main.screenPosition.X, (float)(j * 16 - (int)Main.screenPosition.Y + yCenteringOffset + 5 * MathUtils.sineTiming(30))) + zero;
                    Vector2 origin = new Vector2(texture.Width, texture.Height) / 2 + centerOffset;
                    Main.EntitySpriteDraw
                    (
                        texture,
                        position,
                        null,
                        Color.White,
                        0,
                        origin,
                        1,
                        SpriteEffects.None,
                        0
                    );
                    if (entity.containerPlaced != null && entity.containerPlaced.RadianceAdjustingTexture != null)
                    {
                        Texture2D radianceAdjustingTexture = entity.containerPlaced.RadianceAdjustingTexture;

                        float radianceCharge = Math.Min(entity.containerPlaced.CurrentRadiance, entity.containerPlaced.MaxRadiance);
                        float fill = radianceCharge / entity.containerPlaced.MaxRadiance;

                        Main.EntitySpriteDraw
                        (
                            radianceAdjustingTexture,
                            position,
                            null,
                            Color.Lerp(Radiance.RadianceColor1 * fill, Radiance.RadianceColor2 * fill, fill * (float)MathUtils.sineTiming(5)),
                            0,
                            origin,
                            1,
                            SpriteEffects.None,
                            0
                        );

                        Vector2 vector = new Vector2(
                                i * 16,
                                j * 16
                                ) -
                            new Vector2(
                                Main.tile[i, j].TileFrameX - (2 * Main.tile[i, j].TileFrameX / 18),
                                Main.tile[i, j].TileFrameY - (2 * Main.tile[i, j].TileFrameY / 18)
                                );
                        float strength = 0.4f;
                        Lighting.AddLight(vector - centerOffset + new Vector2(0, (float)(yCenteringOffset + 5 * MathUtils.sineTiming(30))), Color.Lerp(new Color
                        (
                         1 * fill * strength,
                         0.9f * fill * strength,
                         0.4f * fill * strength
                        ), new Color
                        (
                         0.7f * fill * strength,
                         0.65f * fill * strength,
                         0.5f * fill * strength
                        ),
                        fill * (float)MathUtils.sineTiming(20)).ToVector3());
                    }

                    if (Main.LocalPlayer.GetModPlayer<RadiancePlayer>().debugMode)
                    {
                        DynamicSpriteFont font = FontAssets.MouseText.Value;
                        DynamicSpriteFontExtensionMethods.DrawString
                        (
                            spriteBatch,
                            font,
                            entity.itemPlaced.Name,
                            position,
                            Color.White,
                            0,
                            font.MeasureString(entity.itemPlaced.Name) / 2 + centerOffset,
                            1,
                            SpriteEffects.None,
                            0
                        );
                    }
                }
            }
        }

        public override void MouseOver(int i, int j)
        {
            int itemTextureType = ModContent.ItemType<PedestalItem>();
            Player player = Main.LocalPlayer;
            if (TileUtils.TryGetTileEntityAs(i, j, out PedestalTileEntity entity) && entity.itemPlaced.type != ItemID.None)
            {
                RadiancePlayer mp = player.GetModPlayer<RadiancePlayer>();
                if (entity.aoeCircleInfo.Item3 > 0)
                {
                    mp.aoeCirclePosition = entity.aoeCircleInfo.Item1;
                    mp.aoeCircleColor = entity.aoeCircleInfo.Item2.ToVector4();
                    mp.aoeCircleScale = entity.aoeCircleInfo.Item3;
                    mp.aoeCircleMatrix = Main.GameViewMatrix.ZoomMatrix;
                }
                itemTextureType = entity.itemPlaced.netID;
                if(entity.MaxRadiance > 0)
                    mp.radianceContainingTileHoverOverCoords = new Vector2(i, j);
            }
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = itemTextureType;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (TileUtils.TryGetTileEntityAs(i, j, out PedestalTileEntity entity))
            {
                if (entity.itemPlaced.type != ItemID.None)
                {
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

                    item.netDefaults(entity.itemPlaced.netID);
                    item.Prefix(entity.itemPlaced.prefix);
                    item.velocity.Y = Main.rand.Next(-10, 0) * 0.2f;
                    item.velocity.X = Main.rand.Next(-10, 11) * 0.2f;
                    item.position.Y -= item.height;
                    item.newAndShiny = false;
#nullable enable
                    BaseContainer? newContainer = item.ModItem as BaseContainer;
#nullable disable
                    if (entity.containerPlaced != null && newContainer != null)
                    {
                        newContainer.CurrentRadiance = entity.containerPlaced.CurrentRadiance;
                    }
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, num, 0f, 0f, 0f, 0, 0, 0);
                    }
                }
            }
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 16, ModContent.ItemType<PedestalItem>());
            Point16 origin = TileUtils.GetTileOrigin(i, j);
            ModContent.GetInstance<PedestalTileEntity>().Kill(origin.X, origin.Y);
        }
    }

    public class PedestalTileEntity : RadianceUtilizingTileEntity
    {
        #region Fields

        public Item itemPlaced = new Item(0, 1);
#nullable enable
        public BaseContainer? containerPlaced;
#nullable disable
        private float maxRadiance = 0;
        private float currentRadiance = 0;
        private int width = 2;
        private int height = 2;
        private List<int> inputTiles = new List<int>() { 1, 4 };
        private List<int> outputTiles = new List<int>() { 2, 3 };
        private int parentTile = ModContent.TileType<Pedestal>();
        public float actionTimer = 0;
        public (Vector2, Color, float) aoeCircleInfo = (new Vector2(-1, -1), new Color(), 0);

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
            maxRadiance = 0;
            currentRadiance = 0;
            AddToCoordinateList();
            aoeCircleInfo = (new Vector2(-1, -1), new Color(), 0);

            
            if (itemPlaced.type != ItemID.None)
                PedestalItemEffect();
            inputsConnected.Clear();
            outputsConnected.Clear();
        }

        public void PedestalItemEffect()
        {
            Vector2 pos = MathUtils.MultitileCenterWorldCoords(Position.X, Position.Y) + Vector2.UnitX * Width * 8;

            BaseContainer container = itemPlaced.ModItem as BaseContainer;
            bool flag = false;
            if (container != null)
            {
                Vector2 centerOffset = new Vector2(-2, -2) * 8;
                Vector2 yCenteringOffset = new Vector2(0, -TextureAssets.Item[itemPlaced.type].Value.Height);
                Vector2 vector = MathUtils.MultitileCenterWorldCoords(Position.X, Position.Y) - centerOffset + yCenteringOffset;
                containerPlaced = container;
                if (container.ContainerQuirk == BaseContainer.ContainerQuirkEnum.Leaking) container.LeakRadiance();
                if (container.ContainerQuirk != BaseContainer.ContainerQuirkEnum.CantAbsorb && container.ContainerQuirk != BaseContainer.ContainerQuirkEnum.CantAbsorbNonstandardTooltip) 
                    container.AbsorbStars(vector);
                container.FlareglassCreation(vector);
                aoeCircleInfo =
                    (
                        pos,
                        Radiance.RadianceColor1,
                        90
                    );
                GetRadianceFromItem(container);
                flag = true;
            }
            if (!flag) containerPlaced = null;

            if (itemPlaced.type == ModContent.ItemType<FormationCore>())
            {
                FormationCore formationCore = itemPlaced.ModItem as FormationCore;
                formationCore.PedestalEffect(this);
                aoeCircleInfo =
                    (
                        pos,
                        new Color(235, 71, 120, 0),
                        100
                    );
                if (Main.GameUpdateCount % 40 == 0)
                {
                    if (Main.rand.NextBool(3))
                    {
                        int f = Dust.NewDust(pos - new Vector2(0, -5 * (float)MathUtils.sineTiming(30) + 2) - new Vector2(8, 8), 16, 16, DustID.TeleportationPotion, 0, 0);
                        Main.dust[f].velocity *= 0.3f;
                        Main.dust[f].scale = 0.8f;
                    }
                }
                if (actionTimer == 60)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        int f = Dust.NewDust(pos - new Vector2(0, -5 * (float)MathUtils.sineTiming(30) + 2) - new Vector2(8, 8), 16, 16, DustID.TeleportationPotion, 0, 0);
                        Main.dust[f].velocity *= 0.3f;
                        Main.dust[f].scale = Main.rand.NextFloat(1.3f, 1.7f);
                    }
                }
            }
            else if (itemPlaced.type == ModContent.ItemType<AnnihilationCore>())
            {
                AnnihilationCore annihilationCore = itemPlaced.ModItem as AnnihilationCore;
                annihilationCore.PedestalEffect(this);
                aoeCircleInfo =
                    (
                        pos,
                        new Color(158, 98, 234, 0),
                        75
                    );
                if (Main.GameUpdateCount % 120 == 0)
                {
                    int f = Dust.NewDust(pos - new Vector2(0, -5 * (float)MathUtils.sineTiming(30) + 2) - new Vector2(8, 8), 16, 16, DustID.PurpleCrystalShard, 0, 0);
                    Main.dust[f].velocity *= 0.1f;
                    Main.dust[f].noGravity = true;
                    Main.dust[f].scale = Main.rand.NextFloat(1.2f, 1.4f);
                }
                if (actionTimer == 60)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        int f = Dust.NewDust(pos - new Vector2(0, -5 * (float)MathUtils.sineTiming(30) + 2) - new Vector2(8, 8), 16, 16, DustID.PurpleCrystalShard, 0, 0);
                        Main.dust[f].velocity *= 0.3f;
                        Main.dust[f].noGravity = true;
                        Main.dust[f].scale = Main.rand.NextFloat(1.3f, 1.7f);
                    }
                }
            }
        }

        

#nullable enable

        public void GetRadianceFromItem(BaseContainer? container)
#nullable disable
        {
            if (container != null)
            {
                maxRadiance = container.MaxRadiance;
                currentRadiance = container.CurrentRadiance;
            }
            else
                maxRadiance = currentRadiance = 0;
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, Width, Height);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);
            }
            int placedEntity = Place(i, j - 1);
            return placedEntity;
        }

        public override void OnKill()
        {
            RemoveFromCoordinateList();
        }

        public override void SaveData(TagCompound tag)
        {
            if (itemPlaced.type != ItemID.None)
                tag["Item"] = itemPlaced;
        }

        public override void LoadData(TagCompound tag)
        {
            itemPlaced = tag.Get<Item>("Item");
        }
    }
}