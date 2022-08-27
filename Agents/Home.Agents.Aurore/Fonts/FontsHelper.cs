using SixLabors.Fonts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Home.Agents.Aurore.Fonts
{
    internal static class FontsHelper
    {
        public static FontCollection AllFonts { get; private set; }

        static FontsHelper()
        {
            AllFonts = new FontCollection();
            string pth = Assembly.GetEntryAssembly().Location;
            pth = Path.Combine(Path.GetDirectoryName(pth), "Fonts");
            if (Directory.Exists(pth))
                LoadFonts(pth);
            else
                Console.WriteLine($"Folder {pth} does not exist");


            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (Directory.Exists("/home-automation/fonts"))
                {
                    LoadFonts("/home-automation/fonts");
                }
            }

            foreach(var k in AllFonts.Families)
            {
                var sty = k.GetAvailableStyles();
                Console.WriteLine("Available font : " + k.Name + " with " + sty.Count() + " style(s)");
            }
        }

        private static void LoadFonts(string pth)
        {
            foreach (string s in Directory.GetFiles(pth, "*.ttf", SearchOption.AllDirectories))
            {
                AllFonts.Add(s);
                Console.WriteLine($"Added {s} to font list");
            }
        }
    }
}
