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
        private Boolean hasCaughtTutorialFish = false;
        private Boolean isNightMarketToday = false;
        private Boolean isCommunityCenterComplete = false;
        private Boolean hasRustyKey = false;
        private Boolean hasSkullKey = false;
        private Boolean isBusUnlocked = false;
        private Boolean hasExtendedFamilyQuest = false;

        private static readonly Dictionary<string, string> knownFishLocations = new()
        {        //fishid, location
                { "128", "Ocean" }, // Pufferfish
                { "129", "Ocean" }, // Anchovy
                { "130", "Ocean" }, // Tuna
                { "131", "Ocean" }, // Sardine
                { "132", "River" }, // Bream
                { "136", "Mountain Lake" }, // Largemouth Bass
                { "137", "River, Forest Pond" }, // Smallmouth Bass
                { "138", "Freshwater" }, // Rainbow Trout
                { "139", "River, Waterfalls" }, // Salmon
                { "140", "Freshwater" }, // Walleye
                { "141", "Freshwater" }, // Perch
                { "142", "Freshwater" }, // Carp
                { "143", "River" }, // Catfish
                { "144", "River, Forest Pond" }, // Pike
                { "145", "River" }, // Sunfish
                { "146", "Ocean" }, // Red Mullet
                { "147", "Ocean" }, // Herring
                { "148", "Ocean" }, // Eel
                { "149", "Ocean" }, // Octopus
                { "150", "Ocean" }, // Red Snapper
                { "151", "Ocean" }, // Squid
                { "154", "Ocean" }, // Sea Cucumber
                { "155", "Ocean" }, // Super Cucumber
                { "156", "Mines" }, // Ghostfish
                { "158", "Mines" }, // Stonefish
                { "159", "Ocean (Legendary)" }, // Crimsonfish
                { "160", "River (Legendary)" }, // Angler
                { "161", "Mines" }, // Ice Pip
                { "162", "Mines" }, // Lava Eel
                { "163", "Mountain Lake (Legendary)" }, // Legend
                { "164", "Desert" }, // Sandfish
                { "165", "Desert" }, // Scorpion Carp
                { "267", "Ocean" }, // Flounder
                { "269", "Mountain Lake, Forest Pond" }, // Midnight Carp
                { "372", "Crab Pot" }, // Clam
                { "682", "Sewers (Legendary)" }, // Mutant Carp
                { "698", "Mountain Lake" }, // Sturgeon
                { "699", "River" }, // Tiger Trout
                { "700", "Mountain Lake" }, // Bullhead
                { "701", "Ocean" }, // Tilapia
                { "702", "Forest River, Mountain Lake" }, // Chub
                { "704", "Forest River" }, // Dorado
                { "705", "Ocean" }, // Albacore
                { "706", "River" }, // Shad
                { "707", "River, Mountain Lake" }, // Lingcod
                { "708", "Ocean" }, // Halibut
                { "715", "Crab Pot" }, // Lobster
                { "716", "Crab Pot" }, // Crayfish
                { "717", "Crab Pot" }, // Crab
                { "718", "Crab Pot" }, // Cockle
                { "719", "Crab Pot" }, // Mussel
                { "720", "Crab Pot" }, // Shrimp
                { "721", "Crab Pot" }, // Snail
                { "722", "Crab Pot" }, // Periwinkle
                { "723", "Crab Pot" }, // Oyster
                { "734", "Secret Woods Pond" }, // Woodskip
                { "775", "Forest River (Legendary)" }, // Glacierfish
                { "795", "Witch's Swamp" }, // Void Salmon
                { "796", "Mutant Bug Lair" }, // Slimejack
                { "798", "Submarine" }, // Midnight Squid
                { "799", "Submarine" }, // Spook Fish
                { "800", "Submarine" }, // Blobfish
                { "836", "Cove" }, // Stingray
                { "837", "Island" }, // Lionfish
                { "838", "Island" }, // Blue Discus
                { "898", "Ocean (Legendary II)" }, // Son of Crimsonfish
                { "899", "River (Legendary II)" }, // Ms. Angler
                { "900", "Mountain Lake (Legendary II)" }, // Legend II
                { "901", "Sewers (Legendary II)" }, // Radioactive Carp
                { "902", "Forest River (Legendary II)" }, // Glacierfish Jr.
                { "Goby", "Waterfalls" } // Goby
        };


        public override void Entry(IModHelper helper)
        {
            populateFishDatabase();
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.TimeChanged += OnTimeChanged;
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            UpdateCaughtFish();

            isNightMarketToday = (Game1.currentSeason == "winter" && Game1.dayOfMonth >= 15 && Game1.dayOfMonth <= 17);
            isCommunityCenterComplete = Game1.player.hasCompletedCommunityCenter() ||
                                            (Game1.player.mailReceived.Contains("jojaBoilerRoom")
                                            && Game1.player.mailReceived.Contains("jojaCraftsRoom")
                                            && Game1.player.mailReceived.Contains("jojaFishTank")
                                            && Game1.player.mailReceived.Contains("jojaPantry")
                                            && Game1.player.mailReceived.Contains("jojaVault"));
            
            isBusUnlocked = Game1.player.mailReceived.Contains("ccVault") || Game1.player.mailReceived.Contains("jojaVault");

            //printFishCaughtStatusToConsole();
        }

        private void OnTimeChanged(object? sender, TimeChangedEventArgs e)
        {
            var caught = Game1.stats.FishCaught;

            foreach (var fish in unCaughtFish)
            {
                if (fish.IsCatchable(hasCaughtTutorialFish, isNightMarketToday, isCommunityCenterComplete, isBusUnlocked))
                {
                    Monitor.Log($"[Fishing Helper] You can catch: {fish.Name} at {fish.Locations} in {fish.Weather} conditions", LogLevel.Info);
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
                //check that fishId is an int and not "SeaJelly" etc
                if (int.TryParse(fishIdStr, out int fishId)) { 
                    FishInfo currentFish = new();
                    //the key used to reference the fish in .fishCaught is (0)XXX where XXX is the fishid
                    currentFish.Key = $"(O){fishIdStr}";
                    currentFish.Name = rawFish.Split('/')[0];
                    currentFish.Id = Int32.Parse(fishIdStr);

                    //location
                    knownFishLocations.TryGetValue(fishIdStr, out string location);
                    if (location == null || location == "")
                        location = "Unknown";
                    currentFish.Locations = location;

                    if (rawFish.Split('/')[1] == "trap")
                    {   //crab pot fish don't have season/weather/level requirements or indices
                        currentFish.Seasons.Add("spring");
                        currentFish.Seasons.Add("summer");
                        currentFish.Seasons.Add("fall");
                        currentFish.Seasons.Add("winter");
                        currentFish.Times = GetTimeRange(600, 2600);
                        currentFish.Weather = "both";
                        currentFish.MinFishingLevel = 0;
                        currentFish.canBeTutorialFish = false;
                    }
                    else
                    {
                        //seasons
                        string seasonsString = rawFish.Split('/')[6];
                        foreach (var season in seasonsString.Split(' '))
                        {
                            currentFish.Seasons.Add(season);
                        }

                        //times
                        string TimeString = rawFish.Split('/')[5];
                        int startTime = Int32.Parse(TimeString.Split(' ')[0]);
                        int endTime = Int32.Parse(TimeString.Split(' ')[1]);
                        currentFish.Times = GetTimeRange(startTime, endTime);

                        //weather
                        currentFish.Weather = rawFish.Split('/')[7];

                        //fishing level
                        currentFish.MinFishingLevel = Int32.Parse(rawFish.Split('/')[12]);

                        //tutorial fish
                        currentFish.canBeTutorialFish = Boolean.Parse(rawFish.Split('/')[13]);
                    }

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
                        if (!hasCaughtTutorialFish)
                            hasCaughtTutorialFish = true;
                        continue;
                    }
                }

                // If not caught or count is 0
                unCaughtFish.Add(fish);
            }
        }//end UpdateCaughtFish

        private void printFishCaughtStatusToConsole()
        {
            Monitor.Log("=== you have caught ===", LogLevel.Info);
            foreach (var fish in fishDatabase)
            {
                if (fish.HasBeenCaught == true)
                {
                    Monitor.Log(fish.Name, LogLevel.Info);
                    Monitor.Log($"it can be caught at: {fish.Locations} in {fish.Weather} conditions", LogLevel.Info);
                }
            }
            Monitor.Log("=== you still need ===", LogLevel.Info);
            foreach (var fish in unCaughtFish)
            {
                Monitor.Log(fish.Name, LogLevel.Info);
                Monitor.Log($"it can be caught at: {fish.Locations} in {fish.Weather} conditions", LogLevel.Info);
            }
        }
    }//end Mod
}//end Namespace