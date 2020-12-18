using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ParallelProject
{
    class Program
    {
        static class counter
        {
            public static int count = 0;
        }
        class name
        {
            public name(string fname, string lname)
            {
                this.firstName = fname;
                this.lastName = lname;
            }
            public string firstName { get; set;}
            public string lastName { get; set; }
        }
        static void Main(string[] args)
        {
            List<name> Names = new List<name>();
            List<name> NamesCopy = new List<name>();

            using (StreamReader sr = new StreamReader("names.txt"))
            {
                while(sr.Peek() >= 0)
                {
                    string[] s = sr.ReadLine().Split(' ');
                    Names.Add(new name(s[0], s[1]));
                    NamesCopy.Add(new name(s[0], s[1]));
                }
            }

            List<name> testNames = new List<name>();

            using (StreamReader sr = new StreamReader("testNames.txt"))
            {
                while (sr.Peek() >= 0)
                {
                    string[] s = sr.ReadLine().Split(' ');
                    testNames.Add(new name(s[0], s[1]));
                }
            }

            Console.WriteLine("Sorting using merge sort");
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<name>newNames = mergeSort(Names);
            stopwatch.Stop();
            Console.WriteLine("Code took {0} milliseconds to execute", stopwatch.ElapsedMilliseconds);

            Console.WriteLine("\n");

            Console.WriteLine("Sorting using parallel merge sort");
            stopwatch = Stopwatch.StartNew();
            List<name> newerNames = mergeSortParallel(Names, 0, 500);
            stopwatch.Stop();
            Console.WriteLine("Code took {0} milliseconds to execute", stopwatch.ElapsedMilliseconds);

            Console.WriteLine("\n");

            Console.WriteLine("Sorting using parallel merge sort");
            stopwatch = Stopwatch.StartNew();
            List<name> newerNames1 = mergeSortParallel(Names, 0, 5);
            stopwatch.Stop();
            Console.WriteLine("Code took {0} milliseconds to execute", stopwatch.ElapsedMilliseconds);

            Console.WriteLine("\n");

            Console.WriteLine("Sorting using quick sort");
            stopwatch = Stopwatch.StartNew();
            Quick_Sort(Names, 0, Names.Count - 1);
            stopwatch.Stop();
            Console.WriteLine("Code took {0} milliseconds to execute", stopwatch.ElapsedMilliseconds);

            Console.WriteLine("\n");

            Console.WriteLine("Sorting using parallel quick sort");
            stopwatch = Stopwatch.StartNew();
            Quick_SortParallel(NamesCopy, 0, NamesCopy.Count - 1);
            stopwatch.Stop();
            Console.WriteLine("Code took {0} milliseconds to execute", stopwatch.ElapsedMilliseconds);

            Console.WriteLine("\n");

            Console.WriteLine("Sorting using provided solution");
            stopwatch = Stopwatch.StartNew();
            List<name> newNames1 = usingBuiltInSort(Names);
            stopwatch.Stop();
            Console.WriteLine("Code took {0} milliseconds to execute", stopwatch.ElapsedMilliseconds);

            

            /*Console.WriteLine("Sorted List:");
            for (int i = 0; i < newNames.Count; i++)
            {
                Console.WriteLine("{0} {1}", newNames[i].lastName, newNames[i].firstName);
            }*/

            /*Console.WriteLine("Original List:");
            for (int i = 0; i < testNames.Count; i++)
            {
                Console.WriteLine("{0} {1}", testNames[i].lastName, testNames[i].firstName);
            }

            Quick_Sort(testNames, 0, testNames.Count - 1);
            Console.WriteLine("\n");
            Console.WriteLine("Sorted List:");
            for (int i = 0; i < testNames.Count; i++)
            {
                Console.WriteLine("{0} {1}", testNames[i].lastName, testNames[i].firstName);
            }*/

            Console.WriteLine("\n");
            Console.WriteLine("Press Return to go back");
            Console.ReadLine();

        }

        static List<name> usingBuiltInSort(List<name> Names)
        {
            List<name> sortedNames = Names.OrderBy(s => s.lastName).ThenBy(s => s.firstName).ToList();

            return sortedNames;
        }

        static List<name> mergeSort(List<name> Names)
        {
            //base case
            if (Names.Count <= 1) return Names;

            //calculate midpoint
            int midPoint = Names.Count / 2;

            //check if number is even
            if(Names.Count % 2 == 0)
            {
                //recursively call mergesort twice, first with values from 0 to midpoint-1 then from midpoint to last element
                //then send that to merge and return the sorted list
                return merge(mergeSort(Names.GetRange(0, midPoint)), mergeSort(Names.GetRange(midPoint, midPoint)));
            }
            else
            {
                //same thing as when its even except the we get the one extra element from the list at the end
                return merge(mergeSort(Names.GetRange(0, midPoint)), mergeSort(Names.GetRange(midPoint, midPoint + 1)));
            }
            
        }

        static List<name> mergeSortParallel(List<name> Names, int depth, int minDepth)
        {
            if (Names.Count <= 1) return Names;

            var left = new List<name>();
            var right = new List<name>();
            var sortedNames = new List<name>();

            int midPoint = Names.Count / 2;

            for (int i = 0; i < midPoint; i++)
            {
                left.Add(Names[i]);
            }

            for (int i = midPoint; i < Names.Count; i++)
            {
                right.Add(Names[i]);
            }

            if(depth > minDepth)
            {
                left = mergeSortParallel(left, depth, minDepth);
                right = mergeSortParallel(right, depth, minDepth);

                sortedNames = merge(left, right);
            }
            else
            {
                Parallel.Invoke(() => left = mergeSortParallel(left, depth + 1, minDepth),
                            () => right = mergeSortParallel(right, depth + 1, minDepth));

                Parallel.Invoke(() => sortedNames = merge(left, right));
            }


            return sortedNames;
        }

        static List<name> merge(List<name> left, List<name> right)
        {
            List<name> sortedNames = new List<name>();

            int indexLeft = 0;
            int indexRight = 0;

            while(indexLeft < left.Count || indexRight < right.Count)
            {
                if (indexLeft < left.Count && indexRight < right.Count)
                {
                    string l = left[indexLeft].lastName;
                    string r = right[indexRight].lastName;

                    if (string.Compare(l, r) == 0)
                    {
                        string lf = left[indexLeft].firstName;
                        string rf = right[indexRight].firstName;

                        if (string.Compare(lf, rf) <= 0)
                        {
                            sortedNames.Add(left[indexLeft]);
                            indexLeft++;
                        }
                        else
                        {
                            sortedNames.Add(right[indexRight]);
                            indexRight++;
                        }
                    }
                    else if (string.Compare(l, r) < 0)
                    {
                        sortedNames.Add(left[indexLeft]);
                        indexLeft++;
                    }
                    else if (string.Compare(l, r) > 0)
                    {
                        sortedNames.Add(right[indexRight]);
                        indexRight++;
                    }
                }
                else if(indexLeft < left.Count)
                {
                    sortedNames.Add(left[indexLeft]);
                    indexLeft++;
                }
                else if(indexRight < right.Count)
                {
                    sortedNames.Add(right[indexRight]);
                    indexRight++;
                }
            }
            return sortedNames;
        }

        static void Quick_SortParallel(List<name> Names, int left, int right)
        {
            if (left < right)
            {
                int pivot = Partition(Names, left, right);

                if (pivot > 1)
                {
                    Parallel.Invoke(() => Quick_SortParallel(Names, left, pivot - 1));
                }
                if (pivot + 1 < right)
                {
                    Parallel.Invoke(() => Quick_SortParallel(Names, pivot + 1, right));
                }
            }

        }

        static void Quick_Sort(List<name> Names, int left, int right)
        {
            if (left < right)
            {
                int pivot = Partition(Names, left, right);

                if (pivot > 1)
                {
                    Quick_Sort(Names, left, pivot - 1);
                }
                if (pivot + 1 < right)
                {
                    Quick_Sort(Names, pivot + 1, right);
                }
            }

        }
        static int Partition(List<name> Names, int left, int right)
        {
            name pivot = Names[left];
            while (true)
            {
                while (string.Compare(Names[left].lastName, pivot.lastName) == 0 && string.Compare(Names[left].firstName, pivot.firstName) != 0)
                {
                    if (string.Compare(Names[left].firstName, pivot.firstName) < 0)
                    {
                        left++;
                    }
                    else
                    {
                        name temp = Names[left];
                        Names[left] = Names[right];
                        Names[right] = temp;

                        pivot = Names[right];
                    }
                }

                while (string.Compare(Names[left].lastName, pivot.lastName) < 0)
                {
                    left++;
                }

                while (string.Compare(Names[right].lastName, pivot.lastName) > 0)
                {
                    right--;
                }

                if (left < right)
                {
                    if (string.Compare(Names[left].lastName, Names[right].lastName) == 0 && string.Compare(Names[left].firstName, Names[right].firstName) == 0) return right;

                    name temp = Names[left];
                    Names[left] = Names[right];
                    Names[right] = temp;


                }
                else
                {
                    return right;
                }
            }
        }
    }
}
