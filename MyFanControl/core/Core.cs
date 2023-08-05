using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Win32;
using OpenHardwareMonitor.Hardware;

namespace MyFanControl.core
{
    public static class Core
    {
        private static readonly Computer Computer = new() { CPUEnabled = true };

        private static long _lastTimeReconnect;
        private static readonly int[] LastTemps = new int[5];

        public static bool IsBlock { get; set; }

        public static void Init()
        {
            Computer.Open();
            Reconnect();

            new Thread(ThreadStart) { IsBackground = true }.Start();
            new Thread(AppStart) { IsBackground = true }.Start();

            SystemEvents.PowerModeChanged += OnPowerChange;
        }

        private static void Reconnect()
        {
            try
            {
                if (Config.Setting.Port != "")
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
                                var temperature = GetTemperature();

                                Array.Copy(LastTemps, 1,
                                    LastTemps, 0, LastTemps.Length - 1);
                                LastTemps[LastTemps.Length - 1] = temperature;

                                var maxTemp = int.MinValue;
                                foreach (var value in LastTemps)
                                {
                                    maxTemp = Math.Max(maxTemp, value);
                                }
                                
                                FanControl.SetSpeed(Fan.ChipsetFan, Config.Setting.ChipsetFanSpeed);
                                if (Config.Setting.CpuFan.MotherboardControl)
                                {
                                    FanControl.SetMotherboardControl(Fan.CpuFan, true);
                                }
                                else
                                {
                                    FanControl.SetMotherboardControl(Fan.CpuFan, false);
                                    FanControl.SetSpeed(Fan.CpuFan,
                                        GetSpeed(Config.Setting.CpuFan.Profile, maxTemp));
                                }

                                if (Config.Setting.RamFan.MotherboardControl)
                                {
                                    FanControl.SetMotherboardControl(Fan.RamFan, true);
                                }
                                else
                                {
                                    FanControl.SetMotherboardControl(Fan.RamFan, false);
                                    FanControl.SetSpeed(Fan.RamFan,
                                        GetSpeed(Config.Setting.RamFan.Profile, maxTemp));
                                }

                                if (!FanControl.SendPaket())
                                {
                                    Program.MenuGui?.BalloonText("Fail send paket");
                                    _lastTimeReconnect = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                                    FanControl.Disconnect();
                                }

                                FanControl.TimeStepping = Config.Setting.TimeUpdate;
                            }
                            else
                            {
                                Program.MenuGui?.SetState(false);
                                if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - _lastTimeReconnect > 30000)
                                {
                                    _lastTimeReconnect = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                                    Program.MenuGui?.BalloonText("Reconnecting...");
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

                foreach (var sensor in hardwareItem.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue)
                    {
                        maxTemp = Math.Max(maxTemp, (int)Math.Ceiling(sensor.Value.Value));
                    }
                }
            }

            return maxTemp;
        }

        private static void OnPowerChange(object s, PowerModeChangedEventArgs e)
        {
            try
            {
                if (e.Mode == PowerModes.Resume)
                    new Thread(AppStart) { IsBackground = true }.Start();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private static void AppStart()
        {
            try
            {
                Process app1 = new Process { StartInfo = { FileName = Config.Setting.PathApp1 } };
                app1.Start();

                Process app2 = new Process { StartInfo = { FileName = Config.Setting.PathApp2 } };
                app2.Start();

                Thread.Sleep(120000);

                app1.Kill();
                app2.Kill();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}