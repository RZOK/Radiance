using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Common;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Utils;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Radiance.Content.Items.BaseItems
{
    public abstract class BaseContainer : ModItem
    {
        public abstract float MaxRadiance { get; set; }
        public float CurrentRadiance { get; set; }

        public enum ContainerModeEnum
        {
            InputOutput,
            InputOnly,
            OutputOnly
        }

        public enum ContainerQuirkEnum
        {
            Standard, //Completely standard behavior.
            Leaking, //Passively leaks Radiance.
            Absorbing, //Gains 20% extra Radiance from absorbed stars.
            CantAbsorb, //Cannot absorb stars.
            CantAbsorbNonstandardTooltip //Cannot absorb stars + Nonstandard Tooltip
        }

        public abstract ContainerModeEnum ContainerMode { get; set; }
        public abstract ContainerQuirkEnum ContainerQuirk { get; set; }
#nullable enable
        public virtual Texture2D? RadianceAdjustingTexture { get; set; }
#nullable disable
        public int absorbTimer = 0;
        public int transformTimer = 0;

        public Dictionary<int, int> ValidAbsorbableItems = new()
        {
            { ItemID.FallenStar, 20 },
        };

        public override void UpdateInventory(Player player)
        {
            switch (ContainerQuirk)
            {
                case ContainerQuirkEnum.Leaking:
                    LeakRadiance();
                    break;
            }
            if(ContainerMode != ContainerModeEnum.InputOnly)
            {
                player.GetModPlayer<RadiancePlayer>().maxRadianceOnHand += MaxRadiance;
                player.GetModPlayer<RadiancePlayer>().currentRadianceOnHand += CurrentRadiance;
            }
        }

        public override void PostUpdate()
        {
            switch (ContainerQuirk)
            {
                case ContainerQuirkEnum.Leaking:
                    LeakRadiance();
                    break;
            }
            float radianceCharge = Math.Min(CurrentRadiance, MaxRadiance);
            float fill = radianceCharge / MaxRadiance;
            float strength = 0.4f;
            if (ContainerMode == ContainerModeEnum.InputOutput)
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
                fill * (float)MathUtils.sineTiming(20)).ToVector3());
            if (ContainerQuirk != ContainerQuirkEnum.CantAbsorb) AbsorbStars(Item.Center);
            if(ContainerMode != ContainerModeEnum.InputOnly) FlareglassCreation(Item.Center);
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            if (RadianceAdjustingTexture != null)
            {
                Texture2D texture = TextureAssets.Item[Item.type].Value;
                float radianceCharge = Math.Min(CurrentRadiance, MaxRadiance);
                float fill = radianceCharge / MaxRadiance;

                Main.EntitySpriteDraw
                (
                    RadianceAdjustingTexture,
                    new Vector2
                    (
                        Item.Center.X - Main.screenPosition.X,
                        Item.Center.Y - Main.screenPosition.Y
                    ),
                    null,
                    Color.Lerp(Radiance.RadianceColor1 * fill, Radiance.RadianceColor2 * fill, (float)MathUtils.sineTiming(5) * fill),
                    rotation,
                    texture.Size() * 0.5f,
                    scale,
                    SpriteEffects.None,
                    0
                );
            }
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (RadianceAdjustingTexture != null)
            {
                Texture2D texture = TextureAssets.Item[Item.type].Value;
                float radianceCharge = Math.Min(CurrentRadiance, MaxRadiance);
                float fill = radianceCharge / MaxRadiance;

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
                    Color.Lerp(Radiance.RadianceColor1 * fill, Radiance.RadianceColor2 * fill, fill * (float)MathUtils.sineTiming(5)),
                    0,
                    Vector2.Zero,
                    slotScale,
                    SpriteEffects.None,
                    0);
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string radLine = "Stores Radiance within itself";
            foreach (TooltipLine tooltip in tooltips)
            {
                if (tooltip.Name == "Tooltip0")
                {
                    if (ContainerQuirk != ContainerQuirkEnum.CantAbsorb && ContainerQuirk != ContainerQuirkEnum.CantAbsorbNonstandardTooltip) radLine += "\nConverts nearby Fallen Stars into Radiance";
                    if (ContainerMode == ContainerModeEnum.InputOutput && ContainerQuirk != ContainerQuirkEnum.CantAbsorbNonstandardTooltip) radLine += "\nWorks when dropped on the ground or placed upon a Pedestal\nRadiance can be extracted and distributed when placed in a Pedestal as well";
                    if (ContainerQuirk == ContainerQuirkEnum.Leaking) radLine += "\nPassively leaks a small amount of Radiance into the atmosphere";
                    tooltip.Text = radLine;
                }
            }
            TooltipLine line = new TooltipLine(Mod, "RadianceMeter", "        .");
            tooltips.Add(line);
        }

        public override void PostDrawTooltipLine(DrawableTooltipLine line)
        {
            if (line.Name == "RadianceMeter")
            {
                RadianceDrawing.DrawHorizontalRadianceBar(new Vector2(line.X, (line.Y + 1)), "Item");
            }
        }
        public void FlareglassCreation(Vector2 position)
        {
            Item item = new Item();
            if (CurrentRadiance >= 5)
            {
                for (int i = 0; i < Main.maxItems; i++)
                {
                    if (Main.item[i] != null && Main.item[i].active && Vector2.Distance(Main.item[i].Center, position) < 90 && ((Main.item[i].type >= 177 && Main.item[i].type <= 182) || Main.item[i].type == ItemID.Amber))
                    {
                        item = Main.item[i];
                        break;
                    }
                }
                if (item.type != ItemID.None)
                {
                    transformTimer++;
                    for (int i = 0; i < 2; i++)
                    {
                        Texture2D texture = TextureAssets.Item[Item.type].Value;
                        Vector2 pos = item.Center + new Vector2(Main.rand.NextFloat(-item.width, item.width), Main.rand.NextFloat(-item.height, item.height)) / 2;
                        Vector2 pos2 = position + new Vector2(Main.rand.NextFloat(-Item.width, Item.width), Main.rand.NextFloat(-Item.height, Item.height)) / 2;
                        Vector2 dir = Terraria.Utils.DirectionTo(pos2, pos) * Vector2.Distance(pos, pos2) / 10;
                        Dust dust = Dust.NewDustPerfect(pos2 , DustID.GoldCoin);
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
                        if (item.stack > 1)
                            item.stack -= 1;
                        else
                        {
                            item.active = false;
                            item.type = ItemID.None;
                            item.stack = 0;
                        }
                        transformTimer = 0;
                        CurrentRadiance -= 5;
                        return;
                    }
                }
                else
                    item = default;
            }
        }
        public void AbsorbStars(Vector2 position)
        {
            Item item = new Item();
            int val = 0;
            float mult = ContainerQuirk == ContainerQuirkEnum.Absorbing ? 1.2f : 1;

            for (int i = 0; i < Main.maxItems; i++)
            {
                if (Main.item[i] != null && Main.item[i].active && Vector2.Distance(Main.item[i].Center, position) < 90 && ValidAbsorbableItems.TryGetValue(Main.item[i].type, out int value))
                {
                    val = value;
                    item = Main.item[i];
                    break;
                }
            }
            if (item.type != ItemID.None)
            {
                absorbTimer++;
                Vector2 pos = item.Center + new Vector2(Main.rand.NextFloat(-item.width, item.width), Main.rand.NextFloat(-item.height, item.height)) / 2;
                Vector2 dir = Terraria.Utils.DirectionTo(pos, position) * Vector2.Distance(pos, position) / 10;
                Dust dust = Dust.NewDustPerfect(pos, DustID.GoldCoin);
                dust.noGravity = true;
                dust.fadeIn = 1.1f;
                dust.velocity = dir;
                if(Main.rand.NextBool(20)) Gore.NewGore(new EntitySource_Misc("CellAbsorption"), pos, dir / 4, Main.rand.Next(16, 18), 1f);
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
                        Gore.NewGore(new EntitySource_Misc("CellAbsorption"), item.position, Vector2.Zero, Main.rand.Next(16, 18), 1f);

                    if (item.stack > 1)
                        item.stack -= 1;
                    else
                    {
                        item.active = false;
                        item.type = ItemID.None;
                        item.stack = 0;
                    }
                    CurrentRadiance += Math.Min(val * mult, MaxRadiance - CurrentRadiance);
                    absorbTimer = 0;
                    return;
                }
            }
            else
                item = default;
        }

        public void LeakRadiance()
        {
            float leakValue = 0.002f;
            if (CurrentRadiance != 0) CurrentRadiance -= Math.Min(CurrentRadiance, leakValue);
        }

        public override void SaveData(TagCompound tag)
        {
            if (CurrentRadiance > 0)
                tag["CurrentRadiance"] = CurrentRadiance;
        }

        public override void LoadData(TagCompound tag)
        {
            CurrentRadiance = tag.Get<float>("CurrentRadiance");
        }
    }
}