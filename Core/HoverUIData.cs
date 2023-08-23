using Microsoft.Xna.Framework.Input;
using ReLogic.Graphics;
using System.Collections.Generic;
using Terraria.UI;

namespace Radiance.Core
{
    public class HoverUIData
    {
        public ImprovedTileEntity entity;
        public Vector2 position;
        public List<HoverUIElement> elements;
        public HoverUIData(ImprovedTileEntity entity, Vector2 position, params HoverUIElement[] elements)
        {
            this.entity = entity;
            this.position = position;
            this.elements = elements.ToList();
            this.elements.ForEach(x => x.parent = this);
        }
    }

    public abstract class HoverUIElement : ICloneable
    {
        public HoverUIData parent;
        public Vector2 elementPosition;
        public Vector2 targetPosition;
        public const float timerMax = 20;
        public float timer;
        public string name;
        public bool updateTimer = false;
        public Vector2 realDrawPosition => elementPosition - Main.screenPosition;

        public float timerModifier => EaseOutCirc(timer / 20);
        public Vector2 basePosition => parent.position;

        public abstract void Draw(SpriteBatch spriteBatch);
        public HoverUIElement(string name) => this.name = name;
        public void Update()
        {
            elementPosition = Vector2.Lerp(basePosition, basePosition + targetPosition, timerModifier);
        }

        public object Clone() => MemberwiseClone();
    }

    public class TextUIElement : HoverUIElement
    {
        public string text;
        public Color color;

        public TextUIElement(string name, string text, Color color, Vector2 targetPosition) : base(name)
        {
            this.text = text;
            this.color = color;
            this.targetPosition = targetPosition;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float scale = Math.Clamp(timerModifier + 0.5f, 0.5f, 1);
            DynamicSpriteFont font = FontAssets.MouseText.Value;
            Utils.DrawBorderStringFourWay(spriteBatch, font, text, realDrawPosition.X, realDrawPosition.Y, color * timerModifier, CommonColors.GetDarkColor(color, 6) * timerModifier, font.MeasureString(text) / 2, scale);
        }
    }

    public class CircleUIElement : HoverUIElement
    {
        public float radius;
        public Color color;

