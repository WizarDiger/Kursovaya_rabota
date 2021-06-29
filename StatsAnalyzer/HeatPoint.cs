
namespace StatsAnalyzer
{
    public struct HeatPoint
    {
        public float X;
        public float Y;
        public byte Intensity;

        public HeatPoint(float in_x, float in_y, byte in_intensity)
        {
            X = in_x;
            Y = in_y;
            Intensity = in_intensity;
        }
    }
}
