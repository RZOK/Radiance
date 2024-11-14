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
        private static List<Func<(string internalName, int tileItemType, int tileType, AssemblableTileEntity tileEnttity, Color color, int tier)>> blueprintsToLoad = new List<Func<(string internalName, int tileItemType, int tileType, AssemblableTileEntity tileEnttity, Color color, int tier)>>();

        public override void Load()
        {
            foreach (var data in blueprintsToLoad)
            {
                (string internalName, int tileItemType, int tileType, AssemblableTileEntity tileEntity, Color color, int tier) realData = data.Invoke();
                BlueprintData blueprintData = new BlueprintData(realData.tileItemType, realData.tileType, realData.tileEntity, realData.tier);
                AutoloadedBlueprint item = new AutoloadedBlueprint(realData.internalName, realData.color, blueprintData);
                Radiance.Instance.AddContent(item);
                blueprintData.blueprintType = item.Type;
                loadedBlueprints.Add(blueprintData);
            }
            blueprintsToLoad.Clear();
        }
        public static List<BlueprintData> loadedBlueprints = new List<BlueprintData>();
        public static void AddBlueprint(Func<(string internalName, int tileItemType, int tileType, AssemblableTileEntity tileEnttity, Color color, int tier)> data)
        {
            blueprintsToLoad.Add(data);
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
            Item.createTile = blueprintData.tileType;
        }
    }
}
