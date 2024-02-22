using Radiance.Content.Items.BaseItems;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles.StarlightBeacon
{
    public class StarlightBeaconCosmetic : ModTile
    {
        public override string Texture => "Radiance/Content/Tiles/StarlightBeacon/StarlightBeacon";

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.CoordinateHeights = new int[2] { 16, 18 };

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Starcatcher Beacon");
            AddMapEntry(new Color(76, 237, 202), name);

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<StarlightBeaconCosmeticTileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (TryGetTileEntityAs(i, j, out StarlightBeaconCosmeticTileEntity entity))
            {
                Tile tile = Main.tile[i, j];
                if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
                {
                    float deployTimer = entity.deployTimer;
                    Texture2D legsTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/StarlightBeacon/StarlightBeaconLegs").Value;
                    Texture2D mainTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/StarlightBeacon/StarlightBeaconMain").Value;
                    Texture2D mainGlowTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/StarlightBeacon/StarlightBeaconMainGlow").Value;
                    Texture2D coverTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/StarlightBeacon/StarlightBeaconCover").Value;
                    Texture2D coverGlowTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/StarlightBeacon/StarlightBeaconCoverGlow").Value;
                    Color tileColor = Lighting.GetColor(i, j);
                    Color glowColor = Color.Lerp(new Color(255, 50, 50), new Color(0, 255, 255), deployTimer / 100);

                    Vector2 legsPosition = new Vector2(i, j) * 16 - Main.screenPosition + TileDrawingZero;
                    Vector2 mainPosition = legsPosition + Vector2.UnitY * 20 - Vector2.UnitY * (float)(20 * EaseInOutExponent(deployTimer / 600, 4));
                    Vector2 coverOffset1 = new(-coverTexture.Width + 2, -4);
                    Vector2 coverOffset2 = new(2, 4);
                    float coverRotation = (float)((PiOver4 + 2) * EaseInOutExponent(deployTimer / 600, 4));
                    //legs
                    Main.spriteBatch.Draw
                    (
                        legsTexture,
                        legsPosition,
                        null,
                        tileColor,
                        0,
                        Vector2.Zero,
                        1,
                        SpriteEffects.None,
                        0
                    );

                    //main
                    Main.spriteBatch.Draw
                    (
                        mainTexture,
                        mainPosition,
                        null,
                        tileColor,
                        0,
                        Vector2.Zero,
                        1,
                        SpriteEffects.None,
                        0
                    );
                    Main.spriteBatch.Draw
                    (
                        mainGlowTexture,
                        mainPosition,
                        null,
                        glowColor,
                        0,
                        Vector2.Zero,
                        1,
                        SpriteEffects.None,
                        0
                    );
                    //covers
                    Main.spriteBatch.Draw
                    (
                        coverTexture,
                        mainPosition + Vector2.UnitX * coverTexture.Width - coverOffset1,
                        null,
                        tileColor,
                        coverRotation,
                        -coverOffset1,
                        1,
                        SpriteEffects.None,
                        0
                    );
                    Main.spriteBatch.Draw
                    (
                        coverTexture,
                        mainPosition + coverOffset2,
                        null,
                        tileColor,
                        -coverRotation,
                        coverOffset2,
                        1,
                        SpriteEffects.FlipHorizontally,
                        0
                    );
                    Main.spriteBatch.Draw
                    (
                        coverGlowTexture,
                        mainPosition + Vector2.UnitX * coverTexture.Width - coverOffset1,
                        null,
                        glowColor,
                        coverRotation,
                        -coverOffset1,
                        1,
                        SpriteEffects.None,
                        0
                    );
                    Main.spriteBatch.Draw
                    (
                        coverGlowTexture,
                        mainPosition + coverOffset2,
                        null,
                        glowColor,
                        -coverRotation,
                        coverOffset2,
                        1,
                        SpriteEffects.FlipHorizontally,
                        0
                    );
                    if (deployTimer > 0)
                    {
                        Vector2 pos = new Vector2(i * 16, j * 16) + TileDrawingZero + new Vector2(entity.Width / 2, 0.7f) * 16 + Vector2.UnitX * 8; //tile world coords + half entity width (center of multitiletile) + a bit of increase
                        float mult = (float)Math.Clamp(Math.Abs(SineTiming(120)), 0.85f, 1f); //color multiplier
                        for (int h = 0; h < 2; h++)
                            RadianceDrawing.DrawBeam(pos, new Vector2(pos.X, 0), h == 1 ? new Color(255, 255, 255, entity.beamTimer) * mult : new Color(0, 255, 255, entity.beamTimer) * mult, h == 1 ? 10 : 14);

                        RadianceDrawing.DrawSoftGlow(pos - Vector2.UnitY * 2, new Color(0, 255, 255, entity.beamTimer) * mult, 0.25f);
                    }
                }
            }
            return false;
        }

        public override void MouseOver(int i, int j)
        {
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Point origin = GetTileOrigin(i, j);
            ModContent.GetInstance<StarlightBeaconCosmeticTileEntity>().Kill(origin.X, origin.Y);
        }
    }

    public class StarlightBeaconCosmeticTileEntity : ImprovedTileEntity
    {
        public float deployTimer = 600;
        public int beamTimer = 0;
        public int pickupTimer = 0;
        public bool deployed = false;

        public StarlightBeaconCosmeticTileEntity() : base(ModContent.TileType<StarlightBeaconCosmetic>()) { }

        public override void OrderedUpdate()
        {
            if (!Main.dayTime)
            {
                if (deployTimer < 600)
                {
                    if (deployTimer == 40)
                        SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/BeaconLift"), Position.ToVector2() * 16 + new Vector2(Width / 2, -Height / 2));
                    deployTimer++;
                }
                if (deployTimer >= 600)
                {
                    if (beamTimer < 255)
                        beamTimer++;
                }
            }
            else if (beamTimer > 0 && deployTimer < 600)
                beamTimer -= Math.Clamp(beamTimer, 0, 2);
            else if (deployTimer > 0)
            {
                pickupTimer = 0;
                if (deployTimer == 550)
                    SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/BeaconLift"), Position.ToVector2() * 16 + new Vector2(Width / 2, -Height / 2)); //todo: make sound not freeze game for a moment when played for the first time in an instance
                deployTimer--;
            }
        }
    }

    public class StarlightBeaconCosmeticItem : BaseTileItem
    {
        public override string Texture => "Radiance/Content/Tiles/StarlightBeacon/StarlightBeaconItem";
        public StarlightBeaconCosmeticItem() : base("StarlightBeaconCosmeticItem", "Starcatcher Beacon (Cosmetic)", "Mimics the visual functionality of the Starcatcher Beacon", "StarlightBeaconCosmetic", 1) { }
    }
}