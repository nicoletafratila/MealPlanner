namespace Common.Shared.Services
{
    public static class UnitConverter
    {
        public static decimal Convert(int fromUnitId, int toUnitId, decimal value)
        {
            if (fromUnitId == toUnitId) return value;

            return value;
        }
    }
}
