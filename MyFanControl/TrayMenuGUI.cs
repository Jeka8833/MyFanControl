using System;
using System.Threading;
using System.Windows.Forms;
using MyFanControl.core;
using MyFanControl.Properties;

namespace MyFanControl
{
    public class TrayMenuGUI : ApplicationContext
    {
        public readonly NotifyIcon TrayIcon;

        private readonly MenuItem _stateConnection = new MenuItem(@"Connection reset");

        private GUI _gui;

        public TrayMenuGUI()
        {
            _stateConnection.Enabled = false;
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(_stateConnection);
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add(new MenuItem("Open", OpenWindow));
            contextMenu.MenuItems.Add(new MenuItem("Test fan", TestFan));
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add(new MenuItem("Exit", CloseApplication));
            TrayIcon = new NotifyIcon()
            {
                Icon = Resources.icon,

                ContextMenu = contextMenu,
                Visible = true
            };
            TrayIcon.DoubleClick += OpenWindow;
        }

        public void SetState(bool state) =>
            _stateConnection.Text = state ? @"Connection successfully" : @"Connection reset";

        public void BallonText(string text)
        {
            TrayIcon.ShowBalloonTip(5000, "Fan Control", text, ToolTipIcon.Warning);
        }

        private void TestFan(object sender, EventArgs e)
        {
            Core.IsBlock = true;
            FanControl.SetMotherboardControl(Fan.RamFan, false);
            FanControl.SetSpeed(Fan.RamFan, 50);
            FanControl.TimeStepping = 0;
            FanControl.SendPaket();
            Thread.Sleep(2000);
            FanControl.SetSpeed(Fan.RamFan, 0);
            FanControl.SetMotherboardControl(Fan.RamFan, Config.Setting.RamFan.MotherboardControl);
            FanControl.SendPaket();
            FanControl.TimeStepping = Config.Setting.TimeUpdate;
            Core.IsBlock = false;
        }

        private void OpenWindow(object sender, EventArgs e)
        {
            if (_gui != null && _gui.Visible) return;
            _gui = new GUI();
            _gui.Show();
        }

        private void CloseApplication(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}