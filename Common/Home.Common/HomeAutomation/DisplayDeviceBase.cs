using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.HomeAutomation
{
    public enum DisplayKind
    {
        Screen,
        PixelArtScreen,
        EPaper
    }

    public class DisplaySize
    {
        public int Width { get; set; }

        public int Height { get; set; }
    }

    public class DisplayDeviceDescription
    {
        public DisplayKind DisplayKind { get; set; }

        public DisplaySize RealSizeInMm { get; set; }
        public DisplaySize SizeInPixels { get; set; }

        public decimal GetAspectRatio()
        {
            if (SizeInPixels.Height == 0)
                return 0M;

            return Math.Round((decimal)SizeInPixels.Width / (decimal)SizeInPixels.Height, 3);
        }

        public bool IsSquare()
        {
            decimal ratio = GetAspectRatio();
            return ratio >= 0.97M && ratio <= 1.03M;
        }
    }

    public abstract class DisplayDeviceBase : DeviceBase
    {
        public abstract DisplayDeviceDescription GetDescription();
    }
}
