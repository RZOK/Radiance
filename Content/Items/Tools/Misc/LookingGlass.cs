using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.Materials;
using Radiance.Content.UI;
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

        private Vector2 visualPosition;
        private int visualTimer = 0;
        internal ItemDefinition[] notches;
        private static int VISUAL_TIMER_MAX = 45;
        internal LookingGlassNotchData currentSetting;

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
            RadialUICursorSystem.radialUICursorData.Add(new RadialUICursorData(() =>
            !LookingGlassUI.Instance.active && Main.LocalPlayer.HeldItem.type == ModContent.ItemType<LookingGlass>() && Main.LocalPlayer.cursorItemIconEnabled && Main.LocalPlayer.GetModPlayer<RadialUICursorPlayer>().realCursorItemType == ModContent.ItemType<LookingGlass>(),
            DrawLookingGlassMouseUI,
            1f));
        }

        private void AddNotchToMirror(On_ItemSlot.orig_RightClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
        {
            if (Main.mouseRight && Main.mouseRightRelease && !inv[slot].IsAir && inv[slot].type == Type && !Main.LocalPlayer.ItemAnimationActive)
            {
                LookingGlass mirror = inv[slot].ModItem as LookingGlass;
                if (!Main.mouseItem.IsAir && Main.mouseItem.ModItem is BaseNotch notch)
                {
                    for (int i = 0; i < mirror.notches.Length; i++)
                    {
                        ItemDefinition insertedNotch = mirror.notches[i];
                        if (insertedNotch is null)
                        {
                            mirror.notches[i] = new ItemDefinition(Main.mouseItem.type);
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
              new Color(235, 236, 255),
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
            currentSetting = LookingGlassNotchData.loadedNotches.First(x => x.id == MIRROR_KEY);

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

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            visualPosition = position;
            if (Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().realHoveredItem == Item && GetPlayerHeldItem() != Item && Main.mouseItem.IsAir)
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

            BaseNotch notch = null;
            float vfxModifier = SineTiming(120f, index * 10f) * 0.5f + 0.5f;
            float glowModifier = Lerp(0.7f, 1.2f, MathF.Pow(vfxModifier, 1.3f));
            float glowScaleModifier = Lerp(1f, 1.05f, vfxModifier);
            if (notches[index] is not null)
            {
                notch = GetItem(notches[index].Type).ModItem as BaseNotch;
            }
            Color color = Color.White;
            if (notch is not null)
            {
                color = notch.color;
                spriteBatch.Draw(underlayTex, drawPos, null, color * alphaCompletion * glowModifier, 0, glowTex.Size() / 2f, scale * glowScaleModifier, SpriteEffects.None, 0f);
            }
            spriteBatch.Draw(tex, underDrawPos, null, color * alphaCompletion * 0.25f, 0, tex.Size() / 2f, scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, color * alphaCompletion, 0, tex.Size() / 2f, scale, SpriteEffects.None, 0f);

            if (notch is not null)
            {
                Texture2D itemTex = GetItemTexture(notches[index].Type);
                spriteBatch.Draw(itemTex, drawPos, null, Color.White * alphaCompletion, 0, itemTex.Size() / 2f, scale, SpriteEffects.None, 0f);
            }
        }

        private static void DrawLookingGlassMouseUI(SpriteBatch spriteBatch, float opacity)
        {
            LookingGlass lookingGlass = Main.LocalPlayer.HeldItem.ModItem as LookingGlass;
            if (lookingGlass is not null)
            {
                Texture2D tex = ModContent.Request<Texture2D>(lookingGlass.currentSetting.smallTex).Value;
                Vector2 position = Main.MouseScreen + new Vector2(GetItemTexture(Main.LocalPlayer.GetModPlayer<RadialUICursorPlayer>().realCursorItemType).Width / -2f + 12f, 24f);
                Color color = Color.White;
                //if (PLAYER DOES NOT HAVE THE MIRROR CHARGE)
                //    color = new Color(100, 100, 100);

                spriteBatch.Draw(tex, position, null, color * opacity, 0, tex.Size() / 2, 1f, SpriteEffects.None, 0);
            }
        }
        public void MirrorUse(Player player, LookingGlass lookingGlass)
        {

        }

        public int RadianceCost(Player player, LookingGlass lookingGlass, int identicalCount)
        {
            return 10;
        }
        public override void LoadData(TagCompound tag)
        {
            notches = tag.GetList<ItemDefinition>(nameof(notches)).ToArray();
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

            LookingGlassNotchData recall = LookingGlassNotchData.loadedNotches.First(x => x.id == LookingGlass.MIRROR_KEY);
            center = new RadialUIElement(this, recall.bigTex,
            () =>
            {
                if (Main.LocalPlayer.HeldItem.ModItem is LookingGlass lookingGlass)
                    lookingGlass.currentSetting = recall;
            },
            () =>
            {
                if (Main.LocalPlayer.HeldItem.ModItem is LookingGlass lookingGlass)
                    return lookingGlass.currentSetting == recall;

                return false;
            });
            List<LookingGlassNotchData> loadedNotchesNotCenter = LookingGlassNotchData.loadedNotches.Where(x => x.id != LookingGlass.MIRROR_KEY).ToList();
            for (int i = 0; i < loadedNotchesNotCenter.Count; i++)
            {
                LookingGlassNotchData data = loadedNotchesNotCenter[i];
                surroundingElements.Add(new RadialUIElement(this, data.bigTex,
                () =>
                {
                    if (Main.LocalPlayer.HeldItem.ModItem is LookingGlass lookingGlass)
                        lookingGlass.currentSetting = data;
                },
                () =>
                {
                    if (Main.LocalPlayer.HeldItem.ModItem is LookingGlass lookingGlass)
                        return lookingGlass.currentSetting == data;

                    return false;
                },
                () =>
                {
                    if (Main.LocalPlayer.HeldItem.ModItem is LookingGlass lookingGlass)
                        return lookingGlass.notches.Any(x => x.Name == data.id);

                    return false;
                }));
            }
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
        public Func<Player, LookingGlass, int, int> radianceCost;
        internal static List<LookingGlassNotchData> loadedNotches = new List<LookingGlassNotchData>();

        private LookingGlassNotchData(string id, int type, Color color, string bigTex, string smallTex, Action<Player, LookingGlass> mirrorUse, Func<Player, LookingGlass, int, int> radianceCost)
        {
            this.id = id;
            this.type = type;
            this.color = color;
            this.bigTex = bigTex;
            this.smallTex = smallTex;
            this.mirrorUse = mirrorUse;
            this.radianceCost = radianceCost;
        }

        public static void LoadNotchData(int type, Color color, string bigTex, string smallTex, Action<Player, LookingGlass> mirrorUse, Func<Player, LookingGlass, int, int> radianceCost, string overrideID = null)
        {
            Item item = GetItem(type);
            string id = item.ModItem.FullName;
            if (overrideID is not null)
                id = overrideID;

            loadedNotches.Add(new LookingGlassNotchData(id, type, color, bigTex, smallTex, mirrorUse, radianceCost));
        }
    }
}