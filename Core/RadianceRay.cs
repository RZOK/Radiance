using Radiance.Content.Items.Tools.Misc;
using Radiance.Content.Particles;
using Radiance.Content.Tiles;
using Radiance.Content.Tiles.Transmutator;
using Radiance.Core.Systems;
using Radiance.Core.Systems.ParticleSystems;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.Marshalling;
using System.Security;

namespace Radiance.Core
{
    public class RadianceRay : TagSerializable
    {
        public Point16 startPos;
        public Point16 endPos;
        public Vector2 WorldStartPos => new Vector2(startPos.X, startPos.Y) * 16f + new Vector2(8);
        public Vector2 WorldEndPos => new Vector2(endPos.X, endPos.Y) * 16f + new Vector2(8);

        public Vector2 visualStartPosition;
        public Vector2 visualEndPosition;
        
        public bool active = false;
        public float transferRate = 2;
        public bool interferred = false;
        public bool interferredVisual = false;

        public int focusedPlayerIndex = 255;

        public int pickedUpTimer = 0;
        public bool PickedUp => pickedUpTimer > 0;

        public bool disappearing = false;
        public int disappearTimer = 0;

        public int visualTimer = 0;
        public const int VISUAL_TIMER_MAX = 30;

        /// <summary>
        /// flag 0: input at start
        /// flag 1: output at end
        /// flag 2: input at end
        /// flag 3: output at start
        /// </summary>
        public BitsByte hasIoAtEnds;
        public RadianceUtilizingTileEntity inputTE;
        public RadianceUtilizingTileEntity outputTE;

        public static readonly int DISAPPEAR_TIMER_MAX = 30;
        public static readonly int maxDistanceBetweenPoints = 1000;
        public float DisappearProgress => 1 - disappearTimer / (float)DISAPPEAR_TIMER_MAX;

        public static readonly SoundStyle RayClick = new("Radiance/Sounds/RayClick");

        public RadianceRay(Point16 startPos, Point16 endPos)
        {
            this.startPos = startPos;
            this.endPos = endPos;
            visualStartPosition = WorldStartPos;
            visualEndPosition = WorldEndPos;
        }

        #region Static Methods

        public static RadianceRay NewRay(Point16 startPosition, Point16 endPosition)
        {
            RadianceRay radianceRay = new RadianceRay(startPosition, endPosition);
            radianceRay.active = true;
            RadianceTransferSystem.rays.Add(radianceRay);
            return radianceRay;
        }

        public static Vector2 CenterOfTile(Vector2 input) => new Vector2(input.X - input.X % 16 + 8, input.Y - input.Y % 16 + 8);

        public static bool FindRay(Point16 pos, out RadianceRay outRay)
        {
            return RadianceTransferSystem.byPosition.TryGetValue(pos, out outRay);
        }

        public static void SpawnPlaceParticles(Point16 pos)
        {
            Vector2 worldPos = pos.ToWorldCoordinates();
            SoundEngine.PlaySound(RayClick, worldPos);
            for (int i = 0; i < 5; i++)
            {
                WorldParticleSystem.system.AddParticle(new Sparkle(worldPos + Vector2.UnitX * Main.rand.NextFloat(-8, 8), Vector2.UnitY * Main.rand.NextFloat(-5, -3), 45, 100, new Color(255, 236, 173), 0.6f));
            }
        }

        #endregion Static Methods

        #region Ray Methods

        /// <summary>
        /// The following should only be updated, before transfer of Radiance, under X circumstances:
        /// -Ray's input and output tile entities
        /// -Whether the ray should be disappearing
        /// -Whether the ray is interferred or not
        ///
        /// The X circumstances are:
        /// -A ray is picked up
        /// -A ray is placed down
        /// -A RUTE is broken
        /// -A RUTE is placed
        /// -The world is loaded
        ///
        /// Transfering of Radiance should be done every tick still
        /// </summary>

        public void Update()
        {
            if (focusedPlayerIndex != Main.maxPlayers)
            {
                Player player = Main.player[focusedPlayerIndex];
                if (player is null || !player.active || player.dead || player.GetModPlayer<RadianceControlRodPlayer>().focusedRay != this)
                    PlaceRay();
            }

            if (!disappearing)
            {
                if (disappearTimer > 0)
                    disappearTimer -= Math.Min(disappearTimer, 6);
            }
            else
            {
                disappearTimer++;
                if (disappearTimer >= DISAPPEAR_TIMER_MAX)
                    active = false;
            }
            if (interferredVisual && visualTimer < VISUAL_TIMER_MAX)
                visualTimer++;

            if (visualTimer > 0 && !PickedUp && !interferredVisual)
                visualTimer--;

            SnapVisualToPosition();

            if (pickedUpTimer > 0)
                pickedUpTimer--;

            if (inputTE != null && outputTE != null)
                ActuallyMoveRadiance(outputTE, inputTE, transferRate);
        }

