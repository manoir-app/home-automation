using Home.Common.HomeAutomation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Agents.Sarah.Devices.Divoom
{
    public class Pixoo64 : BaseDivoomDevice
    {
        public Pixoo64(string ipV4) : base("Pixoo64", ipV4)
        {
        }

        public override DisplayDeviceDescription GetDescription()
        {
            return new DisplayDeviceDescription()
            {
                DisplayKind = DisplayKind.PixelArtScreen,
                SizeInPixels = new DisplaySize() { Height = 64, Width = 64 },
                RealSizeInMm = new DisplaySize() { Width = 260, Height = 260 }
            };
        }
    }
}
