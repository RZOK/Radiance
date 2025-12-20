using Terraria.UI;
using Microsoft.Xna.Framework.Input;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.Tools.Misc;
using Terraria.Map;

namespace Radiance.Content.Items
{
    public class AureolinNotch : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Aureolin Notch");
            Tooltip.SetDefault("Placeholder Text");
            Item.ResearchUnlockCount = 0;
            LookingGlassNotchData.LoadNotchData
               (
               Type,
               new Color(255, 185, 102),
               $"{nameof(Radiance)}/Content/ExtraTextures/LookingGlass/LookingGlass_Point",
               $"{nameof(Radiance)}/Content/ExtraTextures/LookingGlass/LookingGlass_Point_Small",
               MirrorUse,
               ChargeCost
               );
        }
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Orange;
        }
        public void MirrorUse(Player player, LookingGlass lookingGlass)
        {
            AureolinNotch_Player aNPlayer = player.GetModPlayer<AureolinNotch_Player>();
            if (aNPlayer.recallPosition.HasValue)
            {
                lookingGlass.PreRecallParticles(player);
                foreach (Projectile seal in Main.projectile)
                {
                    if (seal.type == ModContent.ProjectileType<AureolinNotch_Seal>())
                        seal.active = false;
                }
                player.Teleport(aNPlayer.recallPosition.Value, 12);
                aNPlayer.recallPosition = null;

                lookingGlass.PostRecallParticles(player);
            }
            else
            {
                lookingGlass.PreRecallParticles(player);
                Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<AureolinNotch_Seal>(), 0, 0, player.whoAmI);
                player.GetModPlayer<AureolinNotch_Player>().recallPosition = player.position;
            }
                
        }

        public int ChargeCost(int identicalCount)
        {
            return 10;
        }
    }
    public class AureolinNotch_Seal : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 36000;
            Projectile.alpha = 255;
        }
        public override void AI()
        {
            if (Projectile.alpha > 0)
                Projectile.alpha -= 3;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/Items/AureolinNotch_Seal").Value;
            Texture2D outlineTex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/Items/AureolinNotch_SealOutline").Value;
            Rectangle centerSealRect = new Rectangle(10, 0, 24, 24);
            Rectangle innerCircleRect= new Rectangle(0, 26, 44, 44);

            float alphaCompletion = (255f - Projectile.alpha) / 255f;
            Color color = new Color(255, 255, 255, 0) * alphaCompletion;
            if (Projectile.owner != Main.LocalPlayer.whoAmI)
            {
                color *= 0.5f;
            }
            float easedCompletion = EaseOutExponent(alphaCompletion, 4f);
            float innerRingScale = MathF.Pow(Lerp(2f, 1f, easedCompletion), 2f);
            float outerRingScale = MathF.Pow(innerRingScale, 1.2f) * 1.1f;
            float fullRotation = 600f;
            float rotation = Lerp(0, TwoPi, (Main.GameUpdateCount + (300f * (1f - easedCompletion))) % fullRotation / fullRotation);

            for (int i = -1; i <= 1; i += 2)
            {
                Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition - Vector2.UnitY * 1f + Vector2.UnitX * 4f * i, centerSealRect, color * 0.6f * 0.1f, 0, centerSealRect.Size() / 2f, 1f * 1.3f, SpriteEffects.None, 0);
                Main.spriteBatch.Draw(outlineTex, Projectile.Center - Main.screenPosition - Vector2.UnitY * 1f + Vector2.UnitX * 4f * i, centerSealRect, color * 0.35f * 0.1f, 0, centerSealRect.Size() / 2f, 1f * 1.3f, SpriteEffects.None, 0);
                Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + Vector2.UnitX * 4f * i, innerCircleRect, color * 0.6f * 0.1f, rotation, innerCircleRect.Size() / 2f, innerRingScale * 0.85f * 1.3f, SpriteEffects.None, 0);
                Main.spriteBatch.Draw(outlineTex, Projectile.Center - Main.screenPosition + Vector2.UnitX * 4f * i, innerCircleRect, color * 0.35f * 0.1f, rotation, innerCircleRect.Size() / 2f, innerRingScale * 0.85f * 1.3f, SpriteEffects.None, 0);
                Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + Vector2.UnitX * 4f * i, innerCircleRect, color * 0.6f * 0.1f, -rotation, innerCircleRect.Size() / 2f, outerRingScale * 1.3f, SpriteEffects.None, 0);
                Main.spriteBatch.Draw(outlineTex, Projectile.Center - Main.screenPosition + Vector2.UnitX * 4f * i, innerCircleRect, color * 0.35f * 0.1f, -rotation, innerCircleRect.Size() / 2f, outerRingScale * 1.3f, SpriteEffects.None, 0);
            }

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition - Vector2.UnitY * 1f, centerSealRect, color * 0.6f, 0, centerSealRect.Size() / 2f, 1f, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(outlineTex, Projectile.Center - Main.screenPosition - Vector2.UnitY * 1f, centerSealRect, color * 0.35f, 0, centerSealRect.Size() / 2f, 1f, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, innerCircleRect, color * 0.6f, rotation, innerCircleRect.Size() / 2f, innerRingScale * 0.85f, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(outlineTex, Projectile.Center - Main.screenPosition, innerCircleRect, color * 0.35f, rotation, innerCircleRect.Size() / 2f, innerRingScale * 0.85f, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, innerCircleRect, color * 0.6f, -rotation, innerCircleRect.Size() / 2f, outerRingScale, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(outlineTex, Projectile.Center - Main.screenPosition, innerCircleRect, color * 0.35f, -rotation, innerCircleRect.Size() / 2f, outerRingScale, SpriteEffects.None, 0);

            
            return false;
        }   
    }
    public class AureolinNotch_Player : ModPlayer
    {
        public Vector2? recallPosition;
    }
    public class AureolinNotch_MapLayer : ModMapLayer
    {
        private Texture2D tex;
        public override void Load()
        {
            tex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/Items/AureolinNotch_MapIcon", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        }
        public override void Draw(ref MapOverlayDrawContext context, ref string text)
        {
            AureolinNotch_Player player = Main.LocalPlayer.GetModPlayer<AureolinNotch_Player>();
            if (player.recallPosition.HasValue)
            { 
                if (context.Draw(tex, player.recallPosition.Value.ToTileCoordinates().ToVector2(), Color.White, new SpriteFrame(1, 1, 0, 0), 1f, 1f, Alignment.Center).IsMouseOver)
                {
                    text = "Aureolin Seal";
                }
            }
        }
    }
}