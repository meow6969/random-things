using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using osu_database_reader.Components.Beatmaps;
using osu_database_reader.Components.Events;
using osu_database_reader.Components.HitObjects;
using osu_database_reader.TextFiles;

namespace OsuDatabaseFileCreator;

public class Models
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class MyCoolBeatmapFile
    {
        public MyCoolBeatmapFile FromBeatmapFile(BeatmapFile beatmapFile)
        {
            MyCoolBeatmapFile beatmap = new MyCoolBeatmapFile();
            
            foreach (PropertyInfo item in beatmapFile.GetType().GetProperties())
            {
                try
                {
                    // Console.WriteLine(item.Name);
                    PropertyInfo? targetProperty = typeof(MyCoolBeatmapFile).GetProperty(item.Name);
                    if (targetProperty == null) continue;
                    // object? itemValue = item.GetValue(beatmapFile);
                    // if (itemValue == null) continue;
                    targetProperty.SetValue(beatmap, item.GetValue(beatmapFile));
                }
                catch (Exception e)
                {
                    if (e.InnerException is not ArgumentNullException && e.InnerException is not NullReferenceException &&
                        e is not TargetInvocationException)
                    {
                        throw;
                    }
                }
            }
            foreach (FieldInfo field in beatmapFile.GetType().GetFields())
            {
                try
                {
                    // Console.WriteLine(item.Name);
                    PropertyInfo? targetField = typeof(MyCoolBeatmapFile).GetProperty(field.Name);
                    if (targetField == null) continue;
                    targetField.SetValue(beatmap, field.GetValue(beatmapFile));
                }
                catch (Exception e)
                {
                    if (e.InnerException is not ArgumentNullException && e.InnerException is not FormatException)
                    {
                        throw;
                    }
                }
            }

            return beatmap;
        }
        
        // Section General
        public string? AudioFilename { get; set; } = "";
        public int AudioLeadIn { get; set; } = 0;
        public int PreviewTime { get; set; } = -1;
        public bool Countdown { get; set; } = false;
        public string? SampleSet { get; set; } = "Normal";
        public int SampleVolume { get; set; } = 100;
        public float StackLeniency { get; set; } = 0.7f;
        public int Mode { get; set; } = 0;
        public bool LetterboxInBreaks { get; set; } = false;
        public bool SpecialStyle { get; set; } = false;
        public bool WidescreenStoryboard { get; set; } = false;
        public bool EpilepsyWarning { get; set; } = false;

	    // Section Editor
        public int[]? Bookmarks { get; set; } = [];
        public double DistanceSpacing { get; set; } = 1;
        public int BeatDivisor { get; set; } = 1;
        public int GridSize { get; set; } = 1;
        public double TimelineZoom { get; set; } = 1;

	    // Section Metadata
        public string? Title { get; set; } = "";
        public string? TitleUnicode { get; set; } = "";
        public string? Artist { get; set; } = "";
        public string? ArtistUnicode { get; set; } = "";
        public string? Creator { get; set; } = "";
        public string? Version { get; set; } = "";
        public string? Source { get; set; } = "";
        public string? Tags { get; set; } = "";
        public int? BeatmapID { get; set; }
        public int? BeatmapSetID { get; set; }

	    // Section Difficulty
        public float HPDrainRate { get; set; } = 1;
        public float CircleSize { get; set; } = 1;
        public float OverallDifficulty { get; set; } = 1;
        public float ApproachRate { get; set; } = 1;
        public double SliderMultiplier { get; set; } = 1;
        public double SliderTickRate { get; set; } = 1;

        public List<EventBase> Events { get; set; } = [];
        public List<TimingPoint> TimingPoints { get; set; } = [];
        public List<HitObject> HitObjects { get; set; } = [];
    }
    
    public class LogToConsole
    {
        // lol dis code so bad
        public required int Current { get; set; }
        public required int Goal { get; set; }
        private List<long> _itemsPerSecondBuffer = [];
        private int _timeBuffer = 10;

        public void Print(string? text=null, bool addToBuffer=false)
        {
            float percent = Current / (float)Goal * 100f;
            // Console.WriteLine(percent);
            string progressBar = "";
            progressBar = progressBar.PadLeft((int)Math.Floor(percent / 2), '#');
            progressBar += new string(' ', 50 - (int)Math.Floor(percent / 2));
            
            if (addToBuffer) _itemsPerSecondBuffer.Add(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            double itemsPerSecond = ItemsPerSecond();

            Clear();
            if (text != null) Console.WriteLine(text[..Math.Clamp(Console.WindowWidth, 0, text.Length)]);
            
            Console.Write($"{(int)percent}% - [{progressBar}] ({Current} / {Goal}) {itemsPerSecond:0.0}/s " +
                          $"ETA={GetEtaString(itemsPerSecond)}");
        }
        
        private string GetEtaString(double itemsPerSecond)
        {
            int eta = GetEtaInSeconds(itemsPerSecond);
            if (eta == -1) return "infinite";
            return TimeSpan.FromSeconds(eta).ToString("hh\\:mm\\:ss").Insert(2, "hr").Insert(7, "m").Insert(11, "s");
        }

        private int GetEtaInSeconds(double itemsPerSecond)
        {
            if (itemsPerSecond == 0) return -1;
            return (int)((Goal - Current) / itemsPerSecond);
        }

        private double ItemsPerSecond()
        {
            List<long> newItemsPerSecondBuffer = [];
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
            // Console.WriteLine($"currentTime: {currentTime}");
            
            foreach (long time in _itemsPerSecondBuffer)
            {
                // Console.WriteLine($"time:        {time + 1000L}");
                if (time + _timeBuffer * 1000L > currentTime)
                {
                    newItemsPerSecondBuffer.Add(time);
                }
            }

            _itemsPerSecondBuffer = newItemsPerSecondBuffer;
            return (double)_itemsPerSecondBuffer.Count / _timeBuffer;
        }
        
        private void Clear(int cursorOffset=0)
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop - cursorOffset);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }
    }
}