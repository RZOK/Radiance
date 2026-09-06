using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Radiance.Core.Research.ResearchHandler;

namespace Radiance.Core.Research.Elements
{
    public class MirrorComponent : ResearchComponent
    {
        public MirrorComponent(Vector2 position, float rotation, bool movable = false) : base(position, rotation, 70, 16)
        {
            this.movable = movable;
        }

        public override void Update()
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("Radiance/Core/Research/Assets/MirrorComponent").Value;
            spriteBatch.Draw(tex, position, null, Color.White, rotation, tex.Size() / 2, 1, SpriteEffects.None, 0);
        }

        public override void OnBeamCollide(Vector2 collidePosition, ResearchBeam beam)
        {

        }
    }
}
