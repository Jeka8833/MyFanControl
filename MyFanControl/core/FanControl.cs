using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace MyFanControl.core
{
    public static class FanControl
    {
        public static int TimeStepping { set; get; }

        private static SerialPort _port;

        private static bool _fan1Auto = true;
        private static bool _fan2Auto = true;

        private static ushort _speedFan0;
        private static ushort _speedFan1;
        private static ushort _speedFan2;

        public static void Connect(string? port)
        {
            _port = new SerialPort {PortName = port, BaudRate = 9600, DataBits = 8, ReadTimeout = 1000};
            _port.Open();
            if (SendPaket(new byte[] {0x00})) return;
            Disconnect();
            throw new Exception("Fail Handshake");
        }

        public static void Disconnect() => _port?.Close();

        public static bool IsActive() => _port != null && _port.IsOpen;

        public static string[] GetPortList() => SerialPort.GetPortNames();

        public static string FindComPort()
        {
            foreach (var port in GetPortList())
            {
                try
                {
                    Connect(port);
                    return port;
                }
                catch (Exception)
                {
                    // ignored
                }
                finally
                {
                    Disconnect();
                }
            }

            return "";
        }

        public static void SetSpeed(Fan fan, float speed)
        {
            if (speed < 0 || speed > 100)
                throw new Exception("Incorrect speed");
            switch (fan)
            {
                case Fan.ChipsetFan:
                    _speedFan0 = (ushort) (speed * 3.2f);
                    break;
                case Fan.CpuFan:
                    _speedFan1 = speed == 0 ? (ushort) 0 : (ushort) (49 + speed * 2.06f);
                    break;
                case Fan.RamFan:
                    _speedFan2 = speed == 0 ? (ushort) 0 : (ushort) (60 + speed * 2.6f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fan), fan, null);
            }
        }

        public static void SetMotherboardControl(Fan fan, bool state)
        {
            switch (fan)
            {
                case Fan.CpuFan:
                    _fan1Auto = state;
                    break;
                case Fan.RamFan:
                    _fan2Auto = state;
                    break;
                case Fan.ChipsetFan:
                    throw new Exception("Fan not supported this operation");
                default:
                    throw new ArgumentOutOfRangeException(nameof(fan), fan, null);
            }
        }

        public static bool SendPaket()
        {
            if (!IsActive()) return false;

            var array = new List<byte> {0x01};
            array.AddRange(ToBytes(_speedFan0));
            if (_fan1Auto)
            {
                array.Add(0xFF);
                array.Add(0xFF);
            }
            else
            {
                array.AddRange(ToBytes(_speedFan1));
            }

            if (_fan2Auto)
            {
                array.Add(0xFF);
                array.Add(0xFF);
            }
            else
            {
                array.AddRange(ToBytes(_speedFan2));
            }

            array.AddRange(ToBytes((ushort) TimeStepping));
            return SendPaket(array.ToArray());
        }

        private static bool SendPaket(byte[] data)
        {
            for (int i = 0; i < 4; i++)
            {
                byte sum = 0;
                for (int j = 0; j < data.Length; j++)
                    sum += data[j];
                var outs = new List<byte>(data.Length + 2)
                {
                    0xAA, (byte) (((((sum & 0x0F) ^ ((sum & 0xF0) >> 4)) & 0xF) | (data.Length << 4)) & 0xFF)
                };
                outs.AddRange(data);
                try
                {
                    _port.Write(outs.ToArray(), 0, outs.Count());
                    if (_port.ReadByte() == 0xFF)
                        return true;
                    Thread.Sleep(500);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return false;
        }

        private static byte[] ToBytes(ushort value) => new[] {(byte) ((value & 0xFF00) >> 8), (byte) (value & 0xFF)};
    }

    public enum Fan
    {
        ChipsetFan,
        CpuFan,
        RamFan
    }
}