        public void PlaceRay()
        {
            RadianceTransferSystem.shouldUpdateRays = true;
            focusedPlayerIndex = Main.maxPlayers;
            pickedUpTimer = 0;
            if (endPos == startPos)
            {
                active = false;
                return;
            }

            TryGetIO(out _, out _, out bool startSuccess, out bool endSuccess);
            if (startSuccess)
                SpawnPlaceParticles(startPos);
            if (endSuccess)
                SpawnPlaceParticles(endPos);
        }

        public bool HasIntersection()
        {
            if (PickedUp)
                return false;

            foreach (RadianceRay ray in RadianceTransferSystem.rays)
            {
                if (ray.startPos == startPos || ray.PickedUp || ray.startPos == ray.endPos)
                    continue;

                if (Collision.CheckLinevLine(startPos.ToWorldCoordinates(), endPos.ToWorldCoordinates(), ray.startPos.ToWorldCoordinates(), ray.endPos.ToWorldCoordinates()).Length > 0)
                    return true;
            }
            return false;
        }

        public void SnapVisualToPosition()
        {
            Vector2 startPosWorldCoordinates = startPos.ToWorldCoordinates();
            Vector2 endPosWorldCoordinates = endPos.ToWorldCoordinates();
            if (visualStartPosition != startPosWorldCoordinates || visualEndPosition != endPosWorldCoordinates)
            {
                visualStartPosition = Vector2.Lerp(visualStartPosition, startPosWorldCoordinates, 0.5f);
                visualEndPosition = Vector2.Lerp(visualEndPosition, endPosWorldCoordinates, 0.5f);
            }
        }

        public void TryGetIO(out RadianceUtilizingTileEntity input, out RadianceUtilizingTileEntity output, out bool startSuccess, out bool endSuccess)
        {
            hasIoAtEnds = new BitsByte(); 

            input = null;
            output = null;

            startSuccess = false;
            endSuccess = false;

            if (PickedUp)
                return;

            Tile startTile = Framing.GetTileSafely(startPos.X, startPos.Y);
            Tile endTile = Framing.GetTileSafely(endPos.X, endPos.Y);

            if (TryGetTileEntityAs(startPos.X, startPos.Y, out RadianceUtilizingTileEntity entity))
            {
                int position1 = startTile.TileFrameX / 18 + (startTile.TileFrameY / 18) * entity.Width + 1;
                if (entity.inputTiles.Contains(position1))
                {
                    input = entity;
                    hasIoAtEnds[0] = true;
                    startSuccess = true;
                }
                else if (entity.outputTiles.Contains(position1))
                {
                    output = entity;
                    hasIoAtEnds[3] = true;
                    startSuccess = true;
                }
            }

            if (TryGetTileEntityAs(endPos.X, endPos.Y, out RadianceUtilizingTileEntity entity2))
            {
                int position = endTile.TileFrameX / 18 + (endTile.TileFrameY / 18) * entity2.Width + 1;
                if (entity2.inputTiles.Contains(position))
                {
                    input = entity2;
                    hasIoAtEnds[2] = true;
                    endSuccess = true;
                }
                else if (entity2.outputTiles.Contains(position))
                {
                    output = entity2;
                    hasIoAtEnds[1] = true;
                    endSuccess = true;
                }
            }

            startSuccess |= startTile.HasTile && RadianceSets.RayAnchorTiles[startTile.TileType];
            endSuccess |= endTile.HasTile && RadianceSets.RayAnchorTiles[endTile.TileType];
        }

        public void SetInputToEndOfFixtureChain()
        {
            Tile startTile = Framing.GetTileSafely(startPos.X, startPos.Y);
            Tile endTile = Framing.GetTileSafely(endPos.X, endPos.Y);

            if (ModContent.GetModTile(endTile.TileType) is BaseRelay relay && relay.TileIsInput(endTile) && relay.Active(endTile))
            {
                Point16 start = endPos - new Point16(0, 1);
                hasIoAtEnds[2] = true;
                hasIoAtEnds[3] = true;
                while (TryGetNextItemInFixtureChain(start, out start)) { }
            }
            else if (ModContent.GetModTile(startTile.TileType) is BaseRelay relay2 && relay2.TileIsInput(startTile) && relay2.Active(startTile))
            {
                Point16 start = startPos - new Point16(0, 1);
                hasIoAtEnds[0] = true;
                hasIoAtEnds[1] = true;
                while (TryGetNextItemInFixtureChain(start, out start)) { }
            }
        }

