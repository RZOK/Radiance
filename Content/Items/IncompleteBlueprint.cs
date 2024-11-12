using Radiance.Core.Loaders;
using Terraria.ModLoader.Config;

namespace Radiance.Content.Items
{
    public class IncompleteBlueprint : ModItem
    {
        public AutoloadedBlueprint blueprint;
        public float progress = 0;

        public BlueprintRequirement requirement;
        public BlueprintRequirement condition;


        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Incomplete Blueprint");
            Tooltip.SetDefault("Placeholder Line");
            Item.ResearchUnlockCount = 0;
        }
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Blue;
        }
        public override void UpdateInventory(Player player)
        {
            if(progress >= 1)
            {
                Item.ChangeItemType(blueprint.Type);
            }
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (requirement is not null && condition is not null)
            {
                TooltipLine blueprintTileLine = new TooltipLine(Mod, "BlueprintTile", $"An unfinished schematic for creating a [c/{ItemRarityHex(blueprint.Item)}:{GetItem(blueprint.blueprintData.tileItemType).Name}"); //todo: convert to localizedtext
                TooltipLine reqCondLine = new TooltipLine(Mod, "ReqCondLine", $"Progress this blueprint by [c/FF99C4:{requirement.tooltip}] [c/99FFC4:{condition.tooltip}]");
                tooltips.Insert(tooltips.FindIndex(x => x.Name == "Tooltip1" && x.Mod == "Terraria") + 1, blueprintTileLine);
                /*
                 * line 1: display the tile the blueprint is for
                 * line 2: display the requirement and condition
                 * line 3+: to be drawn blank to display the blueprint visual
                 */
            }
        }
        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            // line 3+ from above
            return true;
        }

        public override void SaveData(TagCompound tag)
        {
            tag[nameof(blueprint)] = blueprint.FullName;
            tag[nameof(progress)] = progress;

            tag[nameof(requirement)] = requirement.name;
            tag[nameof(condition)] = condition.name;

        }
        public override void LoadData(TagCompound tag)
        {
            string blueprintString = tag.GetString(nameof(blueprint));
            if (blueprintString != string.Empty)
            {
                if (!ModContent.TryFind(tag.GetString(nameof(blueprint)), out blueprint))
                {
                    Radiance.Instance.Logger.Warn($"Blueprint with blueprintString of '{blueprintString}' failed to load properly.");
#if DEBUG
                    SoundEngine.PlaySound(SoundID.DoorClosed);
#endif
                }
            }
            progress = tag.GetInt(nameof(progress));
            requirement = BlueprintRequirement.loadedRequirements.FirstOrDefault(x => x.name == tag.GetString(nameof(requirement)));
            condition = BlueprintRequirement.loadedConditions.FirstOrDefault(x => x.name == tag.GetString(nameof(condition)));

        }
    }
    public class BlueprintRequirement : ILoadable
    {
        public static List<BlueprintRequirement> loadedRequirements = new List<BlueprintRequirement>();
        public static List<BlueprintRequirement> loadedConditions = new List<BlueprintRequirement>();
        public readonly string name;
        public readonly Func<bool> requirement;
        public readonly string tooltip;
        public readonly int tier;
        public readonly bool condition;
        public BlueprintRequirement(string name, Func<bool> requirement, string tooltip, int tier, bool condition) 
        {
            this.name = name;
            this.requirement = requirement;
            this.tooltip = tooltip;
            this.tier = tier;
            this.condition = condition;
        }

        public void Load(Mod mod)
        {
            loadedRequirements.Add(new BlueprintRequirement("Requirement_StandInPurity", () => Main.LocalPlayer.ZonePurity, "Stand in the purity ", 1, false));

            loadedConditions.Add(new BlueprintRequirement("Condition_NoArmor", () => Main.LocalPlayer.armor[0].IsAir && Main.LocalPlayer.armor[1].IsAir && Main.LocalPlayer.armor[2].IsAir, "with no armor equipped. ", 1, true));
        }

        public void Unload() { }
    }
}   