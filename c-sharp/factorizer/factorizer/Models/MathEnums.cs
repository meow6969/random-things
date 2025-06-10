using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using static factorizer.UtilityFunctions;

namespace factorizer.Models;

// public class MathEnums
// {
//     private static class EnumExtensions
//     {
//
//     }
// }

public enum SearchingFor
{
    Nothing,
    Parenthesis,
    StartParenthesis,
    EndParenthesis
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
file class CmdFlagsAttribute : Attribute
{
    public string[] Flags { get; init; }
        
    public CmdFlagsAttribute(params string[] flags)
    {
        ArgumentNullException.ThrowIfNull(flags);
        foreach (string flag in flags)
        {
            ArgumentNullException.ThrowIfNull(flag);
        }

        Flags = flags;
    }

    // public CmdFlagsAttribute(string flag)co
    // {
    //     ArgumentNullException.ThrowIfNull(flag);
    //     Flags = [flag];
    // }
}

[AttributeUsage(AttributeTargets.Field)]
file class CmdMethodAttribute : Attribute
{
    public MethodInfo ExecuteMethodInfo { get; init; }
    
    public CmdMethodAttribute(Type executeClass, string executeMethod)
    {
        ArgumentNullException.ThrowIfNull(executeClass);
        ArgumentNullException.ThrowIfNull(executeMethod);
        ExecuteMethodInfo = executeClass.GetMethod(executeMethod, BindingFlags.Static | BindingFlags.Public)
                            ?? throw new InvalidOperationException();
    }
        
    public CmdMethodAttribute(string executeClassName, string executeMethod)
        : this(Type.GetType(executeClassName) ?? throw new InvalidOperationException(), executeMethod)
    {
    }
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
file class CmdParamTypeAttribute(params Type[] typeInfos) : Attribute
{
    // the priority when parsing the args is based on the first defined type
    // this means you should define more restrictive types first
    public TypeInfo[] CompatibleTypes { get; init; } = typeInfos.Select(x => x.GetTypeInfo()).ToArray();
    // -1 just means this isn't used
    [DefaultValue(-1)] 
    public int MinimumRequired { get; init; }
    // -1 just means this isn't used
    [DefaultValue(-1)] 
    public int MaximumAllowed { get; init; }
    
    public (TypeInfo[] compatTypes, int minRequired, int maxAllowed) ToTuple()
    {
        return (
            compatTypes: CompatibleTypes,
            minRequired: MinimumRequired, 
            maxAllowed: MaximumAllowed
            );
    }
    
    public static (TypeInfo[] compatTypes, int minRequired, int maxAllowed)[] ToTupleArray(CmdParamTypeAttribute[] typeParams)
    {
        return typeParams.Select(x => x.ToTuple()).ToArray();
    }
}

[AttributeUsage(AttributeTargets.Field)]
file class CmdParamTypeOptionsAttribute : Attribute
{
     [DefaultValue(false)] 
     public bool AllowUnneededParams { get; init; }
     // // -1 just means this isn't used
     // [DefaultValue(-1)] 
     // public int MinimumParams { get; init; } = -1;
     // // -1 just means this isn't used
     // [DefaultValue(-1)] 
     // public int MaximumParams { get; init; } = -1;
}

// [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
// file class CmdParamsAttribute : Attribute
// {
//     public CmdParamType[] ParamTypes { get; }
//     [DefaultValue(false)] 
//     public bool AllowExtraParams { get; init; } = false;
//     // -1 just means this isn't used
//     [DefaultValue(-1)] 
//     public int MinimumParams { get; init; } = -1;
//     // -1 just means this isn't used
//     [DefaultValue(-1)] 
//     public int MaximumParams { get; init; } = -1;
//         
//     public CmdParamsAttribute( CmdParamType paramTypes)
//     {
//         ArgumentNullException.ThrowIfNull(paramTypes);
//         if (paramTypes.Length == 0)
//         {
//             throw new InvalidDataException();
//         }
//         ParamTypes = paramTypes;
//     }
// }

public enum FuncCode
{
    [CmdMethod(typeof(Program), "OldGetFactorsProgramCmd")]
    None,
    
    // [CmdMethod("Program", "QuadraticFormulaCmd")]
    [CmdMethod(typeof(Program), "QuadraticFormulaCmd")]
    [CmdParamType(typeof(int), typeof(double), MinimumRequired = 3, MaximumAllowed = 3)]
    [CmdParamType(typeof(string), MinimumRequired = 3, MaximumAllowed = 3)]
    // [CmdParamTypeOptions(AllowExtraParams = false)]
    [CmdFlags("q", "-q", "--quad", "--quadratic")]
    // [CmdFlags("q")]
    QuadraticFormula,
    
    [CmdMethod(typeof(Program), "XyToPolarCoordinatesCmd")]
    [CmdParamType(typeof(int), typeof(double), MinimumRequired = 2, MaximumAllowed = 2)]
    [CmdParamType(typeof(string), MinimumRequired = 2, MaximumAllowed = 2)]
    // [CmdParamTypeOptions(AllowExtraParams = false)]
    [CmdFlags("-p", "--polar")]
    // [CmdFlags("q")]
    XyToPolarCoordinates
}

file class EnumExtensionsConstants
{
    // ReSharper disable once InconsistentNaming
    private FuncCode[] _funcCodes;
    // ReSharper disable once InconsistentNaming
    private Dictionary<string, FuncCode> _funcCodeFlags;
    // ReSharper disable once InconsistentNaming
    private Dictionary<FuncCode, string[]> _funcCodeFlagsReversed;
    // ReSharper disable once InconsistentNaming
    private Dictionary<FuncCode, MethodInfo> _executeMethods;
    // ReSharper disable once InconsistentNaming
    private Dictionary<FuncCode, CmdParamTypeAttribute[]> _cmdParamTypes;
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    // private static EnumExtensionsConstants? _instance = null;
    // private static EnumExtensionsConstants _instance = new ();
    public static EnumExtensionsConstants Instance
    {
        get;
        // {
        // return _instance;
        // lock (_lock)
        // {
        //     if (_instance is not null) return _instance;
        //
        //     return _instance;
        // }
        // }
    } = new EnumExtensionsConstants();
    // private static readonly object _lock = new object();
    
    private EnumExtensionsConstants()
    {
        _funcCodes = CreateAllFuncCodes();
        _funcCodeFlags = GetFuncFlags();
        _funcCodeFlagsReversed = ReverseFuncFlags(_funcCodeFlags, _funcCodes);
        _executeMethods = GetExecuteMethods();
        _cmdParamTypes = CreateParamTypes();
    }
    
    private static FuncCode[] CreateAllFuncCodes()
    {
        return Enum.GetValues<FuncCode>();
    }
    
    private static Dictionary<string, FuncCode> GetFuncFlags()
    {
        var flagAttributes = typeof(FuncCode)
            .GetFields()
            .Select(x => new 
            { 
                Value = x, 
                Flags = x.GetCustomAttributes(typeof(CmdFlagsAttribute), false)
                    .Cast<CmdFlagsAttribute>() 
            });
        
        var degrouped = flagAttributes.SelectMany(
            x => x.Flags.SelectMany(y => y.Flags), 
            (x, y) => new { x.Value, Flag = y });
    
        Dictionary<string, FuncCode> flagsDict = degrouped.ToDictionary(
            x => x.Flag, 
            x => (FuncCode)x.Value.GetValue(null)!);
        
        return flagsDict;
    }
    
    private static Dictionary<FuncCode, CmdParamTypeAttribute[]> CreateParamTypes()
    {
        Dictionary<FuncCode, List<CmdParamTypeAttribute>> rDict = [];
        foreach (FuncCode funcValue in Enum.GetValues<FuncCode>())
        {
            rDict[funcValue] = [];
            MemberInfo mInfo = typeof(FuncCode).GetMember(funcValue.ToString())[0];
            CmdParamTypeAttribute[] attr = (CmdParamTypeAttribute[])mInfo.GetCustomAttributes(typeof(CmdParamTypeAttribute), 
                false);
            foreach (CmdParamTypeAttribute paramType in attr)
            {
                rDict[funcValue].Add(paramType);
            }
        }
    
        return rDict.ToDictionary(x => x.Key, x => x.Value.ToArray());
    }

    private static Dictionary<FuncCode, MethodInfo> GetExecuteMethods()
    {
        Dictionary<FuncCode, MethodInfo> executeMethods = [];
        foreach (FuncCode funcValue in Enum.GetValues<FuncCode>())
        {
            MemberInfo mInfo = typeof(FuncCode).GetMember(funcValue.ToString())[0];
            CmdMethodAttribute attr = (CmdMethodAttribute)mInfo.GetCustomAttributes(typeof(CmdMethodAttribute), 
                false)[0];
            executeMethods[funcValue] = attr.ExecuteMethodInfo;
        }
    
        return executeMethods;
    }

    private static Dictionary<FuncCode, string[]> ReverseFuncFlags
        (Dictionary<string, FuncCode> funcFlags, FuncCode[] codes)
    {
        Dictionary<FuncCode, string[]> rDict = ReverseDictionaryWithListValues(funcFlags);
        foreach (FuncCode code in codes)
        {
            if (!rDict.ContainsKey(code)) rDict[code] = [];
        }
        return rDict;
    }

    public static FuncCode[] GetFuncCodes()
    {
        // return _instance._funcCodes;
        return Instance._funcCodes;
    }
    
    public static Dictionary<string, FuncCode> GetFuncCodeFlags()
    {
        return Instance._funcCodeFlags;
    }
    
    public static Dictionary<FuncCode, string[]> GetFuncCodeFlagsReversed()
    {
        return Instance._funcCodeFlagsReversed;
    }
    public static Dictionary<FuncCode, MethodInfo> GetExecMethods()
    {
        return Instance._executeMethods;
    }
    
    public static Dictionary<FuncCode, CmdParamTypeAttribute[]> GetParamTypes()
    {
        return Instance._cmdParamTypes;
    }
}

public static class MathEnums
{
    public static FuncCode? FromArg(string arg)
    {
        FuncCode[] codes = AllCodes();
        arg = arg.Trim();
    
        foreach (FuncCode code in codes)
        {
            if (code.StringInFlags(arg)) return code;
        }

        return null;
    }
    
    public static FuncCode[] FromArgs(string[] args)
    {
        List<FuncCode> newCodes = [];
    
        foreach (string arg in args)
        {
            FuncCode? code = FromArg(arg);
            if (code is not null) newCodes.Add((FuncCode)code);
        }

        return newCodes.ToArray();
    }
    
    private static FuncCode[] AllCodes()
    {
        return EnumExtensionsConstants.GetFuncCodes();
    }
    
    public static string[] GetFlags(this FuncCode code)
    {
        return EnumExtensionsConstants.GetFuncCodeFlagsReversed()[code];
        // switch (code)
        // {
        //     case FuncCode.None:
        //         
        //     case FuncCode.QuadraticFormula:
        //         return EnumExtensionsConstants.GetFuncCodeFlagsReversed()[code];
        //     default:
        //         throw new NotImplementedException();
        // }
    }

    static MethodInfo GetExecMethod(this FuncCode code)
    {
        return EnumExtensionsConstants.GetExecMethods()[code];
    }

    static (TypeInfo[] compatTypes, int minRequired, int maxAllowed)[] ParamTypes(this FuncCode code)
    {
        // ValueTuple<CmdParamTypeAttribute[], int, int> rTuple;
        return CmdParamTypeAttribute.ToTupleArray(EnumExtensionsConstants.GetParamTypes()[code]);
    }

    public static void VerifyNoRepeatTypes(this FuncCode code)
    {
        (TypeInfo[] compatTypes, int minRequired, int maxAllowed)[] paramInfos = code.ParamTypes();
        List<Type> seenTypes = [];
        foreach (Type type in paramInfos.SelectMany(x => x.compatTypes))
        {
            if (seenTypes.Contains(type))
            {
                throw new InvalidDataException();
            }
            seenTypes.Add(type);
        }
    }

    public static void ConvertArgs(this FuncCode code, string[] args)
    {
        List<string> curArgs = args.ToList();
        List<int> paramInfosCountArgs = [];
        Dictionary<string, object> convertedArgs = [];
        (TypeInfo[] compatTypes, int minRequired, int maxAllowed)[] paramInfos = code.ParamTypes();
        for (int i = 0; i < paramInfos.Length; i++)
        {
            paramInfosCountArgs.Add(0);
            
            (TypeInfo[] compatTypes, int minRequired, int maxAllowed) paramInfo = paramInfos[i];
            foreach (string arg in curArgs.ToArray())
            {
                foreach (TypeInfo paramInfoCompatType in paramInfo.compatTypes)
                {
                    object convertedArg;
                    
                    MethodInfo? parseMethod = paramInfoCompatType.GetMethod("TryParse");
                    if (parseMethod is not null)
                    {
                        object r = parseMethod.Invoke(null, [arg, null]) ?? throw new ArgumentException();
                        if ((bool)r)
                        {
                            convertedArg = Convert.ChangeType(r, paramInfoCompatType);
                            convertedArgs[arg] = convertedArg;
                            curArgs.Remove(arg);
                            paramInfosCountArgs[i] += 1;
                            break;
                        }
                    }
                    else parseMethod = paramInfoCompatType.GetMethod("Parse");
                    if (parseMethod is not null)
                    {
                        try
                        {
                            object r = parseMethod.Invoke(null, [arg]) ?? throw new ArgumentException();
                            if (!(bool)r) continue;
                            convertedArg = Convert.ChangeType(r, paramInfoCompatType);
                            convertedArgs[arg] = convertedArg;
                            curArgs.Remove(arg);
                            paramInfosCountArgs[i] += 1;
                            break;
                        }
                        catch (Exception e) when (
                            e is FormatException ||
                            e is OverflowException)
                        {
                            
                        }
                        catch (ArgumentNullException) 
                        {
                            throw new ArgumentException();
                        }
                    }
                    else
                    {
                        try
                        {
                            convertedArg = Convert.ChangeType(arg, paramInfoCompatType);
                            convertedArgs[arg] = convertedArg;
                            curArgs.Remove(arg);
                            paramInfosCountArgs[i] += 1;
                            break;
                        }
                        catch (Exception e) when (
                            e is InvalidCastException ||
                            e is FormatException ||
                            e is ArgumentNullException)
                        {
                            throw new ArgumentException();
                        }
                    }
                }
            }

            if ((paramInfosCountArgs[i] < paramInfo.minRequired && paramInfo.minRequired != -1) ||
                (paramInfosCountArgs[i] > paramInfo.maxAllowed && paramInfo.maxAllowed != -1))
            {
                throw new ArgumentException();
            }
        }
        
    }
    
    public static void ExecuteFunc(this FuncCode code, string[] args)
    {
        // Console.WriteLine("execute func");
        MethodInfo theFunc = code.GetExecMethod();
        // Console.WriteLine($"exec func: {theFunc.Name}");
        List<string> myArgs = args.ToList();
        List<object> convedParams = [];
        foreach (ParameterInfo pInfo in theFunc.GetParameters())
        {
            // Console.WriteLine();
            // Console.WriteLine(pInfo.Name);
            // Console.WriteLine(pInfo.ParameterType);
            TypeInfo pType = pInfo.ParameterType.GetTypeInfo();
            // PrintList(convedParams);
            if (pInfo.ParameterType == typeof(FuncCode))
            {
                // Console.WriteLine(code);
                convedParams.Add(code);
                continue;
            }

            // System.Double.TryParse()
            MethodInfo? parseMethod = pInfo.ParameterType.GetMethod("TryParse",
                BindingFlags.Static | BindingFlags.Public, null,
                new Type[] { typeof(string), pInfo.ParameterType.MakeByRefType() }, null);
            MethodInfo? testMethod = typeof(System.Double).GetMethod(
                "TryParse", new Type[] {typeof(string), typeof(System.Double).MakeByRefType() },
                null);
            // Console.WriteLine($"461 parseMethod: {parseMethod}");
            // Console.WriteLine($"462 testMethod: {testMethod}");
            if (myArgs.Count == 0)
            {
                throw new ArgumentException();
            }
            string curArg = myArgs[0];
            // Console.WriteLine($"curarg: {curArg}");
            // Console.WriteLine($"curparameter: {pInfo.ParameterType}");
            myArgs.Remove(curArg);
            object convertedArg;
            if (parseMethod is not null)
            {
                object?[] parseMethodParams = [curArg, null];
                object r = parseMethod.Invoke(null, parseMethodParams) ?? throw new ArgumentException();
                if ((bool)r && parseMethodParams[1] is not null)
                {
                    convertedArg = Convert.ChangeType(parseMethodParams[1]!, pType);
                    convedParams.Add(convertedArg);
                    continue;
                }
            }
            else parseMethod = pType.GetMethod("Parse", [typeof(string)]);
            if (parseMethod is not null)
            {
                try
                {
                    object r = parseMethod.Invoke(null, [curArg]) ?? throw new ArgumentException();
                    if (!(bool)r) continue;
                    convertedArg = Convert.ChangeType(r, pType);
                    convedParams.Add(convertedArg);
                    continue;
                }
                catch (Exception)
                {
                    throw new ArgumentException();
                }
            }
            else
            {
                try
                {
                    convertedArg = Convert.ChangeType(curArg, pType);
                    convedParams.Add(convertedArg);
                    continue;
                }
                catch (Exception e) when (
                    e is InvalidCastException ||
                    e is FormatException ||
                    e is ArgumentNullException)
                {
                    throw new ArgumentException();
                }
            }
        }
        
        // List<object> convedParams = args.Select(x => (object)x).ToList();
        // convedParams.Insert(0, code);
        
        
        code.GetExecMethod().Invoke(null, convedParams.ToArray());
        
        // switch (code)
        // {
        //     case MathEnums.FuncCode.None:
        //         Program.OldGetFactorsProgram(args);
        //         return;
        //     case FuncCode.QuadraticFormula:
        //         Program.QuadraticFormulaCmd(args);
        //         return;
        //     default:
        //         throw new NotImplementedException();
        // }
    }

    static bool StringInFlags(this FuncCode code, string arg)
    {
        // typeof(FuncCode).GetField("");
        if (code.GetFlags().Contains(arg))
        {
            return true;
        }

        return false;
    }
}



