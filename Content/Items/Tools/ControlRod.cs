using Microsoft.Xna.Framework;
using Radiance.Common;
using Radiance.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.Tools
{
    public class ControlRod : ModItem
    {
        public RadianceRay focusedRay;
        public bool focusedStartPoint = false;
        public bool focusedEndPoint = false;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Radiance Control Rod");
            Tooltip.SetDefault("Allows you to view Radiance inputs, outputs, and rays\nLeft click to draw rays or grab existing ones\nRight click while grabbing a ray to remove it");
        }

        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.maxStack = 1;
            Item.value = Item.buyPrice(0, 0, 5, 0);
            Item.rare = ItemRarityID.Blue;
            Item.useTurn = true;
            Item.channel = true;
            Item.useAnimation = 2;
            Item.useTime = 2;
            Item.useStyle = ItemUseStyleID.HoldUp;
        }

        public override void HoldItem(Player player)
        {
            if (player == Main.LocalPlayer)
            {
                if (Main.mouseLeft && !player.CCed && !player.noItems && !player.mouseInterface)
                {
                    Vector2 mouseSnapped = new Vector2((int)(Math.Floor(Main.MouseWorld.X / 16) * 16), (int)(Math.Floor(Main.MouseWorld.Y / 16) * 16)) + new Vector2(8, 8);
                    player.itemTime = player.itemAnimation = 2;
                    for (int i = 0; i < Radiance.maxRays; i++)
                    {
                        if (Radiance.radianceRay[i] != null && Radiance.radianceRay[i].active && !focusedStartPoint && !focusedEndPoint)
                        {
                            RadianceRay ray = Radiance.radianceRay[i];

                            if (mouseSnapped == ray.endPos)
                            {
                                focusedRay = ray;
                                focusedEndPoint = true;
                            }
                            else if (mouseSnapped == ray.startPos)
                            {
                                focusedRay = ray;
                                focusedStartPoint = true;
                            }
                        }
                    }
                    if (focusedRay == null)
                    {
                        int r = RadianceRay.NewRadianceRay(Main.MouseWorld, Main.MouseWorld);
                        focusedRay = Radiance.radianceRay[r];
                        focusedEndPoint = true;
                    }
                    if (focusedRay != null)
                    {
                        int maxDist = Radiance.maxDistanceBetweenPoints;
                        if (focusedEndPoint)
                        {
                            Vector2 end = Main.MouseWorld;
                            if (Vector2.Distance(end, focusedRay.startPos) > maxDist)
                            {
                                Vector2 v = end - focusedRay.startPos;
                                v = Vector2.Normalize(v) * maxDist;
                                end = focusedRay.startPos + v;
                            }
                            if (RadianceRay.FindRay(RadianceRay.SnapToCenterOfTile(end)) == null)
                            {
                                focusedRay.SnapToPosition(focusedRay.startPos, end);
                            }
                        }
                        if (focusedStartPoint)
                        {
                            Vector2 start = Main.MouseWorld;
                            if (Vector2.Distance(start, focusedRay.endPos) > maxDist)
                            {
                                Vector2 v = start - focusedRay.endPos;
                                v = Vector2.Normalize(v) * maxDist;
                                start = focusedRay.endPos + v;
                            }
                            if (RadianceRay.FindRay(RadianceRay.SnapToCenterOfTile(start)) == null)
                            {
                                focusedRay.SnapToPosition(start, focusedRay.endPos);
                            }
                        }
                    }
                    if (Main.mouseRight)
                    {
                        focusedRay.Kill();
                        player.itemTime = player.itemAnimation = 0;
                    }
                }
                else
                {
                    focusedRay = default;
                    focusedStartPoint = false;
                    focusedEndPoint = false;
                }
                player.GetModPlayer<RadiancePlayer>().canSeeRays = true;
            }
        }
    }
}