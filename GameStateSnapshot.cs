using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingPerfectionHelper
{
    public class GameStateSnapshot
    {
        public string currentSeason { get; set; }
        public int currentTime { get; set; }
        public bool isRaining { get; set; }
        public int fishingLevel { get; set; }
        public bool hasCaughtTutorialFish { get; set; }
        public bool isNightMarketToday { get; set; }
        public bool isCommunityCenterComplete { get; set; } //island, cove, and witch swamp access
        public bool isBusUnlocked { get; set; } //desert access
        public bool hasLegendaryIIQuestActive { get; set; }
        public bool hasRustyKey { get; set; } //sewer access
        public bool hasSkullKey { get; set; } //reached bottom of mines; all Mines fish accessible

        public GameStateSnapshot()
        {
            currentSeason = Game1.currentSeason;
            currentTime = Game1.timeOfDay;
            isRaining = Game1.isRaining;
            fishingLevel = Game1.player.FishingLevel;
            hasCaughtTutorialFish = false; //will populate later...
            isNightMarketToday = (Game1.currentSeason == "winter" && Game1.dayOfMonth >= 15 && Game1.dayOfMonth <= 17);
            isCommunityCenterComplete = Game1.player.hasCompletedCommunityCenter() ||
                                            (Game1.player.mailReceived.Contains("jojaBoilerRoom")
                                            && Game1.player.mailReceived.Contains("jojaCraftsRoom")
                                            && Game1.player.mailReceived.Contains("jojaFishTank")
                                            && Game1.player.mailReceived.Contains("jojaPantry")
                                            && Game1.player.mailReceived.Contains("jojaVault"));
            isBusUnlocked = Game1.player.mailReceived.Contains("ccVault") || Game1.player.mailReceived.Contains("jojaVault");
            hasLegendaryIIQuestActive = Game1.player.team.SpecialOrderRuleActive("LEGENDARY_FAMILY");
            hasRustyKey = Game1.player.mailReceived.Contains("HasRustyKey");
            hasSkullKey = Game1.player.mailReceived.Contains("HasSkullKey");
        }
    }   
}
