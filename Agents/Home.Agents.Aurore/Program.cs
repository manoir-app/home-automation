using Home.Agents.Aurore.Greetings;
using Home.Agents.Aurore.Integrations;
using Home.Agents.Aurore.Integrations.Rhasspy;
using Home.Agents.Aurore.Llm;
using Home.Common;
using Home.Graph.Common;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Agents.Aurore
{
    class Program
    {
        internal static bool _stop = false;

        static void Main(string[] args)
        {
            AgentHelper.WriteStartupMessage("Aurore", typeof(Program).Assembly);

#if DEBUG
            // Test du système d'icônes incorporées
            if (args.Length > 0 && args[0] == "test-icons")
            {
                Console.WriteLine("=== TEST - Icônes incorporées ===\n");
                
                var icons = IconResourceHelper.ListAvailableIcons("8x8");
                Console.WriteLine($"Icônes 8x8 disponibles ({icons.Count}):");
                foreach (var icon in icons.Take(10))
                {
                    Console.WriteLine($"  - {icon}");
                    var base64 = IconResourceHelper.GetIconBase64(icon.Replace(".png", ""), "8x8");
                    if (!string.IsNullOrEmpty(base64))
                    {
                        Console.WriteLine($"    ✓ Chargée ({base64.Length} chars Base64)");
                    }
                }
                
                if (icons.Count > 10)
                {
                    Console.WriteLine($"  ... et {icons.Count - 10} autres");
                }
                
                Console.WriteLine($"\n{IconResourceHelper.GetCacheStats()}");
                
                // Test des énumérations
                Console.WriteLine("\n=== Test énumération AwtrixIcon8x8 ===");
                Console.WriteLine($"DoorOpen -> {AwtrixIcon8x8.DoorOpen.ToFileName()}");
                Console.WriteLine($"Alarm -> {AwtrixIcon8x8.Alarm.ToFileName()}");
                Console.WriteLine($"Sun -> {AwtrixIcon8x8.Sun.ToFileName()}");
                
                var doorIcon = AwtrixIcon8x8.DoorOpen.GetBase64();
                Console.WriteLine($"DoorOpen Base64: {(doorIcon != null ? doorIcon.Substring(0, 50) + "..." : "null")}");
                
                return;
            }

            Console.WriteLine("=== MODE DEBUG - Tests AWTRIX avec énumérations ===");
            
            // Test 1: Message avec énumération
            Console.WriteLine("\nTest 1: Message avec AwtrixIcon8x8.DoorOpen");
            AwtrixHelper.BroadcastMessageAsync("Porte ouverte!", AwtrixIcon8x8.DoorOpen, 5).Wait();
            Thread.Sleep(6000);

            // Test 2: Alerte avec énumération
            Console.WriteLine("\nTest 2: Alerte avec AwtrixIcon8x8.Alarm");
            AwtrixHelper.SendAlertAsync("⚠️ ALERTE TEST ⚠️").Wait();
            Thread.Sleep(8000);

            // Test 3: Bienvenue
            Console.WriteLine("\nTest 3: Message de bienvenue");
            AwtrixHelper.SendWelcomeNotificationAsync("Michael").Wait();
            Thread.Sleep(6000);

            // Test 4: Progression avec énumération
            Console.WriteLine("\nTest 4: Progression avec AwtrixIcon8x8.Power");
            for (int progress = 0; progress <= 100; progress += 20)
            {
                AwtrixHelper.SendProgressNotificationAsync($"Chargement {progress}%", progress, AwtrixIcon8x8.Power).Wait();
                Thread.Sleep(1500);
            }
            Thread.Sleep(3000);

            // Test 5: App météo avec énumération
            Console.WriteLine("\nTest 5: App météo avec AwtrixIcon8x8.Sun");
            AwtrixHelper.SendWeatherNotificationAsync("22°C Ensoleillé", AwtrixIcon8x8.Sun).Wait();
            Thread.Sleep(6000);

            // Test 6: Privacy Mode
            Console.WriteLine("\nTest 6: Activation Privacy Mode");
            AwtrixHelper.UpdateMeshPrivacyStatus(true);
            Thread.Sleep(5000);
            
            Console.WriteLine("\nTest 7: Désactivation Privacy Mode");
            AwtrixHelper.UpdateMeshPrivacyStatus(false);
            Thread.Sleep(3000);

            // Test 8: Chat
            Console.WriteLine("\nTest 8: Notification de chat");
            AwtrixHelper.SendChatNotification("Alice", "Salut! Comment ça va?", "famille");
            Thread.Sleep(6000);

            // Test 9: Température avec énumération
            Console.WriteLine("\nTest 9: App température avec AwtrixIcon8x8.Thermometer");
            AwtrixHelper.CreateOrUpdateCustomAppAsync("temp_test", "21.5°C", AwtrixIcon8x8.Thermometer, 8).Wait();
            Thread.Sleep(10000);

            // Test 10: Démonstration de toutes les catégories d'icônes
            Console.WriteLine("\nTest 10: Démonstration rapide de différentes icônes");
            var demoIcons = new[]
            {
                (AwtrixIcon8x8.Coffee, "Café prêt"),
                (AwtrixIcon8x8.WashingMachine, "Lessive terminée"),
                (AwtrixIcon8x8.Fire, "Alerte incendie"),
                (AwtrixIcon8x8.Rain, "Pluie détectée"),
                (AwtrixIcon8x8.Ok, "Tout va bien")
            };

            foreach (var (icon, message) in demoIcons)
            {
                Console.WriteLine($"  → {icon}: {message}");
                AwtrixHelper.BroadcastMessageAsync(message, icon, 3).Wait();
                Thread.Sleep(3500);
            }

            Console.WriteLine("\n=== Tests AWTRIX terminés ===");
            Console.WriteLine("✓ Toutes les icônes utilisent maintenant l'énumération AwtrixIcon8x8 !");
            Console.WriteLine($"{IconResourceHelper.GetCacheStats()}");
            Console.WriteLine("IP configurée : 192.168.2.11");
            Console.WriteLine("\n💡 Commandes disponibles:");
            Console.WriteLine("  - test-icons : Tester le système d'icônes");
            Console.WriteLine("  - (défaut)   : Tests AWTRIX complets");
            return;
#endif

            AgentHelper.SetupReporting("aurore");
            AgentHelper.SetupLocaleFromServer("aurore");

            AgentHelper.ReportStart("aurore", "user-interaction");

            MqttHelper.Start("agents-aurore");
            AuroreMessageHandler.Start();
            AuroreNewsItemService.Start();
            CommonAppGreetingsUpdater.Start();
            
            while (!_stop)
            {
                Thread.Sleep(500);
                AgentHelper.Ping("aurore");
            }
            
            CommonAppGreetingsUpdater.Stop();
            AuroreNewsItemService.Stop();
            AuroreMessageHandler.Stop();
            MqttHelper.Stop();
        }

        private static void CopyCommonFiles(string root, string folder)
        {
            if(!root.EndsWith("/"))
                root = root + "/";

            foreach(var sub in Directory.GetDirectories(folder))
                CopyCommonFiles(root, sub);

            foreach(var t in Directory.GetFiles(folder))
            {
                string dest = "/home-automation/files/cache/" + t.Substring(root.Length);
                var dir = Path.GetDirectoryName(dest);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.Copy(t, dest, true);
            }
        }
    }
}
