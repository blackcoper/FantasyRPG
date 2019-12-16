namespace FantasyRPG.Utility {
    public class Math {
        public static float LimitedRange(float value, float inMin, float inMax, float outMin, float outMax) {
            return (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }
    }
}

