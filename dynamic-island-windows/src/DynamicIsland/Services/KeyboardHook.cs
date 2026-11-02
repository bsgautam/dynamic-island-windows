using System;
using System.Runtime.InteropServices;

namespace DynamicIsland.Services
{
    public class KeyboardHook
    {
        private const int VK_CAPITAL = 0x14;

        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        [DllImport("winmm.dll")]
        private static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);

        private bool _lastCapsLock;
        private int _lastVolume = -1;

        public event Action<bool> CapsLockChanged;
        public event Action<int> VolumeChanged;

        public KeyboardHook()
        {
            _lastCapsLock = IsCapsLockActive();
            _lastVolume = GetSystemVolume();
        }

        public static bool IsCapsLockActive()
        {
            return (GetKeyState(VK_CAPITAL) & 1) != 0;
        }

        public static int GetSystemVolume()
        {
            try
            {
                uint vol;
                if (waveOutGetVolume(IntPtr.Zero, out vol) == 0)
                {
                    ushort left = (ushort)(vol & 0xFFFF);
                    return (int)Math.Round((left / 65535.0) * 100.0);
                }
            }
            catch (Exception)
            {
            }
            return 50;
        }

        public void CheckState()
        {
            // CapsLock
            bool currentCapsLock = IsCapsLockActive();
            if (currentCapsLock != _lastCapsLock)
            {
                _lastCapsLock = currentCapsLock;
                if (CapsLockChanged != null)
                {
                    CapsLockChanged(currentCapsLock);
                }
            }

            // Volume
            int currentVolume = GetSystemVolume();
            if (_lastVolume >= 0 && Math.Abs(currentVolume - _lastVolume) > 1)
            {
                _lastVolume = currentVolume;
                if (VolumeChanged != null)
                {
                    VolumeChanged(currentVolume);
                }
            }
            else
            {
                _lastVolume = currentVolume;
            }
        }
    }
}
