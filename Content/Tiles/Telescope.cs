using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Particles;
using Radiance.Content.Tiles.Pedestals;
using Radiance.Core.Systems;
using Radiance.Core.Systems.ParticleSystems;
using System.Reflection;
using Terraria.Graphics.CameraModifiers;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles
{
    public class Telescope : ModTile
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
            name.SetDefault("Telescope");
            AddMapEntry(new Color(219, 33, 0), name);

            //TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<CinderCrucibleTileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }
        public override void MouseOver(int i, int j)
        {
            Main.LocalPlayer.SetCursorItem(ModContent.ItemType<TelescopeItem>());
        }
        public override bool RightClick(int i, int j)
        {
            Vector2 center = MultitileWorldCenter(i, j);
            SoundEngine.PlaySound(SoundID.MenuTick);
            TelescopePlayer telescopePlayer = Main.LocalPlayer.GetModPlayer<TelescopePlayer>();
            if (telescopePlayer.telescopePosition != center)
                telescopePlayer.telescopePosition = center;
            else
                telescopePlayer.telescopePosition = null;

            return true;
        }
    }

    public class TelescopeItem : BaseTileItem
    {
        public TelescopeItem() : base("TelescopeItem", "Telescope", "Allows you to look to the stars for boons", nameof(Telescope), 1, Item.sellPrice(0, 0, 50, 0), ItemRarityID.Green) { }
    }
    public class TelescopePlayer : ModPlayer
    {
        public float cameraRaise = 0;
        public Vector2? telescopePosition;
        public float MaxDistanceFromTelescope => (Player.blockRange + Player.tileRangeX) * 16f;
        private static float CAMERA_TIME_MAX = 90;
        private static float CAMERA_HEIGHT_MAX = 240;
        private static float Completion => EaseInOutExponent(Main.LocalPlayer.GetModPlayer<TelescopePlayer>().cameraRaise / CAMERA_TIME_MAX, 4f);
        private static RenderTarget2D backgroundTarget;
        private static MethodInfo DrawSurfaceBG;
        private bool drawBackground = false;
        public override void Load()
        {
            IL_Main.DoDraw += ManipulateSceneArea;
            DrawSurfaceBG = typeof(Main).GetMethod(nameof(DrawSurfaceBG), BindingFlags.Instance | BindingFlags.NonPublic);

            ResizeRenderTarget();
            RenderTargetsManager.DrawToRenderTargetsDelegateEvent += DrawToRenderTarget;
            RenderTargetsManager.ResizeRenderTargetDelegateEvent += ResizeRenderTarget;
            On_Main.DrawSurfaceBG += DrawRenderTarget;
        }

        private void ResizeRenderTarget()
        {
            Main.QueueMainThreadAction(() =>
            {
                if (backgroundTarget != null && !backgroundTarget.IsDisposed)
                    backgroundTarget.Dispose();

                backgroundTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            });
        }
        private void DrawToRenderTarget()
        {
            GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;
            graphicsDevice.SetRenderTarget(backgroundTarget);
            graphicsDevice.Clear(Color.Transparent);
            if (Main.dedServ || Main.gameMenu || Main.spriteBatch is null || backgroundTarget is null || graphicsDevice is null)
            {
                graphicsDevice.SetRenderTargets(null);
                return;
            }
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.BackgroundViewMatrix.TransformationMatrix);
            drawBackground = true;
            DrawSurfaceBG.Invoke(Main.instance, null);
            Main.spriteBatch.End();
            graphicsDevice.SetRenderTargets(null);
        }

        private void DrawRenderTarget(On_Main.orig_DrawSurfaceBG orig, Main self)
        {
            if (drawBackground || Main.gameMenu)
            {
                orig(self);
                drawBackground = false;
                return;
            }
            Main.spriteBatch.GetSpritebatchDetails(out SpriteSortMode spriteSortMode, out BlendState blendState, out SamplerState samplerState, out DepthStencilState depthStencilState, out RasterizerState rasterizerState, out Effect effect, out Matrix matrix);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(spriteSortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, matrix);
            Main.spriteBatch.Draw(backgroundTarget, Vector2.Zero, null, Color.White * (1f - Completion * (Main.dayTime ? 0.5f : 0.7f)), 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
        }
        private void ManipulateSceneArea(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.Before,
                i => i.MatchLdsfld(typeof(Main), nameof(Main.shimmerAlpha)),
                i => i.MatchLdcR4(1),
                i => i.MatchBeq(out _),
                i => i.MatchLdarg0(),
                i => i.MatchLdloc(13),
                i => i.MatchLdcI4(0),
                i => i.MatchCall(typeof(Main), "DrawStarsInBackground")))
            {
                LogIlError("Telescope Height Adjustment", "Couldn't navigate to before DrawStars call");
                return;
            }
            cursor.Emit(OpCodes.Ldloca, 13); 
            cursor.EmitDelegate(ActuallyMainpulateSceneArea);
        }
        private static void ActuallyMainpulateSceneArea(ref Main.SceneArea area)
        {
            if(!Main.gameMenu && Main.LocalPlayer.GetModPlayer<TelescopePlayer>().cameraRaise > 0)
            {
                area.SceneLocalScreenPositionOffset += Vector2.UnitY * CAMERA_HEIGHT_MAX * Completion;
            }
        }
        public override void PostUpdateMiscEffects()
        {
            if (telescopePosition.HasValue)
            {
                if (Player.Distance(telescopePosition.Value) <= MaxDistanceFromTelescope + 24f)
                {
                    if (cameraRaise < CAMERA_TIME_MAX)
                        cameraRaise++;
                }
                else
                    telescopePosition = null;
            }
            else if (cameraRaise > 0)
                cameraRaise--;
        }
        public override void ModifyScreenPosition()
        {
            Main.instance.CameraModifiers.Add(new PunchCameraModifier(Main.LocalPlayer.position, -Vector2.UnitY, Lerp(0, CAMERA_HEIGHT_MAX / Main.GameZoomTarget, Completion), 60f, 1, uniqueIdentity: "Telescope Raise"));
        }
    }
}