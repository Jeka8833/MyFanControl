using System;
using System.IO;
using System.Windows.Forms;
using Utf8Json;

namespace MyFanControl.core
{
    public class Config
    {
        public string Port { get; set; }
        public float ChipsetFanSpeed { get; set; }
        public int TimeUpdate { get; set; }
        public FanProfile CpuFan { get; set; }
        public FanProfile RamFan { get; set; }

        public string PathApp1 { get; set; }
        public string PathApp2 { get; set; }

        public static Config Setting = DefaultConfig();

        public static readonly string FolderPath =
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
            "\\FanControl\\";

        private const string FileName = "config.json";

        public static void Read()
        {
            try
            {
                Setting = JsonSerializer.Deserialize<Config>(File.ReadAllBytes(FolderPath + FileName));
            }
            catch (Exception)
            {
                Write();
            }
        }

        public static void Write()
        {
            try
            {
                Directory.CreateDirectory(FolderPath);
                File.WriteAllBytes(FolderPath + FileName, JsonSerializer.Serialize(Setting));
            }
            catch (Exception)
            {
                MessageBox.Show(@"Fail write or create config", @"Error");
            }
        }

        public static Config DefaultConfig()
        {
            var config = new Config
            {
                Port = "",
                ChipsetFanSpeed = 30,
                TimeUpdate = 3000,
                CpuFan = new FanProfile {MotherboardControl = false, Profile = new TempRow[5]},
                RamFan = new FanProfile {MotherboardControl = false, Profile = new TempRow[5]},
                PathApp1 = "",
                PathApp2 = ""
            };
            config.CpuFan.Profile[0] = new TempRow {Temperature = 40, Speed = 0};
            config.CpuFan.Profile[1] = new TempRow {Temperature = 55, Speed = 30};
            config.CpuFan.Profile[2] = new TempRow {Temperature = 77, Speed = 60};
            config.CpuFan.Profile[3] = new TempRow {Temperature = 80, Speed = 75};
            config.CpuFan.Profile[4] = new TempRow {Temperature = 90, Speed = 100};

            config.RamFan.Profile[0] = new TempRow {Temperature = 40, Speed = 0};
            config.RamFan.Profile[1] = new TempRow {Temperature = 55, Speed = 30};
            config.RamFan.Profile[2] = new TempRow {Temperature = 77, Speed = 60};
            config.RamFan.Profile[3] = new TempRow {Temperature = 80, Speed = 75};
            config.RamFan.Profile[4] = new TempRow {Temperature = 90, Speed = 100};
            return config;
        }
    }
}