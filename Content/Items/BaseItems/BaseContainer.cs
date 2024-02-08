using Microsoft.Xna.Framework.Input;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Content.Tiles;
using Radiance.Content.Tiles.Pedestals;

namespace Radiance.Content.Items.BaseItems
{
    public abstract class BaseContainer : ModItem, IPedestalItem, IRadianceContainer
    {
        public BaseContainer(Dictionary<string, string> extraTextures, float maxRadiance, bool canAbsorbStars, ContainerMode mode, float absorptionModifier = 1)
        {
            this.extraTextures = extraTextures;
            this.maxRadiance = maxRadiance;
            this.canAbsorbStars = canAbsorbStars;
            this.mode = mode;
            this.absorptionModifier = absorptionModifier;
        }

        public float storedRadiance { get; set; }
        public float maxRadiance;
        public bool canAbsorbStars;
        public ContainerMode mode;
        public Dictionary<string, string> extraTextures;
        public float absorptionModifier;
        public enum ContainerMode
        {
            InputOutput,
            InputOnly,
            OutputOnly
        }

        public Color aoeCircleColor => CommonColors.RadianceColor1;
        public float aoeCircleRadius => 100;

        public int absorbTimer = 0;
        public int transformTimer = 0;

        public Dictionary<int, int> ValidAbsorbableItems = new()
        {
            { ItemID.FallenStar, 20 },
            { ModContent.ItemType<GlowstalkItem>(), 12 },
        };

        public override void UpdateInventory(Player player)
        {
            UpdateContainer(null);
        }

        public override void PostUpdate()
        {
            UpdateContainer(null);

            float radianceCharge = Math.Min(storedRadiance, maxRadiance);
            float fill = radianceCharge / maxRadiance;
            float strength = 0.4f;
            if (HasRadianceAdjustingTexture)
                Lighting.AddLight(Item.Center, Color.Lerp(new Color
                    (
                     1 * fill * strength,
                     0.9f * fill * strength,
                     0.4f * fill * strength
                    ), new Color
                    (
                     0.7f * fill * strength,
                     0.65f * fill * strength,
                     0.5f * fill * strength
                    ),
                fill * SineTiming(20)).ToVector3());

            if (canAbsorbStars)
                AbsorbStars(Item.Center, absorptionModifier);

            if (mode != ContainerMode.InputOnly)
                FlareglassCreation(Item.Center);
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            if (HasRadianceAdjustingTexture)
            {
                Texture2D texture = ModContent.Request<Texture2D>(extraTextures["RadianceAdjusting"]).Value;
                float radianceCharge = Math.Min(storedRadiance, maxRadiance);
                float fill = radianceCharge / maxRadiance;
                Color color = Color.Lerp(CommonColors.RadianceColor1 * fill, CommonColors.RadianceColor2 * fill, SineTiming(5) * fill);

                Main.EntitySpriteDraw(texture, Item.Center - Main.screenPosition, null, color, rotation, texture.Size() / 2, scale, SpriteEffects.None, 0);
            }
        }

        /// <summary>
        /// Used for setting a tile entities Radiance values to that of the container's. Projector and Pedestals utilize this.
        /// </summary>
        /// <param name="entity">The tile entity being affected.</param>
        public void InInterfacableInventory(IInterfaceableRadianceCell entity)
        {
            UpdateContainer(entity);
            entity.GetRadianceFromItem();
        }

