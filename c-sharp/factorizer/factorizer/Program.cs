﻿// ReSharper disable UnusedMember.Local

using System.Collections.ObjectModel;
using System.Numerics;
using System.Reflection;
using factorizer.Latex;
using factorizer.Models;
using static factorizer.UtilityFunctions;

namespace factorizer;

internal static class Program
{
    public class PrimeFactors
    {
        private int _generatedToNum = 2;
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private List<int> _primeNumbers = [1, 2];

        private PrimeFactors()
        {
            
        }

        public static void OutputPrimeNumbersList()
        {
            Console.Write($"Prime numbers up to {_instance._generatedToNum}: ");
            string txt = "";
            foreach (int num in _instance._primeNumbers)
            {
                txt += num + ", ";
            }
            Console.WriteLine(txt[..^2]);
        }

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
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

        public static PrimeFactors GetInstance()
        {
            return _instance;
        }
    }
    private static void Main(string[] args)
    {
        // GetFactors(9.88d).PrintFactors(duplicates:true);
        // GetFactors(9.88d).PrintFactors();
        
        // DoQuadraticFormula(-25, 150, 200);
        // MethodInfo mInfo = typeof(Program).GetMethod("GetLeastCommonMultiple", 
        //     BindingFlags.Static | BindingFlags.NonPublic)!;
        // foreach (ParameterInfo pInfo in mInfo.GetParameters())
        // {
        //     Console.WriteLine(pInfo);
        // }
        ArgParser parsed = new ArgParser(args);
        // Console.WriteLine("original");
        // parsed.OriginalArgs.ToList().ForEach(Console.WriteLine);
        // Console.WriteLine("no flags");
        // parsed.ArgsNoFlags.ToList().ForEach(Console.WriteLine);
        // Console.WriteLine("flags");
        // parsed.Flags.ToList().ForEach(Console.WriteLine);
        foreach (FuncCode code in parsed.Flags)
        {
            // Console.WriteLine(code);
            code.ExecuteFunc(parsed.ArgsNoFlags);
        }
        
        // if (args[0] == "-q") DoQuadraticFormula(int.Parse(args[1]), int.Parse(args[2]), int.Parse(args[3]));
        // if (parsed.Flags.Length > 0)
        // {
        //     foreach (FuncCode code in parsed.Flags)
        //     {
        //         code.ExecuteFunc(parsed.ArgsNoFlags);
        //     }
        //     // DoQuadraticFormula(double.Parse(args[1]), double.Parse(args[2]), double.Parse(args[3]));
        // }
        // else OldGetFactorsProgram(args);
        // DoTrials(5, 10, 1000000);

    }

    private static double RadiansToDegrees(double radians)
    {
        return radians * 180 / Math.PI;
    }

    public static void XyToPolarCoordinatesCmd(double x, double y)
    {
        double magnitude = Math.Sqrt(x * x + y * y);
        double baseAngle = RadiansToDegrees(Math.Atan2(Math.Abs(y), Math.Abs(x)));
        double polarAngle;
        if (x < 0 && y > 0) polarAngle = 180 - baseAngle;
        else if (x < 0 && y < 0) polarAngle = 180 + baseAngle;
        else if (x > 0 && y < 0) polarAngle = 360 - baseAngle;
        else polarAngle = baseAngle;
        
        Console.WriteLine($"polar coordinates: ({x},{y})");
        Console.WriteLine($"magnitude: {magnitude}");
        Console.WriteLine($"baseAngle: {baseAngle}");
        Console.WriteLine($"polarAngle: {polarAngle}");
        Console.WriteLine($"polar coordinates: ({magnitude},{polarAngle})");
    }

    private static int Factorial(int n)
    {
        return Enumerable.Range(1, n).Aggregate(1, (p, item) => p * item);
    }

