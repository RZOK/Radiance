using Terraria;

namespace Radiance.Core.Interfaces
{
    public interface IInventory
    {
        Item[] inventory { get; set; }
        byte[] inputtableSlots { get; }
        byte[] outputtableSlots { get; }
    }
}