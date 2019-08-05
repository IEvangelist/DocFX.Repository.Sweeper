namespace DocFX.Repository.Sweeper
{
    static class NullableExtensions
    {
        internal static void Deconstruct<T>(this T? nullable, out bool hasValue, out T value) where T : struct
        {
            hasValue = nullable.HasValue;
            value = nullable.GetValueOrDefault();
        }
    }
}