    private static void DoTrials(int sizeHand, int sizePile, int trials)
    {
        List<int> pile = Enumerable.Range(0, sizePile).ToList() ;
        List<int> wanted = pile[..sizeHand];
        int timesGot = 0;
        long pre = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        
        Random r = new Random();
        for (int i = 0; i < trials; i++)
        {
            List<int> newPile = pile.ConvertAll(card => card);
            List<int> hand = [];

            for (int j = 0; j < sizeHand; j++)
            {
                int pick = r.Next(newPile.Count);
                hand.Add(newPile[pick]);
                newPile.RemoveAt(pick);
            }

            bool got = true;
            foreach (int j in hand)
            {
                if (!wanted.Contains(j))
                {
                    got = false;
                    break;
                }
            }

            if (got) timesGot += 1;
        }
        
        long now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        Console.WriteLine($"time took: {(now - pre) / 1000d}s");
        Console.WriteLine($"timesGot: {timesGot}");
        Console.WriteLine($"trials: {trials}");
        Console.WriteLine($"times_got / trials: {(double)timesGot / trials}");
        Console.WriteLine($"{wanted.Count}! / ({pile.Count}! / {pile.Count - wanted.Count}!): {(double)Factorial(wanted.Count) / (Factorial(pile.Count) / Factorial(pile.Count - wanted.Count))}");
    }
    
    //      4x    -1
    //   4x 16x^2 -4x
    //  -1 -4x    1
    //
    // (4)

    // public static void QuadraticFormulaCmd(FuncCode code, string[] args)
    public static void QuadraticFormulaCmd(FuncCode code, double a, double b, double c)
    {
        // double[] newArgs = args.Select(x => double.Parse(x)).ToArray();
        // DoQuadraticFormula(newArgs[0], newArgs[1], newArgs[2]);
        DoQuadraticFormula(a, b, c);
    }

    public static void OldGetFactorsProgramCmd()
    {
        OldGetFactorsProgram([]);
    }
    
