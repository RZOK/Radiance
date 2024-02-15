using Radiance.Core.Systems;

namespace Radiance.Core.Encycloradia
{
    public enum EntryCategory
    {
        None,
        Influencing,
        Transmutation,
        Apparatuses,
        Instruments,
        Pedestalworks,
        Phenomena
    }

    public enum EntryVisibility
    {
        Visible,
        NotVisibleUntilUnlocked,
        NotVisible
    }

    public enum UnlockedStatus
    {
        Unlocked,
        Incomplete,
        Locked
    }

    public class EncycloradiaEntry : ICloneable
    {
        public string name = string.Empty;
        public string displayName = string.Empty;
        public string tooltip = string.Empty;
        public string fastNavInput = string.Empty;
        public UnlockCondition unlock;
        public UnlockCondition incomplete;
        public EntryCategory category = EntryCategory.None;
        public int icon = ItemID.ManaCrystal;
        public List<EncycloradiaPage> pages = new();
        public EntryVisibility visible = EntryVisibility.Visible;
        public bool unread => Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().unreadEntires.Contains(name);

        public UnlockedStatus unlockedStatus
        {
            get
            {
                bool unlocked = true;
                bool incompleted = true;
                if (!unlock.unlockFunction())
                    unlocked = false;

                if (unlocked)
                    return UnlockedStatus.Unlocked;

                if (!incomplete.unlockFunction())
                    incompleted = false;

                if (incompleted)
                    return UnlockedStatus.Incomplete;
                return UnlockedStatus.Locked;
            }
        }

        public void AddPageToEntry(EncycloradiaPage page)
        {
            if (page.GetType() == typeof(TextPage) && page.text != null && displayName == "Lens of Pathos")
            {
                List<TextPage> textPages = EncycloradiaSystem.ProcessTextPage(page);
                textPages.ForEach(x => EncycloradiaSystem.ForceAddPage(this, x));
                return;
            }
            EncycloradiaSystem.ForceAddPage(this, page);
        }

        public object Clone() => MemberwiseClone();
    }
}