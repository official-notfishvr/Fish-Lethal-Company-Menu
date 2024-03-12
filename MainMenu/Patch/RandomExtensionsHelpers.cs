internal static class RandomExtensionsHelpers
{
    public static void Shuffle<T>(this System.Random rng, T[] array)
    {
        int i = array.Length;
        while (i > 1)
        {
            int num = rng.Next(i--);
            T t = array[i];
            array[i] = array[num];
            array[num] = t;
        }
    }
}