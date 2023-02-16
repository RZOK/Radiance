using Radiance.Content.UI.NewEntryAlert;
using Radiance.Core;
using Radiance.Core.Encycloradia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;

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
        public static Dictionary<UnlockBoolean, bool> UnlockMethods;
        public static Dictionary<UnlockBoolean, bool> SetUnlockDic()
        {
            return new()
            {
                { UnlockBoolean.unlockedByDefault, true },
            
                #region Prehardmode

                { UnlockBoolean.downedEyeOfCthulhu, NPC.downedBoss1 },
                { UnlockBoolean.downedGoblins, NPC.downedGoblins },
                { UnlockBoolean.downedEvilBoss, NPC.downedBoss2 },
                { UnlockBoolean.downedQueenBee, NPC.downedQueenBee },
                { UnlockBoolean.downedSkeletron, NPC.downedBoss3 },

                #endregion

                #region Hardmode

                { UnlockBoolean.hardmode, Main.hardMode },
                { UnlockBoolean.downedAnyMech, NPC.downedMechBossAny },
                { UnlockBoolean.downedDestroyer, NPC.downedMechBoss1 },
                { UnlockBoolean.downedTwins, NPC.downedMechBoss2 },
                { UnlockBoolean.downedSkeletronPrime, NPC.downedMechBoss3 },
                { UnlockBoolean.downedPlantera, NPC.downedPlantBoss },
                { UnlockBoolean.downedGolem, NPC.downedGolemBoss },
                { UnlockBoolean.downedCultist, NPC.downedAncientCultist },
                { UnlockBoolean.downedMoonlord, NPC.downedMoonlord },

                #endregion
            };
        }
        public readonly static Dictionary<UnlockBoolean, string> IncompleteText = new()
        {
            #region Prehardmode

            { UnlockBoolean.downedEyeOfCthulhu, "slaying the Eye of Cthulhu" },
            { UnlockBoolean.downedGoblins, "conquering the Goblin Army" },
            { UnlockBoolean.downedEvilBoss, "slaying the Eater of Worlds of Brain of Cthulhu" },
            { UnlockBoolean.downedQueenBee, "slaying the Queen Bee" },
            { UnlockBoolean.downedSkeletron, "slaying Skeletron" },

            #endregion

            #region Hardmode

            { UnlockBoolean.hardmode, "slaying the Wall of Flesh" },
            { UnlockBoolean.downedAnyMech, "slaying any Mechanical Boss" },
            { UnlockBoolean.downedDestroyer, "slaying The Destroyer" },
            { UnlockBoolean.downedTwins, "slaying The Twins" },
            { UnlockBoolean.downedSkeletronPrime, "slaying Skeletron Prime" },
            { UnlockBoolean.downedPlantera, "slaying Plantera" },
            { UnlockBoolean.downedGolem, "slaying Golem" },
            { UnlockBoolean.downedCultist, "slaying the Lunatic Cultist" },
            { UnlockBoolean.downedMoonlord, "slaying the Moon Lord" },

            #endregion
        };
        public static List<EntryAlertText> unlockedEntries = new();
        public override void PostUpdateEverything()
        {
            //updates the dictionary every three seconds
            if (Main.GameUpdateCount % 180 == 0)
                UpdateDicts();
        }
        public static void UpdateDicts()
        {
            Dictionary<UnlockBoolean, bool> fixedDic = SetUnlockDic();
            foreach (var key in UnlockMethods.Keys)
            {
                if (UnlockMethods[key] != fixedDic[key])
                {
                    if (EncycloradiaSystem.entries.Where(x => x.unlock == key).Count() > 0)
                    {
                        for (int i = 0; i < Main.maxPlayers; i++)
                        {
                            if (Main.player[i].active)
                                Main.player[i].GetModPlayer<RadianceInterfacePlayer>().newEntryUnlockedTimer = NewEntryAlertUI.timerMax;
                        }
                        foreach (var entry in EncycloradiaSystem.entries.Where(x => x.unlock == key))
                        {
                            unlockedEntries.Add(new EntryAlertText(entry));
                        }
                    }
                    UnlockMethods[key] = fixedDic[key];
                }
            }
        }
        public override void OnWorldLoad()
        {
            UnlockMethods = SetUnlockDic();
        }
        public override void OnWorldUnload()
        {
            UnlockMethods.Clear();
        }
    }
}