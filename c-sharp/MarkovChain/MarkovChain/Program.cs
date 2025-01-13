using System.Text.Json;
using System.Web;

// ReSharper disable UnusedMember.Local

namespace MarkovChain;

class Program
{
    static void Main(string[] args)
    {
        // Dictionary<string, AdjustedWord> adjustedWords = CreateAdjustedWords();
        // SaveAdjustedWords(adjustedWords);
        Dictionary<string, AdjustedWord> adjustedWords = ReadAdjustedWords();
        
        long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        for (int i = 0; i < 200; i++)
        {
            Console.WriteLine(MakeSentence(adjustedWords));
        }
        Console.WriteLine(DateTimeOffset.Now.ToUnixTimeMilliseconds() - milliseconds);
    }

    static string MakeSentence(Dictionary<string, AdjustedWord> adjustedWords)
    {
        Random rand = new Random();

        string newWord = adjustedWords.Keys.ElementAt(rand.Next(0, adjustedWords.Count));
        // Console.WriteLine($"First word: {newWord}");
        string sentence = $"{newWord} ";
        int sentenceCap = 20;
        int words = 0;
        
        while (newWord != "\n" && words < sentenceCap)
        {
            
            newWord = adjustedWords[newWord].NextWord;
            sentence += newWord + " ";
            words++;
        }

        return sentence.Trim();
    }

    static Dictionary<string, AdjustedWord> ReadAdjustedWords()
    {
        Dictionary<string, AdjustedWord> adjustedWords = new Dictionary<string, AdjustedWord>();

        StreamReader sr = new StreamReader("pickle.json");
        string json = sr.ReadToEnd();

        Dictionary<string, string> items = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                                           ?? throw new NullReferenceException();
        foreach (string word in items.Keys)
        {
            adjustedWords[word] = new AdjustedWord(word, items[word]);
        }
        
        return adjustedWords;
    }

    static Dictionary<string, AdjustedWord> CreateAdjustedWords()
    {
        StreamReader sr = new StreamReader("./text.txt");
        List<string> lines = [];
        string? line = sr.ReadLine();
        
        while (line != null)
        {
            lines.Add(line);
            line = sr.ReadLine();
        }
        
        // lines.ForEach(Console.WriteLine);

        Dictionary<string, TextWord> words = [];
        string? lastWord = null;
        foreach (string meow in lines)
        {
            foreach (string word in meow.Split())
            {
                if (word.Trim() == "") continue;
                
                words.TryAdd(word, new TextWord(word));
                if (lastWord == null)
                {
                    lastWord = word;
                    continue;
                }
                if (!words[lastWord].Responses.TryAdd(word, 1))
                    words[lastWord].Responses[word]++;
                lastWord = word;
            }
            
            if (lastWord == null) continue;
            if (!words[lastWord].Responses.TryAdd("\n", 1))
                words[lastWord].Responses["\n"]++;
        }

        Dictionary<string, AdjustedWord> adjustedWords = new Dictionary<string, AdjustedWord>();
        
        foreach (TextWord textWord in words.Values)
        {
            string? mostLikelyWord = null;
            int biggestWordCount = 0;
            foreach (string word in textWord.Responses.Keys)
            {
                if (textWord.Responses[word] > biggestWordCount)
                {
                    mostLikelyWord = word;
                    biggestWordCount = textWord.Responses[word];
                }
            }
            
            if (mostLikelyWord == null) continue;
            adjustedWords.Add(textWord.Word, new AdjustedWord(textWord.Word, mostLikelyWord));
        }

        return adjustedWords;
    }

    static void SaveAdjustedWords(Dictionary<string, AdjustedWord> adjustedWords)
    {
        string saveFile = "./pickle.json";
        
        if (File.Exists(saveFile)) File.Delete(saveFile);
        
        StreamWriter sw = new StreamWriter(saveFile);
        sw.WriteLine("{");
        
        for (int i = 0; i < adjustedWords.Count; i++)
        {
            string word = adjustedWords.ElementAt(i).Key;
            // Console.WriteLine($"\"{HttpUtility.JavaScriptStringEncode(word)}\":\"{HttpUtility.JavaScriptStringEncode(adjustedWords[word].NextWord)}\"");
            if (i == adjustedWords.Count - 1) // last key
            {
                sw.Write($"\"{HttpUtility.JavaScriptStringEncode(word)}\":\"{HttpUtility.JavaScriptStringEncode(adjustedWords[word].NextWord)}\"}}");
                continue;
            }
            sw.Write($"\"{HttpUtility.JavaScriptStringEncode(word)}\":\"{HttpUtility.JavaScriptStringEncode(adjustedWords[word].NextWord)}\",");
        }
        sw.Close();
    }
}
