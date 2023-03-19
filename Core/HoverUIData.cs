using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Utilities;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.UI.Chat;
using Microsoft.Xna.Framework.Input;

namespace Radiance.Core
{
    public class HoverUIData
    {
        public ModTileEntity entity;
        public Vector2 position;
        public List<HoverUIElement> elements;
        public HoverUIData(ModTileEntity entity, Vector2 position, params HoverUIElement[] elements)
        {
            this.entity = entity;
            this.position = position;
            this.elements = elements.ToList();
            this.elements.ForEach(x => x.parent = this);
        }
    }
    public abstract class HoverUIElement
    {
        public HoverUIData parent;
        public Vector2 elementPosition;
        public Vector2 targetPosition;
        public const float timerMax = 20;
        public float timer;
        public Vector2 realDrawPosition => elementPosition - Main.screenPosition;

        public float timerModifier => RadianceUtils.EaseOutCirc(timer / 20);
        public Vector2 basePosition => parent.position;
        public abstract void Draw(SpriteBatch spriteBatch);
        public void Update()
        {
            elementPosition = Vector2.Lerp(basePosition, basePosition + targetPosition, timerModifier);
        }
    }
    public class TextUIElement : HoverUIElement
    {
        public string text;
        public Color color;
        public TextUIElement(string text, Color color, Vector2 targetPosition)
        {
            this.text = text;
            this.color = color;
            this.targetPosition = targetPosition;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            float scale = Math.Clamp(timerModifier + 0.5f, 0.5f, 1);
            DynamicSpriteFont font = FontAssets.MouseText.Value;
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, text, realDrawPosition, color * timerModifier, 0, font.MeasureString(text) / 2, Vector2.One * scale);
        }
    }
    public class CircleUIElement : HoverUIElement
    {
        public float radius;
        public Color color;
        public CircleUIElement(float radius, Color color)
        {
            this.radius = radius;
            this.color = color;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            float wackyModifier = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift) ? 0 : (float)(RadianceUtils.SineTiming(30) * radius / 250);
            RadianceDrawing.DrawCircle(basePosition, new Color(color.R, color.G, color.B, (byte)(255 * Math.Max(0.2f, timer * 3 / 255))), radius * timerModifier + wackyModifier, RadianceDrawing.DrawingMode.MPAoeCircle);
        }
    }
    public class ItemUIElement : HoverUIElement
    {
        public int item;
        public ItemUIElement(int item, Vector2 targetPosition)
        {
            this.item = item;
            this.targetPosition = targetPosition;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            float scale = Math.Clamp(timerModifier + 0.5f, 0.5f, 1);
            Texture2D texture = TextureAssets.Item[item].Value;
            Main.instance.LoadItem(item);
            spriteBatch.Draw(texture, realDrawPosition, new Rectangle?(Item.GetDrawHitbox(item, null)), Color.White * timerModifier, 0, new Vector2(Item.GetDrawHitbox(item, null).Width, Item.GetDrawHitbox(item, null).Height) / 2, scale, SpriteEffects.None, 0);
        }
    }
    public class TextureUIElement : HoverUIElement
    {
        public Texture2D texture;
        public TextureUIElement(Texture2D texture, Vector2 targetPosition)
        {
            this.texture = texture;
            this.targetPosition = targetPosition;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, realDrawPosition, null, Color.White * timerModifier, 0, texture.Size() / 2, timerModifier, SpriteEffects.None, 0);
        }
    }
    public class RadianceBarUIElement : HoverUIElement
    {
        public float current;
        public float max;
        public RadianceBarUIElement(float current, float max, Vector2 targetPosition)
        {
            this.current = current;
            this.max = max;
            this.targetPosition = targetPosition;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            RadianceDrawing.DrawHorizontalRadianceBar(realDrawPosition + new Vector2(2 * RadianceUtils.SineTiming(33), -2 * RadianceUtils.SineTiming(55)), max, current, this);
        }
    }
}
