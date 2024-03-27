using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Radiance.Core
{
    public partial class RadiancePlayer : ModPlayer
    {
        private int lifeRegenOverheal = 0;
        public void LoadOverheal()
        {
            // Overheal
            On_Player.Heal += On_Player_Heal;
            On_Player.UpdateLifeRegen += PostUpdateLifeRegen;
            On_Player.PickupItem += On_Player_PickupItem;

            IL_Player.ApplyLifeAndOrMana += ApplyLifeAndOrMana_OverHeal;
            IL_Player.Update += Update_OverHeal;
            IL_Player.UpdateLifeRegen += UpdateLifeRegen_OverHeal;
            IL_Projectile.VanillaAI += ProjectileVanillaAI_OverHeal;

        }
        public void UnloadOverheal()
        {
            // Overheal
            On_Player.Heal -= On_Player_Heal;
            On_Player.UpdateLifeRegen -= PostUpdateLifeRegen;
            On_Player.PickupItem -= On_Player_PickupItem;

            IL_Player.ApplyLifeAndOrMana -= ApplyLifeAndOrMana_OverHeal;
            IL_Player.Update -= Update_OverHeal;
            IL_Player.UpdateLifeRegen -= UpdateLifeRegen_OverHeal;
            IL_Projectile.VanillaAI -= ProjectileVanillaAI_OverHeal;
        }
        #region Overheal
        // if this isn't here then pickupitem inlines the heal call
        private Item On_Player_PickupItem(On_Player.orig_PickupItem orig, Player self, int playerIndex, int worldItemArrayIndex, Item itemToPickUp) => orig(self, playerIndex, worldItemArrayIndex, itemToPickUp);

        private void On_Player_Heal(On_Player.orig_Heal orig, Player self, int amount)
        {
            if (amount > self.statLifeMax2 - self.statLife)
                OverhealEvent.Invoke(self, amount - self.statLifeMax2 + self.statLife);
            
            orig(self, amount);
        }
        private void PostUpdateLifeRegen(On_Player.orig_UpdateLifeRegen orig, Player self)
        {
            orig(self);
            ref int overheal = ref self.GetModPlayer<RadiancePlayer>().lifeRegenOverheal;
            if (overheal > 0)
                OverhealEvent?.Invoke(self, overheal);

            overheal = 0;
        }
        private void ApplyLifeAndOrMana_OverHeal(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(typeof(Player), nameof(Player.statLifeMax2)),
                i => i.MatchStfld(typeof(Player), nameof(Player.statLife))
                ))
            {
                LogIlError($"{nameof(ApplyLifeAndOrMana_OverHeal)} overheal detection", "Couldn't navigate to before statLife = statLifeMax2");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((Player player) => OverhealEvent?.Invoke(player, player.statLife - player.statLifeMax2));
        }
        private void Update_OverHeal(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(typeof(Player), nameof(Player.statLifeMax2)),
                i => i.MatchStfld(typeof(Player), nameof(Player.statLife))
                ))
            {
                LogIlError($"{nameof(Update_OverHeal)} overheal detection", "Couldn't navigate to before statLife = statLifeMax2");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((Player player) => OverhealEvent?.Invoke(player, player.statLife - player.statLifeMax2));
        }
        private void UpdateLifeRegen_OverHeal(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(typeof(Player), nameof(Player.lifeRegenCount)),
                i => i.MatchLdcI4((byte)120),
                i => i.MatchSub(),
                i => i.MatchStfld(typeof(Player), nameof(Player.lifeRegenCount))
                ))
            {
                LogIlError($"{nameof(UpdateLifeRegen_OverHeal)} overheal detection", "Couldn't navigate to after lifeRegenCount -= 120");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((Player player) => 
            {
                if (player.statLife == player.statLifeMax2)
                    player.GetModPlayer<RadiancePlayer>().lifeRegenOverheal++;
            });
        }
        private void ProjectileVanillaAI_OverHeal(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(MoveType.Before,
                i => i.MatchNop(),
                i => i.MatchLdelemRef(),
                i => i.MatchLdfld(typeof(Player), nameof(Player.statLifeMax2)),
                i => i.MatchStfld(typeof(Player), nameof(Player.statLife))
                ))
            {
                LogIlError($"{nameof(ProjectileVanillaAI_OverHeal)} overheal detection", "Couldn't navigate to before statLife = statLifeMax2");
                return;
            }
            cursor.Emit(OpCodes.Ldloc, 633);
            cursor.EmitDelegate((int index) => OverhealEvent?.Invoke(Main.player[index], Main.player[index].statLife - Main.player[index].statLifeMax2));
        }
        #endregion
    }
}
