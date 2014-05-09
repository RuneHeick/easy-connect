using System;

namespace ECRU.Utilities.HelpFunction
{
    public static class StringArrayExtension
    {
        //Quicksort
        public static string[] Quicksort(this string[] elements, int left, int right)
        {
            var sortedElements = new string[elements.Length];
            elements.CopyTo(sortedElements, 0);


            int i = left, j = right;
            string pivot = sortedElements[(left + right)/2];

            while (i <= j)
            {
                while (string.Compare(sortedElements[i], pivot) < 0)
                {
                    i++;
                }

                while (string.Compare(sortedElements[j], pivot) > 0)
                {
                    j--;
                }

                if (i <= j)
                {
                    // Swap
                    string tmp = sortedElements[i];
                    sortedElements[i] = sortedElements[j];
                    sortedElements[j] = tmp;

                    i++;
                    j--;
                }
            }

            // Recursive calls
            if (left < j)
            {
                Quicksort(sortedElements, left, j);
            }

            if (i < right)
            {
                Quicksort(sortedElements, i, right);
            }

            return sortedElements;
        }


        //Add element
        public static string[] Add(this string[] elements, string element)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i] == element)
                {
                    return elements;
                }
            }

            var newElements = new string[elements.Length + 1];

            elements.CopyTo(newElements, 0);

            newElements[elements.Length] = element;

            return newElements;
        }

        //Remove element
        public static string[] Remove(this string[] elements, string element)
        {
            var newElements = new string[elements.Length - 1];

            int j = 0;
            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i] != element)
                {
                    newElements[j] = elements[i];
                    j++;
                }
            }

            return newElements;
        }

        //Has element
        public static bool HasElement(this string[] elements, string element)
        {
            bool result = false;
            foreach (string s in elements)
            {
                if (s == element)
                {
                    result = true;
                }
            }

            return result;
        }
    }
}