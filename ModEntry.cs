using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Specialized;

namespace FishingPerfectionHelper
{
    public class ModEntry : Mod
    {
        private string currentSeason = "";
        private string currentWeather = "";
        private List<FishInfo> unCaughtFish = new();
        private List<FishInfo> fishDatabase = new();

        public override void Entry(IModHelper helper)
        {
            populateFishDatabase();
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.TimeChanged += OnTimeChanged;
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            UpdateCaughtFish();

            //Monitor.Log("=== you have caught ===", LogLevel.Info);
            //foreach (var fish in fishDatabase)
            //{
            //    if (fish.HasBeenCaught == true)
            //    {
            //        Monitor.Log(fish.Name, LogLevel.Info);
            //        Monitor.Log(fish.Seasons, LogLevel.Info);
            //    }
            //}
            //Monitor.Log("=== you still need ===", LogLevel.Info);
            //foreach (var fish in unCaughtFish)
            //{
            //    Monitor.Log(fish.Name, LogLevel.Info);
            //    Monitor.Log(fish.Seasons,LogLevel.Info);
            //}
        }

        private void OnTimeChanged(object? sender, TimeChangedEventArgs e)
        {
            int currentTime = Game1.timeOfDay;
            var caught = Game1.stats.FishCaught;

            foreach (var fish in unCaughtFish)
            {
                if (fish.IsCatchable(Game1.currentSeason, currentTime, Game1.isRaining))
                {
                    Monitor.Log($"[Fishing Helper] You can catch: {fish.Name}", LogLevel.Info);
                }
            }

            Monitor.Log($"[Fishing Helper] Time: {e.NewTime}", LogLevel.Info);
        }

        private static List<int> GetTimeRange(int start, int end)
        {
            //todo make sure end time is exclusive
            List<int> times = new();
            for (int t = start; t <= end; t += 10)
                times.Add(t);
            return times;
        }

        private void populateFishDatabase()
        {
            //get the fish dictionary, keys of their ids are strings
            var fishDic = Game1.content.Load<Dictionary<string, string>>("Data/Fish");

            //iterate the keys in the dictionary to get their details
            foreach (var fishIdStr in fishDic.Keys)
            {
                fishDic.TryGetValue(fishIdStr, out string rawFish);
                /* example output of rawFish:
                 * Pufferfish/80/floater/1/36/1200 1600/summer/sunny/690 .4 685 .1/4/.3/.5/0/true
                 * access from .Split as an array, with the indices as follows:
                 * https://stardewvalleywiki.com/Modding:Fish_data
                 * 0 - name
                 * 1,2,3,4 - difficulty, movement style, minimum size, maximum size
                 * 5 - begin and end times for availability (end is exclusive, start is inclusive)
                 * 6 - seasons (space separated string)
                 * 7 - weather (will be "sunny" "rainy" or "both")
                 * 8 - purportedly this is the location
                 * 9, 10, 11 - related to the chance to catch it, including cast length
                 * 12 - minimum fishing level required
                 * 13 - can be caught as tutorial fish
                 */

                //be sure it's not a trap fish before attempting to access all these indices
                //also check that fishId is an int and not "SeaJelly" etc
                if (rawFish.Split('/')[1] != "trap" && int.TryParse(fishIdStr, out int fishId)) { 
                    FishInfo currentFish = new();
                    currentFish.Id = Int32.Parse(fishIdStr);
                    //the key used to reference the fish in .fishCaught is (0)XXX where XXX is the fishid
                    currentFish.Key = $"(O){fishIdStr}";
                    currentFish.Name = rawFish.Split('/')[0];
                    string seasonsString = rawFish.Split('/')[6];
                    foreach (var season in seasonsString.Split(' ')) {
                        currentFish.Seasons.Add(season);
                    }
                    string TimeString = rawFish.Split('/')[5];
                    int startTime = Int32.Parse(TimeString.Split(' ')[0]);
                    int endTime = Int32.Parse(TimeString.Split(' ')[1]);
                    currentFish.Times = GetTimeRange(startTime, endTime);
                    currentFish.Weather = rawFish.Split('/')[4];
                    currentFish.Location = "todo";
                    currentFish.MinFishingLevel = Int32.Parse(rawFish.Split('/')[12]);
                    currentFish.HasBeenCaught = null;

                    fishDatabase.Add(currentFish);
                }
            }
        }

        private void UpdateCaughtFish()
        {
            unCaughtFish.Clear();
            foreach (var fish in fishDatabase.Where(f => f.HasBeenCaught != true))
            {
                if (Game1.player.fishCaught.TryGetValue(fish.Key, out var catchData) && catchData.Length > 0)
                {
                    int numberCaught = catchData[0];
                    if (numberCaught > 0)
                    {
                        fish.HasBeenCaught = true;
                        continue;
                    }
                }

                // If not caught or count is 0
                unCaughtFish.Add(fish);
            }
        }//end UpdateCaughtFish
    }
}