using MonoMod.RuntimeDetour;
using Radiance.Content.Particles;
using Radiance.Content.Tiles.Pedestals;
using Radiance.Content.UI;
using Radiance.Core.Systems;
using ReLogic.Content;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;
using Terraria.Map;
using Terraria.ModLoader.Config;
using Terraria.UI;
using static Radiance.Core.Visuals.RadianceDrawing;

namespace Radiance.Content.Items.Tools.Misc
{
    public class LookingGlass : ModItem, IPostSetupContentLoadable, IPedestalItem
    {
        internal const string MIRROR_KEY = $"{nameof(Radiance)}.Recall";
        private const int MAX_SLOTS = 6;
        private const int VISUAL_TIMER_MAX = 45;
        internal const float MIRROR_CHARGE_MAX = 100f;
        internal const float MAX_RECHARGE_PER_PYLON = 25f;

        private Vector2 visualPosition;
        private int visualTimer = 0;
        internal ItemDefinition[] notches;
        private LookingGlassNotchData _currentSetting;
        public float mirrorCharge = 0f;

        internal TileDefinition markedPylon;
        internal Point16 markedPylonPosition;
        internal int markedPylonStyle;

        internal static FieldInfo pylonsInWorld;

        internal static Dictionary<int, Asset<Texture2D>> moddedPylonTextures = new Dictionary<int, Asset<Texture2D>>();
        private static Hook ModPylon_DefaultDrawMapIcon_Hook;

        private delegate bool SaveModdedPylonIconDelegate(ModPylon pylon, ref MapOverlayDrawContext context, Asset<Texture2D> mapIcon, Vector2 drawCenter, Color drawColor, float deselectedScale, float selectedScale);

        public Dictionary<LookingGlassNotchData, int> NotchCount
        {
            get
            {
                Dictionary<LookingGlassNotchData, int> notchCount = new Dictionary<LookingGlassNotchData, int>();
                foreach (ItemDefinition notch in notches)
                {
                    if (notch is null)
                        continue;

                    LookingGlassNotchData associatedData = LookingGlassNotchData.loadedNotches.FirstOrDefault(x => x.type == notch.Type);
                    if (associatedData is null)
                        continue;

                    if (!notchCount.TryAdd(associatedData, 1))
                        notchCount[associatedData]++;
                }
                return notchCount;
            }
        }

        public LookingGlassNotchData CurrentSetting
        {
            get
            {
                _currentSetting ??= LookingGlassNotchData.loadedNotches.First(x => x.type == Type);
                return _currentSetting;
            }
            set => _currentSetting = value;
        }

        public static float MaxRecharge
        {
            get
            {
                int pylonCount = ((List<TeleportPylonInfo>)pylonsInWorld.GetValue(Main.PylonSystem)).Count;
                return Min(MIRROR_CHARGE_MAX, pylonCount * MAX_RECHARGE_PER_PYLON);
            }
        }

        public override void Load()
        {
            On_ItemSlot.RightClick_ItemArray_int_int += AddNotchToMirror;

            pylonsInWorld = typeof(TeleportPylonsSystem).GetField("_pylons", BindingFlags.Instance | BindingFlags.NonPublic);
            ModPylon_DefaultDrawMapIcon_Hook ??= new Hook(typeof(ModPylon).GetMethod("DefaultDrawMapIcon"), SaveModdedPylonIcon);

            if (!ModPylon_DefaultDrawMapIcon_Hook.IsApplied)
                ModPylon_DefaultDrawMapIcon_Hook.Apply();
        }

        public override void Unload()
        {
            On_ItemSlot.RightClick_ItemArray_int_int -= AddNotchToMirror;
            if (ModPylon_DefaultDrawMapIcon_Hook.IsApplied)
                ModPylon_DefaultDrawMapIcon_Hook.Undo();
        }

