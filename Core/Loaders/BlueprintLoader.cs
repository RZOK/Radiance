using Radiance.Content.Particles;
using Radiance.Content.Tiles.Transmutator;
using Radiance.Core.Systems;
using Radiance.Core.Systems.ParticleSystems;

namespace Radiance.Core.Loaders
{
    public class BlueprintData
    {
        public int blueprintType;
        public readonly int tileItemType;
        public readonly AssemblableTileEntity tileEntity;
        public int TileType => tileEntity.ParentTile;
        public readonly int tier;

        public BlueprintData(int tileItemType, AssemblableTileEntity tileEntity, int tier)
        {
            this.tileItemType = tileItemType;
            this.tileEntity = tileEntity;
            this.tier = tier;
        }
    }

    public class BlueprintLoader : ModSystem
    {
        private static List<Func<(string internalName, int tileItemType, AssemblableTileEntity tileEnttity, Color color, int tier)>> blueprintsToLoad = new List<Func<(string internalName, int tileItemType, AssemblableTileEntity tileEnttity, Color color, int tier)>>();

        public override void Load()
        {
            foreach (var data in blueprintsToLoad)
            {
                (string internalName, int tileItemType, AssemblableTileEntity tileEntity, Color color, int tier) = data.Invoke();
                BlueprintData blueprintData = new BlueprintData(tileItemType, tileEntity, tier);
                AutoloadedBlueprint item = new AutoloadedBlueprint(internalName, color, blueprintData);
                Radiance.Instance.AddContent(item);
                blueprintData.blueprintType = item.Type;
                loadedBlueprints.Add(blueprintData);
            }
            blueprintsToLoad.Clear();
        }

        public static List<BlueprintData> loadedBlueprints = new List<BlueprintData>();

        public static void AddBlueprint(Func<(string internalName, int tileItemType, AssemblableTileEntity tileEnttity, Color color, int tier)> data)
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
            Tooltip.SetDefault("Placeholder Line");
            Item.ResearchUnlockCount = 0;
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
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/Items/Blueprint_Wrap").Value;
            spriteBatch.Draw(texture, position, null, (color.ToVector4() * drawColor.ToVector4()).ToColor(), 0, texture.Size() / 2, scale, SpriteEffects.None, 0);
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/Items/Blueprint_Wrap").Value;
            spriteBatch.Draw(texture, Item.position - Main.screenPosition, null, (color.ToVector4() * lightColor.ToVector4()).ToColor(), 0, texture.Size() / 2, scale, SpriteEffects.None, 0);
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Item tileItem = GetItem(blueprintData.tileItemType);
            TooltipLine blueprintTileLine = new TooltipLine(Mod, "BlueprintTile", $"A completed draft for creating the [c/{ItemRarityHex(tileItem)}:{tileItem.Name}]");
            TooltipLine tooltip = tooltips.First(x => x.Name == "Tooltip0" && x.Mod == "Terraria");
            tooltip.Text = blueprintTileLine.Text;
        }
    }
    public class BlueprintPlayer : ModPlayer
    {
        public List<BlueprintData> knownBlueprints = new List<BlueprintData>();

        public override void SaveData(TagCompound tag)
        {
            List<string> blueprintStrings = new List<string>();
            knownBlueprints.ForEach(x => blueprintStrings.Add(GetItem(x.blueprintType).ModItem.FullName));
            tag[nameof(knownBlueprints)] = blueprintStrings;
        }
        public override void LoadData(TagCompound tag)
        {
            foreach (string str in tag.GetList<string>(nameof(knownBlueprints)))
            {
                BlueprintData data = BlueprintLoader.loadedBlueprints.FirstOrDefault(x => str == GetItem(x.blueprintType).ModItem.FullName);
                if (data.blueprintType == 0)
                {
                    Radiance.Instance.Logger.Warn($"Player-learned blueprint with string of '{str}' failed to load properly.");
#if DEBUG
                    SoundEngine.PlaySound(SoundID.DoorClosed);
#endif
                }
                else
                    knownBlueprints.Add(data);
            }
        }
    }
}