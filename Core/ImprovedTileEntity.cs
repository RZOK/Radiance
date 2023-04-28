using Microsoft.Xna.Framework;
using Radiance.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Radiance.Core
{
    /// <summary>
    /// An 'improved' abstract ModTileEntity that comes with necessary placement methods, size properties, stability support, Hover UI support, and ordered-updating.
    /// </summary>
    public abstract class ImprovedTileEntity : ModTileEntity
    {
        public readonly int ParentTile;
        public bool isStabilized => idealStability > 0 && Math.Abs(1 - stability / idealStability) <= 0.1f;
        public bool usesStability = false;
        public float stability;
        public float idealStability;
        public bool enabled = true;
        public float updateOrder;
        public int Width => TileObjectData.GetTileData(ParentTile, 0).Width;
        public int Height => TileObjectData.GetTileData(ParentTile, 0).Height;

        public ImprovedTileEntity(int parentTile, float updateOrder = 1, bool usesStability = false)
        {
            ParentTile = parentTile;
            this.usesStability = usesStability;
            this.updateOrder = updateOrder;
        }
        protected virtual HoverUIData ManageHoverUI() => null;
        public void AddHoverUI()
        {
            HoverUIData data = ManageHoverUI();
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
        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ParentTile;
        }
        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            Point16 origin = RadianceUtils.GetTileOrigin(i, j);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, origin.X, origin.Y, Width, Height);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, origin.X, origin.Y, Type);
            }
            int placedEntity = Place(origin.X, origin.Y);
            return placedEntity;
        }
        //Use OrderedUpdate() or PreOrderedUpdate() instead of Update()
        public override sealed void Update() { }
        public virtual void OrderedUpdate() { }
        public virtual void PreOrderedUpdate() { }
        public override void OnNetPlace()
        {
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
        }
    }
}