    private static void DoQuadraticFormula(double a, double b, double c)
    {
        Console.WriteLine($"a={a}, b={b}, c={c}");
        
        double sum1 = -b;
        double sum2 = -b;
        double denominator = 2 * a;
        int gcf;
        string imaginary = "";
        double underTheSqrt = Math.Pow(b, 2) - 4 * a * c;
        // Console.WriteLine(underTheSqrt);
        if (underTheSqrt < 0)
        {
            imaginary = "i";
            underTheSqrt *= -1;
        }
        // Console.WriteLine(underTheSqrt);
        // Console.WriteLine(SimplifyRadical((int)underTheSqrt));
        double theSqrt = Math.Sqrt(underTheSqrt);
        // Console.WriteLine(denominator);
        // Console.WriteLine(imaginary);
        // Console.WriteLine(theSqrt);
        if (!IsNumberInteger(theSqrt))
        {
            // the thingy returned as a non perfect square root
            (int coefficient, int underTheRoot) = SimplifyRadical((int)underTheSqrt);
            // Console.WriteLine($"coefficient={coefficient}, underTheRoot={underTheRoot}");

            if (IsNumberInteger(denominator) && IsNumberInteger(b))
            {
                gcf = GetGreatestCommonFactor(coefficient, (int)denominator, -(int)b);
            }
            else
            {
                gcf = 1;
            }
            // Console.WriteLine($"gcf={gcf}, denominator={denominator}, IsNumberInteger(b={b})={IsNumberInteger(b)}, IsNumberInteger(denominator={denominator})={IsNumberInteger(denominator)}");
            
            coefficient /= gcf;
            denominator /= gcf;
            b /= gcf;
            string bString = "+";
            string coefficientString = "";
            string preDenominatorString = "";
            string denominatorString = "";
            if (coefficient != 1) coefficientString = $"{coefficient}";
            else bString += " ";
            
            if (!NumberEqualsInt(denominator, 1))
            {
                preDenominatorString = "\\frac{";
                denominatorString = $"}}{{{denominator}}}";
            }
            
            if (!NumberEqualsInt(b, 0)) bString = $"{-b} + ";
            string exactEquations = "Quadratic formula result: ";
            exactEquations += $"{preDenominatorString}{bString}{coefficientString}\\sqrt{{{underTheRoot}}}{imaginary}" +
                              $"{denominatorString}, ";
            
            exactEquations += $"{preDenominatorString}{bString.Replace('+', '-')}{coefficientString}" +
                              $"\\sqrt{{{underTheRoot}}}{imaginary}{denominatorString}";
            
            Console.WriteLine(exactEquations);
            if (imaginary == "i") return;
            Console.WriteLine($"Decimal values:           " +
                              $"{(-b + coefficient * Math.Sqrt(underTheRoot)) / denominator}, " +
                              $"{(-b - coefficient * Math.Sqrt(underTheRoot)) / denominator}");
            return;
        }

        string displayMessage = "Quadratic formula result: ";
        if (imaginary != "i")
        {
            sum1 += theSqrt;
            sum2 -= theSqrt;
            double postDivide1 = sum1 / (2 * a);
            double postDivide2 = sum2 / (2 * a);
        
            if (!IsNumberInteger(postDivide1))
            {
                double numerator = sum1;
                Console.WriteLine(numerator);
                Console.WriteLine(denominator);
                if (IsNumberInteger(denominator))
                {
                    gcf = GetGreatestCommonFactor((int)numerator, (int)denominator);
                }
                else
                {
                    gcf = 1;
                }
            
                displayMessage += $"{numerator / gcf} / {denominator / gcf}, ";
            }
            else displayMessage += $"{postDivide1}, ";

            if (!IsNumberInteger(postDivide2))
            {
                double numerator = sum2;
                if (IsNumberInteger(denominator))
                {
                    gcf = GetGreatestCommonFactor((int)numerator, (int)denominator);
                }
                else
                {
                    gcf = 1;
                }
            
                displayMessage += $"{numerator / gcf} / {denominator / gcf}";
            }
            else displayMessage += $"{postDivide2}";
            Console.WriteLine(displayMessage);
            return;
        }
        
        if (IsNumberInteger(denominator))
        {
            gcf = GetGreatestCommonFactor((int)denominator, (int)theSqrt);
        }
        else
        {
            gcf = 1;
        }
        int rightNumerator = (int)theSqrt / gcf;
        double rightDenominator = denominator / gcf;
        if (IsNumberInteger(denominator))
        {
            gcf = GetGreatestCommonFactor((int)denominator, (int)sum1);
        }
        else
        {
            gcf = 1;
        }
        int leftNumerator = (int)sum1;
        leftNumerator /= gcf;
        double leftDenominator = denominator / gcf;
        // Console.WriteLine(sum1);
        string leftSide;
        if (!NumberEqualsInt(rightDenominator, -1) && IsNumberInteger(leftDenominator))
        {
            int lcm = GetLeastCommonMultiple(leftNumerator, (int)leftDenominator);
            
            leftSide = $"\\frac{{{leftNumerator / (lcm / leftNumerator)}}}" +
                       $"{{{leftDenominator / (lcm / leftDenominator)}}}";
        }
        else
        {
            if (NumberEqualsInt(leftDenominator, 1))
            {
                leftSide = $"\\frac{{{leftNumerator}}}{{{leftDenominator}}}";
            }
            else
            {
                leftSide = $"{leftNumerator}";
            }
        }

        string rightSide;

        if (rightNumerator != 1)
        {
            rightSide = $"{rightNumerator}{imaginary}";
        }
        else
        {
            rightSide = imaginary;
        }
        Console.WriteLine($"{displayMessage}{leftSide} + {rightSide}, {leftSide} - {rightSide}");

        // Console.WriteLine($"Quadratic formula result: {sum1}, {sum2}");
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
    
    public static void OldGetFactorsProgram(string[] args)
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

    private static (int, int) SimplifyRadical(int theSquareRooted)
    {
        NumberFactors theFactors = GetFactors(theSquareRooted);
        int greatestPerfectSquareFactor = 1;
        foreach (int factor in theFactors.Factors)
        {
            if (IsNumberInteger(Math.Sqrt(factor)) && factor > greatestPerfectSquareFactor)
            {
                greatestPerfectSquareFactor = factor;
            }
        }

        if (greatestPerfectSquareFactor == 1) return (1, theSquareRooted);
        //          (coefficient, undertheroot)
        return ((int)Math.Sqrt(greatestPerfectSquareFactor), theSquareRooted / greatestPerfectSquareFactor);
    }

    private static int GetGreatestCommonFactor(NumberFactors[] numberFactors)
    {
        int gcf = 1;
        
        if (numberFactors.Length is 0 or 1) return 1;
        
        foreach (int factor in GetCommonFactors(numberFactors))
        {
            if (factor > gcf) gcf = factor;
        }

        return gcf;
    }
    
    private static int GetGreatestCommonFactor(params int[] numbers)
    {
        List<NumberFactors> theFactors = [];
        foreach (int num in numbers)
        {
            theFactors.Add(GetFactors(num));
        }

        return GetGreatestCommonFactor(theFactors.ToArray());
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
