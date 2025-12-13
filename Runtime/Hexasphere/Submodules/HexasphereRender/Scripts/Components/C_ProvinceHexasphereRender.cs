
namespace HS.Hexasphere.Render
{
    public struct C_ProvinceHexasphereRender
    {
        public C_ProvinceHexasphereRender(
            int a)
        {
            parentChunkIndex = 0;
            parentChunkStart = 0;
            parentChunkTriangleStart = 0;
            parentChunkLength = 0;

            thinEdges = 63;
            parentThinEdgesChunkIndex = 0;
            parentThinEdgesChunkStart = 0;
            parentThinEdgesChunkLength = 0;

            thickEdges = 63;
            parentThickEdgesChunkIndex = 0;
            parentThickEdgesChunkStart = 0;
            parentThickEdgesChunkLength = 0;
        }

        public int parentChunkIndex;
        public int parentChunkStart;
        public int parentChunkTriangleStart;
        public int parentChunkLength;

        public int thinEdges;
        public int parentThinEdgesChunkIndex;
        public int parentThinEdgesChunkStart;
        public int parentThinEdgesChunkLength;

        public int thickEdges;
        public int parentThickEdgesChunkIndex;
        public int parentThickEdgesChunkStart;
        public int parentThickEdgesChunkLength;
    }
}
