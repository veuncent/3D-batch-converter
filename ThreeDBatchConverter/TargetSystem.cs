using System.ComponentModel;

namespace ThreeDBatchConverter
{
    public enum TargetSystem
    {
        NotImplemented = 0,
        [Description("Potree")]
        Potree = 1,
        [Description("Nexus")]
        Nexus = 2
    }
}