        private void AddNotchToMirror(On_ItemSlot.orig_RightClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
        {
            if (Main.mouseRight && Main.mouseRightRelease && !inv[slot].IsAir && inv[slot].type == Type && !Main.LocalPlayer.ItemAnimationActive)
            {
                LookingGlass mirror = inv[slot].ModItem as LookingGlass;
                if (!Main.mouseItem.IsAir && LookingGlassNotchData.loadedNotches.Any(x => x.type == Main.mouseItem.type) && Main.mouseItem.type != Type)
                {
                    for (int i = 0; i < mirror.notches.Length; i++)
                    {
                        ItemDefinition insertedNotch = mirror.notches[i];
                        if (insertedNotch is null || insertedNotch.Type == ItemID.None)
                        {
                            mirror.notches[i] = new ItemDefinition(Main.mouseItem.Clone().type);
                            Main.mouseItem.ConsumeOne();

                            SoundEngine.PlaySound(SoundID.Grab);
                            return;
                        }
                    }
                }
            }
            orig(inv, context, slot);
        }

        // since modded pylons use custom textures that aren't consistently defined anywhere, we have to do this jank method of saving the texture and drawing from that when in a pedestal
        private bool SaveModdedPylonIcon(SaveModdedPylonIconDelegate orig, ModPylon self, ref MapOverlayDrawContext context, Asset<Texture2D> mapIcon, Vector2 drawCenter, Color drawColor, float deselectedScale, float selectedScale)
        {
            if (!moddedPylonTextures.ContainsKey(self.Type))
                moddedPylonTextures[self.Type] = mapIcon;

            return orig(self, ref context, mapIcon, drawCenter, drawColor, deselectedScale, selectedScale);
        }

        public void PostSetupContent()
        {
            RadialUICursorSystem.radialUICursorData.Add(new RadialUICursorData(LookingGlassUI.Instance, 1f, DrawLookingGlassMouseUI));
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Looking Glass");
            Tooltip.SetDefault("Gaze in the mirror to return home\nCan be enhanced with Mirror Notches");
            Item.ResearchUnlockCount = 1;
            LookingGlassNotchData.LoadNotchData
              (
              Type,
              new Color(152, 140, 255),
              $"{nameof(Radiance)}/Content/ExtraTextures/LookingGlass/LookingGlass_Recall",
              $"{nameof(Radiance)}/Content/ExtraTextures/LookingGlass/LookingGlass_Recall_Small",
              MirrorUse,
              ChargeCost,
              MIRROR_KEY
              );
        }

        public override void SetDefaults()
        {
            notches = new ItemDefinition[MAX_SLOTS];

            Item.width = 28;
            Item.height = 28;
            Item.maxStack = 1;
            Item.value = Item.sellPrice(0, 0, 1, 0);
            Item.rare = ItemRarityID.Green;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 90;
            Item.UseSound = SoundID.Item6;
            Item.useAnimation = 90;
        }

        public override bool CanUseItem(Player player)
        {
            NotchCount.TryGetValue(CurrentSetting, out int value);
            return mirrorCharge >= CurrentSetting.chargeCost(player, value);
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.ItemTimeIsZero)
                player.SetItemTime(90);

            if (player.itemTime == player.itemTimeMax / 2)
            {
                CurrentSetting.mirrorUse(player, this);
            }
        }

