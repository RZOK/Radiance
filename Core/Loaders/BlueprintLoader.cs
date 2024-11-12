using Radiance.Content.Tiles.Transmutator;


namespace Radiance.Core.Loaders
{
    public class BlueprintLoader : ModSystem
    {
        public static void LoadBlueprintItem(string internalName, string displayNameKey, int tileType, AssemblableTileEntity tileEntity, int itemType, Color color)
        {
            AutoloadedBlueprint item = new AutoloadedBlueprint(internalName, displayNameKey, tileType, tileEntity, itemType, color);
            Radiance.Instance.AddContent(item);
        }
    }
    [Autoload(false)]
    public class AutoloadedBlueprint : ModItem
    {
        public readonly string internalName;
        public readonly string displayNameKey;
        public readonly int tileType;
        public readonly AssemblableTileEntity tileEntity;
        public readonly int itemID;
        public readonly Color color;

        protected override bool CloneNewInstances => true;
        public override string Name => internalName;
        public override string Texture => $"{nameof(Radiance)}/Content/Items/Blueprint";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Completed Blueprint");
        }

        public AutoloadedBlueprint(string internalName, string displayNameKey, int tileType, AssemblableTileEntity tileEntity, int itemID, Color color)
        {
            this.internalName = internalName;
            this.displayNameKey = displayNameKey;
            this.tileType = tileType;
            this.tileEntity = tileEntity;
            this.itemID = itemID;
            this.color = color;
        }
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 1;
            Item.rare = GetItem(itemID).rare;
            Item.createTile = tileType;
        }
    }
}
