using System.Collections;
using factorizer;
using factorizer.Models;
// ReSharper disable MemberCanBePrivate.Global

namespace factorizer;

public abstract class UtilityFunctions
{
    private const int IndentAmount = 4;
    
    public static string RemoveLastFromString(string text, int lastToRemove=1)
    {
        // Console.WriteLine(text);
        // Console.WriteLine(text.Substring(0, text.Length - lastToRemove));
        return text.Substring(0, text.Length - lastToRemove);
    }
    
    public static void PrintWithIndent(string text, int indent, bool newLine=false)
    {
        if (newLine) Console.Write('\n');
        for (int i = 0; i < indent * IndentAmount; i++) Console.Write(' ');
        Console.WriteLine(text);
    }
    
    // TODO: update this
    public static NumberFactors GetFactors(int num)
    {
        if (num == 0) return new NumberFactors([[0, 0]], [0]);
        bool negativeNum = false;
        if (num < 0)
        {
            num = Math.Abs(num);
            negativeNum = true;
        }
        
        int boundary = (int)Math.Ceiling(num / 2f);
        // Console.WriteLine(boundary);
        List<List<int>> factorPairs = [];
        List<int> factors = [];

        if (IsPrime(num))
        {
            Program.TrackFactors(factorPairs, factors, [1, num], negativeNum);
            
            return new NumberFactors(factorPairs, factors);
        }

        for (int i = 1; i <= boundary; i++)
        {
            if (factors.Contains(i)) continue;
            if (num % i != 0) continue;
            
            int otherNum = num / i;
            List<int> factorPair = [i, otherNum];
            Program.TrackFactors(factorPairs, factors, factorPair, negativeNum);
        }
        
        factors.Sort();

        return new NumberFactors(factorPairs, factors);
    }
    
    public class NumberFactors(List<List<int>> factorPairs, List<int> factors)
    {
        public readonly List<List<int>> FactorPairs = factorPairs;
        public readonly List<int> Factors = factors;

        public void PrintFactors(bool duplicates = false)
        {
            string txt = "factors: ";
            if (duplicates)
            {
                foreach (List<int> factorPair in FactorPairs)
                {
                    txt += $"{factorPair[0]}, {factorPair[1]}, ";
                }
            }
            else
            {
                foreach (int factor in Factors)
                {
                    txt += $"{factor}, ";
                }
            }
        
            Console.WriteLine(txt[..^2]);
        }
    
        public void PrintPrimeFactors(bool duplicates = false)
        {
            string txt = "prime factors: ";
            if (duplicates)
            {
                foreach (List<int> factorPair in FactorPairs)
                {
                    if (IsPrime(factorPair[0])) txt += $"{factorPair[0]}, ";
                    if (IsPrime(factorPair[1])) txt += $"{factorPair[1]}, ";
                }
            }
            foreach (int factor in Factors)
            {
                if (!IsPrime(factor)) continue;
                txt += $"{factor}, ";
            }
            Console.WriteLine(txt[..^2]);
        }
    }
    
    public static bool IsPrime(int num)
    {
        num = Math.Abs(num);

        switch (num)
        {
            case 0:
                return false;
            case 1 or 2:
                return true;
        }

        if (num % 2 == 0) return false;

        int boundary = (int)Math.Ceiling(num / 2f);
        for (int i = 3; i <= boundary; i+= 2)
            if (num % i == 0)
                return false;
        return true;
    }
    
    public static int ManyMin(params int[] nums)
    {
        int lowestNum = int.MaxValue;
        foreach (int num in nums) lowestNum = Math.Min(lowestNum, num);
        return lowestNum;
    }

    public static void PrintDict<TKey, TValue>(Dictionary<TKey, TValue> dictionary) where TKey : notnull
    {
        foreach (TKey theKey in dictionary.Keys)
        {
            Console.WriteLine($"{theKey}: {dictionary[theKey]}");
        }
    }
    
    public static void PrintList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Console.WriteLine($"index={i}: {list[i]}");
        }
    }
    
    public static void PrintArray<T>(T[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            Console.WriteLine($"index={i}: {array[i]}");
        }
    }
}