        public override void HoldItem(Player player)
        {
            if (RadialUI.CanOpenRadialUI(player) && !LookingGlassUI.Instance.visible)
            {
                Main.mouseRightRelease = false;
                LookingGlassUI.Instance.EnableRadialUI();
            }
            if (player.ItemAnimationActive && player.itemTime >= player.itemTimeMax / 2)
            {
                float progress = 2f * (1f - player.itemAnimation / (float)player.itemAnimationMax);
                Rectangle playerRect = new Rectangle((int)player.position.X, (int)(player.position.Y + player.height * (1f - progress)), player.width, (int)(4));
                playerRect.Inflate(4, 4);
                Vector2 particlePos = Main.rand.NextVector2FromRectangle(playerRect);
                if (Main.GameUpdateCount % 5 == 0)
                    ParticleSystem.AddParticle(new Sparkle(particlePos, Vector2.UnitY * -Main.rand.NextFloat(1f, 3f) * progress, Main.rand.Next(30, 60), CurrentSetting.color, 0.6f + (0.2f * progress)));
            }
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            visualPosition = position;
            if (Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().realHoveredItem == Item && Main.mouseItem != Item && Main.mouseItem.IsAir)
            {
                if (visualTimer < VISUAL_TIMER_MAX && !Main.gamePaused && Main.hasFocus)
                    visualTimer++;
            }
            else
                visualTimer = 0;

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TooltipLine meterLine = new(Mod, "ChargeMeter", ".");
            tooltips.Insert(tooltips.FindIndex(x => x == tooltips.Last(x => x.Name.StartsWith("Tooltip") && x.Mod == "Terraria")) + 1, meterLine);
        }

        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            if (line.Name == "ChargeMeter")
            {
                NotchCount.TryGetValue(CurrentSetting, out int value);
                DrawMirrorChargeBar(Main.spriteBatch, new Vector2(line.X, line.Y), 1f, CurrentSetting.chargeCost(Main.LocalPlayer, value));
                return false;
            }
            return true;
        }

        public override void PostDrawTooltip(ReadOnlyCollection<DrawableTooltipLine> lines)
        {
            if (notches is null)
                return;

            for (int i = 0; i < MAX_SLOTS; i++)
            {
                DrawNotchSlot(Main.spriteBatch, visualPosition, i);
            }
        }

        public override void LoadData(TagCompound tag)
        {
            List<ItemDefinition> loadableSlots = (List<ItemDefinition>)tag.GetList<ItemDefinition>(nameof(notches));
            for (int i = 0; i < notches.Length; i++)
            {
                if (notches[i] is null || notches[i].Type == ItemID.None)
                    loadableSlots.Add(null);
                else
                    loadableSlots.Add(notches[i]);
            }
            notches = loadableSlots.ToArray();
            if (notches.Length != MAX_SLOTS)
                Array.Resize(ref notches, MAX_SLOTS);
        }

        public override void SaveData(TagCompound tag)
        {
            List<ItemDefinition> saveableSlots = new List<ItemDefinition>();
            for (int i = 0; i < notches.Length; i++)
            {
                if (notches[i] is null)
                    saveableSlots.Add(new ItemDefinition(ItemID.None));
                else
                    saveableSlots.Add(notches[i]);
            }
            tag[nameof(notches)] = saveableSlots;
        }

        public void DrawNotchSlot(SpriteBatch spriteBatch, Vector2 position, int index)
        {
            Texture2D tex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/ExtraTextures/LookingGlass_Slot").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/ExtraTextures/LookingGlass_SlotGlow").Value;
            Texture2D underlayTex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/ExtraTextures/LookingGlass_SlotBackground").Value;
            float completion = visualTimer / (float)VISUAL_TIMER_MAX;
            float adjustedCompletion = EaseOutExponent(completion, 6f);
            float alphaCompletion = EaseOutExponent(completion, 3f);
            float startingAngle = PiOver2;
            float endingAngle = TwoPi;
            float currentAngle = Lerp(startingAngle, endingAngle, index / (MAX_SLOTS - 1f));

            Vector2 floating = new Vector2(2 * SineTiming(45 + index, MathF.Pow(index + 1f, index + 1f)), 2 * SineTiming(45 + index, 30f + MathF.Pow(index + 1f, index + 1f)));
            floating.X -= floating.X % 0.5f;
            floating.Y -= floating.Y % 0.5f;
            if (Main.keyState.PressingShift())
                floating = Vector2.Zero;

            int distance = (int)(48f * adjustedCompletion);
            int underDistance = (int)(40f * adjustedCompletion);
            Vector2 drawPos = position + Vector2.UnitX.RotatedBy(currentAngle) * distance + floating;
            Vector2 underDrawPos = position + Vector2.UnitX.RotatedBy(currentAngle) * underDistance + floating;
            float scale = Math.Clamp(adjustedCompletion + 0.3f, 0.3f, 1);

