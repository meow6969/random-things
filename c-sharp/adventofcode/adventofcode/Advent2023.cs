namespace adventofcode;

public class Advent2023 {
    public static void Day1()
    {
        string example = "1abc2\npqr3stu8vwx\na1b2c3d4e5f\ntreb7uchet";
        
        int firstNum = 0;
        int lastNum = 0;
        bool isTheFirst;
        bool secondNum;
        
        foreach (string line in example.Split("\n"))
        {
            isTheFirst = true;
            secondNum = false;
            foreach (char c in line)
            {
                if (Char.IsDigit(c))
                {
                    if (isTheFirst)
                    {
                        isTheFirst = false;
                        firstNum = (int)Char.GetNumericValue(c);
                    }
                    else
                    {
                        secondNum = true;
                        lastNum = (int)Char.GetNumericValue(c);
                    }
                }
            }

            if (!secondNum)
            {
                lastNum = firstNum;
            }
            Console.WriteLine($"{firstNum}{lastNum}");
        }
    }
}