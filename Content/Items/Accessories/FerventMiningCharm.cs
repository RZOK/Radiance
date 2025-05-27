using Radiance.Content.Items.BaseItems;
using Radiance.Content.Particles;
using Radiance.Core.Systems;
using Radiance.Core.Systems.ParticleSystems;

namespace Radiance.Content.Items.Accessories
{
    public class FerventMiningCharm : BaseAccessory, IInstrument, ITransmutationRecipe
    {
        public static float RADIANCE_CONSUMED => 0.02f;
        public static readonly float PARTICLE_THRESHOLD = 0.3f;
        public override void Load()
        {
            RadiancePlayer.MeleeEffectsEvent += DrawParticlesAroundPickaxe;
            RadiancePlayer.OnTileBreakEvent += AddToStack;
            On_PlayerDrawLayers.DrawPlayer_27_HeldItem += DrawOutline;
        }

        public override void Unload()
        {
            RadiancePlayer.MeleeEffectsEvent -= DrawParticlesAroundPickaxe;
            RadiancePlayer.OnTileBreakEvent -= AddToStack;
            On_PlayerDrawLayers.DrawPlayer_27_HeldItem -= DrawOutline;
        }

        private void DrawParticlesAroundPickaxe(Player player, Item item, Rectangle hitbox)
        {
            float totalBoost = player.GetModPlayer<FerventMiningCharmPlayer>().AdjustedValue;
            if (totalBoost > 0)
            {
                if (player.Equipped<FerventMiningCharm>() && player.ItemAnimationActive && player.GetPlayerHeldItem().pick > 0)
                {
                    if (Main.rand.NextBool(53 - (int)(totalBoost * 100)))
                    {
                        int a = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.GoldFlame, player.velocity.X * 0.2f + (float)(player.direction * 3), player.velocity.Y * 0.2f, 100, default(Color), 0.75f);
                        Main.dust[a].noGravity = true;
                    }
                    if (Main.rand.NextBool(53 - (int)(totalBoost * 100)))
                        WorldParticleSystem.system.AddParticle(new Sparkle(new Vector2(hitbox.X, hitbox.Y) + new Vector2(Main.rand.NextFloat(hitbox.Width), Main.rand.NextFloat(hitbox.Height)), new Vector2(player.velocity.X * 0.2f + player.direction, player.velocity.Y * 0.2f), 30, 0, new Color(200, 180, 100), 0.6f));
                }
            }
        }
        private void AddToStack(Player player, int x, int y)
        {
            if (player.active && player.Equipped<FerventMiningCharm>())
            {
                int tileType = Main.tile[x, y].TileType;
                if (TileID.Sets.Ore[tileType])
                {
                    Dictionary<int, int> miningStack = player.GetModPlayer<FerventMiningCharmPlayer>().miningStack;
                    player.GetModPlayer<FerventMiningCharmPlayer>().stackTimer -= 10;
                    if (miningStack.TryGetValue(tileType, out int value))
                    { 
                        miningStack[tileType] = ++value;
                        if (value > 10)
                            miningStack[tileType] = 10;
                    }
                    else
                        miningStack.Add(tileType, 1);

                    for (int i = 0; i < 3; i++)
                    {
                        WorldParticleSystem.system.AddParticle(new Sparkle(new Vector2(x, y) * 16 + Main.rand.NextVector2Square(0, 16), -Vector2.UnitY * Main.rand.NextFloat(3), 30, 0, new Color(200, 180, 100), 0.6f));
                    }
                    Main.NewText(player.GetModPlayer<FerventMiningCharmPlayer>().AdjustedValue);
                }
            }
        }
        private void DrawOutline(On_PlayerDrawLayers.orig_DrawPlayer_27_HeldItem orig, ref PlayerDrawSet drawinfo)
        {
            Player player = drawinfo.drawPlayer;
            float totalBoost = player.GetModPlayer<FerventMiningCharmPlayer>().AdjustedValue;
            if (totalBoost > PARTICLE_THRESHOLD)
            {
                if (player.active && player.Equipped<FerventMiningCharm>() && player.ItemAnimationActive && player.GetPlayerHeldItem().pick > 0)
                {
                    float rotation = player.itemRotation;
                    if (player.direction == -1)
                        rotation -= PiOver2;

                    WorldParticleSystem.system.AddParticle(new PickaxeTrail(drawinfo.ItemLocation + new Vector2(player.itemWidth / 2, player.itemHeight / -2).RotatedBy(rotation), drawinfo.heldItem.GetItemTexture(), 12, rotation, new Color(200, 180, 100), 255 * (1f - (totalBoost - PARTICLE_THRESHOLD) * 3f), drawinfo.heldItem.scale));
                }
            }
            orig(ref drawinfo);
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fervent Prospector’s Charm");
            Tooltip.SetDefault("Mining ores temporarily increases your mining speed, up to 100%");
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
        /// <summary>
        /// https://www.desmos.com/calculator/mboasi7h9r    
        /// </summary>
        internal float AdjustedValue => Math.Min(0.5f, (float)(Math.Pow(miningStack.Count, 0.8f) * Math.Pow(TotalOres, 0.8f) / 100));
        public FerventMiningCharmPlayer()
        {
            miningStack = new Dictionary<int, int>();
        }
        public override void PostUpdateMiscEffects()
        {
            if (Player.Equipped<FerventMiningCharm>())
            {
                if (miningStack.Count > 0)
                {
                    if (Player.GetPlayerHeldItem().pick > 0)
                    {
                        Player.pickSpeed -= AdjustedValue;
                        Player.GetAttackSpeed<MeleeDamageClass>() += AdjustedValue;
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

                        if (!Player.ConsumeRadianceOnHand(FerventMiningCharm.RADIANCE_CONSUMED * TotalOres))
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