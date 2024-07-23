using ExileCore;
using ExileCore.PoEMemory.Components;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
#pragma warning disable CA1416

namespace LevelingHelper
{
    public class LevelingHelper : BaseSettingsPlugin<LevelingHelperSettings>
    {
        private static SoundController _soundController;
        private static readonly object Locker = new object();
        private static DateTime _timeLastPlayed = DateTime.UnixEpoch;
        private static string _lastArea = "";


        #region ExpTable

        private readonly uint[] ExpTable =
        {
            0,
            525,
            1760,
            3781,
            7184,
            12186,
            19324,
            29377,
            43181,
            61693,
            8599,
            117506,
            157384,
            207736,
            269997,
            346462,
            439268,
            551295,
            685171,
            843709,
            1030734,
            1249629,
            1504995,
            1800847,
            2142652,
            2535122,
            2984677,
            3496798,
            4080655,
            4742836,
            5490247,
            6334393,
            7283446,
            8384398,
            9541110,
            10874351,
            12361842,
            14018289,
            15859432,
            17905634,
            20171471,
            22679999,
            25456123,
            28517857,
            31897771,
            35621447,
            39721017,
            44225461,
            49176560,
            54607467,
            60565335,
            67094245,
            74247659,
            82075627,
            90631041,
            99984974,
            110197515,
            121340161,
            133497202,
            146749362,
            161191120,
            176922628,
            194049893,
            212684946,
            232956711,
            255001620,
            278952403,
            304972236,
            333233648,
            363906163,
            397194041,
            433312945,
            472476370,
            514937180,
            560961898,
            610815862,
            664824416,
            723298169,
            786612664,
            855129128,
            929261318,
            1009443795,
            1096169525,
            1189918242,
            1291270350,
            1400795257,
            1519130326,
            1646943474,
            1784977296,
            1934009687,
            2094900291,
            2268549086,
            2455921256,
            2658074992,
            2876116901,
            3111280300,
            3364828162,
            3638186694,
            3932818530,
            4250334444,
        };

        #endregion


        private class Area
        {
            public Area(string name, int level, float levelWarning)
            {
                Name = name;
                Level = level;
                LevelWarning = levelWarning;
            }

            public string Name { get; set; }
            public int Level { get; set; }
            public float LevelWarning { get; set; }
        }

