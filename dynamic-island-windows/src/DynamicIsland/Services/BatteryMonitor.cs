using System;
using System.Windows.Forms;

namespace DynamicIsland.Services
{
    public class BatteryState
    {
        public int Percent { get; set; }
        public bool IsCharging { get; set; }
        public bool HasBattery { get; set; }
    }

    public class BatteryMonitor
    {
        public event Action<BatteryState, string> BatteryEventTriggered;

        private bool? _lastCharging = null;
        private int _lastPercent = -1;
        private bool _warnedLow = false;

        public BatteryState GetCurrentState()
        {
            var state = new BatteryState();
            try
            {
                PowerStatus power = SystemInformation.PowerStatus;
                if (power.BatteryChargeStatus == BatteryChargeStatus.NoSystemBattery)
                {
                    state.HasBattery = false;
                    state.Percent = 100;
                    state.IsCharging = true;
                }
                else
                {
                    state.HasBattery = true;
                    int pct = (int)Math.Round(power.BatteryLifePercent * 100.0);
                    if (pct < 0) pct = 0;
                    if (pct > 100) pct = 100;
                    state.Percent = pct;
                    state.IsCharging = (power.PowerLineStatus == PowerLineStatus.Online);
                }
            }
            catch (Exception)
            {
                state.HasBattery = false;
                state.Percent = 100;
                state.IsCharging = true;
            }
            return state;
        }

        public void CheckForEvents()
        {
            var current = GetCurrentState();
            if (!current.HasBattery) return;

            if (_lastCharging.HasValue)
            {
                // Plugged in transition
                if (!_lastCharging.Value && current.IsCharging)
                {
                    if (BatteryEventTriggered != null)
                        BatteryEventTriggered(current, "Connected to Power");
                }
                // Unplugged transition
                else if (_lastCharging.Value && !current.IsCharging)
                {
                    if (BatteryEventTriggered != null)
                        BatteryEventTriggered(current, "On Battery Power");
                }

                // Low battery check (< 20%)
                if (!current.IsCharging && current.Percent <= 20 && !_warnedLow)
                {
                    _warnedLow = true;
                    if (BatteryEventTriggered != null)
                        BatteryEventTriggered(current, "Low Battery Warning");
                }
                else if (current.Percent > 25)
                {
                    _warnedLow = false;
                }
            }

            _lastCharging = current.IsCharging;
            _lastPercent = current.Percent;
        }
    }
}
