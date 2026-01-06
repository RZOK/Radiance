using Radiance.Content.Items.BaseItems;
using Radiance.Content.Particles;
using Radiance.Core.Systems;
using Radiance.Core.Systems.ParticleSystems;

namespace Radiance.Content.Items.Accessories
{
    public class FerventMiningCharm : BaseAccessory, IInstrument, ITransmutationRecipe
    {
        public static float RADIANCE_CONSUMED => 0.02f;
        public const float PARTICLE_THRESHOLD = 0.05f;
        public const float TRAIL_THRESHOLD = 0.2f;

        public override void Load()
        {
            RadiancePlayer.MeleeEffectsEvent += PickaxeParticles;
            RadiancePlayer.OnTileBreakEvent += AddToStack;
            On_PlayerDrawLayers.DrawPlayer_27_HeldItem += DrawOutline;

            MeterInfo.Register(nameof(FerventMiningCharm),
               () => Main.LocalPlayer.IsEquipped<FerventMiningCharm>() && Main.LocalPlayer.GetModPlayer<FerventMiningCharmPlayer>().MiningBoost > 0 && Main.LocalPlayer.PlayerHeldItem().pick > 0,
               FerventMiningCharmPlayer.MAX_BOOST,
               () => Main.LocalPlayer.GetModPlayer<FerventMiningCharmPlayer>().MiningBoost,
               (progress) => Color.Lerp(CommonColors.RadianceColor1, CommonColors.RadianceColorPink, progress),
               $"{Texture}_Meter");
        }

        public override void Unload()
        {
            RadiancePlayer.MeleeEffectsEvent -= PickaxeParticles;
            RadiancePlayer.OnTileBreakEvent -= AddToStack;
            On_PlayerDrawLayers.DrawPlayer_27_HeldItem -= DrawOutline;
        }

        private void PickaxeParticles(Player player, Item item, Rectangle hitbox)
        {
            float miningBoost = player.GetModPlayer<FerventMiningCharmPlayer>().MiningBoost;
            if (miningBoost > PARTICLE_THRESHOLD)
            {
                if (player.IsEquipped<FerventMiningCharm>() && player.ItemAnimationActive && player.PlayerHeldItem().pick > 0)
                {
                    if (Main.rand.NextFloat() < Utils.GetLerpValue(PARTICLE_THRESHOLD, FerventMiningCharmPlayer.MAX_BOOST, miningBoost) * 0.25f)
                    {
                        int a = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.GoldFlame, player.velocity.X * 0.2f + (float)(player.direction * 3), player.velocity.Y * 0.2f, 100, default(Color), 0.75f);
                        Main.dust[a].noGravity = true;
                    }
                    if (Main.rand.NextFloat() < Utils.GetLerpValue(PARTICLE_THRESHOLD, FerventMiningCharmPlayer.MAX_BOOST, miningBoost) * 0.25f)
                        WorldParticleSystem.system.AddParticle(new Sparkle(new Vector2(hitbox.X, hitbox.Y) + new Vector2(Main.rand.NextFloat(hitbox.Width), Main.rand.NextFloat(hitbox.Height)), new Vector2(player.velocity.X * 0.2f + player.direction, player.velocity.Y * 0.2f), 30, new Color(200, 180, 100), 0.6f));
                }
            }
        }

        private void AddToStack(Player player, int x, int y)
        {
            if (player.active && player.IsEquipped<FerventMiningCharm>())
            {
                int tileType = Main.tile[x, y].TileType;
                if (TileID.Sets.Ore[tileType])
                {
                    Dictionary<int, int> miningStack = player.GetModPlayer<FerventMiningCharmPlayer>().miningStack;
                    player.GetModPlayer<FerventMiningCharmPlayer>().stackTimer -= 10;
                    if (miningStack.TryGetValue(tileType, out int value))
                    {
                        miningStack[tileType]++;
                        if (value > 10)
                            miningStack[tileType] = 10;
                    }
                    else
                        miningStack.Add(tileType, 1);

                    for (int i = 0; i < 3; i++)
                    {
                        WorldParticleSystem.system.AddParticle(new Sparkle(new Vector2(x, y) * 16 + Main.rand.NextVector2Square(0, 16), -Vector2.UnitY * Main.rand.NextFloat(3), 30, new Color(200, 180, 100), 0.6f));
                    }
                }
            }
        }

