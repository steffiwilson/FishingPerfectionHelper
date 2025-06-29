using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Specialized;
using StardewModdingAPI.Utilities;
using StardewValley.Menus;
using GenericModConfigMenu;
using System.Xml;

namespace FishingPerfectionHelper
{
    public class ModEntry : Mod
    {
        private List<FishInfo> catchableFish = new();
        private List<FishInfo> unCaughtFish = new();
        private List<FishInfo> fishDatabase = new();
        private Boolean hasCaughtTutorialFish = false;
        private Boolean isNightMarketToday = false;
        private Boolean isCommunityCenterComplete = false;
        private Boolean isBusUnlocked = false;
        private ModConfig config;

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
                { "152", "Ocean" }, // Seaweed
                { "153", "Freshwater" }, // Green algae
                { "154", "Ocean" }, // Sea Cucumber
                { "155", "Ocean" }, // Super Cucumber
                { "156", "Mines" }, // Ghostfish
                { "157", "Mines" }, // White Algae
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
            config = helper.ReadConfig<ModConfig>();
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Input.ButtonPressed += OnButtonPressed;            
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (!Context.IsPlayerFree) 
                return;

            if (e.Button == config.ViewAvailableFish) 
            {
                UpdateCaughtFish(); //recheck uncaught fish in the db to see if they're caught now
                UpdateCatchableFish(); //recheck uncaughtfish to find which are currently catchable
                Game1.exitActiveMenu(); // Ensure no menu is open
                string message = BuildCatchableFishListForDisplay(catchableFish);
                Game1.drawLetterMessage(message);
            }
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            //need to repopulate on day start to be sure we have this player's fish caught flagged and not a cache from another loaded save
            populateFishDatabase();

            isNightMarketToday = (Game1.currentSeason == "winter" && Game1.dayOfMonth >= 15 && Game1.dayOfMonth <= 17);
            isCommunityCenterComplete = Game1.player.hasCompletedCommunityCenter() ||
                                            (Game1.player.mailReceived.Contains("jojaBoilerRoom")
                                            && Game1.player.mailReceived.Contains("jojaCraftsRoom")
                                            && Game1.player.mailReceived.Contains("jojaFishTank")
                                            && Game1.player.mailReceived.Contains("jojaPantry")
                                            && Game1.player.mailReceived.Contains("jojaVault"));
            
            isBusUnlocked = Game1.player.mailReceived.Contains("ccVault") || Game1.player.mailReceived.Contains("jojaVault");

