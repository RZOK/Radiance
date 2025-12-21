using Terraria.UI;
using Microsoft.Xna.Framework.Input;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.Tools.Misc;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Terraria.ObjectData;
using Terraria.ModLoader.Config;
using Terraria.Map;

namespace Radiance.Content.Items
{
    public class AlabasterNotch : ModItem
    {
        //private static bool shouldCaptureScale = false;
        //private static float nextIconScale = 0;
        public override void Load()
        {
            IL_Player.TileInteractionsUse += IL_MarkPylon;
            IL_TeleportPylonsMapLayer.Draw += IL_DrawPylonMark;
            //IL_MapOverlayDrawContext.Draw_Texture2D_Vector2_Color_SpriteFrame_float_float_Alignment_SpriteEffects += IL_SetPylonIconScale;
        }

        public override void Unload()
        {
            IL_Player.TileInteractionsUse -= IL_MarkPylon;
            IL_TeleportPylonsMapLayer.Draw -= IL_DrawPylonMark;
            //IL_MapOverlayDrawContext.Draw_Texture2D_Vector2_Color_SpriteFrame_float_float_Alignment_SpriteEffects -= IL_SetPylonIconScale;
        }
        private void IL_MarkPylon(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            ILLabel label = null;
            if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchLdcI4(TileID.TeleportationPylon),
                i => i.MatchBneUn(out label)))
            {
                LogIlError("Alabaster Notch Right Click", "Couldn't navigate to after tile interaction happened variable");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.Emit(OpCodes.Ldarg_2);
            cursor.EmitDelegate(MarkPylonForMirror);
            cursor.Emit(OpCodes.Brtrue, label);
        }

        private static bool MarkPylonForMirror(Player player, int i, int j)
        {
            if(player.PlayerHeldItem().ModItem is LookingGlass lookingGlass)
            {
                Point topLeft = GetTileOrigin(i, j);
                Tile tile = Framing.GetTileSafely(topLeft.X, topLeft.Y);
                int style = 0;
                int alt = 0;
                TileObjectData.GetTileInfo(tile, ref style, ref alt);

                lookingGlass.markedPylon = new TileDefinition(tile.TileType);
                lookingGlass.markedPylonPosition = new Point16(topLeft.X, topLeft.Y);
                lookingGlass.markedPylonStyle = style;


                return true;
            }
            return false;
        }
        private void IL_DrawPylonMark(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            //if (!cursor.TryGotoNext(MoveType.After,
            //  i => i.MatchLdsfld(typeof(Alignment), nameof(Alignment.Center))))
            //{
            //    LogIlError("Alabaster Notch Map Mark", "Couldn't navigate to after alignment set");
            //    return;
            //}
            //cursor.EmitDelegate<Action>(() => shouldCaptureScale = true);

            if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchLdfld(typeof(MapOverlayDrawContext.DrawResult), nameof(MapOverlayDrawContext.DrawResult.IsMouseOver))))
            {
                LogIlError("Alabaster Notch Map Mark", "Couldn't navigate to after icon draw");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_1); // draw context
            cursor.Emit(OpCodes.Ldloc_S, (byte)4); // teleport pylon info
            cursor.EmitDelegate(DrawPylonMark);
        }
        private static void DrawPylonMark(ref MapOverlayDrawContext context, TeleportPylonInfo pylonInfo)
        {
            if (Main.LocalPlayer.PlayerHeldItem().ModItem is LookingGlass lookingGlass && (byte)pylonInfo.TypeOfPylon == lookingGlass.markedPylonStyle && pylonInfo.PositionInTiles == lookingGlass.markedPylonPosition)
            {
                Texture2D tex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/Items/AlabasterNotch_MapIcon").Value;
                context.Draw(tex, pylonInfo.PositionInTiles.ToVector2() + new Vector2(1.5f, 2f), Color.White, new SpriteFrame(1, 1, 0, 0), 1f, 2f, Alignment.Center);
            }
        }
        //private void IL_SetPylonIconScale(ILContext il)
        //{
        //    ILCursor cursor = new ILCursor(il);
        //    if (!cursor.TryGotoNext(MoveType.Before,
        //        i => i.MatchCallvirt<SpriteBatch>(nameof(SpriteBatch.Draw))))
        //    {
        //        LogIlError("Alabaster Notch Map Mark", "Couldn't navigate to before icon draw");
        //        return;
        //    }
        //    cursor.Emit(OpCodes.Ldloc_S, (byte)5); // get scale
        //    cursor.EmitDelegate(SetPylonIconScale);
        //}
        //private void SetPylonIconScale(float scale)
        //{
        //    if(shouldCaptureScale)
        //    {
        //        Main.NewText(scale);
        //        nextIconScale = scale;
        //        shouldCaptureScale = false;
        //    }
        //}

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Alabaster Notch");
            Tooltip.SetDefault("Placeholder Text");
            Item.ResearchUnlockCount = 0;

            LookingGlassNotchData.LoadNotchData
               (
               Type,
               new Color(166, 255, 227),
               $"{nameof(Radiance)}/Content/ExtraTextures/LookingGlass/LookingGlass_Pylon",
               $"{nameof(Radiance)}/Content/ExtraTextures/LookingGlass/LookingGlass_Pylon_Small",
               MirrorUse,
               ChargeCost
               );
        }
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Green;
        }
        public void MirrorUse(Player player, LookingGlass lookingGlass)
        {
            if(lookingGlass.markedPylon is not null && !lookingGlass.markedPylon.IsUnloaded)
            {
                Tile tile = Framing.GetTileSafely(lookingGlass.markedPylonPosition.X, lookingGlass.markedPylonPosition.Y);
                if (tile.HasTile && tile.TileType == lookingGlass.markedPylon.Type && GetTileOrigin(lookingGlass.markedPylonPosition.X, lookingGlass.markedPylonPosition.Y) == new Point(lookingGlass.markedPylonPosition.X, lookingGlass.markedPylonPosition.Y))
                {
                    int style = 0;
                    int alt = 0;
                    TileObjectData.GetTileInfo(tile, ref style, ref alt);
                    if(lookingGlass.markedPylonStyle == style)
                    {
                        TileObjectData data = TileObjectData.GetTileData(tile);
                        Vector2 idealWorldCoordinates = MultitileWorldCenter(lookingGlass.markedPylonPosition.X, lookingGlass.markedPylonPosition.Y) - new Vector2(player.width, player.height) / 2f + Vector2.UnitY * 11f;
                        lookingGlass.PreRecallParticles(player);
                        player.Teleport(idealWorldCoordinates, 12);
                        lookingGlass.PostRecallParticles(player);
                    }
                }
            }
        }
        /*
         Left to do:
            -Visual when pylon is marked (particles and noise)
         */

        public int ChargeCost(int identicalCount)
        {
            return 10;
        }
    }
}