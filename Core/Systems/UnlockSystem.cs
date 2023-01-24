using Radiance.Content.UI.NewEntryAlert;
using Radiance.Core.Encycloradia;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using static Radiance.Core.Systems.UnlockSystem;

namespace Radiance.Core.Systems
{
    public class UnlockSystem : ModSystem
    {
        public enum UnlockBoolean
        {
            unlockedByDefault,

            #region Prehardmode

            downedEyeOfCthulhu,
            downedGoblins,
            downedEvilBoss,
            downedQueenBee,
            downedSkeletron,

            #endregion Prehardmode

            #region Hardmode

            hardmode,
            downedAnyMech,
            downedDestroyer,
            downedTwins,
            downedSkeletronPrime,
            downedPlantera,
            downedGolem,
            downedCultist,
            downedMoonlord

            #endregion Hardmode
        }
        private static bool unlockedByDefaultBool = true;
        public static UnlockCondition unlockedByDefault = new(UnlockBoolean.unlockedByDefault, new Wrapper<bool>() { Value = true }, string.Empty);
        public static UnlockCondition downedEyeOfCthulhu = new(UnlockBoolean.downedEyeOfCthulhu, new Wrapper<bool>() { Value = NPC.downedBoss1 }, "slaying the Eye of Cthulhu");
        public static UnlockCondition downedGoblins = new(UnlockBoolean.downedGoblins, new Wrapper<bool>() { Value = NPC.downedGoblins }, "conquering the Goblin Army");
        public static UnlockCondition downedEvilBoss = new(UnlockBoolean.downedEvilBoss, new Wrapper<bool>() { Value = NPC.downedBoss2 }, "slaying the Eater of Worlds or Brain of Cthulhu");
        public static UnlockCondition downedQueenBee = new(UnlockBoolean.downedQueenBee, new Wrapper<bool>() { Value = NPC.downedQueenBee }, "slaying the Queen Bee");
        public static UnlockCondition downedSkeletron = new(UnlockBoolean.downedSkeletron, new Wrapper<bool>() { Value = NPC.downedBoss3 }, "slaying Skeletron");

        public static UnlockCondition hardmode = new(UnlockBoolean.hardmode, new Wrapper<bool>() { Value = Main.hardMode }, "slaying the Wall of Flesh");
        public static UnlockCondition downedAnyMech = new(UnlockBoolean.downedAnyMech, new Wrapper<bool>() { Value = NPC.downedMechBossAny }, "slaying any Mechanical Boss");
        public static UnlockCondition downedDestroyer = new(UnlockBoolean.downedDestroyer, new Wrapper<bool>() { Value = NPC.downedMechBoss1 }, "slaying The Destroyer");
        public static UnlockCondition downedTwins = new(UnlockBoolean.downedTwins, new Wrapper<bool>() { Value = NPC.downedMechBoss2 }, "slaying The Twins");
        public static UnlockCondition downedSkeletronPrime = new(UnlockBoolean.downedSkeletronPrime, new Wrapper<bool>() { Value = NPC.downedMechBoss3 }, "slaying Skeletron Prime");
        public static UnlockCondition downedPlantera = new(UnlockBoolean.downedPlantera, new Wrapper<bool>() { Value = NPC.downedPlantBoss }, "slaying Plantera");
        public static UnlockCondition downedGolem = new(UnlockBoolean.downedGolem, new Wrapper<bool>() { Value = NPC.downedGolemBoss }, "slaying Golem");
        public static UnlockCondition downedLunaticCultist = new(UnlockBoolean.downedCultist, new Wrapper<bool>() { Value = NPC.downedAncientCultist }, "slaying the Lunatic Cultist");
        public static UnlockCondition downedMoonLord = new(UnlockBoolean.downedMoonlord, new Wrapper<bool>() { Value = NPC.downedMoonlord }, "slaying the Moon Lord");

        public static List<EntryAlertText> unlockedEntries = new List<EntryAlertText>();
    }

    public class UnlockCondition 
    {
        public UnlockBoolean unlockEnum = UnlockBoolean.unlockedByDefault;
        public string incompleteString = string.Empty;

        public Wrapper<bool> unlockBoolValue = new Wrapper<bool>() { Value = true };
        public UnlockCondition(UnlockBoolean unlockEnum, Wrapper<bool> unlockBoolValue, string incompleteString)
        {
            this.unlockEnum = unlockEnum;
            this.unlockBoolValue = unlockBoolValue;
            this.incompleteString = incompleteString;  

        }
    }
    public class Wrapper<T>
    {
        public T Value { get; set; }
    }
}
