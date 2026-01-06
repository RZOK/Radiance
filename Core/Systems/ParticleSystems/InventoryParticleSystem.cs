namespace Radiance.Core.Systems.ParticleSystems
{
    public class InventoryParticleSystem : ILoadable
    {
        public static readonly ParticleSystem system = new ParticleSystem(ParticleSystem.ParticleAnchor.UI);

        public void Load(Mod mod)
        {
            On_Main.DrawInventory += DrawParticles;
            On_Main.UpdateParticleSystems += UpdateParticles;
        }

        private void DrawParticles(On_Main.orig_DrawInventory orig, Main self)
        {
            orig(self);
            system.DrawParticles(Main.spriteBatch);
        }

        private void UpdateParticles(On_Main.orig_UpdateParticleSystems orig, Main self)
        {
            orig(self);
            system.UpdateParticles();
        }

        public void Unload()
        { }
    }
}