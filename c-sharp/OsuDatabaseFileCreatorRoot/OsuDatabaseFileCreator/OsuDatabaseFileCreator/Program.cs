using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CSharpOsuApi;
using CSharpOsuApi.JsonUtils;
using CSharpOsuApi.Models;
using CSharpOsuApi.Models.BeatmapModels;
using CSharpOsuApi.Models.OsuEnums;
using CSharpOsuApi.Models.UserModels;
using osu_database_reader.BinaryFiles;
using osu_database_reader.Components.Beatmaps;
using osu_database_reader.Components.Events;
using osu_database_reader.Components.HitObjects;
using osu_database_reader.TextFiles;
using osu.Shared;
using osu.Shared.Serialization;
using static OsuDatabaseFileCreator.Models;

namespace OsuDatabaseFileCreator;

static class Program
{
    static void Main(string[] args)
    {
        (int osuClientId, string osuClientSecret, string osuSongPath) = GetTheArgs(args);
        CreateOsuDatabase(osuClientId, osuClientSecret, osuSongPath);
    }

    private static (int, string, string) GetTheArgs(string[] args)
    {
        int osuClientId = -1;
        string? osuClientSecret = null;
        string? osuSongPath = null;
        string[] clientIdFlags = ["-clientid", "-client_id", "-id"];
        string[] clientSecretFlags = ["-clientsecret", "-client_secret", "-secret"];
        string[] songPathFlags = ["-songpath", "-song_path", "-songs"];
        string[] allFlags = clientIdFlags.Concat(clientSecretFlags).Concat(songPathFlags).ToArray();
        string theOption = "";
        
        foreach (string arg in args)
        {
            // if (theOption.Length != 0 && allFlags.Contains(theOption)) throw new ArgumentException();
            // if (theOption.Length == 0 && !allFlags.Contains(theOption)) throw new ArgumentException();
            if (allFlags.Contains(theOption) && allFlags.Contains(arg)) throw new ArgumentException();
            
            if (clientIdFlags.Contains(theOption))
            {
                theOption = "";
                if (!int.TryParse(arg, out osuClientId)) throw new ArgumentException();
                continue;
            }

            if (clientSecretFlags.Contains(theOption))
            {
                theOption = "";
                osuClientSecret = arg;
                continue;
            }
            
            if (songPathFlags.Contains(theOption))
            {
                theOption = "";
                osuSongPath = arg;
                continue;
            }

            theOption = arg;
        }

        if (osuClientId == -1 || osuClientSecret is null || osuSongPath is null)
        {
            throw new ArgumentException();
        }

        if (!Directory.Exists(osuSongPath)) throw new DirectoryNotFoundException();

        return (osuClientId, osuClientSecret, osuSongPath);
    }
    
