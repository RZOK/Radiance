using Radiance.Content.Particles;
using Radiance.Content.Tiles.Transmutator;
using Radiance.Core.Systems;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Terraria.Localization;
using static Radiance.Core.PotionColors;

namespace Radiance.Content.Items.ProjectorLenses
{
    public partial class LivingLens : ModItem
    {
        internal const int ABSORPTION_RADIUS = 600;
        private static LocalizedText LivingLensTransmutationKey = LanguageManager.Instance.GetOrRegister($"Mods.{nameof(Radiance)}.Transmutation.TransmutationRequirements.LivingLensRequirement");

        [GeneratedRegex(@"([\w]*)_([\d]*)_([\w]*)")]
        private static partial Regex RequirementParsingRegex();

        public override void Load()
        {
            TransmutatorTileEntity.PreTransmutateItemEvent += LivingLensTransmutate;
        }

        private bool LivingLensTransmutate(TransmutatorTileEntity transmutator, Core.Systems.TransmutationRecipe recipe)
        {
            if (transmutator.projector.LensPlaced.type == ModContent.ItemType<LivingLens>())
            {
                foreach (TransmutationRequirement req in recipe.otherRequirements)
                {
                    if (req.id.EndsWith($"_{nameof(LivingLens)}") && req.condition(transmutator))
                    {
                        string[] parsedID = RequirementParsingRegex().Split(req.id);
                        transmutator.activeBuffTime -= int.Parse(parsedID[2]);

                        for (int i = 0; i < Main.npc.Length; i++)
                        {
                            NPC npc = Main.npc[i];
                            if (npc.active && npc.Distance(transmutator.TileEntityWorldCenter()) < ABSORPTION_RADIUS && npc.CountsAsACritter && !npc.SpawnedFromStatue)
                            {
                                Vector2 tileEntityCenter = transmutator.TileEntityWorldCenter();
                                Vector2 midWay = Vector2.Lerp(tileEntityCenter, npc.Center, 0.5f);
                                midWay += Vector2.UnitY.RotatedBy(midWay.AngleTo(npc.Center)) * 192f * -MathF.Sign(npc.Center.X - tileEntityCenter.X);
                                ParticleSystem.AddParticle(new Lightning(new List<Vector2> { tileEntityCenter, midWay, npc.Center }, new Color(252, 101, 84), 12, 2.5f, 1.5f));
                                npc.active = false;

                                int particleCount = npc.width;
                                for (int h = 0; h < particleCount; h++)
                                {
                                    Rectangle rect = new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height);
                                    Vector2 pos = Main.rand.NextVector2FromRectangle(rect) + Vector2.UnitY * 8f;
                                    ParticleSystem.AddParticle(new LivingLensSoul(pos, Main.rand.Next(25, 35), Main.rand.NextFloat(0.7f, 0.9f)));
                                }

                                SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/LightningZap") with { PitchVariance = 0.5f, Volume = 2f }, npc.Center);
                                return false;
                            }
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Living Lens");
            Tooltip.SetDefault("Allows you to perform transmutations involving the essence of emotions when slotted into a Projector");
            Item.ResearchUnlockCount = 1;

            ProjectorLensData.AddProjectorLensData(Name, Type, DustID.CrimsonTorch, Texture + "_Transmutator");
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 18;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(0, 1, 0);
            Item.rare = ItemRarityID.Pink;
        }

        public static TransmutationRequirement GetLivingLensRequirememt(InfluentialColor color, int duration) =>
            new TransmutationRequirement($"{Enum.GetName(typeof(InfluentialColor), color)}_{duration}_{nameof(LivingLens)}", (t) => t.projector.LensPlaced.type == ModContent.ItemType<LivingLens>() && influentalColorMap[color].Contains(t.activeBuff) && t.activeBuffTime >= duration, LivingLensTransmutationKey.WithFormatArgs(color, TimeConversion(duration)));
    }
    public class LivingLensSoul : Particle
    {
        public override string Texture => "Radiance/Content/Items/ProjectorLenses/LivingLens_Soul";

        public int variant;

        public LivingLensSoul(Vector2 position, int maxTime, float scale = 1)
        {
            this.position = position;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            this.scale = scale;
            
            mode = ParticleSystem.DrawingMode.Additive;
            rotation = Main.rand.NextFloat(Pi);
            color = Color.White;
        }

        public override void Update()
        {
            scale = Lerp(2f, 0.2f, MathF.Pow(Progress, 2f));
            velocity.Y -= 0.07f * Lerp(1f, 0f, MathF.Pow(Progress, 2f));
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, drawPos, null, color, rotation, tex.Size() / 2, scale, 0, 0);
        }
    }
}