        List<Area> Areas = new List<Area>
        {
            // Act 1
            new Area("The Mud Flats", 4, 3.75f),
            new Area("The Tidal Island", 3, 3.9f),
            new Area("The Submerged Passage", 5, 4.75f),
            new Area("The Ledge", 6, 7.0f),
            new Area("The Climb", 7, 7.5f),
            new Area("The Flooded Depths", 6, 8.0f),
            new Area("The Upper Prison", 9, 9.25f),
            new Area("Prisoner's Gate", 10, 10.0f),
            new Area("The Ship Graveyard", 11, 11.25f),
            new Area("The Cavern of Wrath", 12, 11.75f),
            new Area("The Cavern of Anger", 13, 12.0f),
            // Act 2
            new Area("The Southern Forest", 13, 12.5f),
            new Area("The Old Fields", 14, 13.0f),
            new Area("The Chamber of Sins Level 1", 15, 13.5f),
            new Area("The Chamber of Sins Level 2", 16, 14.0f),
            new Area("The Riverways", 15, 14.5f),
            new Area("The Weaver's Chambers", 18, 15.5f),
            new Area("The Broken Bridge", 16, 16.0f),
            new Area("The Wetlands", 19, 17.0f),
            new Area("The Vaal Ruins", 20, 18.0f),
            new Area("The Northern Forest", 21, 19.5f),
            new Area("The Caverns", 22, 20.0f),
            new Area("The Ancient Pyramid", 23, 21.0f),
            // Act 3
            new Area("The City of Sarn", 23, 21.5f),
            new Area("The Slums", 24, 22.0f),
            new Area("The Crematorium", 25, 22.5f),
            new Area("The Sewers", 26, 23.0f),
            new Area("The Catacombs", 26, 23.5f),
            new Area("The Marketplace", 26, 23.5f),
            new Area("The Battlefront", 27, 24.0f),
            new Area("The Docks", 29, 25.0f),
            new Area("The Solaris Temple Level 1", 27, 25.55f),
            new Area("The Solaris Temple Level 2", 28, 26.0f),
            new Area("The Ebony Barracks", 29, 27.5f),
            new Area("The Lunaris Temple Level 1", 29, 27.75f),
            new Area("The Lunaris Temple Level 2", 30, 28.25f),
            new Area("The Imperial Gardens", 30, 28.5f),
            new Area("The Sceptre of God", 32, 29.75f),
            new Area("The Upper Sceptre of God", 33, 30.25f),
            // Act 4
            new Area("The Aqueduct", 33, 31.0f),
            new Area("The Dried Lake", 34, 31.75f),
            new Area("The Mines Level 1", 34, 32.0f),
            new Area("The Mines Level 2", 35, 32.5f),
            new Area("The Crystal Veins", 36, 33.0f),
            new Area("Daresso's Dream", 37, 33.4f),
            new Area("The Grand Arena", 38, 33.75f),
            new Area("Kaom's Dream", 37, 34.0f),
            new Area("Kaom's Stronghold", 38, 34.0f),
            new Area("The Belly of the Beast level 1", 38, 34.75f),
            new Area("The Belly of the Beast level 2", 39, 35.0f),
            new Area("The Harvest", 40, 35.5f),
            new Area("The Ascent", 40, 36.0f),
            // Act 5
            new Area("The Slave Pens", 41, 36.5f),
            new Area("The Control Blocks", 41, 37.0f),
            new Area("Oriath Square", 42, 37.25f),
            new Area("The Templar Courts", 42, 37.75f),
            new Area("The Chamber of Innocence", 43, 40.25f),
            new Area("The Torched Courts", 44, 40.75f),
            new Area("The Ossuary", 44, 41.0f),
            new Area("The Ruined Square", 44, 41.25f),
            new Area("The Reliquary", 44, 41.75f),
            new Area("The Cathedral Rooftop", 45, 42.0f),
            // Act 6
            new Area("The Coast", 45, 42.1f),
            new Area("The Mud Flats", 46, 42.2f),
            new Area("The Karui Fortress", 46, 42.35f),
            new Area("The Ridge", 46, 42.75f),
            new Area("The Lower Prison", 47, 43.0f),
            new Area("Shavronne's Tower", 47, 43.5f),
            new Area("The Western Forest", 48, 44.0f),
            new Area("Prisoner's Gate", 47, 44.25f),
            new Area("The Wetlands", 48, 44.25f),
            new Area("The Riverways", 48, 44.5f),
            new Area("The Southern Forest", 49, 44.75f),
            new Area("The Cavern of Anger", 49, 45.0f),
            new Area("The Beacon", 49, 45.5f),
            new Area("The Brine King's Reef", 50, 46.25f),
            // Act 7
            new Area("The Broken Bridge", 50, 46.5f),
            new Area("The Fellshrine Ruins", 51, 46.75f),
            new Area("The Crypt", 51, 47.0f),
            new Area("Maligaro's Sanctum", 52, 47.5f),
            new Area("The Chamber of Sins Level 1", 52, 47.5f),
            new Area("The Chamber of Sins Level 2", 52, 47.9f),
            new Area("The Den", 53, 48.0f),
            new Area("The Ashen Fields", 53, 48.25f),
            new Area("The Northern Forest", 53, 48.5f),
            new Area("The Causeway", 54, 49.0f),
            new Area("The Vaal City", 54, 49.25f),
            new Area("The Dread Thicket", 53, 49.75f),
            new Area("The Temple of Decay Level 1", 54, 50.5f),
            new Area("The Temple of Decay Level 2", 55, 51.0f),
            // Act 8
            new Area("The Sarn Ramparts", 55, 51.25f),
            new Area("The Toxic Conduits", 56, 51.75f),
            new Area("Doedre's Cesspool", 56, 52.0f),
            new Area("The Quay", 57, 52.75f),
            new Area("The Grain Gate", 57, 53.0f),
            new Area("The Imperial Fields", 58, 53.1f),
            new Area("The Solaris Temple Level 1", 59, 53.35f),
            new Area("The Solaris Temple Level 2", 59, 53.5f),
            new Area("The Solaris Concourse", 58, 53.6f),
            new Area("The Lunaris Concourse", 58, 54.0f),
            new Area("The Lunaris Temple Level 1", 59, 54.25f),
            new Area("The Lunaris Temple Level 2", 59, 54.75f),
            new Area("The Bath House", 57, 54.85f),
            new Area("The High Gardens", 58, 55.0f),
            new Area("The Harbour Bridge", 60, 55.25f),
            // Act 9
            new Area("The Blood Aqueduct", 61, 60.0f),
            new Area("The Descent", 61, 60.25f),
            new Area("The Vastiri Desert", 61, 60.5f),
            new Area("The Boiling Lake", 62, 60.75f),
            new Area("The Tunnel", 62, 61.0f),
            new Area("The Refinery", 63, 61.25f),
            new Area("The Belly of the Beast", 63, 61.5f),
            new Area("The Rotting Core", 64, 62.0f),
            // Act 10
            new Area("The Cathedral Rooftop", 64, 62.25f),
            new Area("The Ravaged Square", 64, 62.75f),
            new Area("The Torched Courts", 65, 63.25f),
            new Area("The Desecrated Chambers", 65, 63.5f),
            new Area("The Canals", 66, 63.75f),
            new Area("The Feeding Trough", 67, 64.0f),
        };