    public static void CreateOsuDatabase(int osuClientId, string osuClientSecret, string osuSongPath)
    {
        Scopes scopes = new Scopes
        {
            Identify = true,
            Public = true
        };
        
        OsuBackend osuBackend = new OsuBackend(
            osuClientId, 
            osuClientSecret, 
            scopes
        );
        
        List<BeatmapEntry> beatmapEntries = new List<BeatmapEntry>();

        int folderCount = 0;
        Console.WriteLine(osuSongPath);
        string[] directories = Directory.GetDirectories(osuSongPath);
        LogToConsole printy = new LogToConsole
        {
            Current = 0,
            Goal = directories.Length
        };
        int osuBeatmapFiles = 0;

        foreach (string folderPath in directories)
        {
            printy.Current++;
            string folderName = new DirectoryInfo(folderPath).Name;
            bool success = int.TryParse(folderName.Split()[0], out int beatmapSetId);
            if (!success) beatmapSetId = -1;
            
            // if (beatmapSetId != -1) continue;
            
            BeatmapsetExtended? beatmapsetExtended;
            try
            {
                beatmapsetExtended = osuBackend.GetBeatmapset(beatmapSetId);
            }
            catch 
            {
                beatmapsetExtended = null;
            }

            string[] osuFiles = Directory.GetFiles(folderPath, "*.osu");

            folderCount++; // idk if we are supposed to increase this number when  theres no osu files in it??
            if (osuFiles.Length == 0)
            {
                Console.WriteLine($"folder has no beatmaps: {folderPath}");
                continue;
            }

            foreach (string fileName in osuFiles)
            {
                osuBeatmapFiles++;
                string filePath = Path.Combine(folderPath, fileName);
                
                // i like that the user defined class names are pink
                BeatmapFile beatmapFile = BeatmapFile.Read(File.OpenRead(filePath));
                
                BeatmapExtended? beatmapExtended = null;
                MyCoolBeatmapFile coolBeatmapFile = new MyCoolBeatmapFile().FromBeatmapFile(beatmapFile);
                if (coolBeatmapFile.BeatmapSetID == null)
                {
                    coolBeatmapFile.BeatmapSetID = beatmapsetExtended?.Id ?? beatmapSetId;
                }
                
                if (coolBeatmapFile.BeatmapID is null or -1)
                {
                    continue;
                } 

                if (beatmapsetExtended is { Beatmaps: not null })
                {
                    foreach (BeatmapExtended beatmap in beatmapsetExtended.Beatmaps)
                    {
                        if (coolBeatmapFile.BeatmapID == null)
                        {
                            if (beatmap.Version == coolBeatmapFile.Version)
                            {
                                beatmapExtended = beatmap;
                                coolBeatmapFile.BeatmapID = beatmapExtended.Id;
                                break;
                            }
                        }
                        else if (beatmap.Id == coolBeatmapFile.BeatmapID)
                        {
                            beatmapExtended = beatmap;
                            break;
                        }
                    }
                }
                
                BeatmapEntry beatmapEntry;
                try
                {
                    beatmapEntry = GetBeatmapEntryFromFile(
                        filePath, beatmapsetExtended, beatmapExtended, coolBeatmapFile);
                }
                catch (Exception e)
                {
                    if (e is FormatException)
                    {
                        Console.WriteLine($"Error in getting beatmap entry for beatmapset: {folderPath}, " +
                                          $"beatmap: {filePath}, " +
                                          $"was unable to parse beatmap");
                        Console.WriteLine(e);
                        Console.WriteLine("the eror ^^^^^\n");
                        continue;
                    }

                    throw;
                }
                
                beatmapEntries.Add(beatmapEntry);
            }
            
            printy.Print($"successfully retrieved beatmapEntries for beatmapset: {folderPath}", true);
            // printy.Print(null, true);
        }
        
        // OsuDb db = OsuDb.Read("osuclient.db");
        User me = osuBackend.GetMe();
        OsuDb db = new OsuDb();
        db.AccountName = me.Username;
        db.OsuVersion = 20250108;
        db.AccountRank = PlayerRank.Default;
        db.AccountUnlocked = true;
        db.AccountUnlockDate = DateTime.ParseExact(
            "01/01/0001 08:00:00", "MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        
        db.Beatmaps = beatmapEntries;
        db.FolderCount = folderCount;
        Console.WriteLine();
        Console.WriteLine(db.Beatmaps.Count);
        Console.WriteLine(db.FolderCount);
        Console.WriteLine($".osu files found: {osuBeatmapFiles}");
        
        string dbFilename = "osu!.db";
        if (File.Exists(dbFilename)) File.Delete(dbFilename);
        using (var stream = File.OpenWrite(dbFilename)) db.WriteToStream(new SerializationWriter(stream));
        Console.WriteLine($"wrote osu database file to {dbFilename}");
    }
    
    static BeatmapEntry GetBeatmapEntryFromFile(string filePath, 
        BeatmapsetExtended? beatmapsetExtended, BeatmapExtended? beatmapExtended, MyCoolBeatmapFile beatmapFile)
    {
        // string checksum = MD5.Create().ComputeHash(File.OpenRead(filePath)).ToString() 
        //                   ?? throw new NullReferenceException();

        Dictionary<Mods, double> diffStarRatingStandard = [];
        Dictionary<Mods, double> diffStarRatingTaiko = [];
        Dictionary<Mods, double> diffStarRatingCtB = [];
        Dictionary<Mods, double> diffStarRatingMania = [];
        SubmissionStatus subStatus;
        ushort countCircles;
        ushort countSliders;
        ushort countSpinners;
        
        if (beatmapExtended != null)
        {
            if (beatmapExtended.Mode == RulesetEnum.Osu)
            {
                diffStarRatingStandard[Mods.None] = beatmapExtended.DifficultyRating;
            }
            else if (beatmapExtended.Mode == RulesetEnum.Taiko)
            {
                diffStarRatingTaiko[Mods.None] = beatmapExtended.DifficultyRating;
            }
            else if (beatmapExtended.Mode == RulesetEnum.Fruits)
            {
                diffStarRatingCtB[Mods.None] = beatmapExtended.DifficultyRating;
            }
            else if (beatmapExtended.Mode == RulesetEnum.Mania)
            {
                diffStarRatingMania[Mods.None] = beatmapExtended.DifficultyRating;
            }

            subStatus = RankedEnumToSubmissionStatus(beatmapExtended.Ranked);
            countCircles = (ushort)beatmapExtended.CountCircles;
            countSliders = (ushort)beatmapExtended.CountSliders;
            countSpinners = (ushort)beatmapExtended.CountSpinners;
        }
        else
        {
            subStatus = SubmissionStatus.Unknown;
            (countCircles, countSliders, countSpinners) = GetHitObjectCountsFromBeatmapFile(beatmapFile);
        }
        
        
        short beatmapOnlineOffset;

        if (beatmapsetExtended != null)
        {
            beatmapOnlineOffset = (short)beatmapsetExtended.Offset;
        }
        else
        {
            beatmapOnlineOffset = 0;
        }
        
        string checksum;

        if (beatmapExtended is { Checksum: not null }) checksum = beatmapExtended.Checksum;
        else
        {
            checksum = MD5.Create().ComputeHash(File.OpenRead(filePath)).ToHex(false)
                       ?? throw new InvalidOperationException($"Unable to get checksum for beatmap: {filePath}");
            // checksum = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(
        }
        
        int threadId = 0;
        // i thought this was referring to thread url but apparently not?? its suposed to be 0
        // if (beatmapsetExtended.LegacyThreadUrl != null)
        // {
        //     // Console.WriteLine(beatmapsetExtended.LegacyThreadUrl);
        //     int.TryParse(beatmapsetExtended.LegacyThreadUrl.Split("/").Last(), out threadId);
        // }

        DirectoryInfo folderPath = new FileInfo(filePath).Directory ?? throw new InvalidOperationException();
        string folderName = folderPath.Name;

        HitObject? lastHitObject;
        int totalTime;
        int drainTime;
        
        try
        {
            (HitObject _, lastHitObject) = GetFirstAndLastHitObject(beatmapFile.HitObjects.ToArray());
            totalTime = lastHitObject.Time;
            if (lastHitObject is HitObjectHold hold)
            {
                totalTime = hold.EndTime;
            }
            else if (lastHitObject is HitObjectSpinner spinner)
            {
                totalTime = spinner.EndTime;
            }
            else if (lastHitObject is HitObjectSlider slider)
            {
                TimingPoint? lastInheritedTimingPoint = null;
                TimingPoint? lastTimingPoint = null;
            
                foreach (TimingPoint timingPoint in beatmapFile.TimingPoints)
                {
                    if (timingPoint.TimingChange)
                    {
                        lastInheritedTimingPoint = timingPoint;
                    }

                    lastTimingPoint = timingPoint;
                }

                double sliderVelocity = beatmapFile.SliderMultiplier;
                if (lastInheritedTimingPoint != null)
                {
                    sliderVelocity *= lastInheritedTimingPoint.MsPerQuarter;
                }
                if (lastTimingPoint == null) throw new NullReferenceException();
                sliderVelocity *= 100;
                totalTime += (int)(slider.Length / sliderVelocity * lastTimingPoint.MsPerQuarter);
            }
            
            drainTime = CalculateDrainTime(beatmapFile);
            // Console.WriteLine($"totalTime: {totalTime}\n\n"); millisecond
            // Console.WriteLine($"drainTime: {drainTime}\n\n"); second
        }
        catch (Exception e)
        {
            if (e is NullReferenceException)
            {
                if (!File.Exists("/bin/ffprobe"))
                {
                    throw new Exception($"could not get Length for beatmap {filePath}\n" +
                                        $"could not find ffprobe exe at /bin/ffprobe");
                }

                if (beatmapFile.AudioFilename == null)
                {
                    throw new Exception($"could not get Length for beatmap {filePath}\n" +
                                        $"beatmapFile.AudioFilename was null");
                }
                
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = "/bin/ffprobe";
                p.StartInfo.Arguments = $"-i \"{Path.Combine(folderPath.FullName, beatmapFile.AudioFilename)}\" " +
                                        $"-show_entries format=duration -v quiet -of csv=\"p=0\"";
                p.Start();
                totalTime = (int)(double.Parse(p.StandardOutput.ReadToEnd()) * 1000);
                drainTime = totalTime / 1000;
            }
            else
            {
                throw;
            }
        }

        
        List<TimingPoint> timingPointSingular = [beatmapFile.TimingPoints[0], beatmapFile.TimingPoints[^1]];
        // idk if the distinction between artist and artistunicode matters?? well anyway
        string artist = GetGenericThing<string>(filePath, [beatmapFile, beatmapExtended, beatmapsetExtended], 
            ["Artist", "ArtistUnicode"]);
        // the first element in the string array becomes the primary attribute it searches for
        string artistUnicode = GetGenericThing<string>(filePath, [beatmapFile, beatmapExtended, beatmapsetExtended], 
            ["ArtistUnicode", "Artist"]);
        string title = GetGenericThing<string>(filePath, [beatmapFile, beatmapExtended, beatmapsetExtended], 
            ["Title", "TitleUnicode"]);
        string titleUnicode = GetGenericThing<string>(filePath, [beatmapFile, beatmapExtended, beatmapsetExtended], 
            ["TitleUnicode", "Title"]);
        string creator = GetGenericThing<string>(filePath, [beatmapFile, beatmapExtended, beatmapsetExtended], 
            ["Creator"]);
        string version = GetGenericThing<string>(filePath, [beatmapFile, beatmapExtended, beatmapsetExtended], 
            ["Version"]);
        string audioFilename = GetGenericThing<string>(filePath, [beatmapFile, beatmapExtended, beatmapsetExtended], 
            ["AudioFilename"]);
        float approachRate = GetGenericThing<float>(filePath, [beatmapFile, beatmapExtended, beatmapsetExtended], 
            ["ApproachRate"]);
        float circleSize = GetGenericThing<float>(filePath, [beatmapFile, beatmapExtended, beatmapsetExtended], 
            ["CircleSize"]);
        float hpDrainRate = GetGenericThing<float>(filePath, [beatmapFile, beatmapExtended, beatmapsetExtended], 
            ["HPDrainRate"]);
        float overallDifficulty = GetGenericThing<float>(filePath, [beatmapFile, beatmapExtended, beatmapsetExtended], 
            ["OverallDifficulty"]);
        double sliderMultiplier = GetGenericThing<double>(filePath, [beatmapFile, beatmapExtended, beatmapsetExtended], 
            ["SliderMultiplier"]);
        int previewTime = GetGenericThing<int>(filePath, [beatmapFile, beatmapExtended, beatmapsetExtended], 
            ["PreviewTime"], -1);
        int beatmapId = GetGenericThing<int>(filePath, [beatmapFile, beatmapExtended, beatmapsetExtended], 
            ["BeatmapID"], 0);
        int beatmapsetId = GetGenericThing<int>(filePath, [beatmapFile, beatmapExtended, beatmapsetExtended], 
            ["BeatmapsetID"], -1);
        float stackLeniency = GetGenericThing<float>(filePath, [beatmapFile, beatmapExtended, beatmapsetExtended], 
            ["StackLeniency"]);
        string source = GetGenericThing<string>(filePath, [beatmapFile, beatmapExtended, beatmapsetExtended], 
            ["Source"], "");
        string tags = GetGenericThing<string>(filePath, [beatmapFile, beatmapExtended, beatmapsetExtended], 
            ["Tags"], "");
        
        BeatmapEntry beatmapEntry = new BeatmapEntry {
            Artist = artist,
            ArtistUnicode = artistUnicode,
            Title = title,
            TitleUnicode = titleUnicode,
            Creator = creator,
            Version = version,
            AudioFileName = audioFilename,
            BeatmapChecksum = checksum,
            BeatmapFileName = Path.GetFileName(filePath),
            RankedStatus = subStatus,
            CountHitCircles = countCircles,
            CountSliders = countSliders,
            CountSpinners = countSpinners,
            LastModifiedTime = DateTime.Now,
            ApproachRate = approachRate,
            CircleSize = circleSize,
            HPDrainRate = hpDrainRate,
            OveralDifficulty = overallDifficulty,
            SliderVelocity = sliderMultiplier,
            DiffStarRatingStandard = diffStarRatingStandard,
            DiffStarRatingTaiko = diffStarRatingTaiko,
            DiffStarRatingCtB = diffStarRatingCtB,
            DiffStarRatingMania = diffStarRatingMania,
            DrainTimeSeconds = drainTime,
            TotalTime = totalTime,
            AudioPreviewTime = previewTime,
            TimingPoints = timingPointSingular,
            BeatmapId = beatmapId,
            BeatmapSetId = beatmapsetId,
            ThreadId = threadId,
            GradeStandard = Ranking.N,
            GradeTaiko = Ranking.N,
            GradeCtB = Ranking.N,
            GradeMania = Ranking.N,
            OffsetLocal = 0,
            StackLeniency = stackLeniency,
            GameMode = (GameMode)Enum.ToObject(typeof(GameMode), beatmapFile.Mode),
            SongSource = source,
            SongTags = tags,
            OffsetOnline = beatmapOnlineOffset,
            TitleFont = "",
            Unplayed = true,
            LastPlayed = DateTime.ParseExact("01/01/0001 08:00:00", 
                "MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
            IsOsz2 = false,
            FolderName = folderName,
            LastCheckAgainstOsuRepo = DateTime.Now,
            IgnoreBeatmapSounds = false,
            IgnoreBeatmapSkin = false,
            DisableStoryBoard = false,
            DisableVideo = false,
            VisualOverride = false,
            OldUnknown1 = 0,
            LastEditTime = 0,
            ManiaScrollSpeed = 0,
            _version = 20250108
        };
        
        return beatmapEntry;
    }
    
    private static T GetGenericThing<T>(string filePath, object?[] theObjects, string[] objectAttributesToSearch, 
        object? defaultValue=null)
    {
        string primaryAttributeToSearch = objectAttributesToSearch[0];
        bool wasInitialized = false;
        T? attributeBuffer = default;
        
        foreach (object? obj in theObjects)
        {
            if (obj is null) continue;
            foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties())
            {
                int occurenceIndex = Array.IndexOf(objectAttributesToSearch, propertyInfo.Name);
                if (occurenceIndex == -1) continue;
                object? theValue = propertyInfo.GetValue(obj);
                if (theValue is not T)
                {
                    continue;
                }
                T theObby = (T)theValue;
                
                if (objectAttributesToSearch[occurenceIndex] == primaryAttributeToSearch)
                {
                    return theObby;
                }

                attributeBuffer = theObby;
                wasInitialized = true;
            }
        }

        if (wasInitialized)
        {
            if (attributeBuffer != null) return attributeBuffer;
        }
        else if (defaultValue != null)
        {
            if (defaultValue is T value)
            {
                return value;
            }

            throw new Exception($"default value passed is of type {defaultValue.GetType()}, " +
                                $"while return type is {typeof(T)}");
        }
        
        // we couldnt find the value
        string attributesSearched = "";
        foreach (string attribute in objectAttributesToSearch)
        {
            attributesSearched += attribute + ", ";
        }

        attributesSearched = attributesSearched[..^2]; // remove last ", "
        throw new Exception($"could not get string value from the attributes: {attributesSearched} " +
                            $"from .osu file: {filePath}");
    }

    private static SubmissionStatus RankedEnumToSubmissionStatus(RankedEnum rankedEnum)
    {
        // Console.WriteLine((int)rankedEnum);
        if (rankedEnum == RankedEnum.Wip) return SubmissionStatus.NotSubmitted;
        if (rankedEnum <= 0) return SubmissionStatus.Pending;
        return (SubmissionStatus)Enum.ToObject(typeof(SubmissionStatus), (int)rankedEnum + 3);
    }

    private static (ushort, ushort, ushort) GetHitObjectCountsFromBeatmapFile(MyCoolBeatmapFile beatmap)
    {
        ushort countCircles = 0;
        ushort countSliders = 0;
        ushort countSpinners = 0;
        foreach (HitObject hitObject in beatmap.HitObjects)
        {
            // TODO: check what mmania hold does (idk wut it does to thes values xddd)
            if (hitObject is HitObjectCircle) countCircles++;
            else if (hitObject is HitObjectSlider) countSliders++;
            else if (hitObject is HitObjectSpinner) countSpinners++;
        }

        return (countCircles, countSliders, countSpinners);
    }

    private static string ToHex(this byte[] bytes, bool upperCase)
    {
        StringBuilder result = new StringBuilder(bytes.Length*2);

        for (int i = 0; i < bytes.Length; i++)
            result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));

        return result.ToString();
    }

    private static (HitObject, HitObject) GetFirstAndLastHitObject(HitObject[] hitObjects)
    {
        HitObject? firstHitObject = hitObjects.MinBy(o => o.Time);
        HitObject? lastHitObject = hitObjects.MaxBy(o => o.Time);
        if (lastHitObject == null || firstHitObject == null)
        {
            throw new NullReferenceException();
        }
        return (firstHitObject, lastHitObject);
    }

    private static int CalculateDrainTime(MyCoolBeatmapFile beatmap)
    {
        (HitObject? firstHitObject, HitObject? lastHitObject) = GetFirstAndLastHitObject(beatmap.HitObjects.ToArray());
        int drainTime = lastHitObject.Time - firstHitObject.Time;
        
        foreach (EventBase eventBase in beatmap.Events)
        {
            if (eventBase is BreakEvent breakEvent)
            {
                drainTime -= (int)(breakEvent.EndTime - breakEvent.StartTime);
            }
        }
        
        return drainTime / 1000;
    }
}