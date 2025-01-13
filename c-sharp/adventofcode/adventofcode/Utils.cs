using System.Text.Json;

namespace adventofcode;

public static class Utils
{
    public static string GetInput(int year, int day)
    {
        using StreamReader r = new StreamReader("../../../input.json");
        string json = r.ReadToEnd();
        List<AdventInput> source = JsonSerializer.Deserialize<List<AdventInput>>(json) ?? 
                                   throw new InvalidOperationException();
        foreach (var i in source.Where(i => i.Year == year && i.Day == day))
        {
            return i.Input ?? throw new InvalidOperationException();
        }
        // if we get here we cant find the day in the input json
        throw new ArgumentException($"cant find entry for year {year} and day {day}");
    }

    public static bool[] ValidatePassPort(PassPort passPort)
    {
        bool isPassPortValid = true;
        bool isPassPortValid2 = true;
        for (int i = 0; i < passPort.Fields.Count; i++)
        {
            if (passPort.Fields[i] == "null" && i != 7)
            {
                isPassPortValid = false;
                isPassPortValid2 = false;
                continue;
            }

            Int64 m;
            string s;
            
            switch (i)
            {
                case 0: // byr
                    m = Int64.Parse(passPort.Fields[i]);
                    if (!(m is >= 1920 and <= 2002)) isPassPortValid2 = false;
                    break;
                case 1: // iyr
                    m = Int64.Parse(passPort.Fields[i]);
                    if (!(m is >= 2010 and <= 2020)) isPassPortValid2 = false;
                    break;
                case 2: // eyr
                    m = Int64.Parse(passPort.Fields[i]);
                    if (!(2020 <= m && m <= 2030)) isPassPortValid2 = false;
                    break;
                case 3: // hgt
                    s = passPort.Fields[i];
                    if (s.EndsWith("cm"))
                    {
                        m = Int64.Parse(s.Substring(0, s.Length - 2));
                        if (!(m is >= 150 and <= 193)) isPassPortValid2 = false;
                    }
                    else if (s.EndsWith("in"))
                    {
                        m = Int64.Parse(s.Substring(0, s.Length - 2));
                        if (!(m is >= 59 and <= 76)) isPassPortValid2 = false;
                    }
                    else
                    {
                        isPassPortValid2 = false;
                    }
                    break;
                case 4: // hcl
                    s = passPort.Fields[i];
                    if (s.StartsWith('#'))
                    {
                        s = s.Substring(1, s.Length - 1);
                        if (!s.All("0123456789abcdefABCDEF".Contains))
                        {
                            isPassPortValid2 = false;
                        }
                    }
                    else isPassPortValid2 = false;
                    break;
                case 5: // ecl
                    string[] validColors =
                    [
                        "amb",
                        "blu",
                        "brn",
                        "gry",
                        "grn",
                        "hzl",
                        "oth"
                    ];
                    if (!validColors.Contains(passPort.Fields[i])) isPassPortValid2 = false;
                    break;
                case 6: // pid
                    s = passPort.Fields[i];
                    if (s.Length != 9 || !int.TryParse(s, out int _)) isPassPortValid2 = false;
                    break;
            }
        }

        return [isPassPortValid, isPassPortValid2];
    }
}

public class AdventInput
{
    public int Year { get; set; }
    public int Day { get; set; }
    public string? Input { get; set; }
}

public class PassPort
{
    public List<string> Fields = [
        new string("null"),
        new string("null"),
        new string("null"),
        new string("null"),
        new string("null"),
        new string("null"),
        new string("null"),
        new string("null")
    ];
}

