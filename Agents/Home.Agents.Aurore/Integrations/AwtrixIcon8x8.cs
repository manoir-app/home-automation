namespace Home.Agents.Aurore.Integrations
{
    /// <summary>
    /// Énumération des icônes 8x8 disponibles pour AWTRIX
    /// </summary>
    public enum AwtrixIcon8x8
    {
        // Cuisine
        Oven,
        Pan,
        Egg,

        // Sécurité
        DoorOpen,
        DoorClosed,
        WindowOpen,
        WindowClosed,
        Doorbell,
        Alarm,
        Motion,
        Camera,

        // Environnement
        WaterLeak,
        AirQuality,
        Fire,
        TemperatureHigh,
        TemperatureLow,
        Humidity,

        // Énergie
        LightOn,
        LightOff,
        Plug,
        Power,
        BatteryLow,
        Solar,

        // Appareils
        WashingMachine,
        Dryer,
        Freezer,
        Vacuum,
        Coffee,
        Tv,

        // Confort
        Heating,
        Cooling,
        Fan,
        Speaker,
        WaterHeater,

        // Statut
        Ok,
        Warning,
        Error,
        Sync,
        Offline,
        Online,

        // Météo
        Sun,
        Cloud,
        Rain,
        Storm,
        Snow,
        Thermometer
    }

    /// <summary>
    /// Extensions pour AwtrixIcon8x8
    /// </summary>
    public static class AwtrixIcon8x8Extensions
    {
        /// <summary>
        /// Convertit l'énumération en nom de fichier
        /// </summary>
        public static string ToFileName(this AwtrixIcon8x8 icon)
        {
            return icon switch
            {
                // Cuisine
                AwtrixIcon8x8.Oven => "oven",
                AwtrixIcon8x8.Pan => "pan",
                AwtrixIcon8x8.Egg => "egg",

                // Sécurité
                AwtrixIcon8x8.DoorOpen => "door-open",
                AwtrixIcon8x8.DoorClosed => "door-closed",
                AwtrixIcon8x8.WindowOpen => "window-open",
                AwtrixIcon8x8.WindowClosed => "window-closed",
                AwtrixIcon8x8.Doorbell => "doorbell",
                AwtrixIcon8x8.Alarm => "alarm",
                AwtrixIcon8x8.Motion => "motion",
                AwtrixIcon8x8.Camera => "camera",

                // Environnement
                AwtrixIcon8x8.WaterLeak => "water-leak",
                AwtrixIcon8x8.AirQuality => "air-quality",
                AwtrixIcon8x8.Fire => "fire",
                AwtrixIcon8x8.TemperatureHigh => "temperature-high",
                AwtrixIcon8x8.TemperatureLow => "temperature-low",
                AwtrixIcon8x8.Humidity => "humidity",

                // Énergie
                AwtrixIcon8x8.LightOn => "light-on",
                AwtrixIcon8x8.LightOff => "light-off",
                AwtrixIcon8x8.Plug => "plug",
                AwtrixIcon8x8.Power => "power",
                AwtrixIcon8x8.BatteryLow => "battery-low",
                AwtrixIcon8x8.Solar => "solar",

                // Appareils
                AwtrixIcon8x8.WashingMachine => "washing-machine",
                AwtrixIcon8x8.Dryer => "dryer",
                AwtrixIcon8x8.Freezer => "freezer",
                AwtrixIcon8x8.Vacuum => "vacuum",
                AwtrixIcon8x8.Coffee => "coffee",
                AwtrixIcon8x8.Tv => "tv",

                // Confort
                AwtrixIcon8x8.Heating => "heating",
                AwtrixIcon8x8.Cooling => "cooling",
                AwtrixIcon8x8.Fan => "fan",
                AwtrixIcon8x8.Speaker => "speaker",
                AwtrixIcon8x8.WaterHeater => "water-heater",

                // Statut
                AwtrixIcon8x8.Ok => "ok",
                AwtrixIcon8x8.Warning => "warning",
                AwtrixIcon8x8.Error => "error",
                AwtrixIcon8x8.Sync => "sync",
                AwtrixIcon8x8.Offline => "offline",
                AwtrixIcon8x8.Online => "online",

                // Météo
                AwtrixIcon8x8.Sun => "sun",
                AwtrixIcon8x8.Cloud => "cloud",
                AwtrixIcon8x8.Rain => "rain",
                AwtrixIcon8x8.Storm => "storm",
                AwtrixIcon8x8.Snow => "snow",
                AwtrixIcon8x8.Thermometer => "thermometer",

                _ => null
            };
        }

        /// <summary>
        /// Obtient l'icône en Base64
        /// </summary>
        public static string GetBase64(this AwtrixIcon8x8 icon)
        {
            string fileName = icon.ToFileName();
            if (string.IsNullOrEmpty(fileName))
                return null;

            return IconResourceHelper.GetIconBase64(fileName, "8x8");
        }
    }
}
