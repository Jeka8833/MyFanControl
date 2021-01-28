using System;
using System.Threading;
using OpenHardwareMonitor.Hardware;

namespace MyFanControl.core
{
    public static class Core
    {
        private static readonly Computer Computer = new Computer {CPUEnabled = true};

        private static long _lastTimeReconnect;

        public static bool IsBlock { get; set; }

        public static void Init()
        {
            Computer.Open();
            Reconnect();
            new Thread(ThreadStart) {IsBackground = true}.Start();
        }

        private static void Reconnect()
        {
            try
            {
                if (Config.Setting.Port != null || Config.Setting.Port != "")
                {
                    FanControl.Connect(Config.Setting.Port);
                }
                else
                {
                    string port = FanControl.FindComPort();
                    if (port != "")
                    {
                        FanControl.Connect(port);
                        Config.Setting.Port = port;
                        Config.Write();
                    }
                }
            }
            catch (Exception)
            {
                string port = FanControl.FindComPort();
                if (port != "")
                {
                    try
                    {
                        FanControl.Connect(port);
                        Config.Setting.Port = port;
                        Config.Write();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        }


        private static void ThreadStart()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        if (!IsBlock)
                        {
                            if (FanControl.IsActive())
                            {
                                Program.MenuGui?.SetState(true);
                                int temperature = GetTemperature();
                                FanControl.SetSpeed(Fan.ChipsetFan, Config.Setting.ChipsetFanSpeed);
                                if (Config.Setting.CpuFan.MotherboardControl)
                                {
                                    FanControl.SetMotherboardControl(Fan.CpuFan, true);
                                }
                                else
                                {
                                    FanControl.SetMotherboardControl(Fan.CpuFan, false);
                                    FanControl.SetSpeed(Fan.CpuFan,
                                        GetSpeed(Config.Setting.CpuFan.Profile, temperature));
                                }

                                if (Config.Setting.RamFan.MotherboardControl)
                                {
                                    FanControl.SetMotherboardControl(Fan.RamFan, true);
                                }
                                else
                                {
                                    FanControl.SetMotherboardControl(Fan.RamFan, false);
                                    FanControl.SetSpeed(Fan.RamFan,
                                        GetSpeed(Config.Setting.RamFan.Profile, temperature));
                                }

                                if (!FanControl.SendPaket())
                                {
                                    _lastTimeReconnect = DateTime.Now.Millisecond;
                                    FanControl.Disconnect();
                                }

                                FanControl.TimeStepping = Config.Setting.TimeUpdate;
                            }
                            else
                            {
                                Program.MenuGui?.SetState(false);
                                if (DateTime.Now.Millisecond - _lastTimeReconnect > 60000)
                                {
                                    _lastTimeReconnect = DateTime.Now.Millisecond;
                                    Reconnect();
                                }
                            }
                        }

                        Thread.Sleep(Config.Setting.TimeUpdate);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
            finally
            {
                FanControl.Disconnect();
            }
        }

        private static float GetSpeed(TempRow[] profile, int temp)
        {
            if (profile[0].Temperature > temp)
                return profile[0].Speed;
            for (var i = 1; i < profile.Length; i++)
            {
                if (profile[i].Temperature > temp)
                {
                    return profile[i - 1].Speed + (temp - profile[i - 1].Temperature) *
                        (profile[i].Speed - profile[i - 1].Speed) /
                        (profile[i].Temperature - profile[i - 1].Temperature);
                }
            }

            return profile[profile.Length - 1].Speed;
        }

        private static int GetTemperature()
        {
            var maxTemp = int.MinValue;
            foreach (var hardwareItem in Computer.Hardware)
            {
                if (hardwareItem.HardwareType != HardwareType.CPU) continue;
                hardwareItem.Update();
                foreach (var subHardware in hardwareItem.SubHardware)
                    subHardware.Update();
                foreach (var sensor in hardwareItem.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue)
                    {
                        maxTemp = Math.Max(maxTemp, (int) sensor.Value.Value);
                    }
                }
            }

            return maxTemp;
        }
    }
}