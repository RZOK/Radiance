using Radiance.Content.UI.NewEntryAlert;
using Radiance.Core.Encycloradia;
using Terraria.Localization;

namespace Radiance.Core.Systems
{
    public class UnlockSystem : ModSystem
    {
        public static List<EntryAlertText> unlockedEntries = new();
        public static Dictionary<UnlockCondition, bool> currentUnlockConditionStates;

        public static bool transmutatorFishUsed = false;
        public static bool debugCondition = false;

        public override void Load()
        {
            WorldFile.OnWorldLoad += SetUnlocksOnWorldLord;
        }

        public override void Unload()
        {
            WorldFile.OnWorldLoad -= SetUnlocksOnWorldLord;
        }

        private void SetUnlocksOnWorldLord()
        {
            ResetUnlockConditions();
            CheckUnlocks(true);
        }

        public static void ResetUnlockConditions() => currentUnlockConditionStates = new Dictionary<UnlockCondition, bool>
            {
                { UnlockCondition.DownedEyeOfCthulhu, false },
                { UnlockCondition.DownedGoblins, false },
                { UnlockCondition.DownedEvilBoss, false },
                { UnlockCondition.DownedQueenBee, false },
                { UnlockCondition.DownedSkeletron, false },
                { UnlockCondition.Hardmode, false },
                { UnlockCondition.DownedAnyMech, false },
                { UnlockCondition.DownedTwoMechs, false },
                { UnlockCondition.DownedAllMechs, false },
                { UnlockCondition.DownedPlantera, false },
                { UnlockCondition.DownedEmpressofLight, false },
                { UnlockCondition.DownedGolem, false },
                { UnlockCondition.DownedLunaticCultist, false },
                { UnlockCondition.DownedMoonLord, false },

                { UnlockCondition.transmutatorFishUsed, false },
                { UnlockCondition.DebugCondition, false }
            };

        public override void PostUpdateEverything()
        {
            if (Main.GameUpdateCount % 180 == 0)
                CheckUnlocks();
        }

        public static void CheckUnlocks(bool onWorldLoad = false)
        {
            bool shouldSortEntries = false;
            bool shouldRebuildCategoryPages = false;
            foreach (var kvp in currentUnlockConditionStates)
            {
                if (kvp.Value)
                    continue;

                if (kvp.Key.condition())
                {
                    currentUnlockConditionStates[kvp.Key] = true;
                    if (!onWorldLoad)
                    {
                        foreach (EncycloradiaEntry entry in EncycloradiaSystem.EncycloradiaEntries)
                        {
                            if (entry.unlock != kvp.Key)
                                continue;

                            shouldSortEntries = true;
                            if (entry.visible == EntryVisibility.NotVisibleUntilUnlocked)
                                shouldRebuildCategoryPages = true;

                            unlockedEntries.Add(new EntryAlertText(entry));
                            Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().unreadEntires.Add(entry.internalName);
                            Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().newEntryUnlockedTimer = NewEntryAlertUI.NEW_ENTRY_ALERT_UI_TIMER_MAX;
                        }
                    }
                }
            }
            if (shouldSortEntries)
                EncycloradiaSystem.SortEntries();

            if (shouldRebuildCategoryPages)
                EncycloradiaSystem.RebuildCategoryPages();
        }

        // todo: bitsbytes flags
        public override void SaveWorldData(TagCompound tag)
        {
            tag[nameof(transmutatorFishUsed)] = transmutatorFishUsed;
            tag[nameof(debugCondition)] = debugCondition;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            transmutatorFishUsed = tag.GetBool(nameof(transmutatorFishUsed));
            debugCondition = tag.GetBool(nameof(debugCondition));
        }
    }

    public record UnlockCondition
    {
        public Func<bool> condition;
        public LocalizedText tooltip;
        public static readonly string LOCALIZATION_PREFIX = $"Mods.{nameof(Radiance)}.UnlockConditions";
        public UnlockCondition(Func<bool> condition, LocalizedText tooltip)
        {
            this.condition = condition;
            this.tooltip = tooltip;
        }

