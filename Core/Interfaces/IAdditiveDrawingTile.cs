
namespace Radiance.Core.Interfaces
{
    public interface IAdditiveDrawingTile
    {
        public void PreDrawAdditive(int i, int j, SpriteBatch spriteBatch);
        public void PostDrawAdditive(int i, int j, SpriteBatch spriteBatch);
    }
    public class AdditiveDrawingTileLoading : ILoadable
    {
        public void Load(Mod mod)
        {
            
        }

        public void Unload()
        {
            throw new NotImplementedException();
        }
    }
}
