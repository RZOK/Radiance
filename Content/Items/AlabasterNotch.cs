using Terraria.UI;
using Microsoft.Xna.Framework.Input;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.Tools.Misc;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Terraria.ObjectData;
using Terraria.ModLoader.Config;
using Terraria.Map;
using Radiance.Core.Systems.ParticleSystems;
using Radiance.Content.Particles;

namespace Radiance.Content.Items
{
    public class AlabasterNotch : ModItem
    {
        public override void Load()
        {
            On_Player.TileInteractionsUse += On_Player_TileInteractionsUse;
            IL_TeleportPylonsMapLayer.Draw += IL_DrawPylonMark;
        }

        public override void Unload()
        {
            On_Player.TileInteractionsUse -= On_Player_TileInteractionsUse;
            IL_TeleportPylonsMapLayer.Draw -= IL_DrawPylonMark;
        }

        private void On_Player_TileInteractionsUse(On_Player.orig_TileInteractionsUse orig, Player self, int myX, int myY)
        {
            if (self.tileInteractAttempted && self.releaseUseTile && self.PlayerHeldItem().ModItem is LookingGlass lookingGlass && lookingGlass.CurrentSetting.type == ModContent.ItemType<AlabasterNotch>())
            {
                Point topLeft = GetTileOrigin(myX, myY);
                Tile tile = Framing.GetTileSafely(topLeft.X, topLeft.Y);
                int style = 0;
                int alt = 0;
                TileObjectData.GetTileInfo(tile, ref style, ref alt);
                TileObjectData data = TileObjectData.GetTileData(tile);
                TileDefinition tileDefinition = new TileDefinition(tile.TileType);
                Point16 tilePoint = new Point16(topLeft.X, topLeft.Y);

                if (TileID.Sets.CountsAsPylon.Contains(tile.TileType))
                {
                    if ((lookingGlass.markedPylon is null || lookingGlass.markedPylon.Type != tile.TileType || lookingGlass.markedPylonPosition != tilePoint || lookingGlass.markedPylonStyle != style))
                    {
                        lookingGlass.markedPylon = tileDefinition;
                        lookingGlass.markedPylonPosition = tilePoint;
                        lookingGlass.markedPylonStyle = style;
                        for (int k = 0; k < 10; k++)
                        {
                            Vector2 topLeftWorld = topLeft.ToVector2() * 16f;
                            Vector2 particlePos = Main.rand.NextVector2FromRectangle(new Rectangle((int)topLeftWorld.X, (int)topLeftWorld.Y, data.Width * 16, data.Height * 16));
                            float modifier = MathF.Pow(Main.rand.NextFloat(), 2.5f);
                            Vector2 velocity = Vector2.UnitY * -(1f + 3f * modifier);
                            WorldParticleSystem.system.AddParticle(new LingeringStar(particlePos, velocity, (int)(30f + 45f * modifier), new Color(166, 255, 227), Main.rand.NextFloat(0.3f, 0.7f), Main.rand.NextFloat(TwoPi), Main.rand.NextSign()));
                        }
                        SoundEngine.PlaySound(SoundID.NPCDeath7);
                    }
                    self.tileInteractionHappened = true;
                    return;
                }
            }
            orig(self, myX, myY);
        }
        private void IL_DrawPylonMark(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchCallvirt(typeof(ModPylon), nameof(ModPylon.DrawMapIcon))))
            {
                LogIlError("Alabaster Notch Map Mark", "Couldn't navigate to after modded icon draw");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_1); // draw context
            cursor.Emit(OpCodes.Ldloc_S, (byte)4); // teleport pylon info
            cursor.EmitDelegate(DrawPylonMark);

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
            Tile tile = Framing.GetTileSafely(pylonInfo.PositionInTiles.X, pylonInfo.PositionInTiles.Y);
            int style = 0;
            int alt = 0;
            TileObjectData.GetTileInfo(tile, ref style, ref alt);
            if (Main.LocalPlayer.PlayerHeldItem().ModItem is LookingGlass lookingGlass && tile.TileType == lookingGlass.markedPylon.Type && (byte)style == lookingGlass.markedPylonStyle && pylonInfo.PositionInTiles == lookingGlass.markedPylonPosition)
            {
                Texture2D tex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/Items/AlabasterNotch_MapIcon").Value;
                context.Draw(tex, pylonInfo.PositionInTiles.ToVector2() + new Vector2(1.5f, 2f), Color.White, new SpriteFrame(1, 1, 0, 0), 1f, 2f, Alignment.Center);
            }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Alabaster Notch");
            Tooltip.SetDefault("Allows teleporting to a marked pylon when socketed into a Looking Glass\nRight click a pylon while selected to mark it");
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

                        lookingGlass.NotchCount.TryGetValue(lookingGlass.CurrentSetting, out int value);
                        (player.PlayerHeldItem().ModItem as LookingGlass).mirrorCharge -= lookingGlass.CurrentSetting.chargeCost(player, value); // for some reason reducing the mirrorcharge directly in usestyle doesn't work?? probably item cloning
                    }
                }
            }
        }

        public float ChargeCost(Player player, int identicalCount)
        {
            return 20f * MathF.Pow(1.35f, 1 - identicalCount);
        }
    }
}