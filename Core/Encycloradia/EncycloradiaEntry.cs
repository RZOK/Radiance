using Radiance.Core.Systems;
using System.Drawing.Printing;
using Terraria.Localization;

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
        public string fastNavInput = string.Empty;
        public UnlockCondition unlock;
        public UnlockCondition incomplete;
        public EntryCategory category = EntryCategory.None;
        public int icon = ItemID.ManaCrystal;
        public List<EncycloradiaPage> pages = new();
        public EntryVisibility visible = EntryVisibility.Visible;
        public Mod mod = Radiance.Instance;
        public bool Unread => Main.LocalPlayer.GetModPlayer<EncycloradiaPlayer>().unreadEntires.Contains(name);

        public UnlockedStatus unlockedStatus
        {
            get
            {
                if (unlock.condition())
                    return UnlockedStatus.Unlocked;

                if (incomplete.condition())
                    return UnlockedStatus.Incomplete;

                return UnlockedStatus.Locked;
            }
        }
        public string GetLocalizedName() => Language.GetTextValue($"Mods.{mod.Name}.Encycloradia.Entries.{name}.DisplayName");
        public object Clone() => MemberwiseClone();
    }
}