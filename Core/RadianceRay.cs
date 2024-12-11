using Radiance.Content.Items.Tools.Misc;
using Radiance.Content.Particles;
using Radiance.Content.Tiles;
using Radiance.Core.Systems;
using Radiance.Core.Systems.ParticleSystems;
using Steamworks;
using System.Runtime.InteropServices.Marshalling;
using System.Security;

namespace Radiance.Core
{
    public class RadianceRay : TagSerializable
    {
        public Vector2 startPos = Vector2.Zero;
        public Vector2 endPos = Vector2.Zero;

        public Vector2 visualStartPosition;
        public Vector2 visualEndPosition;

        public float transferRate = 2;
        public bool interferred = false;
        public bool active = false;
        public int focusedPlayerIndex = 255;

        public int pickedUpTimer = 0;
        public bool PickedUp => pickedUpTimer > 0;

        public bool disappearing = false;
        public int disappearTimer = 0;

        public RadianceUtilizingTileEntity inputTE;
        public RadianceUtilizingTileEntity outputTE;

        public static readonly int DISAPPEAR_TIMER_MAX = 30;
        public static readonly int maxDistanceBetweenPoints = 1000;
        public float DisappearProgress => 1 - disappearTimer / (float)DISAPPEAR_TIMER_MAX;

        public static readonly SoundStyle RayClick = new("Radiance/Sounds/RayClick");

        public RadianceRay(Vector2 startPos, Vector2 endPos)
        {
            this.startPos = startPos;
            this.endPos = endPos;
            visualStartPosition = startPos;
            visualEndPosition = endPos;
        }

        #region Static Methods

        public static RadianceRay NewRay(Vector2 startPosition, Vector2 endPosition)
        {
            RadianceRay radianceRay = new RadianceRay(startPosition, endPosition);
            radianceRay.active = true;
            RadianceTransferSystem.rays.Add(radianceRay);
            return radianceRay;
        }

        public static Vector2 SnapToCenterOfTile(Vector2 input) => new Vector2(input.X - input.X % 16 + 8, input.Y - input.Y % 16 + 8);

        public static bool FindRay(Vector2 pos, out RadianceRay outRay)
        {
            outRay = RadianceTransferSystem.rays.FirstOrDefault(x => x.active && (x.startPos == pos || x.endPos == pos));
            return outRay != null;
        }

        public static bool FindRay(Point tilePosition, out RadianceRay outRay)
        {
            outRay = RadianceTransferSystem.rays.FirstOrDefault(x => x.active && (x.startPos == tilePosition.ToWorldCoordinates() || x.endPos == tilePosition.ToWorldCoordinates()));
            return outRay != null;
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
        /// 
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

            if (pickedUpTimer > 0)
                pickedUpTimer--;

            SnapToPosition(startPos, endPos);

            if (inputTE != null && outputTE != null)
                ActuallyMoveRadiance(outputTE, inputTE, transferRate);
        }

        public void PlaceRay()
        {
            RadianceTransferSystem.shouldUpdateRays = true;
            focusedPlayerIndex = Main.maxPlayers;
            pickedUpTimer = 0;

            TryGetIO(out _, out _, out bool startSuccess, out bool endSuccess);

            if (startSuccess)
                SpawnPlaceParticles(startPos);
            if (endSuccess)
                SpawnPlaceParticles(endPos);
        }

        public static void SpawnPlaceParticles(Vector2 pos)
        {
            SoundEngine.PlaySound(RayClick, pos);
            for (int i = 0; i < 5; i++)
            {
                WorldParticleSystem.system.AddParticle(new Sparkle(pos, Vector2.UnitX.RotatedByRandom(TwoPi) * Main.rand.NextFloat(2, 5), 60, 100, new Color(255, 236, 173), 0.6f));
            }
        }

        public bool HasIntersection()
        {
            if (PickedUp)
                return false;

            foreach (RadianceRay ray in RadianceTransferSystem.rays)
            {
                if (ray.startPos == startPos || ray.PickedUp)
                    continue;

                if (Collision.CheckLinevLine(startPos, endPos, ray.startPos, ray.endPos).Length > 0)
                    return true;
            }
            return false;
        }

        public void SnapToPosition(Vector2 start, Vector2 end) //Snaps an endpoint to the center of the tile
        {
            startPos = SnapToCenterOfTile(start);
            endPos = SnapToCenterOfTile(end);
            if (visualStartPosition != startPos || visualEndPosition != endPos)
            {
                visualStartPosition = Vector2.Lerp(visualStartPosition, startPos, 0.5f);
                visualEndPosition = Vector2.Lerp(visualEndPosition, endPos, 0.5f);
            }
        }

