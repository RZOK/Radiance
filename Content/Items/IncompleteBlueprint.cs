using Microsoft.Build.Tasks.Deployment.ManifestUtilities;
using Radiance.Content.Particles;
using Radiance.Core.Loaders;
using Radiance.Core.Systems.ParticleSystems;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Tile_Entities;
using Terraria.ModLoader.Config;

namespace Radiance.Content.Items
{
    public class IncompleteBlueprint : ModItem
    {
        public AutoloadedBlueprint blueprint;
        public float progress = 0;
        private readonly static float MAX_PROGRESS = 100;
        public float CurrentCompletion => progress / MAX_PROGRESS;

        public BlueprintRequirement requirement;
        public BlueprintCondition condition;
        private Vector2 dustPos;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Incomplete Blueprint");
            Tooltip.SetDefault("A strange, blank blueprint dotted with unknown inscriptions");
            Item.ResearchUnlockCount = 0;
        }

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 30;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateInventory(Player player)
        {
            if (requirement is not null && condition is not null)
            {
                if (condition.function())
                    progress += 1f * requirement.function();
            }
            if (progress >= MAX_PROGRESS)
            {
                SoundEngine.PlaySound(SoundID.Item4, player.Center);
                CombatText.NewText(player.Hitbox, Color.LightSkyBlue, "Blueprint complete!");
                Item.ChangeItemType(blueprint.Type);
                SpawnParticles();
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (requirement is not null && condition is not null)
            {
                TooltipLine blueprintTileLine = new TooltipLine(Mod, "BlueprintTile", $"An unfinished schematic for creating a [c/{ItemRarityHex(blueprint.Item)}:{GetItem(blueprint.blueprintData.tileItemType).Name}]"); //todo: convert to localizedtext
                TooltipLine reqCondLine = new TooltipLine(Mod, "ReqCondLine", $"Progress this blueprint by [c/FF99C4:{requirement.tooltip}] [c/99FFC4:{condition.tooltip}]");
                TooltipLine progressLine = new TooltipLine(Mod, "ProgressLine", "m\n\n");
                TooltipLine tooltip = tooltips.First(x => x.Name == "Tooltip0" && x.Mod == "Terraria");
                tooltip.Text = blueprintTileLine.Text;
                tooltips.Insert(tooltips.FindIndex(x => x.Name == "Tooltip0" && x.Mod == "Terraria") + 1, reqCondLine);
                tooltips.Insert(tooltips.FindIndex(x => x.Name == "Tooltip0" && x.Mod == "Terraria") + 2, progressLine);
            }
        }

        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            if (line.Name == "ProgressLine")
            {
                Texture2D backgroundTex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/UI/BlueprintUI/BlueprintBG").Value;
                Texture2D overlayTex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/UI/BlueprintUI/BlueprintOverlay").Value;
                Vector2 pos = new Vector2(line.X - 4, line.Y - 4);
                Vector2 overlayOffset = (backgroundTex.Size() - overlayTex.Size()) / 2f;
                float fadeThreshold = 0.1f;
                float steps = 50;
                float steppedProgress = CurrentCompletion - CurrentCompletion % (1f / steps);
                float pulseModifier = SineTiming(60) * 0.05f;
                if (CurrentCompletion > 0)
                    steppedProgress += fadeThreshold + pulseModifier;

                Effect circleEffect = Terraria.Graphics.Effects.Filters.Scene["BlueprintFade"].GetShader().Shader;
                circleEffect.Parameters["progress"].SetValue(steppedProgress);
                circleEffect.Parameters["sampleTexture"].SetValue(overlayTex);
                circleEffect.Parameters["fadeThreshold"].SetValue(fadeThreshold);
                circleEffect.Parameters["pixelate"].SetValue(true);
                circleEffect.Parameters["resolution"].SetValue(overlayTex.Size() / 4f);

                Main.spriteBatch.Draw(backgroundTex, pos, null, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);

                Main.spriteBatch.End();
                RadianceDrawing.SpriteBatchData.UIDrawingDataScale.BeginSpriteBatchFromTemplate(effect: circleEffect);

                Main.spriteBatch.Draw(overlayTex, pos + overlayOffset, null, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);

                Main.spriteBatch.End();
                RadianceDrawing.SpriteBatchData.UIDrawingDataScale.BeginSpriteBatchFromTemplate();
                return false;
            }
            return true;
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (blueprint is not null)
            {
                Texture2D texture = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/Items/IncompleteBlueprint_Wrap").Value;
                spriteBatch.Draw(texture, position, null, (blueprint.color.ToVector4() * drawColor.ToVector4()).ToColor(), 0, texture.Size() / 2, scale, SpriteEffects.None, 0);
            }
            dustPos = position;
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            if (blueprint is not null)
            {
                Texture2D texture = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/Items/IncompleteBlueprint_Wrap").Value;
                spriteBatch.Draw(texture, Item.Center - Main.screenPosition, null, (blueprint.color.ToVector4() * lightColor.ToVector4()).ToColor(), rotation, texture.Size() / 2, scale, SpriteEffects.None, 0);
            }
        }
        public void SpawnParticles()
        {
            int numParticles = Main.rand.Next(8, 12);
            for (int i = 0; i < numParticles; i++)
            {
                Vector2 offset = new Vector2(i * 28f / (numParticles - 1) - 14f, Main.rand.NextFloat(-16, 16) + 8f);
                Vector2 velocity = Vector2.UnitY * Main.rand.NextFloat(-3f, -1f);
                InventoryParticleSystem.system.AddParticle(new ShimmerSparkle(dustPos + offset, velocity, Main.rand.Next(20, 40), Main.rand.Next(0, 50), new Color(89, 132, 255), 0.4f));
            }
        }
        public override void SaveData(TagCompound tag)
        {
            if (blueprint is not null)
            {
                tag[nameof(blueprint)] = blueprint.FullName;
                tag[nameof(progress)] = progress;

                tag[nameof(requirement)] = requirement.name;
                tag[nameof(condition)] = condition.name;
            }
        }

