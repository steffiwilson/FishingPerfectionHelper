using StardewValley;
using System.Collections.Generic;

namespace FishingPerfectionHelper
{
    public class Fish
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public List<string> Seasons { get; set; } = new(); //lowercase
        public List<int> Times { get; set; } = new(); // 600 = 6:00 AM, 1700 = 5:00 PM, etc.
        public string Weather { get; set; } // "sunny", "rainy" or "both"
        public string Locations { get; set; } 
        public int MinFishingLevel { get; set; }
        public Boolean canBeTutorialFish { get; set; }
        public Boolean? HasBeenCaught { get; set; }
        public int Difficulty { get; set; }

        public Fish() { }

    }
}