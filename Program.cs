using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    static int listSize = 500000;  // Full dataset size for all algorithms
    static int numParts = 10;      // Number of parts to divide the list into for parallel sorting
    static int numRuns = 10;       // Number of runs for benchmarking
    static Random random = new Random();

    static async Task Main()
    {
        int[] wins = new int[5]; // Array to track the number of wins for each sorting algorithm

        for (int run = 0; run < numRuns; run++)
        {
            Console.WriteLine($"Run {run + 1}/{numRuns}");

            // Create a large array to be partitioned
            var array = CreateRandomList(listSize);

            // Store tasks for each sorting algorithm
            Task<double>[] tasks = new Task<double>[5];

            // Each task will handle its own partitioned sorting
            tasks[0] = Task.Run(() => ParallelSortAndMeasureTime(array, QuickSort));
            tasks[1] = Task.Run(() => ParallelSortAndMeasureTime(array, MergeSort));
            tasks[2] = Task.Run(() => ParallelSortAndMeasureTime(array, ShellSort));
            tasks[3] = Task.Run(() => ParallelSortAndMeasureTime(array, SelectionSort));
            tasks[4] = Task.Run(() => ParallelSortAndMeasureTime(array, OptimizedBubbleSort));

            // Wait for all tasks to complete and get their execution times
            double[] times = await Task.WhenAll(tasks);

            // Determine the winner of the current run based on the shortest time
            int winnerIndex = Array.IndexOf(times, times.Min());
            wins[winnerIndex]++;
            Console.WriteLine($"Winner of Run {run + 1}: Algorithm {winnerIndex + 1} with {times[winnerIndex]} ms");
        }

        // Display the total wins for each algorithm after all runs
        Console.WriteLine("\nTotal Wins:");
        Console.WriteLine($"Quick Sort: {wins[0]} wins");
        Console.WriteLine($"Merge Sort: {wins[1]} wins");
        Console.WriteLine($"Shell Sort: {wins[2]} wins");
        Console.WriteLine($"Selection Sort: {wins[3]} wins");
        Console.WriteLine($"Optimized Bubble Sort: {wins[4]} wins");
    }

    static double ParallelSortAndMeasureTime(int[] array, Action<int[], int, int> sortAlgorithm)
    {
        int[][] subArrays = new int[numParts][];
        int partSize = listSize / numParts;

        // Split the array into parts
        for (int i = 0; i < numParts; i++)
        {
            subArrays[i] = new int[partSize];
            Array.Copy(array, i * partSize, subArrays[i], 0, partSize);
        }

        // Sort each part in parallel
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        Parallel.ForEach(subArrays, (subArray) =>
        {
            sortAlgorithm(subArray, 0, subArray.Length - 1);
        });

        // Merge the sorted parts
        var sortedArray = MergeSortedArrays(subArrays);
        stopwatch.Stop();
        return stopwatch.Elapsed.TotalMilliseconds;
    }

    static int[] MergeSortedArrays(int[][] arrays)
    {
        int[] result = new int[listSize];
        int[] indices = new int[numParts];

        for (int i = 0; i < listSize; i++)
        {
            int minIndex = -1;
            int minValue = int.MaxValue;
            for (int j = 0; j < numParts; j++)
            {
                if (indices[j] < arrays[j].Length && arrays[j][indices[j]] < minValue)
                {
                    minValue = arrays[j][indices[j]];
                    minIndex = j;
                }
            }
            result[i] = minValue;
            indices[minIndex]++;
        }

        return result;
    }

    static int[] CreateRandomList(int size)
    {
        return Enumerable.Range(0, size).Select(_ => random.Next(1000000)).ToArray();
    }

    // Quick Sort algorithm implementation
    static void QuickSort(int[] array, int low, int high)
    {
        if (low < high)
        {
            int pi = Partition(array, low, high);
            QuickSort(array, low, pi - 1);
            QuickSort(array, pi + 1, high);
        }
    }

    static int Partition(int[] array, int low, int high)
    {
        int pivot = array[high];
        int i = low - 1;
        for (int j = low; j < high; j++)
        {
            if (array[j] <= pivot)
            {
                i++;
                int temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }
        int temp1 = array[i + 1];
        array[i + 1] = array[high];
        array[high] = temp1;
        return i + 1;
    }

    // Merge Sort algorithm implementation
    static void MergeSort(int[] array, int left, int right)
    {
        if (left < right)
        {
            int middle = (left + right) / 2;
            MergeSort(array, left, middle);
            MergeSort(array, middle + 1, right);
            Merge(array, left, middle, right);
        }
    }

    static void Merge(int[] array, int left, int middle, int right)
    {
        int n1 = middle - left + 1;
        int n2 = right - middle;
        int[] L = new int[n1];
        int[] R = new int[n2];
        Array.Copy(array, left, L, 0, n1);
        Array.Copy(array, middle + 1, R, 0, n2);
        int i = 0, j = 0, k = left;
        while (i < n1 && j < n2)
        {
            if (L[i] <= R[j])
            {
                array[k++] = L[i++];
            }
            else
            {
                array[k++] = R[j++];
            }
        }
        while (i < n1)
        {
            array[k++] = L[i++];
        }
        while (j < n2)
        {
            array[k++] = R[j++];
        }
    }

    // Shell Sort algorithm implementation
    static void ShellSort(int[] array, int low, int high)
    {
        int n = array.Length;
        for (int gap = n / 2; gap > 0; gap /= 2)
        {
            for (int i = gap; i < n; i++)
            {
                int temp = array[i];
                int j;
                for (j = i; j >= gap && array[j - gap] > temp; j -= gap)
                {
                    array[j] = array[j - gap];
                }
                array[j] = temp;
            }
        }
    }

    // Selection Sort algorithm implementation
    static void SelectionSort(int[] array, int low, int high)
    {
        int n = high;
        for (int i = low; i < n; i++)
        {
            int minIndex = i;
            for (int j = i + 1; j <= n; j++)
            {
                if (array[j] < array[minIndex])
                {
                    minIndex = j;
                }
            }
            int temp = array[minIndex];
            array[minIndex] = array[i];
            array[i] = temp;
        }
    }

    // Optimized Bubble Sort algorithm implementation with early exit
    static void OptimizedBubbleSort(int[] array, int low, int high)
    {
        bool swapped;
        int n = high;
        for (int i = low; i < n; i++)
        {
            swapped = false;
            for (int j = low; j < n - i; j++)
            {
                if (array[j] > array[j + 1])
                {
                    int temp = array[j];
                    array[j] = array[j + 1];
                    array[j + 1] = temp;
                    swapped = true;
                }
            }
            // If no two elements were swapped, the array is sorted
            if (!swapped)
                break;
        }
    }
}