            debug_printFishCaughtStatusToConsole();
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this.config)
            );

            configMenu.AddKeybind(
                mod: this.ModManifest,
                getValue: () => this.config.ViewAvailableFish,
                setValue: value => this.config.ViewAvailableFish = value,
                name: () => "View available fish",
                tooltip: () => "Opens the list of fish that can be caught currently. Automatically hides fish that are unavailable due to locked areas or requirements such as fishing level.",
                fieldId: null
            );
        }

        private static List<int> GetTimeRange(int start, int end)
        {
            //we need all times in the range even though fish catchability only has hour precision
            //note that the end time is excluded
            List<int> times = new();
            for (int t = start; t < end; t += 10)
                times.Add(t);
            return times;
        }

        private void populateFishDatabase()
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

                //check that fishId is an int and not "SeaJelly" etc
                if (int.TryParse(fishIdStr, out int fishId)) { 
                    FishInfo currentFish = new();
                    //the key used to reference the fish in .fishCaught is (0)XXX where XXX is the fishid
                    currentFish.Key = $"(O){fishIdStr}";
                    currentFish.Name = rawFish.Split('/')[0];
                    currentFish.Id = Int32.Parse(fishIdStr);

                    //location (hard-coded, sorry D: )
                    knownFishLocations.TryGetValue(fishIdStr, out string location);
                    if (location == null || location == "")
                        location = "Unknown";
                    currentFish.Locations = location;

                    if (rawFish.Split('/')[1] == "trap")
                    {   //crab pot fish don't have season/weather/level requirements (or indices)
                        currentFish.Seasons.Add("spring");
                        currentFish.Seasons.Add("summer");
                        currentFish.Seasons.Add("fall");
                        currentFish.Seasons.Add("winter");
                        currentFish.Times = GetTimeRange(600, 2600);
                        currentFish.Weather = "both";
                        currentFish.MinFishingLevel = 0;
                        currentFish.canBeTutorialFish = false;
                        currentFish.Difficulty = 0;
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

                        currentFish.Difficulty = Int32.Parse(rawFish.Split('/')[1]);
                    }

                    currentFish.HasBeenCaught = false; //initialize it as uncaught
                    fishDatabase.Add(currentFish);
                }
            }
        }

        private void UpdateCaughtFish()
        {
            //check which fish not marked as caught in the fishDatabase are actually caught
            //we do this each time the player opens the view (first time per day it will check all fish,
            //then subsequent times it will just recheck the ones that weren't caught at the start of
            //the day (efficiency!))
            unCaughtFish.Clear();
            foreach (var fish in fishDatabase.Where(f => f.HasBeenCaught != true))
            {
                if (Game1.player.fishCaught.TryGetValue(fish.Key, out var catchData) && catchData.Length > 0)
                {
                    // I suspect that the TryGetValue() fails for uncaught fish so we're only here if it has been caught...
                    int numberCaught = catchData[0];
                    if (numberCaught > 0)
                    {
                        fish.HasBeenCaught = true;
                        if (!hasCaughtTutorialFish)
                            hasCaughtTutorialFish = true;
                        continue;
                    }
                }

                // If not caught (or count was 0)
                unCaughtFish.Add(fish);
            }
        }

        private void UpdateCatchableFish()
        {
            //check which fish that haven't been caught yet are catchable under current game state

            catchableFish.Clear();

            foreach (var fish in unCaughtFish)
            {
                //IsCatchable checks time, season, weather, fishing level, area unlocks, and quest conditions
                if (fish.IsCatchable(hasCaughtTutorialFish, isNightMarketToday, isCommunityCenterComplete, isBusUnlocked))
                {
                    catchableFish.Add(fish);
                }
            }
        }

        public string BuildCatchableFishListForDisplay(List<FishInfo> catchableFish)
        {
            string message = //line breaks as they display in the 52?-character monospace window in-game
            "These are the fish that you haven't caught yet     " +
            "that you should be able to catch under the current " +
            "season, conditions, and time:                      " +
            "                                                     ";

            int lineCount = 4;
            catchableFish = catchableFish.OrderBy(f => f.Difficulty).ToList();
            foreach (var fish in catchableFish)
            {
                //the pagination is weird ok, each page seems to have a different length
                //available for line 1 so I'm hard-coding those lines at just the right
                //size with the spaces (idk what i'm doing)
                switch (lineCount)
                {
                    case 11:
                        message += "Page 2...                                      ";
                        break;
                    case 22:
                        message += "Page 3...                                     ";
                        break;
                    case 33:
                        message += "Page 4...                                       ";
                        break;
                    case 44:
                        message += "Page 5...                                        ";
                        break;
                    case 55:
                        message += "Page 6...                                         ";
                        break;
                    case 66:
                        message += "Page 7...                                      ";
                        break;
                    case 77: //this is enough for all fish in 1.6
                        message += "Page 8...                                       ";
                        break;
                    default: //not a first line on a page, so just build a fish info line
                        //limit what we display bc of the ~50 char limit which runs out easily
                        string rain = "";
                        if (fish.Weather == "rainy")
                        {
                            rain = " - rain";
                        }

                        string thisFish = ($"> {fish.Name} ({fish.Locations}){rain}");
                        // eg '> Pufferfish (Ocean)' or '> Walleye (Freshwater) - rain'

                        //truncate (sorry) if somehow too long 
                        if (thisFish.Length > 52)
                            thisFish = thisFish.Substring(0, 50);

                        while (thisFish.Length < 52)
                        {
                            thisFish += " "; //append spaces until it fills the line lol what could possibly go wrong
                        }

                        message += thisFish;
                        break;
                } //end switch
                lineCount++;
            } //end foreach

            return message;

        }

        private void debug_printFishCaughtStatusToConsole()
        {
            UpdateCaughtFish();
            UpdateCatchableFish();

            Monitor.Log("..........................................................", LogLevel.Info);
            Monitor.Log("============= you have caught =============", LogLevel.Info);
            foreach (var fish in fishDatabase)
            {
                if (fish.HasBeenCaught == true)
                {
                    Monitor.Log(fish.Name, LogLevel.Info);
                    Monitor.Log($"it can be caught at: {fish.Locations} in {fish.Weather} conditions", LogLevel.Info);
                }
            }
            Monitor.Log("============= you still need =============", LogLevel.Info);
            foreach (var fish in unCaughtFish)
            {
                Monitor.Log(fish.Name, LogLevel.Info);
                Monitor.Log($"it can be caught at: {fish.Locations} in {fish.Weather} conditions", LogLevel.Info);
            }
            Monitor.Log("..........................................................", LogLevel.Info);
        }
    }//end Mod
}//end Namespace
