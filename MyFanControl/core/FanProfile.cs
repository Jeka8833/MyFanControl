namespace MyFanControl.core
{
    public class FanProfile
    {
        public bool MotherboardControl { get; set; }
        public TempRow[] Profile { get; set; }
    }
}