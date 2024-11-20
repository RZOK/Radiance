using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Radiance.Core
{
    public partial class RadiancePlayer : ModPlayer
    {
        public delegate void PostUpdateDelegate(Player player);
        public static event PostUpdateDelegate PostUpdateEvent;
        public override void PostUpdate()
        {
            itemsUsedInLastCraft.Clear();
            PostUpdateEvent?.Invoke(Player);

        }
        public delegate void PostUpdateEquipsDelegate(Player player);
        public static event PostUpdateEquipsDelegate PostUpdateEquipsEvent;
        public override void PostUpdateEquips()
        {
            PostUpdateEquipsEvent?.Invoke(Player);

            if (dashTimer > 0)
                dashTimer--;
        }
        public delegate bool CanUseItemDelegate(Player player, Item item);
        public static event CanUseItemDelegate CanUseItemEvent;
        public override bool CanUseItem(Item item)
        {
            if (CanUseItemEvent != null)
            {
                bool result = true;
                foreach (CanUseItemDelegate del in CanUseItemEvent.GetInvocationList())
                {
                    result &= del(Player, item);
                }
                return result;
            }
            return base.CanUseItem(item);
        }
        public delegate void PostHurtDelegate(Player player, Player.HurtInfo info);
        public static event PostHurtDelegate PostHurtEvent;
        public override void PostHurt(Player.HurtInfo info)
        {
            PostHurtEvent?.Invoke(Player, info);
        }
        public delegate void MeleeEffectsDelegate(Player player, Item item, Rectangle hitbox);
        public static event MeleeEffectsDelegate MeleeEffectsEvent;
        public override void MeleeEffects(Item item, Rectangle hitbox)
        {
            MeleeEffectsEvent?.Invoke(Player, item, hitbox);
        }
        public delegate void OnTileBreakDelegate(Player player, int x, int y);
        public static event OnTileBreakDelegate OnTileBreakEvent;
        private void DetectTileBreak(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(typeof(Player), nameof(Player.hitTile)),
                i => i.MatchLdloc(0),
                i => i.MatchLdloc(2),
                i => i.MatchLdcI4(1),
                i => i.MatchCallvirt(typeof(HitTile), nameof(HitTile.AddDamage)),
                i => i.MatchLdcI4(100),
                i => i.MatchBlt(out var _)
                ))
            {
                LogIlError("OnTileBreak tile break detection", "Couldn't navigate to after if(hitTile.AddDamage(num, num2) >= 100)");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.Emit(OpCodes.Ldarg_2);
            cursor.EmitDelegate((Player player, int x, int y) => OnTileBreakEvent?.Invoke(player, x, y));
        }

        public delegate void OverhealDelegate(Player player, int amount);
        public static event OverhealDelegate OverhealEvent;

        public delegate void OvermanaDelegate(Player player, int amount);
        public static event OvermanaDelegate OvermanaEvent;
        public void LoadEvents()
        {
            IL_Player.PickTile += DetectTileBreak;
            //OverhealEvent += (Player player, int amount) => Main.NewText($"{player.name} overhealed by {amount}.");
            //OvermanaEvent += (Player player, int amount) => Main.NewText($"{player.name} over-restored {amount} mana.");
        }
        public void UnloadEvents()
        {
            IL_Player.PickTile -= DetectTileBreak;
        }
    }
}
