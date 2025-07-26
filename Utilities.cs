using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingPerfectionHelper
{
    public static class Utilities
    {
        public static string BuildCatchableFishListForDisplay(List<Fish> catchableFish)
        {
            // the ^ character creates new lines
            string message = 
            "These are the fish that you haven't caught yet that you should be able to catch under the " +
            "current season, conditions, and time:^^";

            int lineCount = 4; //number of lines taken by the above message + empty line
            catchableFish = catchableFish.OrderBy(f => f.Difficulty).ToList();
            foreach (var fish in catchableFish)
            {
                switch (lineCount)
                {
                    case 11:
                        message += "Page 2...^";
                        break;
                    case 22:
                        message += "Page 3...^";
                        break;
                    case 33:
                        message += "Page 4...^";
                        break;
                    case 44:
                        message += "Page 5...^";
                        break;
                    case 55:
                        message += "Page 6...^";
                        break;
                    case 66:
                        message += "Page 7...^";
                        break;
                    case 77: //this is enough for all fish in 1.6
                        message += "Page 8...^ ";
                        break;
                    default: //not a first line on a page, so just build a fish info line
                        string rain = "";
                        if (fish.Weather == "rainy")
                        {
                            rain = " - rain";
                        }

                        string thisFish = ($"> {fish.Name} ({fish.Locations}){rain}^");
                        // eg '> Pufferfish (Ocean)' or '> Walleye (Freshwater) - rain'

                        //truncate (sorry) if somehow too long (don't mess up the line counts for pages)
                        if (thisFish.Length > 50)
                        {
                            thisFish = thisFish.Substring(0, 49);
                            thisFish += "^";
                        }
                        message += thisFish;
                        break;
                } //end switch
                lineCount++;
            } //end foreach

            return message;
        }

        public static string BuildMissingFishListForDisplay(List<Fish> missingFish)
        {
            // the ^ character creates new lines
            string message =
            "There are no currently catchable fish that you are still missing. Below are listed the " +
            missingFish.Count + " fish you still need according to season:^^";

            int lineCount = 4; //number of lines taken by the above message + empty line
            missingFish = missingFish.OrderBy(f => f.Difficulty).ToList();
            foreach (var fish in missingFish)
            {
                if (!fish.Locations.Contains("Legendary II"))
                {
                    switch (lineCount)
                    {
                        case 11:
                            message += "Page 2...^";
                            break;
                        case 22:
                            message += "Page 3...^";
                            break;
                        case 33:
                            message += "Page 4...^";
                            break;
                        case 44:
                            message += "Page 5...^";
                            break;
                        case 55:
                            message += "Page 6...^";
                            break;
                        case 66:
                            message += "Page 7...^";
                            break;
                        case 77: //this is enough for all fish in 1.6
                            message += "Page 8...^ ";
                            break;
                        default: //not a first line on a page, so just build a fish info line
                            string rain = "";
                            if (fish.Weather == "rainy")
                            {
                                rain = " - rain";
                            }

                            string SeasonsString = "";
                            foreach (var s in fish.Seasons)
                            {
                                SeasonsString += s;
                                SeasonsString += "/";
                            }
                            SeasonsString = SeasonsString.TrimEnd('/');

                            string thisFish = ($"> {SeasonsString}: {fish.Name} {rain}^");

                            //truncate (sorry) if somehow too long (don't mess up the line counts for pages)
                            if (thisFish.Length > 50)
                            {
                                thisFish = thisFish.Substring(0, 49);
                                thisFish += "^";
                            }
                            message += thisFish;
                            break;
                    } //end switch
                    lineCount++;
                } //end check that it's not legendary II               
            } //end foreach

            return message;
        }
        public static List<int> GetTimeRange(int start, int end)
        {
            //we need all times in the range even though fish catchability only has hour precision
            //note that the end time is excluded
            List<int> times = new();
            for (int t = start; t < end; t += 10)
                times.Add(t);
            return times;
        }
    }
}
