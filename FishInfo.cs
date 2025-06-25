using System.Collections.Generic;

namespace FishingPerfectionHelper
{
    public class FishInfo
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public List<string> Seasons { get; set; }
        public List<int> Times { get; set; } // 600 = 6:00 AM, 1700 = 5:00 PM, etc.
        public string Weather { get; set; } // "sunny", "rainy" or "both"
        public string Location { get; set; } //todo; ocean/river/pond/island
        public int MinFishingLevel { get; set; }
        public Boolean? HasBeenCaught { get; set; }

        public FishInfo() { }

        //public FishInfo(int id, string key, string name, string seasons, string times, string weather, string location, Boolean hasBeenCaught)
        //{
        //    Id = id;
        //    Key = key;
        //    Name = name;
        //    Seasons = seasons;
        //    Times = times;
        //    Weather = weather;
        //    Location = location;
        //    HasBeenCaught = hasBeenCaught;
        //}

        public bool IsCatchable(string currentSeason, int currentTime, bool isRaining)
        {
            //weather for fish can be "sunny" "rainy" or "both"
            return Seasons.Contains(currentSeason.ToLower())
                && Times.Contains(currentTime)
                && ((isRaining == true && Weather != "sunny") || (isRaining == false && Weather != "rainy"));

            //todo: filter out legend II challenge (unless quest active)
            //      filter out night market fish unless day/time of market
            //      check fishign level
            //      check if witch swamp available, check if ginger island available
        }
    }
}