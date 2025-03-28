using Radiance.Content.Items.Materials;
using Radiance.Content.Particles;
using Radiance.Core.Systems;
using Radiance.Core.Systems.ParticleSystems;

namespace Radiance.Content.Items.Tools.Misc
{
    public class CalcificationPowder : ModItem, ITransmutationRecipe
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Calcification Powder");
            Tooltip.SetDefault("Petrifies plants when thrown");
            Item.ResearchUnlockCount = 10;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 9999;
            Item.rare = ItemRarityID.Blue;
            Item.useTime = Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<CalcificationPowderDust>();
            Item.shootSpeed = 4;
            Item.consumable = true;
            Item.UseSound = SoundID.Item1;
        }
        public void AddTransmutationRecipe(TransmutationRecipe recipe)
        {
            recipe.inputItems = new int[] { ItemID.PurificationPowder, ItemID.VilePowder, ItemID.ViciousPowder };
            recipe.requiredRadiance = 5;
        }
    }

    #region Projectile

    public class CalcificationPowderDust : ModProjectile
    {
        public override string Texture => "Radiance/Content/ExtraTextures/Blank";
        public bool spawnedDust
        {
            get => Projectile.ai[0] == 1;
            set => Projectile.ai[0] = value.ToInt();
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Calcification Powder");
        }

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1; 
            Projectile.timeLeft = 30;
        }

        public override void AI()
        {
            if(!spawnedDust)
            {
                for (int h = 0; h < 30; h++)
                {
                    Vector2 position = Projectile.Center + Main.rand.NextVector2Circular(8, 8);
                    Vector2 velocity = Vector2.Normalize(Projectile.velocity).RotatedByRandom(0.6f) * 12 * Main.rand.NextFloat(0.1f, 1);
                    WorldParticleSystem.system.AddParticle(new CalcificationSprinkle(position, velocity, Main.rand.Next(60, 100), 0, new Color(100, Main.rand.Next(100, 170), Main.rand.Next(150, 255)), Main.rand.NextFloat(0.7f, 1f)));
                }
                spawnedDust = true;
            }

            Projectile.velocity *= 0.98f;

            Rectangle clampBox = new Rectangle(0, 0, Main.maxTilesX, Main.maxTilesY);
            Point center = new Point((int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16));
            int radius = 2;
            int leftBound = Utils.Clamp(center.X - radius, clampBox.Left, clampBox.Right);
            int rightBound = Utils.Clamp(center.X + radius + 1, clampBox.Left, clampBox.Right);
            int topBound = Utils.Clamp(center.Y - radius, clampBox.Top, clampBox.Bottom);
            int bottomBound = Utils.Clamp(center.Y + radius + 1, clampBox.Top, clampBox.Bottom);

            for (int i = leftBound; i < rightBound; i++)
            {
                for (int j = topBound; j < bottomBound; j++)
                {
                    Tile tile = Framing.GetTileSafely(i, j);
                    if (tile.TileType == 71 && (tile.TileFrameX / 18) > 1)
                    {
                        WorldGen.KillTile(i, j, false, false, true);
                        for (int h = 0; h < 9; h++)
                        {
                            Dust dust = Main.dust[Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, DustID.BlueCrystalShard)];
                            dust.noGravity = true;
                            dust.velocity *= 0.3f;
                            dust.scale = 1.7f;
                            dust.fadeIn = 1.1f;
                        }
                        SoundEngine.PlaySound(SoundID.Tink, new Vector2(i * 16, j * 16));

                        Item.NewItem(new EntitySource_Misc(nameof(CalcificationPowder)), i * 16, j * 16, 16, 16, ModContent.ItemType<PetrifiedCrystal>());
                    }
                }
            }
        }
        public override bool? CanDamage() => false;
    }

    #endregion Projectile
}