        private void DrawOutline(On_PlayerDrawLayers.orig_DrawPlayer_27_HeldItem orig, ref PlayerDrawSet drawinfo)
        {
            Player player = drawinfo.drawPlayer;
            float miningBoost = player.GetModPlayer<FerventMiningCharmPlayer>().MiningBoost;
            if (miningBoost > TRAIL_THRESHOLD)
            {
                if (player.active && player.IsEquipped<FerventMiningCharm>() && player.ItemAnimationActive && player.PlayerHeldItem().pick > 0)
                {
                    float rotation = player.itemRotation;
                    if (player.direction == -1)
                        rotation -= PiOver2;

                    Vector2 position = drawinfo.ItemLocation + new Vector2(player.itemWidth / 2, player.itemHeight / -2).RotatedBy(rotation);
                    Color color = new Color(75, 60, 30) * MathF.Pow(Utils.GetLerpValue(TRAIL_THRESHOLD, FerventMiningCharmPlayer.MAX_BOOST, miningBoost), 0.6f);
                    WorldParticleSystem.system.AddParticle(new PickaxeTrail(position, drawinfo.heldItem.GetItemTexture(), 12, rotation, color, drawinfo.heldItem.scale));
                }
            }
            orig(ref drawinfo);
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fervent Prospector’s Charm");
            Tooltip.SetDefault($"Mining ores temporarily increases your mining speed, up to {(int)(FerventMiningCharmPlayer.MAX_BOOST * 100f)}%");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.value = Item.sellPrice(0, 0, 50);
            Item.rare = ItemRarityID.Green;
            Item.accessory = true;
        }

        public void AddTransmutationRecipe(TransmutationRecipe recipe)
        {
            recipe.inputItems = new int[] { ItemID.AncientChisel };
            recipe.requiredRadiance = 400;
            recipe.unlock = UnlockCondition.DownedEyeOfCthulhu;
        }
    }

    public class FerventMiningCharmPlayer : ModPlayer
    {
        internal Dictionary<int, int> miningStack;
        internal int stackTimer = 0;
        internal int TotalOres => miningStack.Values.ToList().Sum();

        internal const float MAX_BOOST = 0.25f;
        internal float MiningBoost => Lerp(0f, MAX_BOOST, Math.Min(1f, (float)(Math.Pow(miningStack.Count, 0.8f) * Math.Pow(TotalOres, 0.8f) / 50)));

        public FerventMiningCharmPlayer()
        {
            miningStack = new Dictionary<int, int>();
        }

        public override void PostUpdateMiscEffects()
        {
            if (Player.IsEquipped<FerventMiningCharm>())
            {
                if (miningStack.Count > 0)
                {
                    if (Player.PlayerHeldItem().pick > 0)
                    {
                        Player.pickSpeed -= MiningBoost;
                        Player.GetAttackSpeed<MeleeDamageClass>() += MiningBoost;
                    }

                    stackTimer++;
                    if (stackTimer == 600)
                    {
                        List<int> oresToRemove = new List<int>();
                        foreach (int item in miningStack.Keys)
                        {
                            miningStack[item]--;
                            if (miningStack[item] == 0)
                                oresToRemove.Add(item);
                        }
                        oresToRemove.ForEach(x => miningStack.Remove(x));

                        if (!Player.ConsumeRadiance(FerventMiningCharm.RADIANCE_CONSUMED * TotalOres))
                            miningStack.Clear();

                        stackTimer = 0;
                    }
                }
                else
                    stackTimer = 0;
            }
            else
            {
                miningStack.Clear();
                stackTimer = 0;
            }
        }
    }
}