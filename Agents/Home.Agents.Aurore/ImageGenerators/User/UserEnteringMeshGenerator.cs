using Home.Common;
using Home.Common.HomeAutomation;
using Home.Common.Messages;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Agents.Aurore.ImageGenerators.User
{
    internal class UserEnteringMeshGenerator : ImageGeneratorBase
    {

        public override Image[] CreateImage(string imageKind, DisplayDeviceDescription destination, VisualTheme theme, object data)
        {
            Home.Common.Model.User user = ConvertData(data);
            if(user==null)
                return new Image[0];

            switch(destination.DisplayKind)
            {
                case DisplayKind.PixelArtScreen:
                    if (destination.IsSquare())
                        return MakePixelSquareImage(destination, theme, user);
                    break;
            }


            return new Image[0];
        }

        private Image[] MakePixelSquareImage(DisplayDeviceDescription destination, VisualTheme theme, Common.Model.User user)
        {
            var img = new Image<Rgba32>(destination.SizeInPixels.Width, destination.SizeInPixels.Height);

            img.Mutate(context =>
            {
                context.BackgroundColor(Color.Transparent);
                context.FillPolygon(Color.Red, new PointF(10, 10),
                    new PointF(destination.SizeInPixels.Width - 20, 10),
                    new PointF(destination.SizeInPixels.Width - 20, destination.SizeInPixels.Height - 20),
                    new PointF(10, destination.SizeInPixels.Height - 20));
            });

            return new Image[] { img };
        }

        private Common.Model.User ConvertData(object data)
        {
            if (data is Home.Common.Model.User)
                return data as  Home.Common.Model.User;

            if (data is PresenceChangedMessage)
                data = (data as PresenceChangedMessage).Data;

            if (data is PresenceChangedMessageData)
            {
                var pcmData = data as PresenceChangedMessageData;
                return AgentHelper.GetUser("aurore", pcmData.UserId);
            }

            return null;
        }
    }
}
