namespace XIV.DesignPatterns.Common.Extensions
{
    public static class ArrayExtensions
    {
        public static T PickRandom<T>(this T[] arr)
        {
            return arr.Length == 0 ? default : arr[UnityEngine.Random.Range(0, arr.Length)];
        }
        
        public static T PickRandom<T>(this T[] arr, int length)
        {
            return arr.Length == 0 ? default : arr[UnityEngine.Random.Range(0, length)];
        }
    }
}