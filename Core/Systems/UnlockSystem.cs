using Radiance.Content.UI.NewEntryAlert;
using Radiance.Core.Encycloradia;
using System.Collections.Generic;
using System.Security.Cryptography;

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
                { UnlockCondition.downedEyeOfCthulhu, false },
                { UnlockCondition.downedGoblins, false },
                { UnlockCondition.downedEvilBoss, false },
                { UnlockCondition.downedQueenBee, false },
                { UnlockCondition.downedSkeletron, false },
                { UnlockCondition.hardmode, false },
                { UnlockCondition.downedAnyMech, false },
                { UnlockCondition.downedTwoMechs, false },
                { UnlockCondition.downedAllMechs, false },
                { UnlockCondition.downedPlantera, false },
                { UnlockCondition.downedEmpressofLight, false },
                { UnlockCondition.downedGolem, false },
                { UnlockCondition.downedLunaticCultist, false },
                { UnlockCondition.downedMoonLord, false },

                { UnlockCondition.transmutatorFishUsed, false },
                { UnlockCondition.debugCondition, false }
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

                if (kvp.Key.unlockFunction())
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
                            Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().unreadEntires.Add(entry.name);
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
        public Func<bool> unlockFunction;
        public string unlockCondition;
        public UnlockCondition(Func<bool> unlockFunction, string unlockCondition)
        {
            this.unlockFunction = unlockFunction;
            this.unlockCondition = unlockCondition;
        }

        public static readonly UnlockCondition unlockedByDefault = new UnlockCondition(() => true, string.Empty);
        public static readonly UnlockCondition downedEyeOfCthulhu = new UnlockCondition(() => NPC.downedBoss1, "slaying the Eye of Cthulhu");
        public static readonly UnlockCondition downedGoblins = new UnlockCondition(() => NPC.downedGoblins, "conquering the Goblin Army");
        public static readonly UnlockCondition downedEvilBoss = new UnlockCondition(() => NPC.downedBoss2, "slaying the Eater of Worlds of Brain of Cthulhu");
        public static readonly UnlockCondition downedQueenBee = new UnlockCondition(() => NPC.downedQueenBee, "slaying the Queen Bee");
        public static readonly UnlockCondition downedSkeletron = new UnlockCondition(() => NPC.downedBoss3, "slaying Skeletron");
        public static readonly UnlockCondition hardmode = new UnlockCondition(() => Main.hardMode, "slaying the Wall of Flesh");
        public static readonly UnlockCondition downedAnyMech = new UnlockCondition(() => NPC.downedMechBossAny, "slaying any Mechanical Boss");
        public static readonly UnlockCondition downedTwoMechs = new UnlockCondition(() => (NPC.downedMechBoss1 && NPC.downedMechBoss2) || (NPC.downedMechBoss1 && NPC.downedMechBoss3) || (NPC.downedMechBoss2 && NPC.downedMechBoss3), "slaying any two Mechanical Bosses");
        public static readonly UnlockCondition downedAllMechs = new UnlockCondition(() => NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3, "slaying all of the Mechanical Bosses");
        public static readonly UnlockCondition downedPlantera = new UnlockCondition(() => NPC.downedPlantBoss, "slaying Plantera");
        public static readonly UnlockCondition downedEmpressofLight = new UnlockCondition(() => NPC.downedEmpressOfLight, "slaying the Empress of Light");
        public static readonly UnlockCondition downedGolem = new UnlockCondition(() => NPC.downedGolemBoss, "slaying Golem");
        public static readonly UnlockCondition downedLunaticCultist = new UnlockCondition(() => NPC.downedAncientCultist, "slaying the Lunatic Cultist");
        public static readonly UnlockCondition downedMoonLord = new UnlockCondition(() => NPC.downedMoonlord, "slaying the Moon Lord");

        public static readonly UnlockCondition transmutatorFishUsed = new UnlockCondition(() => UnlockSystem.transmutatorFishUsed, "???");
        public static readonly UnlockCondition debugCondition = new UnlockCondition(() => UnlockSystem.debugCondition, "???");
    }
}