using System.Collections.Generic;

namespace FishingPerfectionHelper
{
    public class FishInfo
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public List<string> Seasons { get; set; } = new();
        public List<int> Times { get; set; } = new(); // 600 = 6:00 AM, 1700 = 5:00 PM, etc.
        public string Weather { get; set; } // "sunny", "rainy" or "both"
        public string Locations { get; set; } 
        public int MinFishingLevel { get; set; }
        public Boolean canBeTutorialFish { get; set; }
        public Boolean? HasBeenCaught { get; set; }

        public FishInfo() { }

        public bool IsCatchable(string currentSeason, int currentTime, bool isRaining,
            bool hasCaughtTutorialFish, int fishingLevel, bool isNightMarket, bool communityCenterComplete)
        {
            //weather for fish can be "sunny" "rainy" or "both"
            return Seasons.Contains(currentSeason.ToLower())
                && Times.Contains(currentTime)
                && ((isRaining == true && Weather != "sunny") || (isRaining == false && Weather != "rainy"))
                && (hasCaughtTutorialFish || canBeTutorialFish)
                && (MinFishingLevel <= fishingLevel)
                && (Locations != "Submarine" || isNightMarket)
                && ((Locations != "Witch's Swamp" && Locations != "Island") || communityCenterComplete)
            ;

            //todo: filter out qi challenge fish (unless quest active)

        }
    }
}