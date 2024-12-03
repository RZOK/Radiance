
using static Radiance.Core.Systems.ParticleSystem;

namespace Radiance.Core.Systems.ParticleSystems
{
    public class BehindTilesParticleSystem : ILoadable
    {
        public static readonly ParticleSystem system = new ParticleSystem(ParticleSystem.ParticleAnchor.World);

        public void Load(Mod mod)
        {
            On_Main.DoDraw_Tiles_NonSolid += DrawParticles;
            On_Main.UpdateParticleSystems += UpdateParticles;
        }

        private void DrawParticles(On_Main.orig_DoDraw_Tiles_NonSolid orig, Main self)
        {
            system.DrawParticles(Main.spriteBatch);
            orig(self);
        }
        private void UpdateParticles(On_Main.orig_UpdateParticleSystems orig, Main self)
        {
            system.UpdateParticles();
            orig(self);
        }

        public void Unload() { }
    }
}
