namespace Radiance.Core.Encycloradia
{
    public class EncycloradiaPlayer : ModPlayer
    {
        public List<string> unreadEntires = new List<string>();
        public EncycloradiaEntry currentEntry;
        public int leftPageIndex;

        public override void Load()
        {
            unreadEntires = new List<string>();
        }

        public override void Unload()
        {
            unreadEntires = null;
        }

        public override void OnEnterWorld()
        {
            // remove all entries from the unread list that aren't actually real
            List<string> entryNames = EncycloradiaSystem.EncycloradiaEntries.Select(x => x.internalName).ToList();
            unreadEntires.RemoveAll(x => !entryNames.Contains(x));
        }

        public override void SaveData(TagCompound tag)
        {
            if (unreadEntires.Count > 0)
                tag[nameof(unreadEntires)] = unreadEntires;
        }

        public override void LoadData(TagCompound tag)
        {
            unreadEntires = (List<string>)tag.GetList<string>(nameof(unreadEntires));
        }
    }
}