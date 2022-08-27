using Home.Common;
using Home.Common.HomeAutomation;
using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing.PolygonClipper;
using System.Text;
using SixLabors.ImageSharp.PixelFormats;
using System.Linq;
using SixLabors.Fonts;

namespace Home.Agents.Aurore.ImageGenerators.User
{
    internal class UserDefaultAvatarGenerator : ImageGeneratorBase
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

            if (user.IsGuest)
                return new UserDiceBearAvatarGenerator().CreateImage(imageKind, destination, theme, user);

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
            else if(imageKind.Equals(UserDefaultAvatarGenerator.UserDefaultRoundAvatar))
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
            var img = new Image<Rgba32>(destination.SizeInPixels.Width, destination.SizeInPixels.Height);

            string initials = "";
            if (!string.IsNullOrWhiteSpace(user.FirstName))
                initials += user.FirstName.Substring(0, 1);
            if (!string.IsNullOrWhiteSpace(user.Name))
                initials += user.Name.Substring(0, 1);

            if (string.IsNullOrEmpty(initials) && !string.IsNullOrWhiteSpace(user.CommonName))
                initials = user.CommonName.Substring(0, 1);
            Color c = _defaultBackgroundColors[new Random().Next(0, _defaultBackgroundColors.Length - 1)];
            img.Mutate(context =>
            {
                context.BackgroundColor(Color.Transparent);
                context.FillPolygon(c, new PointF(0, 0),
                    new PointF(destination.SizeInPixels.Width, 0),
                    new PointF(destination.SizeInPixels.Width - 0, destination.SizeInPixels.Height - 0),
                    new PointF(10, destination.SizeInPixels.Height - 0));

                FontFamily fn;
                if(Fonts.FontsHelper.AllFonts.TryGet("Exo", out fn))
                {
                    int size = destination.SizeInPixels.Height / 3;
                    Font font = fn.CreateFont(size, FontStyle.Regular);
                    TextOptions options = new TextOptions(font)
                    {
                        TextAlignment= TextAlignment.Center,
                    };

                    FontRectangle rect = TextMeasurer.Measure(initials, options);
                    context.DrawText(initials, font, Color.White, 
                        new PointF((destination.SizeInPixels.Width - rect.Width) /2.0f,
                        (destination.SizeInPixels.Width - rect.Height) / 2));
                }
                else
                    Console.WriteLine($"Exo not in font list");

            });

            return new Image[] { img };
        }

        private Common.Model.User ConvertData(object data)
        {
            if (data is Home.Common.Model.User)
                return data as Home.Common.Model.User;
            return null;
        }
    }
}
