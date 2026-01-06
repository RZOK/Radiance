using Mono.Cecil.Cil;
using MonoMod.Cil;
using Radiance.Content.Items.Tools.Misc;
using Terraria.GameContent.ObjectInteractions;

namespace Radiance.Content.Items
{
    public class JetNotch : ModItem
    {
        public override void Load()
        {
            IL_PotionOfReturnGateInteractionChecker.DoHoverEffect += SetItemIcon;
        }

        public override void Unload()
        {
            IL_PotionOfReturnGateInteractionChecker.DoHoverEffect -= SetItemIcon;
        }

        private void SetItemIcon(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            cursor.Index = cursor.Instrs.Count;

            if (!cursor.TryGotoPrev(MoveType.After,
                i => i.MatchLdarg(1),
                i => i.MatchLdcI4(out _)))
            {
                LogIlError("Jet Notch return portal", "Couldn't navigate to before cursor item set");
                return;
            }
            cursor.EmitPop();
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate(
                (Player x) =>
            {
                if (x.TryGetModPlayer(out RadiancePlayer result))
                    return result.LastUsedReturnType;
                return ItemID.PotionOfReturn;
            });
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jet Notch");
            Tooltip.SetDefault("Teleports you home and creates a portal when socketed into a Looking Glass\nUse portal to return when you are done");
            Item.ResearchUnlockCount = 0;
            LookingGlassNotchData.LoadNotchData
                (
                Type,
                new Color(193, 94, 255),
                $"{nameof(Radiance)}/Content/ExtraTextures/LookingGlass/LookingGlass_Return",
                $"{nameof(Radiance)}/Content/ExtraTextures/LookingGlass/LookingGlass_Return_Small",
                MirrorUse,
                ChargeCost
                );
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Blue;
        }

        public void MirrorUse(Player player, LookingGlass lookingGlass)
        {
            lookingGlass.PreRecallParticles(player);
            player.DoPotionOfReturnTeleportationAndSetTheComebackPoint();
            player.GetModPlayer<RadiancePlayer>().LastUsedReturnType = Type;
            lookingGlass.PostRecallParticles(player);

            lookingGlass.NotchCount.TryGetValue(lookingGlass.CurrentSetting, out int value);
            (player.PlayerHeldItem().ModItem as LookingGlass).mirrorCharge -= lookingGlass.CurrentSetting.chargeCost(player, value);
        }

        public float ChargeCost(Player player, int identicalCount)
        {
            return 20f * MathF.Pow(1.35f, 1 - identicalCount);
        }
    }
}