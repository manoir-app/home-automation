using Home.Agents.Aurore.ImageGenerators;
using Home.Agents.Aurore.ImageGenerators.User;
using Home.Common;
using Home.Common.HomeAutomation;
using Home.Common.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing.PolygonClipper;
using System.Text;
using SixLabors.ImageSharp.PixelFormats;

namespace Home.Agents.Aurore.UserNotifications
{
    internal static class UserChangeNotificationHandler
    {
        public static MessageResponse HandleMessage(MessageOrigin origin, string topic, string messageBody)
        {
            if (!topic.StartsWith("users.accounts."))
                return MessageResponse.GenericFail;

            var gts = JsonConvert.DeserializeObject<UserChangeMessage>(messageBody);

            if(gts.Topic.StartsWith(UserChangeMessage.CreatedTopic)
                || gts.Topic.StartsWith(UserChangeMessage.UpdatedTopic))
            {
                var usr = AgentHelper.GetUser("aurore", gts.UserId);
                if (usr == null)
                    return MessageResponse.GenericFail;

                if (MakeImages(usr))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            using (var cli = new MainApiAgentWebClient("aurore"))
                            {
                                usr = cli.UploadData<Common.Model.User, Common.Model.UserImageData>($"v1.0/users/all/{usr.Id}/avatar/set", "POST", usr.Avatar);
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }

                return MessageResponse.OK;
            }
            else if(gts.Topic.StartsWith(UserChangeMessage.DeletedTopic))
            {
                string folder = FileCacheHelper.GetLocalFolder("users", $"avatars/{gts.UserId}");
                if(folder!=null)
                {
                    if(Directory.Exists(folder))
                        Directory.Delete(folder, true);
                }
                return MessageResponse.OK;
            }

            return MessageResponse.GenericFail;

        }

        private static bool MakeImages(Common.Model.User usr)
        {
            bool hasChanged = false;
            if(usr.Avatar.UrlSquareBig==null)
            {
                var imgs = new UserDefaultAvatarGenerator().CreateImage(ImageGeneratorBase.UserDefaultAvatar,
                    new DisplayDeviceDescription()
                    {
                        DisplayKind = DisplayKind.Screen,
                        SizeInPixels = new DisplaySize() { Width = 600, Height = 600 }
                    },
                    VisualTheme.MaNoirDefault, usr);
                usr.Avatar.UrlSquareBig = SaveImage(usr, imgs,"big-square");
                hasChanged = true;
            }
            if (usr.Avatar.UrlSquareSmall == null)
            {
                var imgs = new UserDefaultAvatarGenerator().CreateImage(ImageGeneratorBase.UserDefaultAvatar,
                    new DisplayDeviceDescription()
                    {
                        DisplayKind = DisplayKind.Screen,
                        SizeInPixels = new DisplaySize() { Width = 250, Height = 250 }
                    },
                    VisualTheme.MaNoirDefault, usr);
                usr.Avatar.UrlSquareSmall = SaveImage(usr, imgs, "small-square");
                hasChanged = true;

            }
            if (usr.Avatar.UrlSquareTiny == null)
            {
                var imgs = new UserDefaultAvatarGenerator().CreateImage(ImageGeneratorBase.UserDefaultAvatar,
                    new DisplayDeviceDescription()
                    {
                        DisplayKind = DisplayKind.Screen,
                        SizeInPixels = new DisplaySize() { Width = 64, Height = 64 }
                    },
                    VisualTheme.MaNoirDefault, usr);
                usr.Avatar.UrlSquareTiny = SaveImage(usr, imgs, "tiny-square");
                hasChanged = true;

            }

            return hasChanged;
        }

        private static string SaveImage(Common.Model.User usr, Image[] imgs, string imgName)
        {
            if (imgs == null || imgs.Length == 0)
                return null;
            var path =  FileCacheHelper.GetLocalFilename("users", $"avatars/{usr.Id}", imgName +".png") ;
            if (path == null)
                return null;
            imgs[0].SaveAsPng(path);
            string rootPath = FileCacheHelper.GetRootPath();
            path = path.Substring(rootPath.Length);
            if(!path.StartsWith("/"))
                path = "/" + path;

            // pour l'instant, on met l'url public en dur, pour les tests
            path = "https://public.anzin.carbenay.manoir.app/v1.0/services/files" + path;

            return path;
        }
    }
}
