namespace Radiance.Core.Interfaces
{
    public enum ProjectorLensID
    {
        None,
        Flareglass,
        Pathos
    }

    public interface IProjectorLens
    {
        public ProjectorLensID ID { get; }
        public int DustID { get; }
        public string LensTexture { get; }
    }
}