using System.Collections.Generic;

namespace YS_Tool
{
    public class ArrayUtility
    {

        public static bool IsOutOfArray(System.Array array, int idx)
        {
            if (array == null)
                return true;

            return idx < 0 || idx >= array.Length;
        }

        public static bool IsOutOfList<T>(List<T> list, int idx)
        {
            if (list == null)
                return true;

            return idx < 0 || idx >= list.Count;
        }
    }
}