            float vfxModifier = SineTiming(120f, index * 20f) * 0.5f + 0.5f;
            float glowModifier = Lerp(0.7f, 1.2f, MathF.Pow(vfxModifier, 1.3f));
            float glowScaleModifier = Lerp(1f, 1.05f, vfxModifier);
            LookingGlassNotchData notch = null;
            ItemDefinition notchItemDef = notches[index];
            Color color = Color.White;
            if (notchItemDef is not null && notchItemDef.Type != ItemID.None)
            {
                notch = LookingGlassNotchData.loadedNotches.FirstOrDefault(x => x.type == notches[index].Type);
                if (notch is not null)
                    color = notch.color;

                spriteBatch.Draw(underlayTex, drawPos, null, color * alphaCompletion * glowModifier, 0, glowTex.Size() / 2f, scale * glowScaleModifier, SpriteEffects.None, 0f);
            }
            spriteBatch.Draw(tex, underDrawPos, null, color * alphaCompletion * 0.25f, 0, tex.Size() / 2f, scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, color * alphaCompletion, 0, tex.Size() / 2f, scale, SpriteEffects.None, 0f);

            if (notchItemDef is not null && notchItemDef.Type != ItemID.None)
            {
                Texture2D itemTex;
                if (notchItemDef.IsUnloaded)
                    itemTex = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/Missing").Value;
                else
                    itemTex = GetItemTexture(notch.type);

                spriteBatch.Draw(itemTex, drawPos, null, Color.White * alphaCompletion, 0, itemTex.Size() / 2f, scale, SpriteEffects.None, 0f);
            }
        }

        private static void DrawLookingGlassMouseUI(SpriteBatch spriteBatch, float opacity)
        {
            LookingGlass lookingGlass = Main.LocalPlayer.HeldItem.ModItem as LookingGlass;
            if (lookingGlass is not null)
            {
                Texture2D tex = ModContent.Request<Texture2D>(lookingGlass.CurrentSetting.smallTex).Value;
                float itemOffset = 0f;
                int cursorItemType = Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().realCursorItemType;
                if (cursorItemType != ItemID.None)
                    itemOffset = GetItemTexture(cursorItemType).Width / -2f;

                Vector2 offset = new Vector2(itemOffset, 24f);
                Vector2 position = Main.MouseScreen + offset;
                Color color = Color.White;
                if (lookingGlass.CurrentSetting.type != ModContent.ItemType<LookingGlass>() && lookingGlass.mirrorCharge < lookingGlass.CurrentSetting.chargeCost(Main.LocalPlayer, lookingGlass.NotchCount[lookingGlass._currentSetting]))
                    color = new Color(100, 100, 100);

                spriteBatch.Draw(tex, position, null, color * opacity, 0, tex.Size() / 2, 1f, SpriteEffects.None, 0);
            }
        }

        public void PreRecallParticles(Player player)
        {
            int particleCount = 30;
            for (int i = 0; i < particleCount; i++)
            {
                Rectangle playerRect = new Rectangle((int)player.position.X, (int)(player.position.Y) + 8, player.width, player.height);
                playerRect.Inflate(4, 4);
                Vector2 particlePos = Main.rand.NextVector2FromRectangle(playerRect);
                float modifier = MathF.Pow(Main.rand.NextFloat(), 2.5f);
                Vector2 velocity = Vector2.UnitY * -(1f + 10f * modifier);

                ParticleSystem.AddParticle(new GlowSpeck(particlePos, velocity, (int)(30f + 45f * modifier), CurrentSetting.color, Main.rand.NextFloat(0.9f, 1.5f)));
            }
        }

        public void PostRecallParticles(Player player)
        {
            int particleCount = 30;
            for (int i = 0; i < particleCount; i++)
            {
                Vector2 particlePos = player.Center + (new Vector2(Main.rand.Next(-35, 36), player.height / 2f + Main.rand.Next(-3, 4)));
                float modifier = MathF.Pow(Main.rand.NextFloat(), 2.5f);
                Vector2 velocity = Vector2.UnitY * -(1f + 10f * modifier);
                ParticleSystem.AddParticle(new GlowSpeck(particlePos, velocity, (int)(30f + 45f * modifier), CurrentSetting.color, Main.rand.NextFloat(0.9f, 1.5f)));
            }
        }

        public void MirrorUse(Player player, LookingGlass lookingGlass)
        {
            PreRecallParticles(player);
            player.Spawn(PlayerSpawnContext.RecallFromItem);
            PostRecallParticles(player);
        }

        public static float ChargeCost(Player player, int identicalCount) => 0;

        public void PreUpdatePedestal(PedestalTileEntity pte)
        { }

        public void UpdatePedestal(PedestalTileEntity pte)
        {
            int pylonCount = ((List<TeleportPylonInfo>)pylonsInWorld.GetValue(Main.PylonSystem)).Count;
            if (mirrorCharge < MaxRecharge)
                mirrorCharge += 0.005f * MathF.Pow(pylonCount, 1.4f);

            mirrorCharge = Clamp(mirrorCharge, 0f, MIRROR_CHARGE_MAX);
            if (mirrorCharge < MaxRecharge && Main.GameUpdateCount % (600 / pylonCount) == 0)
            {
                Vector2 itemCenter = pte.FloatingItemCenter(Item);
                Vector2 pos = itemCenter - Vector2.UnitY.RotatedByRandom(PiOver2) * Main.rand.NextFloat(100f, 200f);
                ParticleSystem.AddParticle(new TravelSparkle(pos, itemCenter, 300, new Color(248, 150, 255), Main.rand.NextFloat(0.6f, 0.8f)));
            }
        }

        public List<HoverUIElement> GetHoverData(PedestalTileEntity pte)
        {
            return new List<HoverUIElement>() { new LookingGlassHoverUI(Vector2.UnitY * -56f), new MirrorChargeHoverUI(Vector2.UnitY * 24f, this) };
        }

        public void DrawMirrorChargeBar(SpriteBatch spriteBatch, Vector2 position, float scale, float threshold = 0, Color? color = null, AnchorStyle anchorStyle = AnchorStyle.TopLeft)
        {
            Texture2D meterTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/MirrorCharge_Meter").Value;
            Texture2D barTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/MirrorCharge_Bar").Value;
            Texture2D thresholdBarTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/MirrorCharge_ThresholdBar").Value;

            int meterWidth = meterTexture.Width;
            Vector2 padding = (meterTexture.Size() - barTexture.Size()) / 2 + new Vector2(5, -1);
            int barWidth = barTexture.Width;
            int barHeight = barTexture.Height;
            float fill = mirrorCharge / MIRROR_CHARGE_MAX;
            float thresholdFill = threshold / MIRROR_CHARGE_MAX;
            float minThreshold = 6f / barWidth;
            if (fill > 0 && fill < minThreshold)
                fill = minThreshold;

            Color flickerColor = new Color(235, 240, 255);
            if (thresholdFill > 1f)
            {
                flickerColor = new Color(255, 102, 150);
                thresholdFill = 1f;
            }

            switch (anchorStyle)
            {
                case AnchorStyle.TopLeft:
                    position += meterTexture.Size() / 2f;
                    break;

                case AnchorStyle.Top:
                    position += Vector2.UnitY * meterTexture.Height / 2f;
                    break;
            }
            if (!color.HasValue)
                color = Color.White;

            Main.spriteBatch.Draw(meterTexture, new Vector2(position.X, position.Y), null, color.Value, 0, meterTexture.Size() / 2f, scale, SpriteEffects.None, 0);
            if (thresholdFill > 0 && fill < thresholdFill)
            {
                int ticksPerFlicker = 240;
                int flickerDowntime = 30;
                if (Main.GameUpdateCount % ticksPerFlicker > flickerDowntime && Main.GameUpdateCount % ticksPerFlicker < ticksPerFlicker - flickerDowntime)
                {
                    float completion = (Main.GameUpdateCount % ticksPerFlicker - flickerDowntime) / (ticksPerFlicker - flickerDowntime * 2f);
                    if (completion > 0.5f)
                        completion = 1f - completion;

                    completion *= 2f;
                    float thresholdAlpha = completion * 0.5f;

                    flickerColor *= thresholdAlpha;
                    Main.spriteBatch.Draw(thresholdBarTexture, new Vector2(position.X, position.Y) + padding, new Rectangle(0, 0, Math.Max((int)(thresholdFill * (barWidth - 2f)), 0), barHeight), flickerColor, 0, meterTexture.Size() / 2f, scale, SpriteEffects.None, 0);
                    Main.spriteBatch.Draw(thresholdBarTexture, new Vector2(position.X + (int)((barWidth - 2f) * thresholdFill), position.Y) + padding, new Rectangle(barWidth - 2, 0, 2, barHeight), flickerColor, 0, meterTexture.Size() / 2f, scale, SpriteEffects.None, 0);
                }
            }

            Main.spriteBatch.Draw(barTexture, new Vector2(position.X, position.Y) + padding, new Rectangle(0, 0, Math.Max((int)(fill * (barWidth - 2f)), 0), barHeight), color.Value, 0, meterTexture.Size() / 2f, scale, SpriteEffects.None, 0);
            if (fill > 0)
                Main.spriteBatch.Draw(barTexture, new Vector2(position.X + (int)((barWidth - 2f) * fill), position.Y) + padding, new Rectangle(barWidth - 2, 0, 2, barHeight), color.Value, 0, meterTexture.Size() / 2f, scale, SpriteEffects.None, 0);
        }
    }

    public class LookingGlassHoverUI : HoverUIElement
    {
        public LookingGlassHoverUI(Vector2 targetPosition) : base("ExistingPylons")
        {
            this.targetPosition = targetPosition;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D backgroundTex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/Items/Tools/Misc/LookingGlass_PylonBackground").Value;
            List<TeleportPylonInfo> pylons = ((List<TeleportPylonInfo>)LookingGlass.pylonsInWorld.GetValue(Main.PylonSystem)).OrderBy(x => x.TypeOfPylon).ToList();
            if (pylons.Count == 0)
                return;

            int distanceBetweenItems = 32;
            int width = pylons.Count * distanceBetweenItems;
            int height = 38;
            Vector2 drawPos = realDrawPosition - new Vector2(width / 2f, height / 2f);
            if (Main.SettingsEnabled_OpaqueBoxBehindTooltips)
                RadianceDrawing.DrawInventoryBackground(Main.spriteBatch, backgroundTex, (int)drawPos.X - 8, (int)drawPos.Y - 14, width + 16, height + 10, Color.White * timerModifier * 0.8f);

            for (int i = 0; i < pylons.Count; i++)
            {
                Texture2D pylonTex = TextureAssets.Extra[ExtrasID.PylonMapIcons].Value;
                TeleportPylonInfo pylon = pylons[i];
                Vector2 pos = new Vector2(drawPos.X + 16 + distanceBetweenItems * i, drawPos.Y + 10);
                Rectangle? rect = new Rectangle(30 * (int)pylon.TypeOfPylon, 0, 28, 38);
                Vector2 origin = rect.Value.Size() / 2f;

                if (pylon.ModPylon is not null)
                {
                    if (LookingGlass.moddedPylonTextures.TryGetValue(pylon.ModPylon.Type, out Asset<Texture2D> tex))
                    {
                        pylonTex = tex.Value;
                        rect = null;
                        origin = tex.Size() / 2f;
                    }
                }
                Main.spriteBatch.Draw(pylonTex, pos, rect, Color.White * timerModifier, 0, origin, 1f, SpriteEffects.None, 0);
            }
        }
    }

    public class MirrorChargeHoverUI : HoverUIElement
    {
        public LookingGlass lookingGlass;

        public MirrorChargeHoverUI(Vector2 targetPosition, LookingGlass lookingGlass) : base("MirrorCharge")
        {
            this.lookingGlass = lookingGlass;
            this.targetPosition = targetPosition;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Vector2 modifier = new Vector2(2 * SineTiming(33), -2 * SineTiming(55));
            if (Main.keyState.PressingShift())
                modifier = Vector2.Zero;

            lookingGlass.DrawMirrorChargeBar(spriteBatch, realDrawPosition + modifier, 1f, LookingGlass.MaxRecharge, Color.White * timerModifier, AnchorStyle.Top);
        }
    }

    public class LookingGlassUI : RadialUI
    {
        public static LookingGlassUI Instance;

        public LookingGlassUI()
        {
            Instance = this;
        }

        public override bool Active() => Main.LocalPlayer.PlayerHeldItem().type == ModContent.ItemType<LookingGlass>();

        public override List<RadialUIElement> GetElementsToDraw()
        {
            List<RadialUIElement> elements = new List<RadialUIElement>();
            LookingGlass lookingGlass = Main.LocalPlayer.PlayerHeldItem().ModItem as LookingGlass;
            List<LookingGlassNotchData> uniqueDataInGlass = new List<LookingGlassNotchData>();
            List<LookingGlassNotchData> loadedNotches = LookingGlassNotchData.loadedNotches.OrderBy(x => GetItem(x.type).rare).ToList();

            foreach (LookingGlassNotchData notchData in loadedNotches)
            {
                if (notchData.type == ModContent.ItemType<LookingGlass>())
                    continue;

                if (lookingGlass.notches.Any(x => x is not null && x.Type == notchData.type))
                    uniqueDataInGlass.Add(notchData);
            }

            for (int i = 0; i < uniqueDataInGlass.Count; i++)
            {
                LookingGlassNotchData currentNotch = uniqueDataInGlass[i];
                elements.Add(new RadialUIElement(
                    this,
                    currentNotch.bigTex,
                    lookingGlass.CurrentSetting == currentNotch,
                    () => lookingGlass.CurrentSetting = currentNotch,
                    lookingGlass.mirrorCharge < currentNotch.chargeCost(Main.LocalPlayer, lookingGlass.NotchCount[currentNotch])
                    ));
            }
            return elements;
        }

        public override RadialUIElement GetCenterElement()
        {
            LookingGlass lookingGlass = Main.LocalPlayer.PlayerHeldItem().ModItem as LookingGlass;
            LookingGlassNotchData centerNotch = LookingGlassNotchData.loadedNotches.First(x => x.type == ModContent.ItemType<LookingGlass>());
            return new RadialUIElement(
                this,
                centerNotch.bigTex,
                lookingGlass.CurrentSetting == centerNotch,
                () => lookingGlass.CurrentSetting = centerNotch
            );
        }
    }

    public class LookingGlassNotchData
    {
        public Color color;
        public string id;
        public string bigTex;
        public string smallTex;
        public int type;
        public Action<Player, LookingGlass> mirrorUse;
        public Func<Player, int, float> chargeCost;
        internal static List<LookingGlassNotchData> loadedNotches = new List<LookingGlassNotchData>();

        private LookingGlassNotchData(string id, int type, Color color, string bigTex, string smallTex, Action<Player, LookingGlass> mirrorUse, Func<Player, int, float> chargeCost)
        {
            this.id = id;
            this.type = type;
            this.color = color;
            this.bigTex = bigTex;
            this.smallTex = smallTex;
            this.mirrorUse = mirrorUse;
            this.chargeCost = chargeCost;
        }

        public static void LoadNotchData(int type, Color color, string bigTex, string smallTex, Action<Player, LookingGlass> mirrorUse, Func<Player, int, float> chargeCost, string overrideID = null)
        {
            Item item = GetItem(type);
            string id = item.ModItem.FullName;
            if (overrideID is not null)
                id = overrideID;

            loadedNotches.Add(new LookingGlassNotchData(id, type, color, bigTex, smallTex, mirrorUse, chargeCost));
        }
    }
}