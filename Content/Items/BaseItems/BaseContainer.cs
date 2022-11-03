using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Common.Globals;
using Radiance.Utils;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Radiance.Content.Items.BaseItems
{
    public abstract class BaseContainer : ModItem
    {
        public abstract float MaxRadiance { get; set; }
        public abstract float CurrentRadiance { get; set; }
        public bool NoExtraTexture = false;

        public enum ContainerQuirkEnum
        {
            Standard, //Completely standard behavior.
            Leaking, //Passively leaks Radiance.
            Absorbing, //Gains 20% extra Radiance from absorbed stars.
            CantAbsorb, //Cannot absorb stars.
        }

        public enum ContainerModeEnum
        {
            InputOutput,
            InputOnly,
            OutputOnly
        }

        public ContainerQuirkEnum ContainerQuirk = ContainerQuirkEnum.Standard;
        public ContainerModeEnum ContainerMode = ContainerModeEnum.InputOutput;

        public Asset<Texture2D> radianceAdjustingTexture;

        public override void Load()
        {
            if (!NoExtraTexture)
            {
                radianceAdjustingTexture = ModContent.Request<Texture2D>(Texture + "Glow"); //GLOWS MUST BE PLACED IN THE SAME DIRECTORY AS THE ITEM
            }
        }

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
            if (ContainerQuirk != ContainerQuirkEnum.CantAbsorb) AbsorbStars();
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            if (NoExtraTexture)
            {
                Texture2D texture;
                texture = TextureAssets.Item[Item.type].Value;
                Main.EntitySpriteDraw
                (
                    (Texture2D)radianceAdjustingTexture,
                    new Vector2
                    (
                        Item.position.X - Main.screenPosition.X + Item.width * 0.5f,
                        Item.position.Y - Main.screenPosition.Y + Item.height - texture.Height * 0.5f
                    ),
                    new Rectangle(0, 0, texture.Width, texture.Height),
                    Color.White,
                    rotation,
                    texture.Size() * 0.5f,
                    scale,
                    SpriteEffects.None,
                    0
                );
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TooltipLine line = new TooltipLine(Mod, "RadianceMeter", ".");
            tooltips.Add(line);
        }
        public override void PostDrawTooltipLine(DrawableTooltipLine line)
        {
            if(line.Name == "RadianceMeter")
            {
                RadianceBarDrawer.DrawHorizontalRadianceBar(new Vector2(line.X, (line.Y + 1)), "Item");
            }
        }
        
        public void AbsorbStars()
        {
            float mult = 1;
            if (ContainerQuirk == ContainerQuirkEnum.Absorbing)
            {
                mult = 1.2f;
            }
        }

        public void LeakRadiance()
        {
            float leakValue = 0.02f;
            if (CurrentRadiance != 0) CurrentRadiance -= Math.Min(CurrentRadiance, leakValue);
        }
    }
}