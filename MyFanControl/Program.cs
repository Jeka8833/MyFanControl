using System;
using System.Windows.Forms;
using MyFanControl.core;

namespace MyFanControl
{
    internal static class Program
    {
        public static TrayMenuGUI? MenuGui;

        [STAThread]
        static void Main()
        {
            Config.Read();
            Core.Init();
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MenuGui = new TrayMenuGUI();
            Application.Run(MenuGui);
        }
    }
}