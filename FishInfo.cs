using StardewValley;
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

        public bool IsCatchable(bool hasCaughtTutorialFish, bool isNightMarket, bool communityCenterComplete,
            bool busUnlocked)
        {
            string currentSeason = Game1.currentSeason;
            int currentTime = Game1.timeOfDay;
            bool isRaining = Game1.isRaining;
            int fishingLevel = Game1.player.FishingLevel;
            bool legendaryIIActive = Game1.player.team.SpecialOrderRuleActive("LEGENDARY_FAMILY");
            bool hasRustyKey = Game1.player.mailReceived.Contains("HasRustyKey");
            bool hasSkullKey = Game1.player.mailReceived.Contains("HasSkullKey");

            return (Seasons.Contains(currentSeason.ToLower())
                && Times.Contains(currentTime)
                && ((isRaining == true && Weather != "sunny") || (isRaining == false && Weather != "rainy"))
                && (hasCaughtTutorialFish || canBeTutorialFish)
                && (MinFishingLevel <= fishingLevel)
                && (Locations != "Submarine" || isNightMarket)
                && ((Locations != "Witch's Swamp" && Locations != "Island" && Locations != "Cove") || communityCenterComplete)
                && (!Locations.Contains("Sewer") || hasRustyKey)
                && (Locations != "Mines" || hasSkullKey) //mine fish definitely good after bottom reached
                && (Locations != "Desert" || busUnlocked))
                || (Locations.Contains("Legendary II") && legendaryIIActive) //show legendary II fish if quest active
            ;
            //     todo: test legendary fish seasons?

        }
    }
}