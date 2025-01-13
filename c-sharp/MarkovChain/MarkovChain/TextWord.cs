namespace MarkovChain;

public class TextWord(string word)
{
    public string Word = word;
    public Dictionary<string, int> Responses = new();
}