        public void PedestalEffect(PedestalTileEntity pte)
        {
            if (canAbsorbStars)
                AbsorbStars(pte.GetFloatingItemCenter(Item), pte.cellAbsorptionBoost + absorptionModifier, pte);

            if (mode != ContainerMode.InputOnly)
                FlareglassCreation(pte.GetFloatingItemCenter(Item), pte);
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (HasRadianceAdjustingTexture)
            {
                float radianceCharge = Math.Min(storedRadiance, maxRadiance);
                float fill = radianceCharge / maxRadiance;
                Texture2D texture = ModContent.Request<Texture2D>(extraTextures["RadianceAdjusting"]).Value;
                Color color = Color.Lerp(CommonColors.RadianceColor1 * fill, CommonColors.RadianceColor2 * fill, fill * SineTiming(5));

                spriteBatch.Draw(texture, position, null, color, 0, texture.Size() / 2, scale, SpriteEffects.None, 0);
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TooltipLine detailsLine = new(Mod, "RadianceCellDetails", "Stores Radiance within itself");

            if (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift))
            {
                detailsLine.OverrideColor = new Color(255, 220, 150);
                if (canAbsorbStars)
                    detailsLine.Text += "\nConverts nearby Fallen Stars into Radiance\nWorks when dropped on the ground or placed upon a Pedestal\nRadiance can be extracted and distributed when placed on a Pedestal as well";
            }
            else
            {
                detailsLine.OverrideColor = new Color(112, 122, 122);
                detailsLine.Text = "-Hold SHIFT for Radiance Cell information-";
            }
            List<TooltipLine> tooltipLines = tooltips.Where(x => x.Name.StartsWith("Tooltip") && x.Mod == "Terraria").ToList();
            tooltips.Insert(tooltips.FindIndex(x => x == tooltipLines.First()), detailsLine);

            TooltipLine meterLine = new(Mod, "RadianceMeter", ".");
            tooltips.Insert(tooltips.FindIndex(x => x == tooltipLines.Last()) + 1, meterLine);
        }

        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            if (line.Name == "RadianceMeter")
            {
                Texture2D meterTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/ItemRadianceMeter").Value;
                RadianceDrawing.DrawHorizontalRadianceBar(new Vector2(line.X + meterTexture.Width / 2, line.Y + meterTexture.Height / 2) - Vector2.UnitY * 2, maxRadiance, storedRadiance);
                return false;
            }
            return true;
        }

