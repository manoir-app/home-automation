using Home.Common;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Home.Agents.Aurore
{

    public static class SpeechHelper
    {
        public static string GetFile(string content, string voice)
        {
            string hash = ComputeFilename(content);
            string pth = FileCacheHelper.GetLocalFilename("speech", voice, hash + ".wav");
            if (!File.Exists(pth))
            {
                content = "<speak xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:mstts=\"http://www.w3.org/2001/mstts\" xmlns:emo=\"http://www.w3.org/2009/10/emotionml\" version=\"1.0\" xml:lang=\"fr-CH\"><voice name=\"Microsoft Server Speech Text to Speech Voice (fr-FR, " + voice + ")\">" + content + "</voice></speak>";
                var key = ConfigurationSettingsHelper.GetAzureSpeechKey();
                var rg = ConfigurationSettingsHelper.GetAzureSpeechRegion();
                var config = SpeechConfig.FromSubscription(key, rg);
                using var audioConfig = AudioConfig.FromWavFileOutput(pth);
                using var synthesizer = new SpeechSynthesizer(config, audioConfig);
                var res = synthesizer.SpeakSsmlAsync(content).Result;
            }

            return pth;
        }

        private static string ComputeFilename(string content)
        {
            var md5 = MD5.Create();
            var bs = md5.ComputeHash(Encoding.UTF8.GetBytes(content));
            StringBuilder blr = new StringBuilder();
            foreach (var b in bs)
            {
                blr.Append(b.ToString("x2"));
            }
            string hash = blr.ToString();
            return hash;
        }

    }
}
