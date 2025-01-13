

namespace adventofcode;

public static class Advent2020
{
    public static void BoilerPlate()
    {
        // string input = "BFFFBBFRRR\nFFFBBBFRRR\nBBFFBBFRLL";
        string input = Utils.GetInput(2020, 0);
    }

    public static void Day1()
    {
        // string example = "1721\n979\n366\n299\n675\n1456";
        string example = Utils.GetInput(2020, 1);
        
        var allNums = new List<int>();
        foreach (string i in example.Split("\n"))
        {
            int num = Int32.Parse(i);
            allNums.Add(num);
        }

        int first = 0;
        int second = 0;
        
        foreach (int i in allNums)
        {
            foreach (int i2 in allNums)
            {
                foreach (int i3 in allNums)
                {
                    if (i + i2 + i3 == 2020)
                    {
                        second = i * i2 * i3;
                    }
                }
                if (i + i2== 2020)
                {
                    first = i * i2;
                }
            }
        }
        
        Console.WriteLine($"Day 1\nPart 1: {first}\nPart 2: {second}");
    }

    public static void Day2()
    {
        // string input = "1-3 a: abcde\n1-3 b: cdefg\n2-9 c: ccccccccc";
        string input = Utils.GetInput(2020, 2);
        
        int validPass = 0;
        int validPass2 = 0;
        
        foreach (string line in input.Split("\n"))
        {
            string[] newLine = line.Split();

            var lowerBound = Int32.Parse(newLine[0].Split("-")[0]);
            var upperBound = Int32.Parse(newLine[0].Split("-")[1]);
            var theChar = newLine[1][0];
            var thePass = newLine[2];
            
            // Console.WriteLine($"{lowerBound}-{upperBound} {theChar}: {thePass}");

            int appearances = 0;
            int appearances2 = 0;

            int currentIndex = 0;
            
            foreach (char letter in thePass)
            {
                currentIndex++;
                if (letter == theChar)
                {
                    if (currentIndex == lowerBound || currentIndex == upperBound) appearances2++;

                    appearances++;
                }
            }

            if (appearances2 == 1) validPass2++;

            if (appearances >= lowerBound && appearances <= upperBound) validPass++;
        }
        
        Console.WriteLine($"Day 2\nPart 1: {validPass}\nPart 2: {validPass2}");
    }

    public static void Day3()
    {
        // string input = "..##.......\n#...#...#..\n.#....#..#.\n..#.#...#.#\n.#...##..#.\n..#.##.....\n.#.#.#....#\n" +
        //                ".#........#\n#.##...#...\n#...##....#\n.#..#...#.#";
        string input = Utils.GetInput(2020, 3);

        string[] map = input.Split("\n");

        List<int>[] slopes =
        [
            [1, 1],
            [3, 1],
            [5, 1],
            [7, 1],
            [1, 2]
        ];
        
        int iterations = 0;
        List<int> trees = [];
        foreach (List<int> i in slopes)
        {
            int xPos = i[0];
            int yPos = i[1];
            
            trees.Add(0);
            while (yPos < map.Length)
            {
                if (xPos >= map[0].Length) xPos -= (map[0].Length);
                if (map[yPos][xPos] == '#') trees[iterations]++;

                xPos += i[0];
                yPos += i[1];
            }

            iterations++;
        }
        
        Int64 multiplied = 0;
        foreach (int tree in trees)
        {
            if (multiplied == 0)
            {
                multiplied = tree;
                continue;
            }

            multiplied *= tree;
        }
        
        Console.WriteLine($"Day 3\nPart 1: {trees[1]}\nPart 2: {multiplied}");
    }
    
