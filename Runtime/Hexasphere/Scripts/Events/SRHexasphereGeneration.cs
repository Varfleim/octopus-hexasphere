
using Leopotam.EcsLite;

namespace HS
{
    public readonly struct SRHexasphereGeneration
    {
        public SRHexasphereGeneration(
            EcsPackedEntity mapPE,
            int subdivisions)
        {
            this.mapPE = mapPE;

            this.subdivisions = subdivisions;
        }

        public readonly EcsPackedEntity mapPE;

        public readonly int subdivisions;
    }
}
