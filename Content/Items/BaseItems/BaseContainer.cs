    using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Content.Tiles;
using Radiance.Core;
using Radiance.Core.Interfaces;
using Radiance.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Radiance.Content.Items.BaseItems
{
    public abstract class BaseContainer : ModItem, IPedestalItem
    {
        public BaseContainer(Texture2D radianceAdjustingTexture, Texture2D miniTexture, float maxRadiance, ContainerMode mode, ContainerQuirk quirk)
        {
            RadianceAdjustingTexture = radianceAdjustingTexture;
            MiniTexture = miniTexture;
            this.maxRadiance = maxRadiance;
            this.mode = mode;
            this.quirk = quirk;
        }

        public float currentRadiance = 0;
        public float maxRadiance;
        public ContainerMode mode;
        public ContainerQuirk quirk;
        public Texture2D RadianceAdjustingTexture;
        public Texture2D MiniTexture;

        public enum ContainerMode
        {
            InputOutput,
            InputOnly,
            OutputOnly
        }

        public enum ContainerQuirk
        {
            Standard, //Completely standard behavior.
            Leaking, //Passively leaks Radiance.
            Absorbing, //Gains 20% extra Radiance from absorbed stars.
            CantAbsorb, //Cannot absorb stars.
            CantAbsorbNonstandardTooltip //Cannot absorb stars + Nonstandard Tooltip
        }

        public Color aoeCircleColor => CommonColors.RadianceColor1;
        public float aoeCircleRadius => 100;

        public int absorbTimer = 0;
        public int transformTimer = 0;

        public Dictionary<int, int> ValidAbsorbableItems = new()
        {
            { ItemID.FallenStar, 20 },
            { ModContent.ItemType<GlowtusItem>(), 12 },
        };

        public override void UpdateInventory(Player player)
        {
            switch (quirk)
            {
                case ContainerQuirk.Leaking:
                    LeakRadiance();
                    break;
            }
        }

        public override void PostUpdate()
        {
            switch (quirk)
            {
                case ContainerQuirk.Leaking:
                    LeakRadiance();
                    break;
            }
            float radianceCharge = Math.Min(currentRadiance, maxRadiance);
            float fill = radianceCharge / maxRadiance;
            float strength = 0.4f;
            if (mode == ContainerMode.InputOutput)
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
                fill * RadianceUtils.SineTiming(20)).ToVector3());
            if (quirk != ContainerQuirk.CantAbsorb)
                AbsorbStars(Item.Center);
            if (mode != ContainerMode.InputOnly)
                FlareglassCreation(Item.Center);
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            if (RadianceAdjustingTexture != null)
            {
                Texture2D texture = TextureAssets.Item[Item.type].Value;
                float radianceCharge = Math.Min(currentRadiance, maxRadiance);
                float fill = radianceCharge / maxRadiance;

                Main.EntitySpriteDraw
                (
                    RadianceAdjustingTexture,
                    new Vector2
                    (
                        Item.Center.X - Main.screenPosition.X,
                        Item.Center.Y - Main.screenPosition.Y
                    ),
                    null,
                    Color.Lerp(CommonColors.RadianceColor1 * fill, CommonColors.RadianceColor2 * fill, RadianceUtils.SineTiming(5) * fill),
                    rotation,
                    texture.Size() * 0.5f,
                    scale,
                    SpriteEffects.None,
                    0
                );
            }
        }

        public void OnPedestal(PedestalTileEntity pte)
        {
            if (quirk == ContainerQuirk.Leaking)
                LeakRadiance();

            pte.GetRadianceFromItem();
        }

        public void PedestalEffect(PedestalTileEntity pte)
        {
            Vector2 centerOffset = new Vector2(-16, -16);
            Vector2 yCenteringOffset = new(0, -TextureAssets.Item[Item.type].Value.Height);
            Vector2 vector = RadianceUtils.GetMultitileWorldPosition(pte.Position.X, pte.Position.Y) - centerOffset + yCenteringOffset;

            if (quirk != ContainerQuirk.CantAbsorb && quirk != ContainerQuirk.CantAbsorbNonstandardTooltip)
                AbsorbStars(vector + (Vector2.UnitY * 5 * RadianceUtils.SineTiming(30) - yCenteringOffset / 5));
            if (mode != ContainerMode.InputOnly)
                FlareglassCreation(vector + (Vector2.UnitY * 5 * RadianceUtils.SineTiming(30) - yCenteringOffset / 5));
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (RadianceAdjustingTexture != null)
            {
                float radianceCharge = Math.Min(currentRadiance, maxRadiance);
                float fill = radianceCharge / maxRadiance;

                float slotScale = 1f;
                if (frame.Width > 32 || frame.Height > 32)
                {
                    if (frame.Width > frame.Height)
                        slotScale = 32f / frame.Width;
                    else
                        slotScale = 32f / frame.Height;
                }
                slotScale *= Main.inventoryScale;
                spriteBatch.Draw(
                    RadianceAdjustingTexture,
                    position,
                    null,
                    Color.Lerp(CommonColors.RadianceColor1 * fill, CommonColors.RadianceColor2 * fill, fill * RadianceUtils.SineTiming(5)),
                    0,
                    RadianceAdjustingTexture.Size() / 2,
                    slotScale,
                    SpriteEffects.None,
                    0);
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TooltipLine detailsLine = new(Mod, "RadianceCellDetails", "Stores Radiance within itself");

            if (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift))
            {
                if (quirk != ContainerQuirk.CantAbsorb && quirk != ContainerQuirk.CantAbsorbNonstandardTooltip)
                    detailsLine.Text += "\nConverts nearby Fallen Stars into Radiance";

                if (mode == ContainerMode.InputOutput && quirk != ContainerQuirk.CantAbsorbNonstandardTooltip)
                    detailsLine.Text += "\nWorks when dropped on the ground or placed upon a Pedestal\nRadiance can be extracted and distributed when placed on a Pedestal as well";
            }
            else
            {
                detailsLine.Text = "[c/707070:-Hold SHIFT for Radiance Cell information-]";
            }
            if (quirk == ContainerQuirk.Leaking)
                detailsLine.Text += "\nPassively leaks a small amount of Radiance into the atmosphere";
            tooltips.Insert(tooltips.FindIndex(x => x.Name == "Tooltip0" && x.Mod == "Terraria"), detailsLine);

            TooltipLine meterLine = new(Mod, "RadianceMeter", "        ."); //it works
            tooltips.Insert(tooltips.FindIndex(x => x.Name == "Tooltip0" && x.Mod == "Terraria") + 1, meterLine);
        }

        public override void PostDrawTooltipLine(DrawableTooltipLine line)
        {
            if (line.Name == "RadianceMeter")
            {
                Texture2D meterTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/ItemRadianceMeter").Value;
                RadianceDrawing.DrawHorizontalRadianceBar(new Vector2(line.X + meterTexture.Width / 2, line.Y + meterTexture.Height / 2) - Vector2.UnitY * 2, maxRadiance, currentRadiance);
            }
        }

        public void FlareglassCreation(Vector2 position)
        {
            Item item = new();
            if (currentRadiance >= 5)
            {
                for (int i = 0; i < Main.maxItems; i++)
                {
                    if (Main.item[i] != null && Main.item[i].active && Vector2.Distance(Main.item[i].Center, position) < 90 && ((Main.item[i].type >= ItemID.Sapphire && Main.item[i].type <= ItemID.Diamond) || Main.item[i].type == ItemID.Amber))
                    {
                        item = Main.item[i];
                        break;
                    }
                }
                if (!item.IsAir)
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
                        currentRadiance -= 5;
                        return;
                    }
                }
            }
        }

        public void AbsorbStars(Vector2 position)
        {
            Item item = new();
            int val = 0;
            float mult = quirk == ContainerQuirk.Absorbing ? 1.25f : 1;

            for (int i = 0; i < Main.maxItems; i++)
            {
                if (Main.item[i] != null && Main.item[i].active && Vector2.Distance(Main.item[i].Center, position) < 90 && ValidAbsorbableItems.TryGetValue(Main.item[i].type, out int value))
                {
                    val = value;
                    item = Main.item[i];
                    break;
                }
            }
            if (!item.IsAir)
            {
                absorbTimer += (item.type == ModContent.ItemType<GlowtusItem>() ? 2 : 1);
                Vector2 pos = item.Center + new Vector2(Main.rand.NextFloat(-item.width, item.width), Main.rand.NextFloat(-item.height, item.height)) / 2;
                Vector2 dir = Utils.DirectionTo(pos, position) * Vector2.Distance(pos, position) / 10;
                for (int i = 0; i < (item.type == ModContent.ItemType<GlowtusItem>() ? 2 : 1); i++)
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

                    item.stack -= 1;
                    if (item.stack <= 0)
                        item.TurnToAir();
                    currentRadiance += Math.Min(val * mult, maxRadiance - currentRadiance);
                    absorbTimer = 0;
                    return;
                }
            }
        }

        public void LeakRadiance()
        {
            float leakValue = 0.002f;
            if (currentRadiance != 0)
                currentRadiance -= Math.Min(currentRadiance, leakValue);
        }

        public override ModItem Clone(Item newItem)
        {
            BaseContainer item = base.Clone(newItem) as BaseContainer;
            item.currentRadiance = currentRadiance;
            return item;
        }

        public override void SaveData(TagCompound tag)
        {
            if (currentRadiance > 0)
                tag["currentRadiance"] = currentRadiance;
        }

        public override void LoadData(TagCompound tag)
        {
            currentRadiance = tag.Get<float>("currentRadiance");
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(currentRadiance);
        }

        public override void NetReceive(BinaryReader reader)
        {
            currentRadiance = reader.ReadSingle();
        }
    }
}