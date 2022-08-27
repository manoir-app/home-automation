using Home.Common;
using Home.Common.HomeAutomation;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Agents.Aurore.ImageGenerators
{
    public abstract class ImageGeneratorBase
    {
        public static Dictionary<string, ImageGeneratorBase> All = new Dictionary<string,ImageGeneratorBase>();

        public const string UserDefaultAvatar = "user.default-avatar";
        public const string UserDefaultRoundAvatar = "user.default-avatar.round";
        public const string UserGreeting = "user.greetings";
        public const string UserEnteringMesh = "user.presence.in";
        public const string UserLeavingMesh = "user.presence.out";

        public static void Init()
        {
            All.Clear();
            All.Add(UserEnteringMesh, new User.UserEnteringMeshGenerator());
            All.Add(UserDefaultAvatar, new User.UserDefaultAvatarGenerator());
            All.Add(UserDefaultRoundAvatar, new User.UserDefaultAvatarGenerator());

            // prévoir une composition mef à la place ?
        }

        public abstract Image[] CreateImage(string imageKind, DisplayDeviceDescription destination, VisualTheme theme, object data);
    }
}