        public void FlareglassCreation(Vector2 position, PedestalTileEntity pte = null)
        {
            Item item = null;
            if (storedRadiance >= 5)
            {
                for (int i = 0; i < Main.maxItems; i++)
                {
                    if (Main.item[i] != null && Main.item[i].active && Vector2.Distance(Main.item[i].Center, position) < 90 && ((Main.item[i].type >= ItemID.Sapphire && Main.item[i].type <= ItemID.Diamond) || Main.item[i].type == ItemID.Amber))
                    {
                        bool canTransmutate = true;
                        if (pte != null && !pte.itemImprintData.IsItemValid(Main.item[i]))
                            canTransmutate = false;

                        if(canTransmutate)
                            item = Main.item[i];

                        break;
                    }
                }
                if (item is not null && !item.IsAir)
                {
                    transformTimer++;
                    Texture2D cellTexture = TextureAssets.Item[Item.type].Value;
                    Texture2D gemTexture = TextureAssets.Item[item.type].Value;
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 pos = item.Center + new Vector2(Main.rand.NextFloat(-gemTexture.Width, gemTexture.Width), Main.rand.NextFloat(-gemTexture.Height, gemTexture.Height)) / 2;
                        Vector2 pos2 = position + new Vector2(Main.rand.NextFloat(-cellTexture.Width, cellTexture.Width), Main.rand.NextFloat(-cellTexture.Height, cellTexture.Height)) / 2;
                        Vector2 dir = Utils.DirectionTo(pos2, pos) * Vector2.Distance(pos, pos2) / 10;
                        Dust dust = Dust.NewDustPerfect(pos2, DustID.GoldCoin);
                        dust.noGravity = true;
                        dust.fadeIn = 1.1f;
                        dust.velocity = dir;
                    }
                    if (transformTimer >= 120)
                    {
                        SoundEngine.PlaySound(SoundID.NPCDeath7, item.position);
                        for (int j = 0; j < 40; j++)
                        {
                            int d = Dust.NewDust(item.position, item.width, item.height, DustID.GoldCoin, 0, 0, 150, default(Color), 1.2f);
                            Main.dust[d].noGravity = true;
                            Main.dust[d].fadeIn = 1.5f;
                            Main.dust[d].velocity *= Main.rand.NextFloat(1, 3);
                        }
                        int flareglass = Item.NewItem(new EntitySource_Misc("FlareglassTransform"), item.position, ModContent.ItemType<ShimmeringGlass>());
                        Main.item[flareglass].velocity.X = Main.rand.NextFloat(-3, 3);
                        Main.item[flareglass].velocity.Y = Main.rand.NextFloat(-4, -2);
                        Main.item[flareglass].noGrabDelay = 30;
                        item.stack -= 1;
                        if (item.stack <= 0)
                            item.TurnToAir();

                        transformTimer = 0;
                        storedRadiance -= 5;
                        return;
                    }
                }
            }
        }

        public void AbsorbStars(Vector2 position, float mult, PedestalTileEntity pte = null)
        {
            Item item = null;

            for (int i = 0; i < Main.maxItems; i++)
            {
                if (Main.item[i] != null && Main.item[i].active && Vector2.Distance(Main.item[i].Center, position) < 90 && ValidAbsorbableItems.ContainsKey(Main.item[i].type))
                {
                    bool canAbsorb = true;
                    if (pte != null && !pte.itemImprintData.IsItemValid(Main.item[i]))
                        canAbsorb = false;

                    if (canAbsorb)
                        item = Main.item[i];
                    
                    break;
                }
            }
            if (item is not null && !item.IsAir)
            {
                absorbTimer += item.type == ModContent.ItemType<GlowstalkItem>() ? 2 : 1;
                Vector2 pos = item.Center + new Vector2(Main.rand.NextFloat(-item.width, item.width), Main.rand.NextFloat(-item.height, item.height)) / 2;
                Vector2 dir = Utils.DirectionTo(pos, position) * Vector2.Distance(pos, position) / 10;
                for (int i = 0; i < (item.type == ModContent.ItemType<GlowstalkItem>() ? 2 : 1); i++)
                {
                    Dust dust = Dust.NewDustPerfect(pos, DustID.GoldCoin);
                    dust.noGravity = true;
                    dust.fadeIn = 1.1f;
                    dust.velocity = dir;
                }
                if (Main.rand.NextBool(20))
                    Gore.NewGore(new EntitySource_Misc("CellAbsorption"), pos, dir / 4, Main.rand.Next(16, 18), 1f);

                if (absorbTimer >= 120)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath7, item.position);
                    for (int j = 0; j < 40; j++)
                    {
                        Dust.NewDust(item.position, item.width, item.height, DustID.MagicMirror, 0, 0, 150, default(Color), 1.2f);
                        if (j % 2 == 0)
                        {
                            int d = Dust.NewDust(item.position, item.width, item.height, DustID.GoldCoin, 0, 0, 150, default(Color), 1.2f);
                            Main.dust[d].noGravity = true;
                            Main.dust[d].fadeIn = 1.5f;
                            Main.dust[d].velocity *= 3;
                        }
                    }
                    for (int k = 0; k < 5; k++)
                    {
                        Gore.NewGore(new EntitySource_Misc("CellAbsorption"), item.position, Vector2.Zero, Main.rand.Next(16, 18), 1f);
                    }

                    ValidAbsorbableItems.TryGetValue(item.type, out int value);
                    storedRadiance += Math.Min(value * mult, maxRadiance - storedRadiance);

                    item.stack -= 1;
                    if (item.stack <= 0)
                        item.TurnToAir();

                    absorbTimer = 0;
                    return;
                }
            }
        }
        public bool HasRadianceAdjustingTexture => extraTextures is not null && extraTextures.ContainsKey("RadianceAdjusting");
        public bool HasMiniTexture => extraTextures is not null && extraTextures.ContainsKey("RadianceAdjusting");
        public virtual void UpdateContainer(IInterfaceableRadianceCell tileEntity) { }

        public override ModItem Clone(Item newItem)
        {
            BaseContainer item = base.Clone(newItem) as BaseContainer;
            item.storedRadiance = storedRadiance;
            return item;
        }

        public override void SaveData(TagCompound tag)
        {
            if (storedRadiance > 0)
                tag["storedRadiance"] = storedRadiance;
        }

        public override void LoadData(TagCompound tag)
        {
            storedRadiance = tag.Get<float>("storedRadiance");
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(storedRadiance);
        }

        public override void NetReceive(BinaryReader reader)
        {
            storedRadiance = reader.ReadSingle();
        }
    }
}