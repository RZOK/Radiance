using Radiance.Content.Items.BaseItems;
using Radiance.Content.Particles;
using Radiance.Core.Systems;
using Terraria.UI;

namespace Radiance.Content.Items.Accessories
{
    public class DeepwaterLocket : BaseAccessory, IModPlayerTimer
    {
        internal const int SPRITES_STORED_MAX = 3;
        internal const int CHARGE_PER_SPRITE = 210;
        internal const int MAX_SPRITES_PER_NPC = 3;
        internal const int MINIMUM_MANA_REQUIRED = 40;
        public int timerCount => 1;

        public override void Load()
        {
            MeterInfo.Register(nameof(DeepwaterLocket),
                () => Main.LocalPlayer.Equipped<DeepwaterLocket>() && Main.LocalPlayer.GetTimer<DeepwaterLocket>() < CHARGE_PER_SPRITE * SPRITES_STORED_MAX,
                CHARGE_PER_SPRITE,
                () => Main.LocalPlayer.GetTimer<DeepwaterLocket>(),
                (progress) =>
                {
                    if (progress < 1f)
                        return Color.Lerp(Color.RoyalBlue, Color.DodgerBlue, progress);
                    if (progress <= 2f)
                        return Color.Lerp(Color.DodgerBlue, Color.DeepSkyBlue, progress - 1f);
                    return Color.Lerp(Color.DeepSkyBlue, Color.MediumTurquoise, progress - 2f);
                },
                $"{Texture}_Meter");

            On_ItemSlot.RightClick_ItemArray_int_int += MarkItem;
            On_Main.DrawProjectiles += DrawSprites;
        }

        public override void Unload()
        {
            On_ItemSlot.RightClick_ItemArray_int_int -= MarkItem;
        }

        private void MarkItem(On_ItemSlot.orig_RightClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
        {
            if (Main.mouseRight && Main.mouseRightRelease && !inv[slot].IsAir && !Main.LocalPlayer.ItemAnimationActive)
            {
                if (!Main.mouseItem.IsAir && Main.mouseItem.type == Type && inv[slot].DamageType == DamageClass.Magic)
                {
                    SoundEngine.PlaySound(SoundID.Grab);
                    inv[slot].GetGlobalItem<DeepwaterLocketItem>().deepWaterMark = !inv[slot].GetGlobalItem<DeepwaterLocketItem>().deepWaterMark;
                    return;
                }
            }
            orig(inv, context, slot);
        }

        private void DrawSprites(On_Main.orig_DrawProjectiles orig, Main self)
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            foreach (DeepwaterBlob particle in Main.LocalPlayer.GetModPlayer<DeepwaterLocketPlayer>().particles)
            {
                particle.SpecialDraw(Main.spriteBatch, particle.position - Main.screenPosition);
            }

            Main.spriteBatch.End();
            orig(self);
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Deepwater Locket");
            Tooltip.SetDefault("Dealing damage with unmarked magic weapons summons sprites\nMarked weapons consume sprites to restore mana\nRight Click this item over a weapon to mark it");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 36;
            Item.value = Item.sellPrice(0, 0, 50);
            Item.rare = ItemRarityID.Green;
            Item.accessory = true;
        }

        public override void SafeUpdateAccessory(Player player, bool hideVisual)
        {
            int timer = (int)player.GetTimer<DeepwaterLocket>();
            if (timer < CHARGE_PER_SPRITE * SPRITES_STORED_MAX)
                player.IncrementTimer<DeepwaterLocket>();
        }
    }

    public class DeepwaterLocketPlayer : ModPlayer
    {
        internal Dictionary<NPC, List<DeepwaterSprite>> markedNPCS = new Dictionary<NPC, List<DeepwaterSprite>>();
        internal List<DeepwaterSprite> returningSprites = new List<DeepwaterSprite>();
        internal List<DeepwaterBlob> particles = new List<DeepwaterBlob>();

        public override void UpdateDead()
        {
            markedNPCS.Clear();
            returningSprites.Clear();
        }

