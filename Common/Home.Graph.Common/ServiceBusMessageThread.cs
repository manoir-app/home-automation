using Home.Common.Messages;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Home.Common
{
    public static class ServiceBusMessageThread
    {
        private static string cnString = null;

        public static async void Run(string channelName, MessageHandler hndl)
        {
            cnString = ConfigurationSettingsHelper.GetServiceBusConnectionString();
            Console.WriteLine(cnString);
            while (!_shouldStop)
            {
                _shouldRefresh = false;
                var sub = new MessageReceiver(cnString, channelName);

                while (!_shouldStop && !_shouldRefresh)
                {
                    try
                    {
                        var m = await sub.ReceiveAsync(TimeSpan.FromMilliseconds(500));
                        if (m == null)
                            continue;
                        var messagebody = Encoding.UTF8.GetString(m.Body, 0, m.Body.Length);
                        await sub.CompleteAsync(m.SystemProperties.LockToken);
                        try
                        {
                            string top = BaseMessage.GetTopic(messagebody);
                            if (top == null)
                                top = channelName;
                            hndl.Invoke(MessageOrigin.External, top, messagebody);
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                    catch (System.TimeoutException)
                    {
                        _shouldRefresh = true;
                    }
                    catch (Exception)
                    {
                    }
                }

                try
                {
                    await sub.CloseAsync();
                }
                catch
                {

                }

            }
        }

        //public static async Task SendMessageToServer(HubServerMessage rsp)
        //{
        //    string jsonRsp = JsonConvert.SerializeObject(rsp);
        //    byte[] byteRps = Encoding.UTF8.GetBytes(jsonRsp);

        //    for (int i = 0; i < 3; i++) // on va faire 3 essais
        //    {
        //        var psuh = new MessageSender(cnString, "toadmin");
        //        try
        //        {
        //            Message mRsp = new Message(byteRps);
        //            await psuh.SendAsync(mRsp);

        //            break; // si c'est OK, pas besoin de retry :)
        //        }
        //        catch (Exception ex)
        //        {
        //            if (i == 2)
        //                CentralizedLog.LogError(ex);
        //        }
        //        finally
        //        {
        //            try
        //            {
        //                await psuh.CloseAsync();
        //            }
        //            catch
        //            {
        //                // on ignore c'est pas chez nous
        //            }
        //        }
        //    }
        //}

        private static bool _shouldStop = false;
        private static bool _shouldRefresh = false;

        public static void Stop()
        {
            _shouldStop = true;
        }
    }
}
