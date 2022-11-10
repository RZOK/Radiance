using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Common.Globals;
using Radiance.Content.Items.TileItems;
using Radiance.Utils;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Radiance.Content.Items.BaseItems
{
    public abstract class BaseContainer : ModItem
    {
        public abstract float MaxRadiance { get; set; }
        public abstract float CurrentRadiance { get; set; }

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
        }

        public abstract ContainerModeEnum ContainerMode { get; set; }
        public abstract ContainerQuirkEnum ContainerQuirk { get; set; }
#nullable enable
        public virtual Texture2D? RadianceAdjustingTexture { get; set; }
#nullable disable
        public int absorbTimer = 0;

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
                        Item.position.X - Main.screenPosition.X + Item.width * 0.5f,
                        Item.position.Y - Main.screenPosition.Y + Item.height * 0.5f
                    ),
                    null,
                    Color.Lerp(Radiance.RadianceColor1 * fill, Radiance.RadianceColor2 * fill, fill * (float)MathUtils.sineTiming(5)),
                    rotation + MathHelper.Pi,
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
                    Main.inventoryScale, 
                    SpriteEffects.None, 
                    0);
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string radLine = "Stores Radiance within itself";
            foreach (TooltipLine tooltip  in tooltips)
            {
                if (tooltip.Name == "Tooltip0")
                {
                    if(ContainerQuirk != ContainerQuirkEnum.CantAbsorb) radLine += "\nConverts nearby Fallen Stars into Radiance";
                    if(ContainerMode == ContainerModeEnum.InputOutput) radLine += "\nWorks when dropped on the ground or placed upon a Pedestal\nRadiance can be extracted and distributed when placed in a Pedestal as well";
                    if(ContainerQuirk == ContainerQuirkEnum.Leaking) radLine += "\nPassively leaks a small amount of Radiance into the atmosphere";    
                    tooltip.Text = radLine;
                }
            }
            TooltipLine line = new TooltipLine(Mod, "RadianceMeter", ".");
            tooltips.Add(line);
        }
        public override void PostDrawTooltipLine(DrawableTooltipLine line)
        {
            if(line.Name == "RadianceMeter")
            {
                RadianceDrawing.DrawHorizontalRadianceBar(new Vector2(line.X, (line.Y + 1)), "Item");
            }
        }
        public void AbsorbStars(Vector2 position)
        {
            Item item = new Item();
            int val = 0;
            float mult = ContainerQuirk == ContainerQuirkEnum.Absorbing ? 1.2f : 1;

            for (int i = 0; i < Main.maxItems; i++)
            {
                if (Main.item[i] != null && Main.item[i].active && Vector2.Distance(Main.item[i].Center, position) < 360 && ValidAbsorbableItems.TryGetValue(Main.item[i].type, out int value))
                {
                    val = value;
                    item = Main.item[i];
                    absorbTimer++;
                    break;
                }
            }
            Main.NewText(absorbTimer);
            if (item.type != ItemID.None)
            {
                if (absorbTimer >= 60)
                {
                    if (item.stack > 1)
                        item.stack -= 1;
                    else
                        item.active = false;
                    CurrentRadiance += Math.Min(val * mult, MaxRadiance - CurrentRadiance);
                    absorbTimer = 0;
                    return;
                }
            }
            else
            {
                item = default;
                absorbTimer = 0;
            }
        }

        public void LeakRadiance()
        {
            float leakValue = 0.002f;
            if (CurrentRadiance != 0) CurrentRadiance -= Math.Min(CurrentRadiance, leakValue);
        }
        public override void SaveData(TagCompound tag)
        {
            tag["CurrentRadiance"] = CurrentRadiance;
        }
        public override void LoadData(TagCompound tag)
        {
            CurrentRadiance = tag.Get<float>("CurrentRadiance");
        }
    }
}