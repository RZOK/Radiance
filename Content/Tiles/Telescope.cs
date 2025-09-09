using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Particles;
using Radiance.Content.Tiles.Pedestals;
using Radiance.Core.Systems;
using Radiance.Core.Systems.ParticleSystems;
using ReLogic.Graphics;
using System.Globalization;
using System.Reflection;
using Terraria.Graphics.CameraModifiers;
using Terraria.Localization;
using Terraria.ObjectData;
using Terraria.UI;
using static Radiance.Content.Tiles.TelescopeBoostInfo;

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

            TileObjectData.addTile(Type);
        }
        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            if (drawData.tileFrameX == 0 && drawData.tileFrameY == 0)
                Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
        }
        public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
        {
            spriteBatch.GetSpritebatchDetails(out SpriteSortMode spriteSortMode, out BlendState blendState, out SamplerState samplerState, out DepthStencilState depthStencilState, out RasterizerState rasterizerState, out Effect effect, out Matrix matrix);
            spriteBatch.End();
            spriteBatch.Begin(spriteSortMode, BlendState.Additive, samplerState, depthStencilState, rasterizerState, effect, matrix);

            if (TelescopeSystem.usedTelescopeTonight)
            {
                Tile tile = Main.tile[i, j];
                if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
                {
                    Texture2D tex = ModContent.Request<Texture2D>($"{Texture}Outline").Value;
                    Vector2 mainPosition = MultitileWorldCenter(i, j) + TileDrawingZero - Main.screenPosition + Vector2.UnitY;
                    Main.spriteBatch.Draw(tex, mainPosition, null, Color.Aqua * 1f * (float)Math.Clamp(Math.Abs(SineTiming(240)) * 1.2f, 0.5f, 1f), 0, tex.Size() / 2f, 1, SpriteEffects.None, 0);
                }
            }
            spriteBatch.End();
            spriteBatch.Begin(spriteSortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, matrix);
        }
        public override void NearbyEffects(int i, int j, bool closer)
        {
            Tile tile = Main.tile[i, j];
            if (tile.TileFrameX == 0 && tile.TileFrameY == 0 && Main.rand.NextBool(60) && TelescopeSystem.usedTelescopeTonight)
            {
                TileObjectData data = TileObjectData.GetTileData(tile);
                Vector2 dustPosition = new Point(i, j).ToWorldCoordinates(0, 0) + new Vector2(Main.rand.Next(data.Width * 16), Main.rand.Next(data.Height * 16));
                WorldParticleSystem.system.AddParticle(new TreasureSparkle(dustPosition, Vector2.UnitY * Main.rand.NextFloat(-0.2f, -0.1f), 300, 0.6f, Color.Aqua * 0.7f));
            }
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
        public float textVisiblity = 0;
        public Vector2? telescopePosition;
        private static RenderTarget2D backgroundTarget;
        private static MethodInfo DrawSurfaceBG;
        private bool drawBackground = false;

        private const float CAMERA_TIME_MAX = 90;
        private const float CAMERA_HEIGHT_MAX = 240;
        private const float TEXT_VISIBILITY_MAX = 120;

        private float MaxDistanceFromTelescope => (Player.blockRange + Player.tileRangeX) * 16f;
        internal static float Completion => EaseInOutExponent(Main.LocalPlayer.GetModPlayer<TelescopePlayer>().cameraRaise / CAMERA_TIME_MAX, 4f);
        internal static float TextCompletion => EaseInOutExponent(Main.LocalPlayer.GetModPlayer<TelescopePlayer>().textVisiblity / TEXT_VISIBILITY_MAX, 4f);
        public override void Load()
        {
            IL_Main.DoDraw += ManipulateSceneArea;
            DrawSurfaceBG = typeof(Main).GetMethod(nameof(DrawSurfaceBG), BindingFlags.Instance | BindingFlags.NonPublic);

            ResizeRenderTarget();
            RenderTargetsManager.ResizeRenderTargetDelegateEvent += ResizeRenderTarget;
            RenderTargetsManager.DrawToRenderTargetsDelegateEvent += DrawToRenderTarget;
            On_Main.DrawSurfaceBG += DrawRenderTarget;
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
            if (!Main.gameMenu && Main.LocalPlayer.GetModPlayer<TelescopePlayer>().cameraRaise > 0)
                area.SceneLocalScreenPositionOffset += Vector2.UnitY * CAMERA_HEIGHT_MAX * Completion / 2f;
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
            if (Main.gameMenu || drawBackground || Completion == 0)
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
        public override void PostUpdateMiscEffects()
        {
            if (telescopePosition.HasValue)
            {
                Tile tile = Framing.GetTileSafely(Utils.ToTileCoordinates(telescopePosition.Value));
                if (Player.Distance(telescopePosition.Value) <= MaxDistanceFromTelescope + 24f && tile.TileType == ModContent.TileType<Telescope>() && tile.HasTile)
                {
                    if (cameraRaise < CAMERA_TIME_MAX)
                        cameraRaise++;
                }
                else
                    telescopePosition = null;

                if (cameraRaise >= CAMERA_TIME_MAX && textVisiblity < TEXT_VISIBILITY_MAX)
                    textVisiblity++;
                else if (textVisiblity > 0)
                    textVisiblity--;

                if (TelescopeSystem.currentBoost.boost > 0)
                    TelescopeSystem.usedTelescopeTonight = true;
            }
            else
            {
                if (textVisiblity > 0)
                    textVisiblity--;

                if (cameraRaise > 0)
                    cameraRaise--;
            }
        }
        public override void ModifyScreenPosition()
        {
            if (cameraRaise > 0)
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(Main.LocalPlayer.position, -Vector2.UnitY, Lerp(0, CAMERA_HEIGHT_MAX / Main.GameZoomTarget, Completion), 60f, 1, uniqueIdentity: "Telescope Raise"));
        }
    }
    public class TelescopeSystem : ModSystem
    {
        public static TelescopeBoostInfo currentBoost;
        public static bool usedTelescopeTonight = false;

        private const int BOX_HORIZONTAL_PADDING = 52;
        private const int VERTICAL_PADDING = 160;
        private const float TOP_TEXT_SCALE = 1.5f;
        public override void Load()
        {
            TelescopeBoostStrings.LoadStrings();
        }
        public override void PostUpdateTime()
        {
            if(Main.dayTime)
            {
                usedTelescopeTonight = false;
                if (Main.eclipse && currentBoost.type != TelescopeBoostType.Eclipse)
                    currentBoost = new TelescopeBoostInfo(TelescopeBoostType.Eclipse, 0, Main.rand.Next(TelescopeBoostStrings.EclipseStrings));
                else if (currentBoost.type != TelescopeBoostType.DayTime)
                    currentBoost = new TelescopeBoostInfo(TelescopeBoostType.DayTime, 0, Main.rand.Next(TelescopeBoostStrings.DayStrings));
            }
            else
            {
                if (Main.bloodMoon)
                {
                    usedTelescopeTonight = false;
                    if (currentBoost.type != TelescopeBoostType.BloodMoon)
                        currentBoost = new TelescopeBoostInfo(TelescopeBoostType.BloodMoon, 0, Main.rand.Next(TelescopeBoostStrings.BloodMoonStrings));
                }
                else if (currentBoost.type != TelescopeBoostType.NightTime || currentBoost.tooltip == null)
                {
                    float boost = Main.rand.NextFloat(0f, 0.1f);
                    LocalizedText toolTip = Main.rand.Next(TelescopeBoostStrings.HighBoostStrings);
                    if (boost < 0.01f)
                        boost = 0f;
                    if (boost > 0.09f)
                        boost = 0.1f;

                    if (boost < 0.033f)
                        toolTip = Main.rand.Next(TelescopeBoostStrings.LowBoostStrings);
                    else if (boost < 0.067f)
                        toolTip = Main.rand.Next(TelescopeBoostStrings.MedBoostStrings);

                    currentBoost = new TelescopeBoostInfo(TelescopeBoostType.NightTime, boost, toolTip);
                }
            }
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            for (int k = 0; k < layers.Count; k++)
            {
                if (layers[k].Name == "Vanilla: Achievement Complete Popups")
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Telescope Bonus Display", DrawTelescopeBonus, InterfaceScaleType.UI));
            }
        }
        private static bool DrawTelescopeBonus()
        {
            if (currentBoost.tooltip is null)
                return true;

            DynamicSpriteFont font = FontAssets.MouseText.Value;
            string top = currentBoost.tooltip.Value;
            string bottom = $"Radiance Cells are {MathF.Round(currentBoost.boost * 100f, 1)}% more efficient.";
            int longestString = (int)Max(font.MeasureString(top).X + 32f, font.MeasureString(bottom).X);
            Vector2 position = new Vector2(Main.screenWidth / 2f, VERTICAL_PADDING);
            float completion = TelescopePlayer.TextCompletion;

            Main.spriteBatch.GetSpritebatchDetails(out SpriteSortMode spriteSortMode, out BlendState blendState, out SamplerState samplerState, out DepthStencilState depthStencilState, out RasterizerState rasterizerState, out Effect effect, out Matrix matrix);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(spriteSortMode, blendState, SamplerState.PointClamp, depthStencilState, rasterizerState, effect, matrix);
            DrawTelescopeBonus_Background(position - Vector2.UnitY * 8, longestString, Color.White * completion);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(spriteSortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, matrix);

            Utils.DrawBorderStringFourWay(Main.spriteBatch, font, top, position.X, position.Y, CommonColors.RadianceColor1 * completion, CommonColors.RadianceColor1.GetDarkColor() * completion, font.MeasureString(top) / 2f, 1f);
            Utils.DrawBorderStringFourWay(Main.spriteBatch, font, bottom, position.X, position.Y + 36f, CommonColors.RadianceColor1 * completion, CommonColors.RadianceColor1.GetDarkColor() * completion, font.MeasureString(bottom) / 2f);
            return true;
        }
        private static void DrawTelescopeBonus_Background(Vector2 position, int longestString, Color color)
        {
            int leftWidth = 120;
            int midLeftWidth = 24;
            int midWidth = 30;
            int midRightWidth = midLeftWidth;
            int rightWidth = leftWidth;
            int lengthToDraw = Math.Max(longestString + BOX_HORIZONTAL_PADDING - leftWidth - midWidth - rightWidth, 0) / 2;
            
            Texture2D tex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/ExtraTextures/TelescopeUI").Value;
            Rectangle leftRectangle = new Rectangle(0, 0, leftWidth, tex.Height);
            Rectangle midLeftRectangle = new Rectangle(leftWidth + 2, 0, midLeftWidth, tex.Height);
            Rectangle midRectangle = new Rectangle(leftWidth + 4 + midLeftWidth, 0, midWidth, tex.Height);
            Rectangle midRightRectangle = new Rectangle(tex.Width - rightWidth - midRightWidth - 2, 0, midRightWidth, tex.Height);
            Rectangle rightRectangle = new Rectangle(tex.Width - rightWidth, 0, rightWidth, tex.Height);

            Main.spriteBatch.Draw(tex, position - Vector2.UnitX * (leftWidth / 2f + lengthToDraw + midWidth / 2f), leftRectangle, color, 0, leftRectangle.Size() / 2f, 1f, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(tex, position - Vector2.UnitX * (midWidth / 2f + lengthToDraw / 2f), midLeftRectangle, color, 0, midLeftRectangle.Size() / 2f, new Vector2(lengthToDraw / (float)midLeftWidth, 1), SpriteEffects.None, 0);
            Main.spriteBatch.Draw(tex, position, midRectangle, color, 0, midRectangle.Size() / 2f, 1f, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(tex, position + Vector2.UnitX * (midWidth / 2f + lengthToDraw / 2f), midRightRectangle, color, 0, midRightRectangle.Size() / 2f, new Vector2(lengthToDraw / (float)midRightWidth, 1), SpriteEffects.None, 0);
            Main.spriteBatch.Draw(tex, position + Vector2.UnitX * (rightWidth / 2f + lengthToDraw + midWidth / 2f), rightRectangle, color, 0, rightRectangle.Size() / 2f, 1f, SpriteEffects.None, 0);
        }
    }
    public struct TelescopeBoostInfo
    {
        public enum TelescopeBoostType
        {
            NightTime,
            DayTime,
            BloodMoon,
            Eclipse
        }
        public TelescopeBoostType type;
        public float boost;
        public LocalizedText tooltip;
        public TelescopeBoostInfo(TelescopeBoostType type, float boost, LocalizedText tooltip)
        {
            this.type = type;
            this.boost = boost;
            this.tooltip = tooltip;
        }
    }
    public static class TelescopeBoostStrings
    {
        private static readonly string LOCALIZATION_KEY = $"Mods.{Radiance.Instance.Name}.TelescopeBlurbs.";
        internal static LocalizedText[] LowBoostStrings = new LocalizedText[3];
        internal static LocalizedText[] MedBoostStrings = new LocalizedText[3];
        internal static LocalizedText[] HighBoostStrings = new LocalizedText[3];
        internal static LocalizedText[] DayStrings = new LocalizedText[3];
        internal static LocalizedText[] BloodMoonStrings = new LocalizedText[1];
        internal static LocalizedText[] EclipseStrings = new LocalizedText[1];

        internal static void LoadStrings()
        {
            LowBoostStrings.AddBlurb(nameof(LowBoostStrings), "The dim shimmer of the stars is rather peaceful tonight.");
            LowBoostStrings.AddBlurb(nameof(LowBoostStrings), "The stars shine faintly above your head.");
            LowBoostStrings.AddBlurb(nameof(LowBoostStrings), "The song of the stars is sung in whispers tonight.");

            MedBoostStrings.AddBlurb(nameof(MedBoostStrings), "A diverse and abundant collection of stars fills the night sky.");
            MedBoostStrings.AddBlurb(nameof(MedBoostStrings), "The warm light of the stars helps take your mind off the cool night wind.");
            MedBoostStrings.AddBlurb(nameof(MedBoostStrings), "The song of the stars is sung with an impressive finesse.");

            HighBoostStrings.AddBlurb(nameof(HighBoostStrings), "The stars shine brilliantly in a multitude of beautiful colors.");
            HighBoostStrings.AddBlurb(nameof(HighBoostStrings), "The song of the stars is sung together in beautiful harmony.");
            HighBoostStrings.AddBlurb(nameof(HighBoostStrings), "You can feel the stars smile upon you tonight.");

            DayStrings.AddBlurb(nameof(DayStrings), "The ever-burning light of the sun brings forth a new day.");
            DayStrings.AddBlurb(nameof(DayStrings), "The sun shines down upon a clear and relaxing day.");
            DayStrings.AddBlurb(nameof(DayStrings), "Your own star hides all the others behind its blinding light.");

            BloodMoonStrings.AddBlurb(nameof(BloodMoonStrings), "The scarlet sheen of the moon dims the stars.");

            EclipseStrings.AddBlurb(nameof(EclipseStrings), "The halo of the sun shines ominously beyond the moon.");
        }
        private static void AddBlurb(this LocalizedText[] array, string key, string value)
        {
            int val = 0;
            for (int i = 0; i < array.Length; i++)
            {
                val = i;
                if (array[i] is null)
                    break;

                if (val == array.Length - 1)
                    throw new Exception($"Attempted to add blurb {key}{val} when array was already full.");
            }
            LocalizedText text = Language.GetOrRegister($"{LOCALIZATION_KEY}{key}{val}", () => value);
            array[val] = text;
        }
    }
}