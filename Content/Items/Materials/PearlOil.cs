using Microsoft.Build.Construction;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Radiance.Content.Particles;
using Radiance.Content.Tiles.Transmutator;
using Radiance.Core.Systems;

using Terraria.Localization;
using Terraria.ObjectData;

namespace Radiance.Content.Items.Materials
{
    public class PearlOil : ModItem, ITransmutationRecipe
    {
        private const int SEARCH_DISTANCE = 8;
        private const int BUFF_DURATION = 10 * 60 * 60;
        public override void Load()
        {
            TransmutatorTileEntity.PostTransmutateItemEvent += CreateOrDiffusePearlOil;
        }
        private readonly Dictionary<int, int> pearlMap = new Dictionary<int, int>() 
        { 
            [ItemID.WhitePearl] = 1,
            //[ItemID.BlackPearl] = 2,
            //[ItemID.PinkPearl] = 4,
        };
        private void CreateOrDiffusePearlOil(TransmutatorTileEntity transmutator, TransmutationRecipe recipe)
        {
            if (recipe.inputItems[0] == ModContent.ItemType<PearlOil>())
            {
                if (transmutator.activeEffect == EnvironmentalEffect.GetEnvironmentalEffect<PearlOil_Effect>())
                    transmutator.activeEffectTime += BUFF_DURATION;
                else
                {
                    transmutator.activeEffect = EnvironmentalEffect.GetEnvironmentalEffect<PearlOil_Effect>();
                    transmutator.activeEffectTime = BUFF_DURATION;
                }
                return;
            }
            pearlMap.TryGetValue(recipe.inputItems[0], out int oilCount);
            if (oilCount <= 0)
                return;

            for (int i = -SEARCH_DISTANCE; i < SEARCH_DISTANCE + 1; i++)
            {
                for (int j = -SEARCH_DISTANCE; j < SEARCH_DISTANCE + 1; j++)
                {
                    Point16 pos = transmutator.Position + new Point16(i, j);
                    Tile tile = Framing.GetTileSafely(pos);
                    if(tile.HasTile && tile.TileType == TileID.Bottles && tile.TileFrameX == 0)
                    {
                        WorldGen.PlaceTile(pos.X, pos.Y, ModContent.TileType<PearlOil_Tile>(), true, true);

                        int numParticles = 4;
                        Vector2 worldPos = pos.ToWorldCoordinates(0, 0);
                        for (int h = 0; h < numParticles; h++)
                        {
                            ParticleSystem.AddParticle(new Sparkle(worldPos + Main.rand.NextVector2FromRectangle(new Rectangle(0, 0, 16, 16)), Main.rand.NextVector2Circular(1, 1), (int)(45f + 15f * Main.rand.NextFloat()), new Color(214, 203, 241), Main.rand.NextFloat(0.6f, 0.9f)));
                        }

                        oilCount--;
                        if (oilCount <= 0)
                            return;
                    }
                }
            }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Pearl Oil");
            Tooltip.SetDefault("'Slicker than grease'");
            Item.ResearchUnlockCount = 50;
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(0, 0, 50, 0);
            Item.rare = ItemRarityID.Green;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.createTile = ModContent.TileType<PearlOil_Tile>();
        }
        public void AddTransmutationRecipe(TransmutationRecipe recipe)
        {
            recipe.inputItems = new int[] { ItemID.WhitePearl, ItemID.BlackPearl, ItemID.PinkPearl };
            recipe.requiredRadiance = 100;
            recipe.outputItem = ItemID.None;
            recipe.unlock = UnlockCondition.UnlockedByDefault;
            recipe.outputStack = 0;

            TransmutationRecipe diffusionRecipe = new TransmutationRecipe();
            diffusionRecipe.inputItems = new int[] { Type };
            diffusionRecipe.requiredRadiance = 20;
            diffusionRecipe.outputItem = ItemID.None;
            diffusionRecipe.unlock = UnlockCondition.UnlockedByDefault;
            TransmutationRecipeSystem.AddRecipe(diffusionRecipe);
        }
    }
    public class PearlOil_Tile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.StyleOnTable1x1);
            TileObjectData.newTile.DrawFlipHorizontal = true;
            HitSound = SoundID.Shatter;
            DustType = -1;

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Pearl Oil");
            AddMapEntry(new Color(245, 103, 122), name);
            RegisterItemDrop(ModContent.ItemType<PearlOil>());
        }
    }
    public class PearlOil_Effect : EnvironmentalEffect
    {
        private const int BOOST_DISTANCE = 10;
        public PearlOil_Effect() : base(nameof(PearlOil), Radiance.Instance, new Color(214, 203, 241))
        {
        }
        public override void PreOrderedUpdate(TransmutatorTileEntity transmutator)
        { 
            List<ImprovedTileEntity> transmutatorsNearby = transmutator.TileEntitySearchHard(BOOST_DISTANCE);
            foreach (ImprovedTileEntity tileEntity in transmutatorsNearby)
            {
                if (tileEntity.GetType() != typeof(TransmutatorTileEntity))
                    continue;

                ((TransmutatorTileEntity)tileEntity).queuedDiscounts.Add(0.05f);
            }
        }
        public override List<HoverUIElement> GetHoverUI(TransmutatorTileEntity transmutator)
        {
            return new List<HoverUIElement>() { new RectangleUIElement(nameof(PearlOil), BOOST_DISTANCE * 16f + 8f, BOOST_DISTANCE * 16f, new Color(214, 203, 241)) };
        }
    }
}