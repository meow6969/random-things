// ReSharper disable UnusedMember.Local

using factorizer.Latex;
using factorizer.Models;
using static factorizer.UtilityFunctions;

namespace factorizer;

internal static class Program
{
    public class PrimeFactors
    {
        private int _generatedToNum = 2;
        private List<int> _primeNumbers = [1, 2];

        private PrimeFactors()
        {
            
        }

        public void OutputPrimeNumbersList()
        {
            Console.Write($"Prime numbers up to {_generatedToNum}: ");
            string txt = "";
            foreach (int num in _primeNumbers)
            {
                txt += num + ", ";
            }
            Console.WriteLine(txt[..^2]);
        }

        private static PrimeFactors _instance = new();
    
        public static List<int> GetPrimeNumbers(int limit=100)
        {
            for (int i = 3; _instance._generatedToNum <= i && i <= limit; i += 2)
            {
                // Console.ReadLine();
                if (IsPrime(i))
                {
                    _instance._primeNumbers.Add(i);
                }
            }
            _instance._generatedToNum = int.Max(limit, _instance._generatedToNum);
            return _instance._primeNumbers;
        }
    }
    
    private static void Main(string[] args)
    {
        OldGetFactorsProgram(args);
        
        // Dictionary<char, int> meow = new Dictionary<char, int>();
        // meow['x'] = 1;
        // SolveExpressionAsX(LatexToMath.LatexExpressionToMathExpression("70x+80-360"), meow);
        // SolveExpressionForY(LatexToMath.LatexExpressionToMathExpression("17x+8"));
        // Console.WriteLine(Math.Pow(1D/144, 1.0D / 2D));
        // Console.WriteLine(1D/12D);
        // DoQuadraticFormula(1, -9, -10);
        // int lcm = GetLeastCommonMultiple(9, 8);
        // Console.WriteLine(lcm);
        // Console.WriteLine($"{lcm} / 5 = {lcm / 5}, {lcm} / 16 = {lcm / 16}");
        // OutputCommonFactors([GetFactors(11),  GetFactors(-110)]);
    }
    //      4x    -1
    //   4x 16x^2 -4x
    //  -1 -4x    1
    //
    // (4)
    private static void DoQuadraticFormula(int a, int b, int c)
    {
        double sum1 = -b;
        double sum2 = -b;
        double theSqrt = Math.Sqrt(Math.Pow(b, 2) - 4 * a * c);
        sum1 += theSqrt;
        sum2 -= theSqrt;
        sum1 /= 2 * a;
        sum2 /= 2 * a;
        
        Console.WriteLine($"Quadratic formula result: {sum1}, {sum2}");
    }
    
    private static void SolveExpressionAsX(MathExpression expression, Dictionary<char, int> variablesAs)
    {
        expression = MathExpression.CombineMathExpressionMathTerms(expression);
        Console.Write($"Solving {expression.StringRepresentation} where: ");
        foreach (KeyValuePair<char, int> varAs in variablesAs)
        {
            Console.Write($"{varAs.Key} = {varAs.Value}, ");
        }
        Console.WriteLine();

        double theResult = 0;
        
        foreach (MathTerm term in expression.Terms)
        {
            // Console.WriteLine($"solving math term: {term.StringRepresentation}");
            double termValue = 1;
            foreach (MathVariable theVar in term.Variables)
            {
                if (!variablesAs.ContainsKey(theVar.Name)) 
                    throw new Exception($"cannot solve expression as {theVar.Name} was not given a value");
                double varValue = Math.Pow(variablesAs[theVar.Name], theVar.Exponent);
                termValue *= varValue;
                // Console.WriteLine($"{theVar.StringRepresentation} = {varValue}");
            }
            termValue *= term.Coefficient;
            // Console.WriteLine($"{term.StringRepresentation} = {termValue}");
            theResult += termValue;
        }
        Console.WriteLine($"Expression result: {theResult}");
    }
    
    private static void SolveExpressionForY(MathExpression expression, int forInt=0)
    {
        expression = MathExpression.CombineMathExpressionMathTerms(expression);
        Console.WriteLine($"Solving {expression.StringRepresentation} equals to {forInt}: ");
        
        int variableValue = 0;
        bool flipValue = false;

        while (true)
        {
            // Console.WriteLine(variableValue);
            double theResult = 0;
            foreach (MathTerm term in expression.Terms)
            {
                double termValue = 1;
                foreach (MathVariable theVar in term.Variables)
                {
                    double varValue = Math.Pow(variableValue, theVar.Exponent);
                    termValue *= varValue;
                    // Console.WriteLine($"{theVar.StringRepresentation} = {varValue}");
                }
                termValue *= term.Coefficient;
                // Console.WriteLine($"{term.StringRepresentation} = {termValue}");
                theResult += termValue;
            }
            // Console.WriteLine(theResult);
            if ((int)theResult == forInt) break;
            if (flipValue)
            {
                variableValue *= -1;
                flipValue = false;
            }
            else
            {
                if (variableValue < 0) variableValue--;
                else variableValue++;
                flipValue = true;
            }

            if (variableValue > 1000) throw new Exception("could not find");
        }
        Console.WriteLine($"Expression result: f(x) = {variableValue}");
    }