        public bool TryGetNextItemInFixtureChain(Point16 start, out Point16 destination)
        {
            destination = new Point16();
            if (!FindRay(start, out RadianceRay nextRay) || nextRay.PickedUp)
                return false;

            if (nextRay.interferred)
                interferred = true;

            if (nextRay.inputTE is not null)
            {
                if (nextRay.hasIoAtEnds[0])
                    nextRay.hasIoAtEnds[1] = true;
                else
                    nextRay.hasIoAtEnds[3] = true;

                inputTE = nextRay.inputTE;
                return false;
            }

            Tile startTile = Framing.GetTileSafely(nextRay.startPos.X, nextRay.startPos.Y);
            Tile endTile = Framing.GetTileSafely(nextRay.endPos.X, nextRay.endPos.Y);

            if (ModContent.GetModTile(startTile.TileType) is BaseRelay relay && relay.TileIsInput(startTile) && relay.Active(startTile))
            {
                nextRay.hasIoAtEnds[0] = true;
                nextRay.hasIoAtEnds[1] = true;
                destination = nextRay.startPos - new Point16(0, 1);
            }
            else if (ModContent.GetModTile(endTile.TileType) is BaseRelay relay2 && relay2.TileIsInput(endTile) && relay2.Active(endTile))
            {
                nextRay.hasIoAtEnds[2] = true;
                nextRay.hasIoAtEnds[3] = true;
                destination = nextRay.endPos - new Point16(0, 1);
            }
            return true;
        }

        public void ActuallyMoveRadiance(RadianceUtilizingTileEntity source, RadianceUtilizingTileEntity destination, float amount) //Actually manipulates Radiance values between source and destination
        {
            if (interferred)
                return;

            float val = Math.Min(source.storedRadiance, destination.maxRadiance - destination.storedRadiance);
            if (source.storedRadiance < amount * source.outputTiles.Count)
                amount /= source.outputTiles.Count;

            float amountMoved = Math.Clamp(val, 0, amount);

            if (TryGetTileEntityAs(source.Position.X, source.Position.Y, out RadianceUtilizingTileEntity sourceInventory) && sourceInventory is IInterfaceableRadianceCell cellInterface)
            {
                if (cellInterface.ContainerPlaced != null)
                {
                    cellInterface.ContainerPlaced.storedRadiance -= amountMoved;
                    cellInterface.GetRadianceFromItem();
                }
            }
            else
                source.storedRadiance -= amountMoved;

            if (TryGetTileEntityAs(destination.Position.X, destination.Position.Y, out RadianceUtilizingTileEntity destinationInventory) && destinationInventory is IInterfaceableRadianceCell cellInterface2)
            {
                if (cellInterface2.ContainerPlaced != null)
                {
                    cellInterface2.ContainerPlaced.storedRadiance += amountMoved;
                    cellInterface2.GetRadianceFromItem();
                }
            }
            else
                destination.storedRadiance += amountMoved;
        }

