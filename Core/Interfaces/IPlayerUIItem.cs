namespace Radiance.Core.Interfaces
{
    public interface IPlayerUIItem
    {
        public string SlotTexture { get; }
        public void OnOpen();
        public void OnClear();
    }
}
