using Radiance.Content.Items.BaseItems;
using Radiance.Content.Particles;
using Radiance.Core.Systems;
using ReLogic.Graphics;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles.StarlightBeacon
{
    public class StarlightBeacon : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.CoordinateHeights = new int[2] { 16, 18 };
            HitSound = SoundID.Item52;
            DustType = -1;

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Starcatcher Beacon");
            AddMapEntry(new Color(76, 237, 202), name);

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<StarlightBeaconTileEntity>().Hook_AfterPlacement, -1, 0, false);
            TileObjectData.addTile(Type);
        }
        public override void HitWire(int i, int j)
        {
            ToggleTileEntity(i, j);
        }
        public override bool CanPlace(int i, int j) => !TileEntity.ByID.Values.Any(x => x.type == ModContent.TileEntityType<StarlightBeaconTileEntity>());
        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (TryGetTileEntityAs(i, j, out StarlightBeaconTileEntity entity))
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
                    // legs
                    Main.spriteBatch.Draw(legsTexture, legsPosition, null, tileColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                    // main
                    Main.spriteBatch.Draw(mainTexture, mainPosition, null, tileColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                    Main.spriteBatch.Draw(mainGlowTexture, mainPosition, null, glowColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    
                    // covers
                    Main.spriteBatch.Draw(coverTexture, mainPosition + Vector2.UnitX * coverTexture.Width - coverOffset1, null, tileColor, coverRotation, -coverOffset1, 1, SpriteEffects.None, 0);
                    Main.spriteBatch.Draw(coverTexture, mainPosition + coverOffset2, null, tileColor, -coverRotation, coverOffset2, 1, SpriteEffects.FlipHorizontally, 0);
                    Main.spriteBatch.Draw(coverGlowTexture, mainPosition + Vector2.UnitX * coverTexture.Width - coverOffset1, null, glowColor, coverRotation, -coverOffset1, 1, SpriteEffects.None, 0);
                    Main.spriteBatch.Draw(coverGlowTexture, mainPosition + coverOffset2, null, glowColor, -coverRotation, coverOffset2, 1, SpriteEffects.FlipHorizontally, 0);
                    if (deployTimer > 0)
                    {
                        Vector2 pos = entity.TileEntityWorldCenter() + TileDrawingZero - Vector2.UnitY * 4; 
                        float mult = (float)Math.Clamp(Math.Abs(SineTiming(120)), 0.7f, 0.9f); //color multiplier
                        for (int h = 0; h < 2; h++)
                            RadianceDrawing.DrawBeam(pos, new Vector2(pos.X, 0), h == 1 ? new Color(255, 255, 255, entity.beamTimer) * mult : new Color(0, 255, 255, entity.beamTimer) * mult, h == 1 ? 10 : 14);

                        RadianceDrawing.DrawSoftGlow(pos - Vector2.UnitY * 2, new Color(0, 255, 255, entity.beamTimer) * mult, 0.25f);
                    }
                }
            }
            return false;
        }

        public override bool RightClick(int i, int j)
        {
            if (TryGetTileEntityAs(i, j, out StarlightBeaconTileEntity entity))
            {
                Player player = Main.LocalPlayer;
                Item item = GetPlayerHeldItem();
                if (item.type == ItemID.SoulofFlight)
                {
                    SoundEngine.PlaySound(SoundID.Item42);
                    entity.soulCharge += item.stack * 5;
                    item.TurnToAir();
                }
            }
            return false;
        }
        public override void MouseOver(int i, int j)
        {
            if (TryGetTileEntityAs(i, j, out StarlightBeaconTileEntity entity))
            {
                Main.LocalPlayer.SetCursorItem(ItemID.SoulofFlight);
                entity.AddHoverUI();       
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (TryGetTileEntityAs(i, j, out StarlightBeaconTileEntity entity) && entity.soulCharge >= 5)
            {
                int stackCount = entity.soulCharge / 5;
                int num = (int)Math.Ceiling((float)stackCount / Item.CommonMaxStack);
                for (int h = 0; h < num; h++)
                {
                    Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 16, ItemID.SoulofFlight, Math.Min(Item.CommonMaxStack, stackCount));
                    stackCount -= Math.Min(Item.CommonMaxStack, stackCount);
                }
            }
            Point origin = GetTileOrigin(i, j);
            ModContent.GetInstance<StarlightBeaconTileEntity>().Kill(origin.X, origin.Y);
        }
    }

    public class StarlightBeaconTileEntity : RadianceUtilizingTileEntity
    {
        public StarlightBeaconTileEntity() : base(ModContent.TileType<StarlightBeacon>(), 20, new() { 4, 6 }, new()) { }

        public float deployTimer = 600;
        public int beamTimer = 0;
        public int pickupTimer = 0;
        public int soulCharge = 0;
        public bool deployed = false;
        public static readonly int STARLIGHT_BEACON_AOE = 256;
        protected override HoverUIData ManageHoverUI()
        {
            Vector2 modifier = new Vector2(-2 * SineTiming(33), 2 * SineTiming(55));
            if (Main.keyState.PressingShift())
                modifier = Vector2.Zero;
            List<HoverUIElement> data = new List<HoverUIElement>()
                {
                    new RadianceBarUIElement("RadianceBar", storedRadiance, maxRadiance, Vector2.UnitY * 40),
                    new StarlightBeaconHoverElement("SoulCharge", -Vector2.UnitY * 40 + modifier),
                };

            if (deployTimer == 600)
                data.Add(new CircleUIElement("AoECircle", STARLIGHT_BEACON_AOE, new Color(0, 255, 255)));

            return new HoverUIData(this, this.TileEntityWorldCenter(), data.ToArray());
        }
        public override void OrderedUpdate()
        {
            Vector2 center = this.TileEntityWorldCenter();
            if (!Main.dayTime && storedRadiance >= 1 && soulCharge >= 1 && enabled)
            {
                if (deployTimer < 600)
                {
                    if (deployTimer == 40)
                        SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/BeaconLift"), center);

                    deployTimer++;
                }
                if (deployTimer >= 600)
                {
                    if (beamTimer < 255)
                        beamTimer++;

                    pickupTimer++;
                    if (pickupTimer >= 60)
                    {
                        for (int i = 0; i < Main.maxItems; i++)
                        {
                            if (Main.item[i].active && !Main.item[i].IsAir && Main.item[i].type == ItemID.FallenStar && Vector2.Distance(center, Main.item[i].Center) > STARLIGHT_BEACON_AOE && Vector2.Distance(center, Main.item[i].Center) < 51200) //51200 is width of a medium world in pixels halved
                            {
                                Item item = Main.item[i];

                                bool makeInitialParticles = item.Center.Distance(center) > STARLIGHT_BEACON_AOE + 944;
                                int dir = (center.X - item.Center.X).NonZeroSign();
                                Vector2 chosenPosition = TryGetStarNewPosition(item, null, dir);
                                int attempts = 0;

                                // Try to mitigate the chances that the chosen position isn't inside of blocks, and also rotate the position a bit if it's offscreen to add variety
                                while ((Collision.SolidCollision(chosenPosition, item.width, item.height) || (makeInitialParticles && !Main.rand.NextBool(3))) && attempts < 100)
                                {
                                    chosenPosition = TryGetStarNewPosition(item, chosenPosition, dir, TwoPi / 100f);
                                    attempts++;
                                }
                                if (makeInitialParticles)
                                    CreateParticles(item.Center + item.Center.DirectionTo(chosenPosition) * 150, chosenPosition);

                                item.Center = chosenPosition;
                                item.velocity = item.Center.AngleTo(center).ToRotationVector2() * 12f + Vector2.UnitY * -6f;
                                SoundEngine.PlaySound(SoundID.NPCHit5, center);
                                CreateParticles(item.Center - item.Center.DirectionTo(center) * 300, center);

                                storedRadiance--;
                                soulCharge--;
                                pickupTimer = 0;
                                break;
                            }
                        }
                    }
                }
            }
            else if (beamTimer > 0 && deployTimer < 600)
            {
                beamTimer -= 2;
                if (beamTimer < 0)
                    beamTimer = 0;
            }
            else if (deployTimer > 0)
            {
                pickupTimer = 0;
                if (deployTimer == 550)
                    SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/BeaconLift"), this.TileEntityWorldCenter()); 

                deployTimer--;
            }
        }
        public void CreateParticles(Vector2 from, Vector2 to)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector2 directionTo = from.DirectionTo(to);
                Vector2 position = from + directionTo * i * 60;
                Vector2 velocity = directionTo * 2;

                ParticleSystem.AddParticle(new SpeedLine(position + Main.rand.NextVector2Circular(24, 24), velocity, 10 + 3 * i, Color.CornflowerBlue, directionTo.ToRotation(), 240, 1.3f));
                Gore.NewGore(new EntitySource_TileEntity(this), position + Main.rand.NextVector2Circular(24, 24), new Vector2(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2)) + velocity / 2, Main.rand.Next(16, 18), 1f);

            }
        }
        public Vector2 TryGetStarNewPosition(Item item, Vector2? currentPositionAttempt, float dir, float rotate = 0)
        {
            Vector2 center = this.TileEntityWorldCenter();
            if (currentPositionAttempt == null)
                currentPositionAttempt = item.Center;

            return center + Vector2.UnitX.RotatedBy(center.AngleTo(currentPositionAttempt.Value) + rotate * dir) * STARLIGHT_BEACON_AOE;
        }

        public override void SaveExtraExtraData(TagCompound tag)
        {
            if (soulCharge > 0)
                tag["SoulCharge"] = soulCharge;
        }

        public override void LoadExtraExtraData(TagCompound tag)
        {
            soulCharge = tag.Get<int>("SoulCharge");
        }
    }
    public class StarlightBeaconHoverElement : HoverUIElement
    {
        public StarlightBeaconHoverElement(string name, Vector2 targetPosition) : base(name)
        {
            this.targetPosition = targetPosition;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (parent.entity is StarlightBeaconTileEntity entity)
            {
                DynamicSpriteFont font = FontAssets.MouseText.Value;
                Vector2 origin = font.MeasureString(entity.soulCharge.ToString()) / 2;
                Color textColor = new Color(157, 232, 232);
                float scale = Math.Clamp(timerModifier + 0.5f, 0.5f, 1);
                Vector2 iconOffset = new Vector2(12f, 3f);

                Utils.DrawBorderStringFourWay(Main.spriteBatch, font, entity.soulCharge.ToString(), realDrawPosition.X, realDrawPosition.Y, textColor * timerModifier, CommonColors.GetDarkColor(textColor, 6) * timerModifier, origin, scale);
                RadianceDrawing.DrawHoverableItem(Main.spriteBatch, ItemID.SoulofFlight, realDrawPosition - iconOffset - Vector2.UnitX * origin.X * scale, 1, Color.White * timerModifier);
            }
        }
    }
    public class StarlightBeaconItem : BaseTileItem
    {
        public StarlightBeaconItem() : base("StarlightBeaconItem", "Starcatcher Beacon", "Draws in all stars in a massive radius when deployed\nRequires a small amount of Radiance and Souls of Flight to operate", "StarlightBeacon", 1, Item.sellPrice(0, 0, 50, 0), ItemRarityID.LightRed) { }
    }
}