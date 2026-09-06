using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Effects;
using Terraria.Localization;
using static Radiance.Core.Research.ResearchHandler;

namespace Radiance.Core.Research.Elements
{
    public class BeamSpawnerComponent : ResearchComponent
    {
        public ResearchBeam beam;
        public BeamSpawnerComponent(Vector2 position, float rotation) : base(position, rotation, 36, 26)
        {
        }

        public override void Update()
        {
            
        }

        public override void DrawExtra(SpriteBatch spriteBatch, Color drawColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>("Radiance/Core/Research/Assets/BeamSpawnComponent").Value;
            //Texture2D outlineTex = ModContent.Request<Texture2D>("Radiance/Core/Encycloradia/Assets/ResearchElements/BeamSpawnerOutline").Value;
            spriteBatch.Draw(tex, position, null, drawColor, rotation, tex.Size() / 2, 1, SpriteEffects.None, 0);
        }

        public override void OnBeamCollide(Vector2 collidePosition, ResearchBeam beam)
        {
        }
    }
    public class ResearchBeam
    {
        List<ResearchBeamSegment> segments;
    }
    public class ResearchBeamSegment
    {

    }
}
