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