        public override void PostUpdateEquips()
        {
            List<NPC> npcsToRemove = new List<NPC>();
            foreach (var kvp in markedNPCS)
            {
                NPC npc = kvp.Key;
                List<DeepwaterSprite> sprites = kvp.Value;
                if (!npc.active)
                {
                    //refund some charge if the npc dies preemptively
                    Player.IncrementTimer<DeepwaterLocket>(sprites.Count * DeepwaterLocket.CHARGE_PER_SPRITE * DeepwaterLocket.SPRITES_STORED_MAX / 2);
                    if (Player.GetTimer<DeepwaterLocket>() > DeepwaterLocket.CHARGE_PER_SPRITE * DeepwaterLocket.SPRITES_STORED_MAX)
                        Player.SetTimer<DeepwaterLocket>(DeepwaterLocket.CHARGE_PER_SPRITE * DeepwaterLocket.SPRITES_STORED_MAX);

                    npcsToRemove.Add(npc);
                    continue;
                }
                for (int i = 0; i < sprites.Count; i++)
                {
                    DeepwaterSprite sprite = sprites[i];
                    sprite.Update(npc, i, sprites.Count);
                }
            }
            foreach (NPC npc in npcsToRemove)
            {
                markedNPCS.Remove(npc);
            }

            List<DeepwaterSprite> spritesToRemove = new List<DeepwaterSprite>();
            foreach (DeepwaterSprite sprite in returningSprites)
            {
                sprite.UpdateReturning();
                if (sprite.returningTicks >= DeepwaterSprite.TICKS_UNTIL_RETURNED)
                {
                    spritesToRemove.Add(sprite);
                    int manaIncrease = (int)(Player.statManaMax2 / 3f * Main.rand.NextFloat(0.8f, 1.2f));
                    Player.statMana += manaIncrease;
                    Player.ManaEffect(manaIncrease);
                }
            }
            returningSprites.RemoveAll(spritesToRemove.Contains);

            foreach (DeepwaterBlob particle in particles)
            {
                if (particle == null)
                    continue;

                particle.timeLeft--;
                particle.Update();
                particle.position += particle.velocity;
            }
            particles.RemoveAll(x => x.timeLeft <= 0);
        }
    }

