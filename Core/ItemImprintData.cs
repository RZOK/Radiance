namespace Radiance.Core
{
    public struct ItemImprintData : TagSerializable
    {
        public bool blacklist = false;
        public List<string> imprintedItems = new List<string>();
        public ItemImprintData() { }
        public bool IsItemValid(Item item)
        {
            if (!imprintedItems.AnyAndExists())
                return true;

            if (blacklist)
                return !imprintedItems.Contains(item.GetTypeOrFullNameFromItem());
            return imprintedItems.Contains(item.GetTypeOrFullNameFromItem());
        }
        #region TagCompound Stuff

        public static readonly Func<TagCompound, ItemImprintData> DESERIALIZER = DeserializeData;

        public TagCompound SerializeData()
        {
            return new TagCompound()
            {
                [nameof(blacklist)] = blacklist,
                [nameof(imprintedItems)] = imprintedItems,
            };
        }

        public static ItemImprintData DeserializeData(TagCompound tag)
        {
            ItemImprintData itemImprintData = new()
            {
                blacklist = tag.GetBool(nameof(blacklist)),
                imprintedItems = (List<string>)tag.GetList<string>(nameof(imprintedItems))
            };
            return itemImprintData;
        }

        #endregion TagCompound Stuff
    }
}
