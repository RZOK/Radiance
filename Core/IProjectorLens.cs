namespace Radiance.Core
{
    public enum ProjectorLensID
    {
        None,
        Flareglass,
        Pathos
    }
    internal interface IProjectorLens
    {
        public abstract ProjectorLensID ID { get; }
        public abstract int DustID { get; }
    }
}
