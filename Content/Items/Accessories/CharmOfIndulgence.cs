using Radiance.Content.Items.BaseItems;
using Radiance.Content.Particles;
using Radiance.Core.Systems;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Default;

namespace Radiance.Content.Items.Accessories
{
    public class CharmOfIndulgence : BaseAccessory
    {
        public List<ItemDefinition> consumedFoods = new List<ItemDefinition>();
        private const int FOOD_BUFF_RATIO = 25; //how many foods does the player need to have consumed for food buff potency to be doubled?
        private const int ITEMS_PER_ROW = 16;
        internal static readonly int[] FOOD_BUFF_TYPES = new int[] { BuffID.WellFed, BuffID.WellFed2, BuffID.WellFed3 };

        public override void Load()
        {
            On_Player.AddBuff += ModifyFoodTime;
        }

        private void ModifyFoodTime(On_Player.orig_AddBuff orig, Player self, int type, int timeToAdd, bool quiet, bool foodHack)
        {
            if (FOOD_BUFF_TYPES.Contains(type) && self.TryGetEquipped(out CharmOfIndulgence charm))
                timeToAdd *= 1 + (int)(charm.consumedFoods.Count / (float)FOOD_BUFF_RATIO);

            orig(self, type, timeToAdd, quiet, foodHack);
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Charm of Indulgence");
            Tooltip.SetDefault("Effects of food are enhanced for each unique food item consumed");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 34;
            Item.maxStack = 1;
            Item.accessory = true;
            Item.rare = ItemRarityID.Green;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (consumedFoods.AnyAndExists() && !Item.social)
            {
                for (int i = 0; i < Math.Ceiling((float)consumedFoods.Count / ITEMS_PER_ROW); i++)
                {
                    int realAmountToDraw = Math.Min(ITEMS_PER_ROW, consumedFoods.Count - i * ITEMS_PER_ROW);
                    TooltipLine itemDisplayLine = new(Mod, $"{nameof(CharmOfIndulgence)}{i}", "");
                    if (i == 0)
                        itemDisplayLine.Text = new String('M', 2 * realAmountToDraw + 3) + i;
                    else
                        itemDisplayLine.Text = ".";

                    tooltips.Insert(tooltips.FindIndex(x => x.Name == "Tooltip0") + 1, itemDisplayLine);
                }
            }
        }

        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            if (line.Name == $"{nameof(CharmOfIndulgence)}0")
            {
                List<Item> items = new List<Item>();
                Texture2D bgTex = ModContent.Request<Texture2D>($"{Texture}_InventoryBackground").Value;
                foreach (ItemDefinition item in consumedFoods)
                {
                    if (item.Type == -1)
                        items.Add(new Item(ModContent.ItemType<UnloadedItem>()));
                    else
                        items.Add(new Item(item.Type));
                }
                RadianceDrawing.DrawItemGrid(items, new Vector2(line.X, line.Y), bgTex, ITEMS_PER_ROW);
            }
            return !line.Name.StartsWith(nameof(CharmOfIndulgence));
        }

        public override void SafeUpdateAccessory(Player player, bool hideVisual)
        {
            if (consumedFoods.Count != 0)
            {
                float modifier = 0;
                if (player.HasBuff(BuffID.WellFed))
                    modifier = 1f;
                else if (player.HasBuff(BuffID.WellFed2))
                    modifier = 1.5f;
                else if (player.HasBuff(BuffID.WellFed3))
                    modifier = 2f;
                else
                    return;

                modifier *= consumedFoods.Count / (float)FOOD_BUFF_RATIO;

                player.statDefense += (int)(2f * modifier);
                player.GetCritChance(DamageClass.Generic) += 2f * modifier;
                player.GetDamage(DamageClass.Generic) += 0.05f * modifier;
                player.GetAttackSpeed(DamageClass.Melee) += 0.05f * modifier;
                player.GetKnockback(DamageClass.Summon) += 0.5f * modifier;
                player.moveSpeed += 0.2f * modifier;
                player.pickSpeed -= 0.05f * modifier;
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag[nameof(consumedFoods)] = consumedFoods;
        }

        public override void LoadData(TagCompound tag)
        {
            consumedFoods = tag.GetList<ItemDefinition>(nameof(consumedFoods)).ToList();
        }
    }

    public class CharmOfIndulgenceItem : GlobalItem
    {
        public override bool ConsumeItem(Item item, Player player)
        {
            if (player.TryGetEquipped(out CharmOfIndulgence charm))
            {
                List<ItemDefinition> consumedFoods = charm.consumedFoods;
                if (CharmOfIndulgence.FOOD_BUFF_TYPES.Contains(item.buffType) && !consumedFoods.Select(x => x.Type).Contains(item.type))
                {
                    consumedFoods.Add(new ItemDefinition(item.type));
                    int numParticles = 5;
                    for (int i = 0; i < numParticles; i++)
                    {
                        Rectangle playerRect = new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height);
                        Vector2 pos = player.Center + Main.rand.NextVector2CircularEdge(player.width, player.height) * 0.75f;
                        Vector2 vel = pos.DirectionTo(player.Center) * 2f;
                        ParticleSystem.AddParticle(new Sparkle(pos, vel, Main.rand.Next(30, 60), new Color(255, 64, 64), 0.7f));
                        ParticleSystem.AddParticle(new StretchStar(player.position + new Vector2(Lerp(0, player.width, i / (numParticles - 1f)), Main.rand.Next(player.height) + 16), Vector2.UnitY * -Main.rand.NextFloat(6f, 8f), (int)(15f + 20f * (i / (float)numParticles)), new Color(255, 122, 122), 0.8f));
                        //ParticleSystem.AddParticle(new PlayerXPStar(player, player.Center, new Vector2(Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-6f, -2f)) * 1.7f, Main.rand.Next(50, 90), 0.45f, new Color(255, 80, 110), 1f));
                    }
                }
            }
            return base.ConsumeItem(item, player);
        }
    }
}