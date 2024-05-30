namespace SwapChains.Runtime.Utilities.Extensions
{
    public static class ArrayExtensions
    {
        public static T PickRandom<T>(this T[] arr)
        => arr.Length is 0 ? default : arr[UnityEngine.Random.Range(0, arr.Length)];

        public static T PickRandom<T>(this T[] arr, int length)
        => arr.Length is 0 ? default : arr[UnityEngine.Random.Range(0, length)];
    }
}