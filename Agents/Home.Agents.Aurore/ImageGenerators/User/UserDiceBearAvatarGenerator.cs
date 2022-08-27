using Home.Common;
using Home.Common.HomeAutomation;
using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using System.IO;
using System.Net.Http;

namespace Home.Agents.Aurore.ImageGenerators.User
{
    internal class UserDiceBearAvatarGenerator : ImageGeneratorBase
    {
        private Color[] _defaultBackgroundColors =
      {
            Color.FromRgb(0xBD, 0x2A, 0x2E),
            Color.FromRgb(0x3B, 0x39, 0x36),
            Color.FromRgb(0x48, 0x69, 0x66),
            Color.FromRgb(0x96, 0x2B, 0x09)
        };

        public override Image[] CreateImage(string imageKind, DisplayDeviceDescription destination, VisualTheme theme, object data)
        {
            Home.Common.Model.User user = ConvertData(data);
            if (user == null)
                return new Image[0];

            if (imageKind == null || imageKind.Equals(ImageGeneratorBase.UserDefaultAvatar))
            {
                switch (destination.DisplayKind)
                {
                    case DisplayKind.EPaper:
                        return MakeSquareImage(destination, theme, user);
                    default:
                        return MakeSquareImage(destination, theme, user);
                }
            }
            else if (imageKind.Equals(UserDefaultAvatarGenerator.UserDefaultRoundAvatar))
            {
                switch (destination.DisplayKind)
                {
                    case DisplayKind.EPaper:
                        return MakeSquareImage(destination, theme, user);
                    default:
                        return MakeSquareImage(destination, theme, user);
                }
            }

            return new Image[0];
        }

        private Image[] MakeSquareImage(DisplayDeviceDescription destination, VisualTheme theme, Common.Model.User user)
        {
            int width = destination.SizeInPixels.Width, height = destination.SizeInPixels.Height;

            var tmpPath = Path.Combine(Path.GetTempPath(),
                Path.ChangeExtension(Path.GetRandomFileName(), ".png"));

            Color back = _defaultBackgroundColors[new Random().Next(0, _defaultBackgroundColors.Length)];

            string color = back.ToString();
            if (color.Length > 6)
                color = color.Substring(0, 6);
            string url = $"https://avatars.dicebear.com/api/adventurer/{user.Id}.png?background=%23{color}";

            using(var t = new HttpClient())
            using(var stLocal = File.Create(tmpPath))
            {
                t.GetStreamAsync(url).Result.CopyTo(stLocal);
            }

            return new Image[] { Image.Load(tmpPath) };
        }

        private Common.Model.User ConvertData(object data)
        {
            if (data is Home.Common.Model.User)
                return data as Home.Common.Model.User;
            return null;
        }
    }
}