        public void DrawRay()
        {
            if (ModContent.GetInstance<ControlRodBuilderToggle>().Active() && ModContent.GetInstance<ControlRodBuilderToggle>().CurrentState == 1 && !interferredVisual && ((hasIoAtEnds[0] && hasIoAtEnds[1]) || (hasIoAtEnds[2] && hasIoAtEnds[3])))
            {
                Vector2 start = visualEndPosition;
                Vector2 end = visualStartPosition;
                if (hasIoAtEnds[0] && hasIoAtEnds[1])
                {
                    start = visualStartPosition;
                    end = visualEndPosition;
                }

                float number = 120;
                int duration = 120;
                int timer = (int)(Main.GameUpdateCount % number);
                if (timer > number - duration)
                {
                    Color color = CommonColors.RadianceColorPink;
                    int time = (int)(timer - number + duration);
                    float prog = time / (float)duration;
                    float alpha = prog;
                    float scale = 1f - visualTimer / (float)VISUAL_TIMER_MAX;
                    prog = EaseInOutExponent(prog, 4f);
                    if (alpha > 0.5f)
                        alpha = 1f - alpha;
                    alpha *= 2.5f;

                    Vector2 pos = end + (start - end) * prog;
                    RadianceDrawing.DrawSoftGlow(pos, color * alpha, 0.29f * scale);
                    RadianceDrawing.DrawSoftGlow(pos, Color.White * alpha, 0.23f * scale);
                }
            }
            Color realColor = !interferredVisual ? CommonColors.RadianceColor1 : new Color(200, 50, 50);
            realColor *= DisappearProgress;
            int j = CenterOfTile(visualStartPosition) == CenterOfTile(visualEndPosition) ? 1 : 2;

            float mult = (float)Math.Clamp(Math.Abs(SineTiming(240)) + 0.4f, 0.9f, 1.1f);
            RadianceDrawing.DrawBeam(visualStartPosition, visualEndPosition, realColor * mult, 14f * DisappearProgress * mult);
            RadianceDrawing.DrawBeam(visualStartPosition, visualEndPosition, Color.White * DisappearProgress * mult, 6f * DisappearProgress * mult);

            for (int i = 0; i < j; i++)
            {
                RadianceDrawing.DrawSoftGlow(i == 0 ? visualEndPosition : visualStartPosition, realColor * DisappearProgress, 0.22f);
                RadianceDrawing.DrawSoftGlow(i == 0 ? visualEndPosition : visualStartPosition, Color.White * DisappearProgress, 0.19f);
            }

            #region Debug Mode Behavior

            if (Main.LocalPlayer.GetModPlayer<RadiancePlayer>().debugMode)
            {
                if (inputTE is not null && outputTE is not null)
                {
                    Vector2 drawPos = inputTE.TileEntityWorldCenter() - new Vector2(inputTE.Width * 8f, inputTE.Height * 8f);
                    Rectangle rect = new Rectangle((int)drawPos.X, (int)drawPos.Y, inputTE.Width * 16, inputTE.Height * 16);
                    Utils.DrawRect(Main.spriteBatch, rect, Color.Blue);

                    Vector2 drawPosOutput = outputTE.TileEntityWorldCenter() - new Vector2(outputTE.Width * 8f, outputTE.Height * 8f);
                    Rectangle rectOutput = new Rectangle((int)drawPosOutput.X, (int)drawPosOutput.Y, outputTE.Width * 16, outputTE.Height * 16);
                    Utils.DrawRect(Main.spriteBatch, rectOutput, Color.Red);
                }
            }

            #endregion Debug Mode Behavior
        }
        public void DrawRayOverlay()
        {
            Vector2 start = visualEndPosition;
            Vector2 end = visualStartPosition;
            if (hasIoAtEnds[0] && hasIoAtEnds[1])
            {
                start = visualStartPosition;
                end = visualEndPosition;
            }
            Color realColor = !interferredVisual ? CommonColors.RadianceColor1 : new Color(200, 50, 50) * visualTimer;
            realColor *= DisappearProgress;
            Texture2D tex = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/RayTiling2").Value;

            float mult = (float)Math.Clamp(Math.Abs(SineTiming(240)) + 0.4f, 0.9f, 1.1f);
            float speed = 1.8f;
            if (interferredVisual)
                speed = 0.3f;

            int tileWidth = (int)(10 * mult * EaseInOutCirc(1f - visualTimer / (float)VISUAL_TIMER_MAX));
            int tileHeight = 128;

            Color color = realColor * 0.4f * mult;
            Main.spriteBatch.DrawScrollingSprite(tex, start - Main.screenPosition, tileWidth, tileHeight, (int)Vector2.Distance(start, end), color, speed, (end - start).ToRotation(), 0);
            Main.spriteBatch.DrawScrollingSprite(tex, start - Main.screenPosition, tileWidth, tileHeight, (int)Vector2.Distance(start, end), color, speed * 1.5f, (end - start).ToRotation(), 0);

        }

        #endregion Ray Methods

        #region TagCompound Stuff

        public static readonly Func<TagCompound, RadianceRay> DESERIALIZER = DeserializeData;

        public TagCompound SerializeData()
        {
            return new TagCompound()
            {
                ["StartPos"] = startPos,
                ["EndPos"] = endPos,
                ["Active"] = active,
            };
        }

        public static RadianceRay DeserializeData(TagCompound tag)
        {
            RadianceRay radianceRay = new(tag.Get<Point16>("StartPos"), tag.Get<Point16>("EndPos"))
            {
                active = tag.Get<bool>("Active")
            };
            return radianceRay;
        }

        #endregion TagCompound Stuff
    }
}