    public class DeepwaterLocketProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public bool spriteStealer = false;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (source is EntitySource_ItemUse { Item: Item item })
            {
                if (item.GetGlobalItem<DeepwaterLocketItem>().deepWaterMark)
                    spriteStealer = true;
            }
            else if (source is EntitySource_Parent { Entity: Projectile parentProjectile })
                spriteStealer |= parentProjectile.GetGlobalProjectile<DeepwaterLocketProjectile>().spriteStealer;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.LocalPlayer;
            if (projectile.owner == player.whoAmI && projectile.DamageType == DamageClass.Magic && !target.immortal)
            {
                Dictionary<NPC, List<DeepwaterSprite>> markedNPCS = player.GetModPlayer<DeepwaterLocketPlayer>().markedNPCS;
                if (!spriteStealer && player.GetTimer<DeepwaterLocket>() > DeepwaterLocket.CHARGE_PER_SPRITE)
                {
                    if (!markedNPCS.TryGetValue(target, out _))
                        markedNPCS.Add(target, new List<DeepwaterSprite>());
                    if (markedNPCS[target].Count < DeepwaterLocket.MAX_SPRITES_PER_NPC)
                    {
                        markedNPCS[target].Add(new DeepwaterSprite(target.Center));
                        player.IncrementTimer<DeepwaterLocket>(-DeepwaterLocket.CHARGE_PER_SPRITE);
                    }
                }
                else if (spriteStealer && markedNPCS.TryGetValue(target, out List<DeepwaterSprite> sprites) && sprites.Count > 0)
                    player.GetModPlayer<DeepwaterLocketPlayer>().returningSprites.Add(sprites.Pop());
            }
        }
    }

    public class DeepwaterLocketItem : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public bool deepWaterMark = false;

        public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
        {
            if (deepWaterMark)
                damage *= 0.5f;
        }

        public override bool CanUseItem(Item item, Player player)
        {
            if (deepWaterMark && player.statMana < DeepwaterLocket.MINIMUM_MANA_REQUIRED)
                return false;

            return true;
        }

        public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (deepWaterMark)
            {
                float slotScale = 1f;
                if (frame.Width > 32 || frame.Height > 32)
                {
                    if (frame.Width > frame.Height)
                        slotScale = 32f / frame.Width;
                    else
                        slotScale = 32f / frame.Height;
                }
                slotScale *= Main.inventoryScale;
                Texture2D texture = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/Items/Accessories/DeepwaterLocket_Meter").Value;
                spriteBatch.Draw(texture, position + frame.Size() * slotScale * 0.15f, null, drawColor, 0, Vector2.Zero, Main.inventoryScale, SpriteEffects.None, 0);
            }
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (deepWaterMark)
            {
                List<TooltipLine> tooltipLines = tooltips.Where(x => x.Name.StartsWith("Tooltip") && x.Mod == "Terraria").ToList();
                if (tooltipLines.Count == 0)
                {
                    tooltipLines = tooltips.Where(x => x.Name == ("UseMana") && x.Mod == "Terraria").ToList();
                    if (tooltipLines.Count == 0) // failsafe in case a magic weapon uses... no mana?
                        tooltipLines = tooltips;
                }
                TooltipLine markLine = new(Mod, "DeepwaterMark", $"[Mark of the Deepwater]\n-50% damage\nCannot be used below {DeepwaterLocket.MINIMUM_MANA_REQUIRED} mana\nConverts Deepwater sprites surrounding enemies into mana");
                markLine.OverrideColor = Color.DeepSkyBlue;
                tooltips.Insert(tooltips.FindIndex(x => x == tooltipLines.Last()) + 1, markLine);
            }
        }
    }

    internal class DeepwaterSprite(Vector2 position)
    {
        public Vector2 position = position;
        private Vector2? initialPosition;
        private Vector2? curveOffset;
        internal float returningTicks = 0;
        internal static float TICKS_UNTIL_RETURNED = 30;

        public void Update(NPC npc, int index, int max)
        {
            Vector2 idealPosition = npc.Center + Vector2.UnitX.RotatedBy((Main.GameUpdateCount / 100f) + npc.whoAmI * 5 + TwoPi / max * (float)index) * (npc.width / 2f + 32f);
            position = Vector2.Lerp(position, idealPosition, 0.3f);
            if ((Main.GameUpdateCount + index) % 3 == 0)
                SpawnParticles();
        }

        public void UpdateReturning()
        {
            if (!initialPosition.HasValue)
                initialPosition = position;
            if (!curveOffset.HasValue)
            {
                curveOffset = Main.LocalPlayer.Center.DirectionTo(initialPosition.Value).RotatedBy(PiOver4);
                if (Main.rand.NextBool())
                    curveOffset = curveOffset.Value.RotatedBy(-PiOver2);

                curveOffset *= Math.Max(64, initialPosition.Value.Distance(Main.LocalPlayer.Center));
            }

            Vector2 curvePoint = ((Main.LocalPlayer.Center - initialPosition.Value) / 2f) + curveOffset.Value;
            float modifiedProgress = returningTicks / TICKS_UNTIL_RETURNED;
            if (Main.GameUpdateCount % 2 == 0)
                SpawnParticles();

            position = Vector2.Hermite(initialPosition.Value, curvePoint, Main.LocalPlayer.Center, -curvePoint, modifiedProgress);
            returningTicks++;
        }

        private void SpawnParticles()
        {
            Main.LocalPlayer.GetModPlayer<DeepwaterLocketPlayer>().particles.Add(new DeepwaterBlob(position, Main.rand.NextVector2Circular(1, 1) * MathF.Pow(Main.rand.NextFloat(), 2.5f), 30, 1f));
        }
    }
}