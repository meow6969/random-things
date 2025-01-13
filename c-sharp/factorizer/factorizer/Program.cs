namespace factorizer;

internal static class Program
{
    private class NumberFactors(List<List<int>> factorPairs, List<int> factors)
    {
        public readonly List<List<int>> FactorPairs = factorPairs;
        public readonly List<int> Factors = factors;
    }
    
    
    private static void Main(string[] args)
    {
        GetFactorsProgram(args);
        // TestGetFactors();
    }
    
    
    private static void GetFactorsProgram(string[] args)
    {
        if (args.Length == 0)
        {
            string? userInput = "";
            while (userInput != "q")
            {
                Console.Write("Enter an int: ");
                userInput = Console.ReadLine();
                if (userInput == null)
                {
                    Console.WriteLine("Did not get user input");
                    continue;
                }

                string[] userInputSplit = userInput.Split();
                string? userInputNumberSum = null;
                
                if (userInputSplit.Length == 2)
                {
                    userInput = userInputSplit[0];
                    userInputNumberSum = userInputSplit[1];
                }
                NumberFactors? factors = OutputFactorsResult(userInput);
                if (factors == null) continue;
                if (userInputNumberSum != null) OutputDoFactorsAddUpTo(factors, userInputNumberSum);
            }
            
        }
        else if (args.Length == 1) OutputFactorsResult(args[0]);
        else if (args.Length == 2)
        {
            NumberFactors? factors = OutputFactorsResult(args[0]);
            if (factors == null) return;
            OutputDoFactorsAddUpTo(factors, args[1]);
        }
        else Console.WriteLine("Too many inputs, exiting...");
    }
    

    private static void OutputDoFactorsAddUpTo(NumberFactors factors, string i, int? m=null)
    {
        if (m == null)
        {
            m = GetNumberFromString(i);
            if (m == null) return;
        }

        var sum = (int)m;
        
        List<int>? correctFactorPair = null;
        
        foreach (List<int> factorPair in factors.FactorPairs)
        {
            if (factorPair[0] + factorPair[1] == sum) correctFactorPair = factorPair;
        }

        if (correctFactorPair != null)
        {
            Console.WriteLine($"Factors that add up to {sum}: ({correctFactorPair[0]}, {correctFactorPair[1]})\n");
            return;
        }
        Console.WriteLine($"No factors add up the the sum: {sum}\n");
    }
    

    private static int? GetNumberFromString(string numString)
    {
        int? num = null;
        
        try
        {
            num = Convert.ToInt32(numString);
        }
        catch (Exception e)
        {
            if (e is OverflowException)
            {
                Console.WriteLine($"{numString} is too big for size int32, skipping");
                return num;
            }

            Console.WriteLine($"could not convert {numString} to int, skipping");
            return num;
        }

        return num;
    }
    
    
    private static NumberFactors? OutputFactorsResult(string i, int? m=null)
    {
        if (m == null)
        {
            m = GetNumberFromString(i);
            if (m == null) return null;
        }
        
        var num = (int)m;
        
        NumberFactors factors = GetFactors(num);
        Console.Write($"Factors for {num}: ");
        foreach (int factor in factors.Factors)
        {
            Console.Write($"{factor}, ");
        }
        Console.Write($"\nFactors for {num} in pairs: ");
        foreach (List<int> factorPair in factors.FactorPairs)
        {
            Console.Write($"({factorPair[0]}, {factorPair[1]}), ");
        }
        Console.WriteLine("\n");

        return factors;
    }
    

    private static NumberFactors GetFactors(int num)
    {
        if (num == 0) return new NumberFactors([[0, 0]], [0]);
        bool negativeNum = false;
        if (num < 0)
        {
            num = Math.Abs(num);
            negativeNum = true;
        }
        
        int boundary = (int)Math.Ceiling(Math.Sqrt(num));
        List<List<int>> factorPairs = [];
        List<int> factors = [];

        if (IsPrime(num))
        {
            TrackFactors(factorPairs, factors, [1, num], negativeNum);
            
            return new NumberFactors(factorPairs, factors);
        }

        for (int i = 1; i < boundary; i++)
        {
            if (factors.Contains(i)) continue;
            if (num % i != 0) continue;
            
            int otherNum = num / i;
            List<int> factorPair = [i, otherNum];
            TrackFactors(factorPairs, factors, factorPair, negativeNum);
        }
        
        factors.Sort();

        return new NumberFactors(factorPairs, factors);
    }


    private static void TrackFactors(List<List<int>> factorPairs, List<int> factors, 
        List<int> factorPair, bool negativeNum)
    {
        if (negativeNum)
        {
            factorPairs.Add([-factorPair[0], factorPair[1]]);
            factorPairs.Add([factorPair[0], -factorPair[1]]);
        }
        else
        {
            factorPairs.Add([factorPair[0], factorPair[1]]);
            factorPairs.Add([-factorPair[0], -factorPair[1]]);
        }

        if (!factors.Contains(factorPair[0]))
        {
            factors.Add(factorPair[0]);
            factors.Add(-factorPair[0]);
        }
        if (!factors.Contains(factorPair[1]))
        {
            factors.Add(factorPair[1]);
            factors.Add(-factorPair[1]);
        }
    }
    

    private static bool IsPrime(int num)
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

        int boundary = (int)Math.Floor(Math.Sqrt(num));
        for (int i = 3; i <= boundary; i+= 2)
            if (num % i == 0)
                return false;
        return true;
    }
    
    // private static void TestGetFactors()
    // {
    //     int[] numbers = [0, 5, 3, -6, 10];
    //
    //     foreach (int num in numbers)
    //     {
    //         OutputFactorsResult("", num);
    //     }
    // }
    //
    //
    // private static void IsPrimeProgram(string[] args)
    // {
    //     if (args.Length == 0)
    //     {
    //         string? userInput = "";
    //         while (userInput != "q")
    //         {
    //             Console.Write("Enter an int: ");
    //             userInput = Console.ReadLine();
    //             OutputResult(userInput);
    //         }
    //         
    //     }
    //     else
    //     {
    //         foreach (string i in args)
    //         {
    //             OutputResult(i);
    //         }
    //     }
    // }
    //
    //
    // private static void OutputResult(string? i)
    // {
    //     if (i == null)
    //     {
    //         Console.WriteLine("No user input found");
    //         return;
    //     }
    //     
    //     int? num = GetNumberFromString(i);
    //     
    //     if (num == null) return;
    //     
    //
    //     Console.WriteLine(IsPrime((int)num) ? $"{i} is prime" : $"{i} is composite");
    // }
}