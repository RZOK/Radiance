using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Radiance.Core
{
    public partial class RadiancePlayer : ModPlayer
    {
        private int lifeRegenOverheal = 0;
        private int manaRegenOvermana = 0;

        public void LoadOverheal()
        {
            // Overheal
            On_Player.Heal += On_Player_Heal;
            On_Player.UpdateLifeRegen += PostUpdateLifeRegen;

            IL_Player.ApplyLifeAndOrMana += ApplyLifeAndOrMana_OverHeal;
            IL_Player.Update += Update_OverHeal;
            IL_Player.UpdateLifeRegen += UpdateLifeRegen_OverHeal;
            IL_Projectile.VanillaAI += ProjectileVanillaAI_OverHeal;

            // Overmana
            On_Player.UpdateManaRegen += PostUpdateManaRegen;

            IL_Player.UpdateManaRegen += UpdateManaRegen_OverMana;
            IL_Player.Update += Update_OverMana;
            IL_Player.PickupItem += PickupItem_OverMana;
            IL_Player.OnHurt_Part2 += OnHurt_Part2_OverMana;
            IL_Player.ApplyLifeAndOrMana += ApplyLifeOrMana_OverMana;
        }

        public void UnloadOverheal()
        {
            // Overheal
            On_Player.Heal -= On_Player_Heal;
            On_Player.UpdateLifeRegen -= PostUpdateLifeRegen;

            IL_Player.ApplyLifeAndOrMana -= ApplyLifeAndOrMana_OverHeal;
            IL_Player.Update -= Update_OverHeal;
            IL_Player.UpdateLifeRegen -= UpdateLifeRegen_OverHeal;
            IL_Projectile.VanillaAI -= ProjectileVanillaAI_OverHeal;

            // Overmana
            IL_Player.UpdateManaRegen -= UpdateManaRegen_OverMana;
            IL_Player.Update -= Update_OverMana;
            IL_Player.PickupItem -= PickupItem_OverMana;
            IL_Player.OnHurt_Part2 -= OnHurt_Part2_OverMana;
            IL_Player.ApplyLifeAndOrMana -= ApplyLifeOrMana_OverMana;
        }

        #region Overheal

        private void On_Player_Heal(On_Player.orig_Heal orig, Player self, int amount)
        {
            if (amount > self.statLifeMax2 - self.statLife)
                OverhealEvent?.Invoke(self, amount - self.statLifeMax2 + self.statLife);

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
            cursor.Emit(OpCodes.Ldloc, 635);
            cursor.EmitDelegate((int index) => OverhealEvent?.Invoke(Main.player[index], Main.player[index].statLife - Main.player[index].statLifeMax2));
        }

        #endregion Overheal

        #region Overmana

        private void PostUpdateManaRegen(On_Player.orig_UpdateManaRegen orig, Player self)
        {
            orig(self);
            ref int overmana = ref self.GetModPlayer<RadiancePlayer>().manaRegenOvermana;
            if (overmana > 0)
                OvermanaEvent?.Invoke(self, overmana);

            overmana = 0;
        }

        private void UpdateManaRegen_OverMana(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(typeof(Player), nameof(Player.statManaMax2)),
                i => i.MatchStfld(typeof(Player), nameof(Player.statMana))
                ))
            {
                LogIlError($"{nameof(UpdateManaRegen_OverMana)} overmana detection", "Couldn't navigate to before the first statMana assignment");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((Player player) => OvermanaEvent?.Invoke(player, player.statMana - player.statManaMax2));

            if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(typeof(Player), nameof(Player.manaRegenCount)),
                i => i.MatchLdcI4((byte)120),
                i => i.MatchSub(),
                i => i.MatchStfld(typeof(Player), nameof(Player.manaRegenCount))
                ))
            {
                LogIlError($"{nameof(UpdateManaRegen_OverMana)} overmana detection", "Couldn't navigate to after manaRegenCount sub 120");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((Player player) =>
            {
                if (player.statMana == player.statManaMax2)
                    player.GetModPlayer<RadiancePlayer>().manaRegenOvermana++;
            });
        }

        private void Update_OverMana(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(typeof(Player), nameof(Player.statManaMax2)),
                i => i.MatchStfld(typeof(Player), nameof(Player.statMana))
                ))
            {
                LogIlError($"{nameof(Update_OverMana)} overmana detection", "Couldn't navigate to before the first statMana assignment");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((Player player) => OvermanaEvent?.Invoke(player, player.statMana - player.statManaMax2));
            cursor.Index += 4; // we move before so we need to push the cursor forward to reach the second match

            if (!cursor.TryGotoNext(MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(typeof(Player), nameof(Player.statManaMax2)),
                i => i.MatchStfld(typeof(Player), nameof(Player.statMana))
                ))
            {
                LogIlError($"{nameof(Update_OverMana)} overmana detection", "Couldn't navigate to before the second statMana assignment");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((Player player) => OvermanaEvent?.Invoke(player, player.statMana - player.statManaMax2));
        }

        private void PickupItem_OverMana(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(typeof(Player), nameof(Player.statManaMax2)),
                i => i.MatchStfld(typeof(Player), nameof(Player.statMana))
                ))
            {
                LogIlError($"{nameof(PickupItem_OverMana)} overmana detection", "Couldn't navigate to before the first statMana assignment");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((Player player) => OvermanaEvent?.Invoke(player, player.statMana - player.statManaMax2));
            cursor.Index += 4; // we move before so we need to push the cursor forward to reach the second match

            if (!cursor.TryGotoNext(MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(typeof(Player), nameof(Player.statManaMax2)),
                i => i.MatchStfld(typeof(Player), nameof(Player.statMana))
                ))
            {
                LogIlError($"{nameof(PickupItem_OverMana)} overmana detection", "Couldn't navigate to before the second statMana assignment");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((Player player) => OvermanaEvent?.Invoke(player, player.statMana - player.statManaMax2));
        }

        private void OnHurt_Part2_OverMana(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(typeof(Player), nameof(Player.statManaMax2)),
                i => i.MatchStfld(typeof(Player), nameof(Player.statMana))
                ))
            {
                LogIlError($"{nameof(OnHurt_Part2_OverMana)} overmana detection", "Couldn't navigate to before the first statMana assignment");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((Player player) => OvermanaEvent?.Invoke(player, player.statMana - player.statManaMax2));
        }

        private void ApplyLifeOrMana_OverMana(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(typeof(Player), nameof(Player.statManaMax2)),
                i => i.MatchStfld(typeof(Player), nameof(Player.statMana))
                ))
            {
                LogIlError($"{nameof(ApplyLifeOrMana_OverMana)} overmana detection", "Couldn't navigate to before the first statMana assignment");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((Player player) => OvermanaEvent?.Invoke(player, player.statMana - player.statManaMax2));
        }

        #endregion Overmana
    }
}