        public static readonly UnlockCondition UnlockedByDefault = new UnlockCondition(() => true, LanguageManager.Instance.GetOrRegister($"{LOCALIZATION_PREFIX}.UnlockedByDefault", () => "Nothing!"));
        public static readonly UnlockCondition DownedEyeOfCthulhu = new UnlockCondition(() => NPC.downedBoss1, LanguageManager.Instance.GetOrRegister($"{LOCALIZATION_PREFIX}.DownedEyeOfCthulhu", () => "slaying the Eye of Cthulhu."));
        public static readonly UnlockCondition DownedGoblins = new UnlockCondition(() => NPC.downedGoblins, LanguageManager.Instance.GetOrRegister($"{LOCALIZATION_PREFIX}.DownedGoblins", () => "conquering the Goblin Army."));
        public static readonly UnlockCondition DownedEvilBoss = new UnlockCondition(() => NPC.downedBoss2, LanguageManager.Instance.GetOrRegister($"{LOCALIZATION_PREFIX}.DownedEvilBoss", () => "slaying the Eater of Worlds or Brain of Cthulhu."));
        public static readonly UnlockCondition DownedQueenBee = new UnlockCondition(() => NPC.downedQueenBee, LanguageManager.Instance.GetOrRegister($"{LOCALIZATION_PREFIX}.DownedQueenBee", () => "slaying the Queen Bee."));
        public static readonly UnlockCondition DownedSkeletron = new UnlockCondition(() => NPC.downedBoss3, LanguageManager.Instance.GetOrRegister($"{LOCALIZATION_PREFIX}.DownedSkeletron", () => "slaying Skeletron."));
        public static readonly UnlockCondition Hardmode = new UnlockCondition(() => Main.hardMode, LanguageManager.Instance.GetOrRegister($"{LOCALIZATION_PREFIX}.Hardmode", () => "slaying the Wall of Flesh."));
        public static readonly UnlockCondition DownedAnyMech = new UnlockCondition(() => NPC.downedMechBossAny, LanguageManager.Instance.GetOrRegister($"{LOCALIZATION_PREFIX}.DownedAnyMech", () => "slaying any Mechanical Boss."));
        public static readonly UnlockCondition DownedTwoMechs = new UnlockCondition(() => (NPC.downedMechBoss1 && NPC.downedMechBoss2) || (NPC.downedMechBoss1 && NPC.downedMechBoss3) || (NPC.downedMechBoss2 && NPC.downedMechBoss3), LanguageManager.Instance.GetOrRegister($"{LOCALIZATION_PREFIX}.DownedTwoMechs", () => "slaying any two Mechanical Bosses."));
        public static readonly UnlockCondition DownedAllMechs = new UnlockCondition(() => NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3, LanguageManager.Instance.GetOrRegister($"{LOCALIZATION_PREFIX}.DownedAllMechs", () => "slaying all of the Mechanical Bosses."));
        public static readonly UnlockCondition DownedPlantera = new UnlockCondition(() => NPC.downedPlantBoss, LanguageManager.Instance.GetOrRegister($"{LOCALIZATION_PREFIX}.DownedPlantera", () => "slaying Plantera."));
        public static readonly UnlockCondition DownedEmpressofLight = new UnlockCondition(() => NPC.downedEmpressOfLight, LanguageManager.Instance.GetOrRegister($"{LOCALIZATION_PREFIX}.DownedEmpressofLight", () => "slaying the Empress of Light."));
        public static readonly UnlockCondition DownedGolem = new UnlockCondition(() => NPC.downedGolemBoss, LanguageManager.Instance.GetOrRegister($"{LOCALIZATION_PREFIX}.DownedGolem", () => "slaying the Golem."));
        public static readonly UnlockCondition DownedLunaticCultist = new UnlockCondition(() => NPC.downedAncientCultist, LanguageManager.Instance.GetOrRegister($"{LOCALIZATION_PREFIX}.DownedLunaticCultist", () => "slaying the Lunatic Cultist."));
        public static readonly UnlockCondition DownedMoonLord = new UnlockCondition(() => NPC.downedMoonlord, LanguageManager.Instance.GetOrRegister($"{LOCALIZATION_PREFIX}.DownedMoonLord", () => "slaying the Moon Lord."));

        public static readonly UnlockCondition transmutatorFishUsed = new UnlockCondition(() => UnlockSystem.transmutatorFishUsed, LanguageManager.Instance.GetOrRegister($"{LOCALIZATION_PREFIX}.QuestionMarks", () => "???"));
        public static readonly UnlockCondition DebugCondition = new UnlockCondition(() => UnlockSystem.debugCondition, LanguageManager.Instance.GetOrRegister($"{LOCALIZATION_PREFIX}.QuestionMarks", () => "???"));
    }
}