    public static void Day4()
    {
        // string input = "eyr:1972 cid:100\nhcl:#18171d ecl:amb hgt:170 pid:186cm iyr:2018 byr:1926\n\n" +
        //                "iyr:2019\nhcl:#602927 eyr:1967 hgt:170cm\necl:grn pid:012533040 byr:1946\n\n" +
        //                "hcl:dab227 iyr:2012\necl:brn hgt:182cm pid:021572410 eyr:2020 byr:1992 cid:277\n\n" +
        //                "hgt:59cm ecl:zzz\neyr:2038 hcl:74454a iyr:2023";
        string input = Utils.GetInput(2020, 4);
        PassPort passPort = new PassPort();
        int validPassPorts = 0;
        int validPassPorts2 = 0;
        bool[] temp;

        foreach (string line in input.Split("\n"))
        {
            if (line == "")  // blank space for new passport
            {
                temp = Utils.ValidatePassPort(passPort);
                if (temp[0]) validPassPorts++;
                if (temp[1]) validPassPorts2++;
                
                passPort = new PassPort();
                continue;
            }
            
            foreach (string data in line.Split())
            {
                string[] infos = data.Split(':');
                switch (infos[0])
                {
                    case "byr":
                        passPort.Fields[0] = infos[1];
                        break;
                    case "iyr":
                        passPort.Fields[1] = infos[1];
                        break;
                    case "eyr":
                        passPort.Fields[2] = infos[1];
                        break;
                    case "hgt":
                        passPort.Fields[3] = infos[1];
                        break;
                    case "hcl":
                        passPort.Fields[4] = infos[1];
                        break;
                    case "ecl":
                        passPort.Fields[5] = infos[1];
                        break;
                    case "pid":
                        passPort.Fields[6] = infos[1];
                        break;
                    case "cid":
                        passPort.Fields[7] = infos[1];
                        break;
                    default:
                        Console.WriteLine(infos);
                        break;
                }
            }
        }

        temp = Utils.ValidatePassPort(passPort);
        if (temp[0]) validPassPorts++;
        if (temp[1]) validPassPorts2++;
        
        Console.WriteLine($"Day 4\nPart 1: {validPassPorts}\nPart 2: {validPassPorts2}");
    }

    public static void Day5()
    {
        static int GetSeatId(int row, int column)
        {
            return row * 8 + column;
        }
        
        // string input = "BFFFBBFRRR\nFFFBBBFRRR\nBBFFBBFRLL";
        string input = Utils.GetInput(2020, 5);

        string[] boardingPasses = input.Split("\n");
        int highestId = 0;
        List<List<int>> takenSeats = [];

        foreach (string boardingPass in boardingPasses)
        {
            // int[] row = Enumerable.Range(0, 127).ToArray();
            // int[] column = [0, 7];
            // Console.WriteLine("New boarding pass");
            int[] row = [0, 127];
            int[] column = [0, 7];
            
            for (int j = 0; j < 8; j++)
            {
                List<int> nyaaa = [];
                for (int k = 0; k < 128; k++)
                {
                    nyaaa.Add(0);
                }
                takenSeats.Add(nyaaa);
            }

            for (int i = 0; i < boardingPass.Length; i++)
            {
                // Console.WriteLine($"({row[0]}, {row[1]})");
                
                switch (boardingPass[i])
                {
                    case 'B':
                        row[0] += (row[1] - row[0] + 1) / 2;
                        break;
                    case 'F':
                        row[1] = (row[1] - row[0]) / 2 + row[0];
                        break;
                    case 'R':
                        column[0] += (column[1] - column[0] + 1) / 2;
                        break;
                    case 'L':
                        column[1] = (column[1] - column[0]) / 2 + column[0];
                        break;
                }
            }
            // Console.WriteLine(takenSeats[7].ToArray().Length);
            takenSeats[column[0]][row[0]] = 1;
            
            if (GetSeatId(row[0], column[0]) > highestId)
            {
                highestId = GetSeatId(row[0], column[0]);
            }
        }

        // ReSharper disable once CollectionNeverQueried.Local
        List<string> seatGraph = [];
        int partTwoAnswer = 0;
        
        for (int y = 0; y < 8; y++)
        {
            string meow = "";
            for (int x = 0; x < 128; x++)
            {
                if (takenSeats[y][x] == 1) meow += "x";
                else
                {
                    meow += ".";
                    if (takenSeats[Math.Clamp(y - 1, 0, 7)][x] == 1 &&
                        takenSeats[Math.Clamp(y + 1, 0, 7)][x] == 1 &&
                        takenSeats[y][Math.Clamp(x - 1, 0, 127)] == 1 &&
                        takenSeats[y][Math.Clamp(x + 1, 0, 127)] == 1)
                    {
                        partTwoAnswer = GetSeatId(x, y);
                    }
                }
            }
            seatGraph.Add(meow);
        }
        
        Console.WriteLine($"Part 1: {highestId}\nPart 2: {partTwoAnswer}");
        // seatGraph.ForEach(Console.WriteLine);
    }

