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
        public abstract ProjectorLensID ID { get; }
        public abstract int DustID { get; }
    }
}
