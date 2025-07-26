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
        private List<Fish> catchableFish = new();
        private List<Fish> unCaughtFish = new();
        public List<Fish> fishDatabase = new();
        private Boolean hasCaughtTutorialFish = false;
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
                { "160", "North River (Legendary)" }, // Angler
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
                fishDatabase = FishDataLoader.UpdateCaughtFishInDatabase(fishDatabase);
                if (!hasCaughtTutorialFish && fishDatabase.Any(f => f.HasBeenCaught == true))
                {
                    hasCaughtTutorialFish = true;
                }
                unCaughtFish = fishDatabase.Where(f => f.HasBeenCaught == false).ToList();
                catchableFish = FishDataLoader.GetCurrentlyCatchableFish(unCaughtFish, hasCaughtTutorialFish); 
                Game1.exitActiveMenu(); // Ensure no menu is open

                string message = "";

                if (unCaughtFish.Count == 0)
                {
                    message = "You've caught all the fish!";
                }
                else if (unCaughtFish.All(f => f.Locations.Contains("Legendary II")))
                {
                    message = "You've caught all the normal fish for fishing completion! You can still catch the Legendary Fish II ones if you wish. Look for the Extended Family Quest!";
                }
                else if (catchableFish.Count == 0)
                {
                    message = Utilities.BuildMissingFishListForDisplay(unCaughtFish);
                }
                else
                {
                    message = Utilities.BuildCatchableFishListForDisplay(catchableFish);
                }                
                Game1.drawLetterMessage(message);
            }
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            /*Need to repopulate this on day start, because if the player loads a different save,
              the previous save's database with its markers for caught fish will still be in scope.
              Calling this fresh each day ensures accuracy*/
            FishDataLoader.populateFishDatabase(fishDatabase, knownFishLocations);
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

        private void debug_printFishCaughtStatusToConsole(List<Fish> fishDatabase)
        {
            //refresh the caught fish before printing...
            fishDatabase = FishDataLoader.UpdateCaughtFishInDatabase(fishDatabase);

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
            foreach (var fish in fishDatabase)
            {
                if (fish.HasBeenCaught != true)
                {
                    Monitor.Log(fish.Name, LogLevel.Info);
                    Monitor.Log($"it can be caught at: {fish.Locations} in {fish.Weather} conditions", LogLevel.Info);
                }
            }
            Monitor.Log("..........................................................", LogLevel.Info);
        }
    }//end Mod
}//end Namespace
