using Radiance.Content.Particles;
using Radiance.Content.Tiles.Transmutator;
using Radiance.Core.Systems;
using System.ComponentModel;
using System.Diagnostics;
namespace Radiance.Content.Items.ProjectorLenses
{
    public class EntropicLens : ModItem
    {
        public override void Load()
        {
            TransmutatorTileEntity.PostTransmutateItemEvent += ShatterLens;
        }

        private void ShatterLens(TransmutatorTileEntity transmutator, Core.Systems.TransmutationRecipe recipe)
        {
            ProjectorTileEntity projector = transmutator.projector;
            if (projector.LensPlaced.type == ModContent.ItemType<EntropicLens>())
            {
                projector.GetSlot(0).TurnToAir();
                Vector2 pos = projector.TileEntityWorldCenter() - Vector2.UnitY * 12f;
                Item.NewItem(new EntitySource_DropAsItem(null, nameof(EntropicLens)), pos, new Item(ModContent.ItemType<EntropicLens_Shattered>()));
                SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/Shatter"), pos);
                int particleCount = 8;
                for (int i = 0; i < particleCount; i++)
                {
                    ParticleSystem.AddParticle(new EntropicLens_Shards(pos + Main.rand.NextVector2FromRectangle(new Rectangle(-1, -4, 2, 8)), Vector2.UnitX.RotatedBy(Main.rand.NextFloat(-Pi)) * Main.rand.NextFloat(1f, 2f), Main.rand.Next(45, 85), 0.8f));
                }
            }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Entropic Lens");
            Tooltip.SetDefault("Provides a 10% discount to Radiance consumption for Transmutations\nShatters on use");
            Item.ResearchUnlockCount = 1;

            ProjectorLensData.AddProjectorLensData(Name, Type, DustID.PurpleCrystalShard, Texture + "_Transmutator", null, (x) => x.transmutator.queuedDiscounts.Add(0.1f));
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 40;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(0, 0, 10);
            Item.rare = ItemRarityID.Green;
        }
    }
    public class EntropicLens_Shattered : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shattered Entropic Lens");
            Tooltip.SetDefault("");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 40;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = 0;
            Item.rare = ItemRarityID.Gray;
        }
    }
    public class EntropicLens_Shards : Particle
    {
        public override string Texture => "Radiance/Content/Items/ProjectorLenses/EntropicLens_Shards";

        public Rectangle frame => variant switch
        {
            1 => new Rectangle(0, 12, 8, 10),
            2 => new Rectangle(0, 24, 8, 12),
            _ => new Rectangle(0, 0, 8, 10)
        };

        public int variant;

        public EntropicLens_Shards(Vector2 position, Vector2 velocity, int maxTime, float scale = 1)
        {
            this.position = position;
            this.maxTime = timeLeft = maxTime;
            this.scale = scale;
            this.velocity = velocity;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Regular;
            variant = Main.rand.Next(3);
            rotation = Main.rand.NextFloat(Pi);
        }

        public override void Update()
        {
            velocity.Y += 0.08f;
            color = Lighting.GetColor(position.ToTileCoordinates());
            rotation += velocity.X / 5f;

            Vector2 collision = Collision.TileCollision(position - frame.Size() / 2f * scale, velocity, (int)(frame.Width * scale), (int)(frame.Height * scale), false);
            if (velocity != collision)
                collision.X *= 0.9f;

            velocity = collision;
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, drawPos, frame, color * MathF.Pow(1f - Progress, 0.45f), rotation, frame.Size() / 2, scale, 0, 0);
        }
    }
}