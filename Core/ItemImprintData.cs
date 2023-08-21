using Radiance.Utilities;

namespace Radiance.Core
{
    public struct ItemImprintData
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
    }
}
