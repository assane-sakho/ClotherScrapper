using ClothScrapper.Model;
using System;
using System.Threading.Tasks;

namespace ClothScrapper
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ZalandoScrapper zalandoScrapper = new ZalandoScrapper();
            await zalandoScrapper.Start();

            Console.ReadLine();
        }
    }
}
