namespace Radiance.Core.Interfaces
{
    public interface IRedirectInterfacableInventory
    {
        public IInventory redirectedInventory { get; }
    }
}