namespace Radiance.Core.Interfaces
{
    internal interface ISpecificStackSlotInventory
    {
        Dictionary<int, int> allowedStackPerSlot { get; }
    }
}