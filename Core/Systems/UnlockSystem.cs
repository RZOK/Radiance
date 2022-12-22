using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace Radiance.Core.Systems
{
    public class UnlockSystem : ModSystem
    {
        public class UnlockDictionary<Tkey, Tvalue> : Dictionary<Tkey, Tvalue>
        {
            public void SetItemValue(Tkey key, Tvalue value)
            {
                try
                {
                    TryGetValue(key, out var val);
                    if (!value.Equals(val))
                    {
                        this[key] = value;
                        OnValueChanged(this, EventArgs.Empty);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.InnerException;
                }
            }
            public event EventHandler ValueChanged;
            public void OnValueChanged(object sender, EventArgs e)
            {
                //trigger encycloradia 'new pages unlocked' ui prompt
                EventHandler handler = ValueChanged;
                if (null != handler) handler(this, EventArgs.Empty);
            }
        }
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
        public static UnlockDictionary<UnlockBoolean, bool> UnlockMethods;
        public static UnlockDictionary<UnlockBoolean, bool> SetUnlockDic()
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
        public static Dictionary<UnlockBoolean, string> IncompleteText = new() 
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

        public override void PostUpdateEverything()
        {
            //updates the dictionary every second, updating it if  
            if (Main.GameUpdateCount % 60 == 0)
            {
                UnlockDictionary<UnlockBoolean, bool> fixedDic = SetUnlockDic();
                foreach ((var key, _) in UnlockMethods)
                    UnlockMethods.SetItemValue(key, fixedDic.GetValueOrDefault(key));
            }
        }public override void OnWorldLoad()
        {
            UnlockMethods = SetUnlockDic();
        }
        public override void OnWorldUnload()
        {
            UnlockMethods.Clear();
        }
    }
}
