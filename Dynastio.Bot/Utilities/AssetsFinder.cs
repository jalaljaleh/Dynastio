using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    internal class AssetsFinder
    {
        public static void Intilize()
        {
            var path = "W:\\projects\\Dynastio.Bot\\game\\aa\\Sprite";

            foreach(var item in Enum.GetNames(typeof(ItemType)))
            {
                try
                {
                    var filePath = path + "\\" + item.ToLower() + ".png";

                    File.Move(filePath, "W:\\projects\\Dynastio.Bot\\game\\moved\\" + item.ToLower() + ".png");
                    Console.WriteLine("item moved");

                }
                catch
                {
                    Console.WriteLine("item not found");
                }
            }



            foreach (var item in Enum.GetNames(typeof(EntityType)))
            {
                try
                {
                    var filePath = path + "\\" + item.ToLower() + ".png";

                    File.Move(filePath, "W:\\projects\\Dynastio.Bot\\game\\movedEn\\" + item.ToLower() + ".png");
                    Console.WriteLine("EntityType moved");

                }
                catch
                {
                    Console.WriteLine("EntityType not found");

                }
            }
        }
    }
}