        public void TryGetIO(out RadianceUtilizingTileEntity input, out RadianceUtilizingTileEntity output, out bool startSuccess, out bool endSuccess)
        {
            input = null;
            output = null;

            Point startCoords = Utils.ToTileCoordinates(startPos);
            Point endCoords = Utils.ToTileCoordinates(endPos);

            Tile startTile = Framing.GetTileSafely(startCoords.X, startCoords.Y);
            Tile endTile = Framing.GetTileSafely(endCoords.X, endCoords.Y);

            startSuccess = false;
            endSuccess = false;

            Vector2 startTEPos = new Vector2(startCoords.X - startTile.TileFrameX / 18, startCoords.Y - startTile.TileFrameY / 18);
            Vector2 endTEPos = new Vector2(endCoords.X - endTile.TileFrameX / 18, endCoords.Y - endTile.TileFrameY / 18);

            if (TryGetTileEntityAs((int)startTEPos.X, (int)startTEPos.Y, out RadianceUtilizingTileEntity entity))
            {
                int position1 = startTile.TileFrameX / 18 + (startTile.TileFrameY / 18) * entity.Width + 1;
                if (entity.inputTiles.Contains(position1))
                {
                    input = entity;
                    startSuccess = true;
                }
                else if (entity.outputTiles.Contains(position1))
                {
                    output = entity;
                    startSuccess = true;
                }
            }

            if (TryGetTileEntityAs((int)endTEPos.X, (int)endTEPos.Y, out RadianceUtilizingTileEntity entity2))
            {
                int position = endTile.TileFrameX / 18 + (endTile.TileFrameY / 18) * entity2.Width + 1;
                if (entity2.inputTiles.Contains(position))
                {
                    input = entity2;
                    endSuccess = true;
                }
                else if (entity2.outputTiles.Contains(position))
                {
                    output = entity2;
                    endSuccess = true;
                }
            }

            startSuccess |= startTile.HasTile && RadianceSets.RayAnchorTiles[startTile.TileType];
            endSuccess |= endTile.HasTile && RadianceSets.RayAnchorTiles[endTile.TileType];
        }

        /// <summary>
        /// i don't like how hardcoded this is
        /// </summary>
        public void SetInputToEndOfFixtureChain()
        {
            Point startCoords = Utils.ToTileCoordinates(startPos);
            Point endCoords = Utils.ToTileCoordinates(endPos);

            Tile startTile = Framing.GetTileSafely(startCoords.X, startCoords.Y);
            Tile endTile = Framing.GetTileSafely(endCoords.X, endCoords.Y);

            if (RelayFixture.TileIsInput(endTile))
            {
                Point start = endCoords - new Point(0, 1);
                while (TryGetNextItemInFixtureChain(start, out start)) { }
            }
            else if (RelayFixture.TileIsInput(startTile))
            {
                Point start = startCoords - new Point(0, 1);
                while (TryGetNextItemInFixtureChain(start, out start)) { }
            }
        }

        public bool TryGetNextItemInFixtureChain(Point start, out Point destination)
        {
            destination = new Point();
            if (!FindRay(start, out RadianceRay nextRay))
                return false;

            if (nextRay.inputTE is not null)
            {
                inputTE = nextRay.inputTE;
                return false;
            }

            Point startCoords = nextRay.startPos.ToTileCoordinates() ;
            Point endCoords = nextRay.endPos.ToTileCoordinates();

            Tile startTile = Framing.GetTileSafely(startCoords.X, startCoords.Y);
            Tile endTile = Framing.GetTileSafely(endCoords.X, endCoords.Y);

            if (RelayFixture.TileIsInput(startTile))
                destination = startCoords - new Point(0, 1);
            else if (RelayFixture.TileIsInput(endTile))
                destination = endCoords - new Point(0, 1);

            return true;
        }

        public void ActuallyMoveRadiance(RadianceUtilizingTileEntity source, RadianceUtilizingTileEntity destination, float amount) //Actually manipulates Radiance values between source and destination
        {
            if (interferred)
                amount /= 500;
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

        internal PrimitiveTrail RayPrimDrawer;
        internal PrimitiveTrail RayPrimDrawer2;

        public void DrawRay()
        {
            Color realColor = !interferred ? CommonColors.RadianceColor1 : new Color(200, 50, 50);
            realColor *= DisappearProgress;
            int j = SnapToCenterOfTile(visualStartPosition) == SnapToCenterOfTile(visualEndPosition) ? 1 : 2;
            RadianceDrawing.DrawBeam(visualStartPosition, visualEndPosition, realColor, 14f * DisappearProgress);
            RadianceDrawing.DrawBeam(visualStartPosition, visualEndPosition, Color.White * DisappearProgress, 5f * DisappearProgress);

            for (int i = 0; i < j; i++)
            {
                RadianceDrawing.DrawSoftGlow(i == 0 ? visualEndPosition : visualStartPosition, Color.White * DisappearProgress, 0.18f);
                RadianceDrawing.DrawSoftGlow(i == 0 ? visualEndPosition : visualStartPosition, realColor * DisappearProgress, 0.2f);
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
            RadianceRay radianceRay = new(tag.Get<Vector2>("StartPos"), tag.Get<Vector2>("EndPos"))
            {
                active = tag.Get<bool>("Active")
            };
            return radianceRay;
        }

        #endregion TagCompound Stuff
    }
}