    private static int GetLeastCommonMultiple(params int[] nums)
    {
        if (nums.Length == 0) throw new Exception("nums was empty");
        List<Dictionary<int, int>> primeFactorizations = [];
        foreach (int number in nums)
        {
            // Console.WriteLine(number);
            Dictionary<int, int> primeFactorization = GetPrimeFactorization(number);
            // PrintDict(primeFactorization);
            primeFactorizations.Add(primeFactorization);
        }

        Dictionary<int, int> highestPowers = [];
        foreach (Dictionary<int, int> theFactor in primeFactorizations)
        {
            foreach (KeyValuePair<int, int> aPower in theFactor)
            {
                if (highestPowers.ContainsKey(aPower.Key))
                {
                    if (highestPowers[aPower.Key] < aPower.Value) highestPowers[aPower.Key] = aPower.Value;
                }
                else highestPowers[aPower.Key] = aPower.Value;
            }
        }

        double theLcm = 1;
        foreach (KeyValuePair<int, int> highPower in highestPowers)
        {
            Console.WriteLine($"{highPower.Key}^{highPower.Value}");
            theLcm *= Math.Pow(highPower.Key, highPower.Value);
        }

        return (int)theLcm;
    }

    private static void PrintList<T>(List<T> meow)
    {
        foreach (T t in meow)
        {
            Console.Write($"{t}, ");
        }
        Console.WriteLine();
    }
    
    private static void PrintDict<T, T2>(Dictionary<T, T2> meow) where T : notnull
    {
        foreach (KeyValuePair<T, T2> t in meow)
        {
            Console.Write($"{t.Key} = {t.Value}, ");
        }
        Console.WriteLine();
    }
    
    private static void OldGetFactorsProgram(string[] args)
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
        else
        { 
            List<NumberFactors> nums = [];
            
            foreach (string arg in args)
            {
                if (!int.TryParse(arg, out int num))
                {
                    Console.WriteLine($"arg {arg} was not a number");
                    return;
                }

                NumberFactors? factors = OutputFactorsResult("", num);
                if (factors != null) nums.Add(factors);
            }

            OutputCommonFactors(nums.ToArray());
        }
    }

    private static void OutputPrimeFactorization(int num)
    {
        string txt = "prime factorization: ";
        foreach (KeyValuePair<int, int> prime in GetPrimeFactorization(num))
        {
            txt += $"{prime.Key}^{prime.Value} * ";
        }
        Console.WriteLine(txt[..^2]);
    }

    private static Dictionary<int, int> GetPrimeFactorization(int num)
    {
        if (IsPrime(num))
        {
            return new Dictionary<int, int>([new KeyValuePair<int, int>(num, 1)]);
        }

        Dictionary<int, int> numPrimeFactors = [];
        int factorCeiling = (int)Math.Ceiling(num / 2f);

        List<int> theList = PrimeFactors.GetPrimeNumbers(factorCeiling);
        foreach (int prime in theList)
        {
            if (prime > factorCeiling) break;
            if (prime < 2) continue;
            while (num % prime == 0)
            {
                num /= prime;
                if (!numPrimeFactors.TryAdd(prime, 1)) numPrimeFactors[prime]++;
            }
        }
        // whats left of num should be prime now
        if (num != 1 && !numPrimeFactors.TryAdd(num, 1)) numPrimeFactors[num] = 1;
        PrintDict(numPrimeFactors);

        return numPrimeFactors;
    }

    private static void OutputCommonFactors(NumberFactors[] factorsArray)
    {
        int[] commonFactors = GetCommonFactors(factorsArray);
        
        if (commonFactors.Length == 0)
        {
            Console.WriteLine("\nNo common factors\n");
            return;
        }
        Console.Write("\nCommon factors: ");
        foreach (int factor in commonFactors)
        {
            Console.Write($"{factor}, ");
        }
        Console.WriteLine();
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

    private static int[] GetCommonFactors(NumberFactors[] factorsArray)
    {
        if (factorsArray.Length == 0 || factorsArray.Length == 1) return [];

        List<int> commonFactors = [];
        NumberFactors ogFactor = factorsArray[0];
        foreach (int factor in ogFactor.Factors)
        {
            bool commonFactor = true;
            foreach (NumberFactors numberFactor in factorsArray)
            {
                if (!numberFactor.Factors.Contains(factor))
                {
                    commonFactor = false;
                }
            }
            
            if (commonFactor) commonFactors.Add(factor);
        }

        return commonFactors.ToArray();
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
    
    public static void TrackFactors(List<List<int>> factorPairs, List<int> factors, 
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
    
    private static void TestGetFactors()
    {
        int[] numbers = [0, 5, 3, -6, 10];
    
        foreach (int num in numbers)
        {
            OutputFactorsResult("", num);
        }
    }
    
    private static void IsPrimeProgram(string[] args)
    {
        if (args.Length == 0)
        {
            string? userInput = "";
            while (userInput != "q")
            {
                Console.Write("Enter an int: ");
                userInput = Console.ReadLine();
                OutputResult(userInput);
            }
            
        }
        else
        {
            foreach (string i in args)
            {
                OutputResult(i);
            }
        }
    }
    
    private static void OutputResult(string? i)
    {
        if (i == null)
        {
            Console.WriteLine("No user input found");
            return;
        }
        
        int? num = GetNumberFromString(i);
        
        if (num == null) return;
        
    
        Console.WriteLine(IsPrime((int)num) ? $"{i} is prime" : $"{i} is composite");
    }
}