        public override void LoadData(TagCompound tag)
        {
            string blueprintString = tag.GetString(nameof(blueprint));
            ModItem mItem = null;
            if (blueprintString != string.Empty)
            {
                if (!ModContent.TryFind(blueprintString, out mItem))
                {
                    Radiance.Instance.Logger.Warn($"Blueprint with blueprintString of '{blueprintString}' failed to load properly.");
#if DEBUG
                    SoundEngine.PlaySound(SoundID.DoorClosed);
#endif
                }
                else
                    blueprint = (AutoloadedBlueprint)mItem;
            }
            progress = tag.GetFloat(nameof(progress));
            requirement = BlueprintRequirement.loadedRequirements.FirstOrDefault(x => x.name == tag.GetString(nameof(requirement)));
            condition = BlueprintCondition.loadedConditions.FirstOrDefault(x => x.name == tag.GetString(nameof(condition)));
        }
    }

    public class BlueprintRequirement : ILoadable
    {
        public static List<BlueprintRequirement> loadedRequirements = new List<BlueprintRequirement>();

        public readonly string name;
        public readonly Func<float> function;
        public readonly string tooltip;
        public readonly int tier;
        public readonly bool condition;

        public BlueprintRequirement()
        { }

        public BlueprintRequirement(string name, Func<float> function, string tooltip, int tier, bool condition)
        {
            this.name = name;
            this.function = function;
            this.tooltip = tooltip;
            this.tier = tier;
            this.condition = condition;
        }

        public void Load(Mod mod)
        {
            loadedRequirements.Add(new BlueprintRequirement("StandInPurity", () => Main.LocalPlayer.ZonePurity ? 0.1f : 0, "standing in the purity", 1, false));
            loadedRequirements.Add(new BlueprintRequirement("CraftWithSilver", () =>
            {
                if(Main.LocalPlayer.GetModPlayer<RadiancePlayer>().itemsUsedInLastCraft.TryGetValue(ItemID.SilverBar, out int value))
                {
                    return value * 5f;
                }
                return 0;
            }, "craft items using Silver or Tungsten Bars", 1, false));
            loadedRequirements.Add(new BlueprintRequirement("TouchLava", () =>
            {
                PlayerDeathReason reason = Main.LocalPlayer.GetModPlayer<RadiancePlayer>().lastHitSource;
                if (reason is not null && reason.SourceOtherIndex == 2)
                {
                    return 25f;
                }
                return 0;
            }, "touching lava", 1, false));
            loadedRequirements.Add(new BlueprintRequirement("Drown", () =>
            {
                Player player = Main.LocalPlayer;
                if(Collision.DrownCollision(player.position, player.width, player.height, player.gravDir))
                {
                    return 0.2f;
                }
                return 0;
            }, "resting underwater", 1, false));
        }

        public void Unload()
        { }
    }
    public class BlueprintCondition : ILoadable
    {
        public static List<BlueprintCondition> loadedConditions = new List<BlueprintCondition>();

        public readonly string name;
        public readonly Func<bool> function;
        public readonly string tooltip;
        public readonly int tier;
        public readonly bool condition;

        public BlueprintCondition()
        { }

        public BlueprintCondition(string name, Func<bool> function, string tooltip, int tier, bool condition)
        {
            this.name = name;
            this.function = function;
            this.tooltip = tooltip;
            this.tier = tier;
            this.condition = condition;
        }

        public void Load(Mod mod)
        {
            loadedConditions.Add(new BlueprintCondition("NoArmor", () => Main.LocalPlayer.armor[0].IsAir && Main.LocalPlayer.armor[1].IsAir && Main.LocalPlayer.armor[2].IsAir, "with no armor equipped", 1, true));
            loadedConditions.Add(new BlueprintCondition("ThreeBuffs", () => 
            {
                int numBuffs = 0;
                for (int i = 0; i < Player.MaxBuffs; i++)
                {
                    int buffType = Main.LocalPlayer.buffType[i];
                    if (buffType > 0 && !Main.debuff[buffType] && !Main.buffNoTimeDisplay[buffType])
                    {
                        numBuffs++;
                    }
                }
                return numBuffs >= 3;
            }, "with at least three different potion or food buffs", 1, true));
        }

        public void Unload()
        { }
    }
}