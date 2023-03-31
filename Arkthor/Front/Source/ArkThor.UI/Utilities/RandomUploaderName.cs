using System;

namespace ArkThor.UI.Utilities
{
    public class RandomUploaderName
    {

        public static string[] Names = new string[] {
              "Tom", "Rich", "Barry",
              "Chris","Mary","Kate",
              "Mo","Dil","Eddy","MD","Jawed",
              "Pat","Peter","Matt",
              "Jo","Anne","Don",
              "Sales","Eng","Training",
              "Tommy","Team A","Team B",
              "Andy","Rachel","Les","SriRam","Mohammed",
              "Mannat","Tamanna","ArkThor","Admin","KCST","Falcon"
            };
        public static string GetRandomName()
        {
            Random random = new Random();
           int rand = random.Next(0, RandomUploaderName.Names.Length - 1);

            return RandomUploaderName.Names[rand];
        }
    }
}
