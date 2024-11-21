using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radiance.Core.Interfaces
{
    public interface IDrawOverInventoryItem
    {
        public void DrawOverInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Vector2 origin, float scale);
    }
}
