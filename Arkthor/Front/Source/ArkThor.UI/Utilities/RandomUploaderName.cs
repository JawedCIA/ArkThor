using System;

namespace ArkThor.UI.Utilities
{
    public class RandomUploaderName
    {

        public static string[] Names = new string[] {
             "C3i","MD","Jawed","Anand",
             "SriRam","Mohammed",
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