    public static void Day6()
    {
        static bool DoGroupLogic(string[] lines, int i)
        {
            if (lines.ElementAtOrDefault(i + 1) == null)
            {
                return true;
            }

            if (lines[i + 1] == "")
            {
                return true;
            }

            return false;
        }
        
        // string input = "abc\n\na\nb\nc\n\nab\nac\n\na\na\na\na\n\nb";
        string input = Utils.GetInput(2020, 6);

        int totalAnswers = 0;
        int totalAnswers2 = 0;
        int groupSize = 1;
        List<char> usedLetters = [];
        Dictionary<char, int> groupLetters = [];
        string[] lines = input.Split("\n");
        
        for (int i = 0; i < lines.Length; i++)
        {
            foreach (char letter in lines[i])
            {
                if (!usedLetters.Contains(letter)) usedLetters.Add(letter);
                if (!groupLetters.Keys.Contains(letter)) groupLetters[letter] = 1;
                else groupLetters[letter]++;
            }
            
            groupSize++;
            
            if (DoGroupLogic(lines, i)) // new group next
            {
                groupSize--;
                foreach (char _ in usedLetters)
                {
                    totalAnswers++;
                }

                foreach (int numVotes in groupLetters.Values)
                {
                    if (numVotes >= groupSize)
                    {
                        totalAnswers2++;
                    }
                }

                usedLetters = [];
                groupLetters = [];
                groupSize = 0;
            }
        }
        
        Console.WriteLine($"Part 1: {totalAnswers}\nPart 2: {totalAnswers2}");
    }
    
    public static void Day7()
    {
        string input = "light red bags contain 1 bright white bag, 2 muted yellow bags.\n" +
                       "dark orange bags contain 3 bright white bags, 4 muted yellow bags.\n" +
                       "bright white bags contain 1 shiny gold bag.\n" +
                       "muted yellow bags contain 2 shiny gold bags, 9 faded blue bags.\n" +
                       "shiny gold bags contain 1 dark olive bag, 2 vibrant plum bags.\n" +
                       "dark olive bags contain 3 faded blue bags, 4 dotted black bags.\n" +
                       "vibrant plum bags contain 5 faded blue bags, 6 dotted black bags.\n" +
                       "faded blue bags contain no other bags.\n" +
                       "dotted black bags contain no other bags.";
        // string input = Utils.GetInput(2020, 7);
        
        List<string> bagsThatHaveGold = ["shiny gold"];
        List<string> bagsToSearchFor = ["shiny gold"];
        List<string> bagsSearchedFor = [];
        Dictionary<string, List<Dictionary<string, int>>> allBags = [];

        foreach (string line in input.Split("\n"))
        {
            List<string> newLine = new List<string>(line.Split());
            string bagName = $"{newLine[0]} {newLine[1]}";
            newLine.RemoveRange(0, 4);
            string combinedLine = string.Join(" ", newLine);
            foreach (string baggy in combinedLine.Split(","))
            {
                if (baggy == "no other bags.")
                {
                    allBags[bagName] = [];
                    continue;
                }
                string newbaggy = baggy;
                if (newbaggy.Contains('.')) newbaggy = newbaggy.TrimEnd('.'); // end of lines contain a period
                if (newbaggy[0] == ' ')  newbaggy = newbaggy.Substring(1);
                int numBags = newbaggy[0] - '0';
                newbaggy = newbaggy.Substring(2);
                if (newbaggy[^1] == 's') newbaggy = newbaggy.Substring(0, newbaggy.Length - 5);
                else newbaggy = newbaggy.Substring(0, newbaggy.Length - 4);
                Console.WriteLine(newbaggy);
                Console.WriteLine(numBags);
                
                Dictionary<string, int> tempDict = [];
                tempDict.Add(newbaggy, 1);
                if (!allBags.Keys.Contains(bagName)) allBags[bagName] = new List<Dictionary<string, int>>();
                allBags[bagName].Add(tempDict);
            }
        }
        
        // allBags = Dictionary<string, List<Dictionary<string, int>>>
        foreach (var se in allBags.Keys)
        {
            // List<Dictionary<string, int>>
            Console.Write($"{se}: ");
            allBags[se].ForEach(Console.Write);
            Console.WriteLine();
        }
    }
}