        public override bool Initialise()
        {
            base.Initialise();
            lock (Locker) _soundController = GameController.SoundController;

            // load areas from file
            string path = Path.Combine(DirectoryFullName, "AreaInfo.txt");
            if (File.Exists(path))
            {
                LoadConfig(path);
            }
            else
            {
                SaveConfig(path);
            }

            return true;
        }

        private void SaveConfig(string path)
        {
            using (StreamWriter file = File.CreateText(path))
            {
                string json = JsonConvert.SerializeObject(Areas, Formatting.Indented);

                file.Write(json);
            }
        }

        private void LoadConfig(string path)
        {
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                Areas = JsonConvert.DeserializeObject<List<Area>>(json);
            }
        }

        private double GetExpPct(int Level, uint Exp)
        {
            if (Level >= 100) return 0.0f;
            uint LevelStartExp = ExpTable[Level - 1];
            uint ExpNeededForNextLevel = ExpTable[Level];

            uint CurrLevelExp = Exp - LevelStartExp;
            uint NextLevelExp = ExpNeededForNextLevel - LevelStartExp;
            double LevelPct = (double)CurrLevelExp / NextLevelExp;
            return LevelPct;
        }

        private double ExpPct(int level, int areaLevel)
        {
            int effDiff = Math.Abs(areaLevel - level) - 3 - level / 16;
            if (effDiff <= 0) return 1.0;
            return Math.Pow((level + 5) / (level + 5 + Math.Pow(effDiff, 2.5)), 1.5);
        }

        public override void Render()
        {
            if (!Settings.Enable)
            {
                return;
            }
            var currentArea = GameController.Area.CurrentArea;
            bool areaChanged = _lastArea != currentArea.Name;
            if (Settings.Debug) LogMessage($"Current Area: {currentArea.Name}");
            if (Settings.Debug) LogMessage($"Last Area: {_lastArea}");
            // reset timer if area is changed
            if (areaChanged)
            {
                _lastArea = currentArea.Name;
                if (Settings.Debug) LogMessage("Area changed, skipping processing");
                _timeLastPlayed = DateTime.UnixEpoch;
            }
            // no need to process if we haven't waited long enough already
            if ((DateTime.Now - _timeLastPlayed).TotalSeconds < Settings.AudioDelay.Value)
            {
                if (Settings.Debug) LogMessage("Audio Delay not hit yet, skipping processing");
                return;
            }
            var player = GameController.Player.GetComponent<Player>();
            double playerLevel = player.Level + Math.Round(GetExpPct(player.Level, player.XP), 2);
            if (Settings.Debug) LogMessage($"Player Level: {playerLevel}");
            Area area = Areas.Find(area => area.Name == currentArea.Name && area.Level == currentArea.RealLevel);
            if (area != null)
            {
                float levelWarning = area.LevelWarning;

                if (levelWarning < playerLevel)
                {
                    _soundController.PlaySound(Path.Combine(DirectoryFullName, "stop killing").Replace('\\', '/'));
                    _timeLastPlayed = DateTime.Now;
                    if (Settings.Debug) LogMessage("Stop killing!");
                }
                else if(area.Level > player.Level && ExpPct(player.Level, area.Level) < Settings.ExpPenaltyWarning / 100)
                {
                    _soundController.PlaySound(Path.Combine(DirectoryFullName, "keep killing").Replace('\\', '/'));
                    _timeLastPlayed = DateTime.Now;
                    if (Settings.Debug) LogMessage("Kill more!");
                }
            }
        }
    }
}