        public CircleUIElement(string name, float radius, Color color) : base(name)
        {
            this.radius = radius;
            this.color = color;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float wackyModifier = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift) ? 0 : (float)(SineTiming(30) * radius / 250);
            RadianceDrawing.DrawCircle(basePosition, new Color(color.R, color.G, color.B, (byte)(255 * Math.Max(0.2f, timer * 3 / 255))), radius * timerModifier + wackyModifier, RadianceDrawing.SpriteBatchData.WorldDrawingData);
        }
    }

    public class SquareUIElement : HoverUIElement
    {
        public float halfWidth;
        public Color color;

        public SquareUIElement(string name, float halfWidth, Color color) : base(name) 
        {
            this.halfWidth = halfWidth;
            this.color = color;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float wackyModifier = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift) ? 0 : (float)(SineTiming(30) * halfWidth / 250);
            RadianceDrawing.DrawSquare(basePosition, new Color(color.R, color.G, color.B, (byte)(255 * Math.Max(0.2f, timer * 3 / 255))), halfWidth * timerModifier + wackyModifier + 11f, RadianceDrawing.SpriteBatchData.WorldDrawingData);
        }
    }

    public class ItemUIElement : HoverUIElement
    {
        public int item;
        public int stack;
        public ItemUIElement(string name, int item, Vector2 targetPosition, int stack = 1) : base(name)
        {
            this.item = item;
            this.targetPosition = targetPosition;
            this.stack = stack;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D softGlow = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SoftGlowNoBG").Value;
            Rectangle drawBox = Item.GetDrawHitbox(item, null);
            Texture2D texture = TextureAssets.Item[item].Value;
            Vector2 itemSize = new Vector2(drawBox.Width, drawBox.Height);

            float scale = Math.Clamp(timerModifier + 0.5f, 0.5f, 1);

            spriteBatch.Draw(softGlow, realDrawPosition, null, Color.Black * 0.25f, 0, softGlow.Size() / 2, itemSize.Length() / 80, 0, 0);
            RadianceDrawing.DrawHoverableItem(spriteBatch, item, realDrawPosition, stack, null, hoverable: false);
            //spriteBatch.Draw(texture, realDrawPosition, drawBox, Color.White * timerModifier, 0, new Vector2(drawBox.Width, drawBox.Height) / 2, scale, SpriteEffects.None, 0);
        }
    }

    public class TextureUIElement : HoverUIElement
    {
        public Texture2D texture;

        public TextureUIElement(string name, Texture2D texture, Vector2 targetPosition) : base(name)
        {
            this.texture = texture;
            this.targetPosition = targetPosition;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, realDrawPosition, null, Color.White * timerModifier, 0, texture.Size() / 2, timerModifier, SpriteEffects.None, 0);
        }
    }
    public class StabilityBarElement : HoverUIElement
    {
        public float stability;
        public float idealStability;

        public StabilityBarElement(string name, float stability, float idealStability, Vector2 targetPosition) : base(name)
        {
            this.stability = stability;
            this.idealStability = idealStability;
            this.targetPosition = targetPosition;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float scale = Math.Clamp(timerModifier + 0.7f, 0.7f, 1);
            Texture2D barTex = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/StabilityBar").Value;
            Texture2D barGlowTex = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/StabilityBarGlow").Value;
            Texture2D arrowTex = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/StabilityArrow").Value;
            Vector2 floating = Vector2.Zero;
            Color color = Color.White;
            if (!Main.keyState.IsKeyDown(Keys.LeftShift) && !Main.keyState.IsKeyDown(Keys.RightShift))
            {
                floating = Vector2.UnitY * 2 * SineTiming(80);
                color *= 0.2f;
            }
            float arrowModifier = (arrowTex.Width / 2 + 2 + Lerp(60, 0, timerModifier));
            if (Math.Abs(1 - stability / idealStability) > 0.1f)
                arrowModifier += SineTiming(40) * 2;
            else
                spriteBatch.Draw(barGlowTex, realDrawPosition + floating, null, new Color(0, 255, 255) * ((float)color.A / 255) * timerModifier, 0, barGlowTex.Size() / 2, scale, SpriteEffects.None, 0);

            
            spriteBatch.Draw(barTex, realDrawPosition + floating, null, color * timerModifier * 0.8f, 0, barTex.Size() / 2, scale, SpriteEffects.None, 0);

          

            Vector2 unstableModifier = Vector2.Zero;
            if (stability >= idealStability * 2)
                unstableModifier += Main.rand.NextVector2Circular(2, 2);

            spriteBatch.Draw(arrowTex, realDrawPosition + floating + Vector2.UnitY * Lerp(40, -40, Math.Min(stability / (idealStability * 2), 1)) - Vector2.UnitX * arrowModifier + unstableModifier, null, color * timerModifier * 0.9f, 0, arrowTex.Size() / 2, scale, SpriteEffects.None, 0);
            spriteBatch.Draw(arrowTex, realDrawPosition + floating + Vector2.UnitY * Lerp(40, -40, Math.Min(stability / (idealStability * 2), 1)) + Vector2.UnitX * arrowModifier + unstableModifier, null, color * timerModifier * 0.9f, 0, arrowTex.Size() / 2, scale, SpriteEffects.FlipHorizontally, 0);

        }
    }
    //public class StabilityCircleUIElement : HoverUIElement
    //{
    //    public float radius;
    //    public float stability;
    //    public float idealStability;

    //    public StabilityCircleUIElement(float radius, float stability, float idealStability)
    //    {
    //        this.radius = radius;
    //        this.stability = stability;
    //        this.idealStability = idealStability;
    //    }

    //    public override void Draw(SpriteBatch spriteBatch)
    //    {
    //        Vector4 color = (new Color(100, 200, 255) * timerModifier).ToVector4() * 0.9f;
    //        if (!Main.keyState.IsKeyDown(Keys.LeftShift) && !Main.keyState.IsKeyDown(Keys.RightShift))
    //            color *= 0.5f;
    //        float ringRotation = -MathHelper.Lerp(2, 0, timerModifier);

    //        float wackyModifier = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift) ? 0 : (float)(RadianceUtils.SineTiming(30) * radius / 250);
    //        RadianceDrawing.DrawStabilityCircle(basePosition, radius, RadianceUtils.EaseOutCirc(timer / timerMax) - 0.2f, color, ringRotation);
    //        Texture2D tex = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/StabilityArrow").Value;

    //        float rotation = ringRotation - MathHelper.Lerp(MathHelper.PiOver2 * 0.2f, -MathHelper.PiOver2 * 0.2f - MathHelper.Pi, Math.Min(stability, idealStability * 2) / (idealStability * 2));
    //        spriteBatch.Draw(tex, basePosition - Main.screenPosition - Vector2.UnitX.RotatedBy(rotation) * (radius + MathHelper.Lerp(tex.Height * 3, 0, timerModifier)), null, Color.White * color.W * 1.8f, rotation - MathHelper.PiOver2, tex.Size() / 2, timerModifier, SpriteEffects.None, 0);
    //    }
    //}
    public class RadianceBarUIElement : HoverUIElement
    {
        public float current;
        public float max;

        public RadianceBarUIElement(string name, float current, float max, Vector2 targetPosition) : base(name)
        {
            this.current = current;
            this.max = max;
            this.targetPosition = targetPosition;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            RadianceDrawing.DrawHorizontalRadianceBar(realDrawPosition + new Vector2(2 * SineTiming(33), -2 * SineTiming(55)), max, current, this);
        }
    }
    public class ItemImprintUIElement : HoverUIElement
    {
        public ItemImprintData imprintedData;
        public ItemImprintUIElement(string name, ItemImprintData imprintedData, Vector2 targetPosition) : base(name)
        {
            this.imprintedData = imprintedData;
            this.targetPosition = targetPosition;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            int columns = (int)MathF.Ceiling(MathF.Sqrt(imprintedData.imprintedItems.Count));
            int rows = (int)MathF.Ceiling(imprintedData.imprintedItems.Count / (float)columns);

            const int distanceBetweenItems = 36;
            int drawWidth = columns * distanceBetweenItems;
            int drawHeight = rows * distanceBetweenItems;
            int padding = 4;
            DrawRadianceInvBG(spriteBatch, (int)realDrawPosition.X - drawWidth / 2 - padding / 2, (int)realDrawPosition.Y - drawHeight - padding / 2, drawWidth + padding, drawHeight + padding, 0.75f * timerModifier, imprintedData.blacklist ? RadianceInventoryBGDrawMode.ItemImprintBlacklist : RadianceInventoryBGDrawMode.ItemImprint);

            int x = 0;
            int y = 0;
            for (int i = 0; i < imprintedData.imprintedItems.Count; i++)
            {
                if (TryGetItemTypeFromFullName(imprintedData.imprintedItems[i], out int type))
                {
                    Item item = GetItem(type);
                    Vector2 itemPos = new Vector2(realDrawPosition.X - drawWidth / 2 + x * distanceBetweenItems + distanceBetweenItems / 2, realDrawPosition.Y - drawHeight + y * distanceBetweenItems + 26) - Vector2.UnitY * 8 * timerModifier;
                    ItemSlot.DrawItemIcon(item, 0, spriteBatch, itemPos, 1f, 32, Color.White * timerModifier);
                }
                x++;
                if (x == columns)
                {
                    x = 0;
                    y++;
                }
            }
        }
    }
}