using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Dimensions;

namespace FishingPerfectionHelper
{
    public static class FishDataLoader
    {
        public static void populateFishDatabase(List<Fish> fishDatabase, Dictionary<string, string> knownFishLocations)
        {
            //reset the List<FishInfo> fishDatabase, with which ones are already caught
            //we redo this at the start of each day
            fishDatabase.Clear();

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
                string[] rawFishArray = rawFish.Split('/');

                //check that fishId is an int and not "SeaJelly" etc
                if (int.TryParse(fishIdStr, out int fishId))
                {
                    Fish currentFish = new();
                    //the key used to reference the fish in .fishCaught is (0)XXX where XXX is the fishid
                    currentFish.Key = $"(O){fishIdStr}";
                    currentFish.Name = rawFishArray[0];
                    currentFish.Id = Int32.Parse(fishIdStr);

                    //location (hard-coded, sorry D: )
                    knownFishLocations.TryGetValue(fishIdStr, out string location);
                    if (location == null || location == "")
                        location = "Unknown";
                    currentFish.Locations = location;

                    if (rawFishArray[1] == "trap")
                    {   //crab pot fish don't have season/weather/level requirements (or indices)
                        currentFish.Seasons.Add("spring");
                        currentFish.Seasons.Add("summer");
                        currentFish.Seasons.Add("fall");
                        currentFish.Seasons.Add("winter");
                        currentFish.Times = Utilities.GetTimeRange(600, 2600);
                        currentFish.Weather = "both";
                        currentFish.MinFishingLevel = 3; //level at which crab pots unlock
                        currentFish.canBeTutorialFish = false;
                        currentFish.Difficulty = 0;
                    }
                    else
                    {
                        //seasons
                        string seasonsString = rawFishArray[6];
                        foreach (var season in seasonsString.Split(' '))
                        {
                            currentFish.Seasons.Add(season);
                        }

                        //times
                        string TimeString = rawFishArray[5];
                        string[] TimeArray = TimeString.Split(' ');
                        for (int i = 0; i < TimeArray.Length - 1; i+= 2)
                        {
                            int startTime = Int32.Parse(TimeString.Split(' ')[i]); 
                            int endTime = Int32.Parse(TimeString.Split(' ')[i + 1]);
                            currentFish.Times.AddRange(Utilities.GetTimeRange(startTime, endTime));
                        }                        

                        //weather
                        currentFish.Weather = rawFishArray[7];

                        //fishing level
                        currentFish.MinFishingLevel = Int32.Parse(rawFishArray[12]);

                        //tutorial fish
                        currentFish.canBeTutorialFish = Boolean.Parse(rawFishArray[13]);

                        currentFish.Difficulty = Int32.Parse(rawFishArray[1]);
                    }

                    currentFish.HasBeenCaught = false; //initialize it as uncaught

                    //fix bugs in the Data/Fish records
                    if (currentFish.Id == 160)
                    {
                        //angler is fall only
                        currentFish.Seasons = new List<string> { "Fall" };
                    }
                    if (currentFish.Id == 699)
                    {
                        //tiger trout is fall/winter only
                        currentFish.Seasons = new List<string> { "Fall", "Winter" };
                    }

                    fishDatabase.Add(currentFish);
                }
            }
        }

        public static List<Fish> GetCurrentlyCatchableFish(List<Fish> unCaughtFish, bool hasCaughtTutorialFish)
        {
            //check which fish that haven't been caught yet are catchable under current game state
            //we do this every time the keybind for ViewAvailableFish is pressed

            List<Fish> catchableFish = new();
            GameStateSnapshot gameStateSnapshot = new GameStateSnapshot();
            gameStateSnapshot.hasCaughtTutorialFish = hasCaughtTutorialFish;

            foreach (var fish in unCaughtFish)
            {
                if (IsCatchable(fish, gameStateSnapshot))
                {
                    catchableFish.Add(fish);
                }
            }
            return catchableFish;
        }

        public static List<Fish> UpdateCaughtFishInDatabase(List<Fish> fishDatabase)
        {
            //check which fish not marked as caught in the fishDatabase are actually caught
            //the first time each day this is called it will check all fish in the game,
            //but subsequent calls it will only check the ones that were uncaught since the last check
            //yay efficiency!

            foreach (var fish in fishDatabase.Where(f => f.HasBeenCaught != true))
            {
                if (Game1.player.fishCaught.TryGetValue(fish.Key, out var catchData) && catchData.Length > 0)
                {
                    // I suspect that the TryGetValue() fails for uncaught fish so we're only here if it has been caught...
                    int numberCaught = catchData[0];
                    if (numberCaught > 0)
                    {
                        fish.HasBeenCaught = true;
                        continue;
                    }
                }
                else
                {
                    continue;
                    //do nothing, the fish has not been caught
                }
            }
            return fishDatabase;
        }

        public static bool IsCatchable(Fish fish, GameStateSnapshot state)
        {
            return
            (
              (fish.Seasons.Contains(state.currentSeason.ToLower())
                    && fish.Times.Contains(state.currentTime)
                    && ((state.isRaining == true && fish.Weather != "sunny") || (state.isRaining == false && fish.Weather != "rainy"))
                    && (state.hasCaughtTutorialFish || fish.canBeTutorialFish)
                    && (fish.MinFishingLevel <= state.fishingLevel)
                    && (fish.Locations != "Submarine" || (state.isNightMarketToday && state.currentTime > 1700))
                    && ((fish.Locations != "Witch's Swamp" && fish.Locations != "Island" && fish.Locations != "Cove") || state.isCommunityCenterComplete)
                    && (!fish.Locations.Contains("Sewer") || state.hasRustyKey)
                    && (fish.Locations != "Mutant Bug Lair" || (state.hasRustyKey && state.isCommunityCenterComplete))
                    && (fish.Locations != "Mines" || state.hasSkullKey) //mine fish definitely catchable after bottom reached (I don't want to have to hard-code the others by level)
                    && (fish.Locations != "Desert" || state.isBusUnlocked)
                    && (fish.Locations.Contains("Legendary II") == false) //don't show Legendary II fish even if other conditions met
                )
                || (fish.Locations.Contains("Legendary II") && state.hasLegendaryIIQuestActive) //...just show them if the quest is active
            );
        }
    }
}
