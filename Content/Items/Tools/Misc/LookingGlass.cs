using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.Materials;
using Radiance.Content.Particles;
using Radiance.Content.UI;
using Radiance.Core.Systems.ParticleSystems;
using System.Collections.ObjectModel;
using System.Runtime.Intrinsics.X86;
using Terraria.ModLoader.Config;
using Terraria.UI;

namespace Radiance.Content.Items.Tools.Misc
{
    public class LookingGlass : ModItem, IPostSetupContentLoadable
    {
        internal const string MIRROR_KEY = "Radiance.Recall";
        private const int MAX_SLOTS = 6;
        private const int VISUAL_TIMER_MAX = 45;

        private Vector2 visualPosition;
        private int visualTimer = 0;
        internal ItemDefinition[] notches;
        public float mirrorCharge = 0;
        private LookingGlassNotchData _currentSetting;
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
        public override void Load()
        {
            On_ItemSlot.RightClick_ItemArray_int_int += AddNotchToMirror;
        }

        public override void Unload()
        {
            On_ItemSlot.RightClick_ItemArray_int_int -= AddNotchToMirror;
        }

        public void PostSetupContentLoad()
        {
            RadialUICursorSystem.radialUICursorData.Add(new RadialUICursorData(LookingGlassUI.Instance, 1f, DrawLookingGlassMouseUI));
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
                            Main.mouseItem.stack--;
                            if (Main.mouseItem.stack <= 0)
                                Main.mouseItem.TurnToAir();

                            SoundEngine.PlaySound(SoundID.Grab);
                            return;
                        }
                    }
                }
            }
            orig(inv, context, slot);
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Looking Glass");
            Tooltip.SetDefault("Allows you to view applied Item Imprints and remove them");
            Item.ResearchUnlockCount = 1;
            LookingGlassNotchData.LoadNotchData
              (
              Type,
              new Color(200, 210, 255),
              $"{nameof(Radiance)}/Content/ExtraTextures/LookingGlass/LookingGlass_Recall",
              $"{nameof(Radiance)}/Content/ExtraTextures/LookingGlass/LookingGlass_Recall_Small",
              MirrorUse,
              RadianceCost,
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
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if(player.ItemTimeIsZero)
                player.SetItemTime(90);

            if (player.itemTime == player.itemTimeMax / 2)
            {
                CurrentSetting.mirrorUse(player, this);
            }
        }
        public override void HoldItem(Player player)
        {
            if (!player.IsCCd() && !player.ItemAnimationActive && !player.mouseInterface)
            {
                if (Main.mouseRight && Main.mouseRightRelease && !LookingGlassUI.Instance.visible)
                {
                    Main.mouseRightRelease = false;
                    LookingGlassUI.Instance.EnableRadialUI();
                }
            }
            if(player.ItemAnimationActive)
            {
                float progress = player.itemAnimation / (float)player.itemAnimationMax;
                Rectangle playerRect = new Rectangle((int)player.position.X, (int)(player.position.Y + player.height * progress), player.width, (int)(player.height * (1f - progress)));
                Vector2 particlePos = Main.rand.NextVector2FromRectangle(playerRect);
                if (Main.GameUpdateCount % 4 == 0)
                    WorldParticleSystem.system.AddParticle(new Sparkle(particlePos, Vector2.UnitY * -Main.rand.NextFloat(2f, 5f), 30, CurrentSetting.color));
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

        public override void PostDrawTooltip(ReadOnlyCollection<DrawableTooltipLine> lines)
        {
            if (notches is null)
                return;

            for (int i = 0; i < MAX_SLOTS; i++)
            {
                DrawNotchSlot(Main.spriteBatch, visualPosition, i);
            }
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

            float vfxModifier = SineTiming(120f, index * 10f) * 0.5f + 0.5f;
            float glowModifier = Lerp(0.7f, 1.2f, MathF.Pow(vfxModifier, 1.3f));
            float glowScaleModifier = Lerp(1f, 1.05f, vfxModifier);
            LookingGlassNotchData notch = null;
            ItemDefinition notchItemDef = notches[index];
            Color color = Color.White;
            if (notchItemDef is not null && notchItemDef.Type != ItemID.None)
            { 
                notch = LookingGlassNotchData.loadedNotches.FirstOrDefault(x => x.type == notches[index].Type);
                if(notch is not null)
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
                int cursorItemType = Main.LocalPlayer.GetModPlayer<RadialUICursorPlayer>().realCursorItemType;
                if (cursorItemType != ItemID.None)
                    itemOffset = GetItemTexture(Main.LocalPlayer.GetModPlayer<RadialUICursorPlayer>().realCursorItemType).Width / -2f;

                Vector2 offset = new Vector2(itemOffset, 24f);
                Vector2 position = Main.MouseScreen + offset;
                Color color = Color.White;
                if (lookingGlass._currentSetting.type != ModContent.ItemType<LookingGlass>() && lookingGlass.mirrorCharge < lookingGlass.CurrentSetting.radianceCost(lookingGlass.NotchCount[lookingGlass._currentSetting]))
                    color = new Color(100, 100, 100);

                spriteBatch.Draw(tex, position, null, color * opacity, 0, tex.Size() / 2, 1f, SpriteEffects.None, 0);
            }
        }
        public void MirrorUse(Player player, LookingGlass lookingGlass)
        {
            player.Spawn(PlayerSpawnContext.RecallFromItem);
        }

        public int RadianceCost(int identicalCount)
        {
            return 10;
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
    }

    public class LookingGlassUI : RadialUI
    {
        public static LookingGlassUI Instance;

        public LookingGlassUI()
        {
            Instance = this;
        }

        public override bool Active() => Main.LocalPlayer.GetPlayerHeldItem().type == ModContent.ItemType<LookingGlass>();
        public override List<RadialUIElement> GetElementsToDraw()
        {
            List<RadialUIElement> elements = new List<RadialUIElement>();
            LookingGlass lookingGlass = Main.LocalPlayer.GetPlayerHeldItem().ModItem as LookingGlass;
            List<LookingGlassNotchData> uniqueDataInGlass = new List<LookingGlassNotchData>();
            List<LookingGlassNotchData> loadedNotches = LookingGlassNotchData.loadedNotches.OrderBy(x => GetItem(x.type).rare).ToList();
            Dictionary<LookingGlassNotchData, int> notchCount = lookingGlass.NotchCount;

            foreach (LookingGlassNotchData notchData in loadedNotches)
            {
                if (notchData.type == ModContent.ItemType<LookingGlass>())
                    continue;

                if (lookingGlass.notches.Any(x => x.Type == notchData.type))
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
                    lookingGlass.mirrorCharge > currentNotch.radianceCost(notchCount[currentNotch])
                    ));
            }
            return elements;
        }

        public override RadialUIElement GetCenterElement()
        {
            LookingGlass lookingGlass = Main.LocalPlayer.GetPlayerHeldItem().ModItem as LookingGlass;
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
        public Func<int, int> radianceCost;
        internal static List<LookingGlassNotchData> loadedNotches = new List<LookingGlassNotchData>();

        private LookingGlassNotchData(string id, int type, Color color, string bigTex, string smallTex, Action<Player, LookingGlass> mirrorUse, Func<int, int> radianceCost)
        {
            this.id = id;
            this.type = type;
            this.color = color;
            this.bigTex = bigTex;
            this.smallTex = smallTex;
            this.mirrorUse = mirrorUse;
            this.radianceCost = radianceCost;
        }

        public static void LoadNotchData(int type, Color color, string bigTex, string smallTex, Action<Player, LookingGlass> mirrorUse, Func<int, int> radianceCost, string overrideID = null)
        {
            Item item = GetItem(type);
            string id = item.ModItem.FullName;
            if (overrideID is not null)
                id = overrideID;

            loadedNotches.Add(new LookingGlassNotchData(id, type, color, bigTex, smallTex, mirrorUse, radianceCost));
        }
    }
}