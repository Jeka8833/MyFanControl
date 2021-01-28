using System;
using System.Globalization;
using System.Windows.Forms;
using MyFanControl.core;

namespace MyFanControl
{
    public partial class GUI : Form
    {
        public GUI()
        {
            InitializeComponent();
            UpdateComponents();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                var config = new Config
                {
                    Port = comboBox1.SelectedItem.ToString(),
                    ChipsetFanSpeed = float.Parse(textBox21.Text),
                    TimeUpdate = int.Parse(textBox22.Text),
                    CpuFan = new FanProfile
                    {
                        MotherboardControl = checkBox1.Checked,
                        Profile = new[] {new TempRow(), new TempRow(), new TempRow(), new TempRow(), new TempRow()}
                    },
                    RamFan = new FanProfile
                    {
                        MotherboardControl = checkBox2.Checked,
                        Profile = new[] {new TempRow(), new TempRow(), new TempRow(), new TempRow(), new TempRow()}
                    }
                };
                config.CpuFan.Profile[0].Temperature = int.Parse(textBox1.Text);
                config.CpuFan.Profile[0].Speed = float.Parse(textBox2.Text);
                config.CpuFan.Profile[1].Temperature = int.Parse(textBox4.Text);
                config.CpuFan.Profile[1].Speed = float.Parse(textBox3.Text);
                config.CpuFan.Profile[2].Temperature = int.Parse(textBox6.Text);
                config.CpuFan.Profile[2].Speed = float.Parse(textBox5.Text);
                config.CpuFan.Profile[3].Temperature = int.Parse(textBox8.Text);
                config.CpuFan.Profile[3].Speed = float.Parse(textBox7.Text);
                config.CpuFan.Profile[4].Temperature = int.Parse(textBox10.Text);
                config.CpuFan.Profile[4].Speed = float.Parse(textBox9.Text);
                config.RamFan.Profile[0].Temperature = int.Parse(textBox20.Text);
                config.RamFan.Profile[0].Speed = float.Parse(textBox19.Text);
                config.RamFan.Profile[1].Temperature = int.Parse(textBox18.Text);
                config.RamFan.Profile[1].Speed = float.Parse(textBox17.Text);
                config.RamFan.Profile[2].Temperature = int.Parse(textBox16.Text);
                config.RamFan.Profile[2].Speed = float.Parse(textBox15.Text);
                config.RamFan.Profile[3].Temperature = int.Parse(textBox14.Text);
                config.RamFan.Profile[3].Speed = float.Parse(textBox13.Text);
                config.RamFan.Profile[4].Temperature = int.Parse(textBox12.Text);
                config.RamFan.Profile[4].Speed = float.Parse(textBox11.Text);

                var temp = int.MinValue;
                var speed = float.MinValue;
                foreach (var row in config.CpuFan.Profile)
                {
                    if (row.Speed > 100 || row.Speed < 0)
                        throw new Exception("Incorrect speed");

                    if (row.Speed > speed)
                    {
                        speed = row.Speed;
                    }
                    else
                    {
                        throw new Exception("Incorrect speed");
                    }

                    if (row.Temperature > temp)
                    {
                        temp = row.Temperature;
                    }
                    else
                    {
                        throw new Exception("Incorrect temperature");
                    }
                }

                temp = int.MinValue;
                speed = float.MinValue;
                foreach (var row in config.RamFan.Profile)
                {
                    if (row.Speed > 100 || row.Speed < 0)
                        throw new Exception("Incorrect speed");

                    if (row.Speed > speed)
                    {
                        speed = row.Speed;
                    }
                    else
                    {
                        throw new Exception("Incorrect speed");
                    }

                    if (row.Temperature > temp)
                    {
                        temp = row.Temperature;
                    }
                    else
                    {
                        throw new Exception("Incorrect temperature");
                    }
                }

                Config.Setting = config;
                Config.Write();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Fail apply: " + ex.Message, @"Error");
                UpdateComponents();
            }
        }

        private void UpdateComponents()
        {
            textBox1.Text = Config.Setting.CpuFan.Profile[0].Temperature.ToString();
            textBox2.Text = Config.Setting.CpuFan.Profile[0].Speed.ToString(CultureInfo.CurrentCulture);
            textBox4.Text = Config.Setting.CpuFan.Profile[1].Temperature.ToString();
            textBox3.Text = Config.Setting.CpuFan.Profile[1].Speed.ToString(CultureInfo.CurrentCulture);
            textBox6.Text = Config.Setting.CpuFan.Profile[2].Temperature.ToString();
            textBox5.Text = Config.Setting.CpuFan.Profile[2].Speed.ToString(CultureInfo.CurrentCulture);
            textBox8.Text = Config.Setting.CpuFan.Profile[3].Temperature.ToString();
            textBox7.Text = Config.Setting.CpuFan.Profile[3].Speed.ToString(CultureInfo.CurrentCulture);
            textBox10.Text = Config.Setting.CpuFan.Profile[4].Temperature.ToString();
            textBox9.Text = Config.Setting.CpuFan.Profile[4].Speed.ToString(CultureInfo.CurrentCulture);
            checkBox1.Checked = Config.Setting.CpuFan.MotherboardControl;
            textBox20.Text = Config.Setting.RamFan.Profile[0].Temperature.ToString();
            textBox19.Text = Config.Setting.RamFan.Profile[0].Speed.ToString(CultureInfo.CurrentCulture);
            textBox18.Text = Config.Setting.RamFan.Profile[1].Temperature.ToString();
            textBox17.Text = Config.Setting.RamFan.Profile[1].Speed.ToString(CultureInfo.CurrentCulture);
            textBox16.Text = Config.Setting.RamFan.Profile[2].Temperature.ToString();
            textBox15.Text = Config.Setting.RamFan.Profile[2].Speed.ToString(CultureInfo.CurrentCulture);
            textBox14.Text = Config.Setting.RamFan.Profile[3].Temperature.ToString();
            textBox13.Text = Config.Setting.RamFan.Profile[3].Speed.ToString(CultureInfo.CurrentCulture);
            textBox12.Text = Config.Setting.RamFan.Profile[4].Temperature.ToString();
            textBox11.Text = Config.Setting.RamFan.Profile[4].Speed.ToString(CultureInfo.CurrentCulture);
            checkBox2.Checked = Config.Setting.RamFan.MotherboardControl;

            var ports = FanControl.GetPortList();
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(ports);
            comboBox1.SelectedIndex = Array.IndexOf(ports, Config.Setting.Port);

            textBox21.Text = Config.Setting.ChipsetFanSpeed.ToString(CultureInfo.CurrentCulture);
            textBox22.Text = Config.Setting.TimeUpdate.ToString();
        }
    }
}