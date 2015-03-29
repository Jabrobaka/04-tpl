
using System;
using System.IO;

namespace Balancer
{
    class Program
    {
        static void Main(string[] args)
        {
            var settingsPath = "settings.txt";
            if (args.Length > 0)
            {
                settingsPath = args[0];
            }

            var balancerSettings = File.ReadAllLines(settingsPath);
            var balancer = new Balancer(balancerSettings);
            balancer.Start();
            Console.ReadKey();
        }
    }
}
