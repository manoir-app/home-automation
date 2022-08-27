using Emby.ApiClient;
using Emby.ApiClient.Cryptography;
using Emby.ApiClient.WebSocket;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Session;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Home.Agents.Sarah.Devices.Media
{
    public class EmbyMediaServer
    {
        private class MediaBrowserConfig
        {
            public string ServerUrl { get; set; }
            public MediaBrowserUserMapping[] Users { get; set; }
        }

        private class MediaBrowserUserMapping
        {
            public string MediaBrowserUser { get; set; }
        }

        MediaBrowserConfig _cfg = null;


        protected async void ThreadRun()
        {
            var cryptoProvider = new CryptographyProvider();
            var logger = new NullLogger();
            ApiClient cli = new ApiClient(logger, _cfg.ServerUrl, "93ecea5a36c1493a984bc80576589cee", cryptoProvider);
            cli.LibraryChanged += webSocket_LibraryChanged;
            UserDto[] users = null;

            var capabilities = new ClientCapabilities();
            
            cli.OpenWebSocket();
            cli.WebSocketClosed += Cli_WebSocketClosed;

            await cli.ReportCapabilities(capabilities);

            while (true)
            {
                try
                {
                    if (users == null)
                        users = await cli.GetUsersAsync(new UserQuery() { IsDisabled = false, IsHidden = false });

                    foreach (var user in _cfg.Users)
                    {

                        var qryLatest = new LatestItemsQuery()
                        {
                            IncludeItemTypes = new string[] { "Movie", "Episode" },
                            IsPlayed = false,
                            Fields = new ItemFields[] { ItemFields.DateLastMediaAdded },
                            Limit = 50,
                            GroupItems = true,
                            UserId = user.MediaBrowserUser
                        };
                        var views = await cli.GetLatestItems(qryLatest);
                        foreach (var v in views)
                        {
                            switch (v.Type.ToLower())
                            {
                                case "series":
                                    if ((v.UserData != null && v.UserData.IsFavorite) || v.DateLastMediaAdded.GetValueOrDefault().AddHours(12) > DateTime.Now)
                                    {
                                    }
                                    break;
                            }
                        }
                    }
                }
                catch
                {

                }
                Thread.Sleep(10000);
            }
            cli.CloseWebSocket();
        }

        private void Cli_WebSocketClosed(object sender, EventArgs e)
        {
            try
            {
                (sender as ApiClient).OpenWebSocket();
            }
            catch
            {

            }
        }

        void webSocket_LibraryChanged(object sender, MediaBrowser.Model.Events.GenericEventArgs<MediaBrowser.Model.Entities.LibraryUpdateInfo> e)
        {
            var tmps = e.Argument.ItemsAdded;
        }
    }
}
