using Radiance.Content.Tiles.Transmutator;


namespace Radiance.Core.Loaders
{
    public struct BlueprintData
    {
        public int blueprintType;
        public readonly int tileItemType;
        public readonly int tileType;
        public readonly AssemblableTileEntity tileEntity;
        public readonly int tier;

        public BlueprintData(int tileItemType, int tileType, AssemblableTileEntity tileEntity, int tier)
        {
            this.tileItemType = tileItemType;
            this.tileType = tileType;
            this.tileEntity = tileEntity;
            this.tier = tier;
        }
    }
    public class BlueprintLoader : ModSystem
    {
        public override void Load()
        {
            int tileItemType = ModContent.ItemType<TransmutatorItem>();
            AssemblableTileEntity entity = ModContent.GetInstance<AssemblableTransmutatorTileEntity>();

            BlueprintLoader.AddBlueprint(
                nameof(Transmutator) + "Blueprint",
                tileItemType,
                ModContent.TileType<AssemblableTransmutator>(),
                entity,
                Color.OrangeRed,
                1);
        }
        public static List<BlueprintData> loadedBlueprints = new List<BlueprintData>();
        public static void AddBlueprint(string internalName, int tileItemType, int tileType, AssemblableTileEntity tileEntity, Color color, int tier)
        {
            BlueprintData data = new BlueprintData(tileItemType, tileType, tileEntity, tier);
            AutoloadedBlueprint item = new AutoloadedBlueprint(internalName, color, data);
            Radiance.Instance.AddContent(item);
            data.blueprintType = item.Type;
            loadedBlueprints.Add(data);
        }
    }
    [Autoload(false)]
    public class AutoloadedBlueprint : ModItem
    {
        public readonly BlueprintData blueprintData;
        public readonly string internalName;
        public readonly Color color;
        
        protected override bool CloneNewInstances => true;
        public override string Name => internalName;
        public override string Texture => $"{nameof(Radiance)}/Content/Items/Blueprint";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Completed Blueprint");
        }

        public AutoloadedBlueprint(string internalName, Color color, BlueprintData blueprintData)
        {
            this.internalName = internalName;
            this.color = color;
            this.blueprintData = blueprintData;
        }
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 1;
            Item.rare = GetItem(blueprintData.tileItemType).rare;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = blueprintData.tileType;
        }
    }
}
