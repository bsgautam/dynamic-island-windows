using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace DynamicIsland.Services
{
    public class SystemMetrics
    {
        public int CpuUsage { get; set; }
        public int RamUsage { get; set; }
        public double RamUsedGb { get; set; }
        public double RamTotalGb { get; set; }
        public TimeSpan Uptime { get; set; }
    }

    public class SystemMonitor
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetSystemTimes(out System.Runtime.InteropServices.ComTypes.FILETIME lpIdleTime,
                                                  out System.Runtime.InteropServices.ComTypes.FILETIME lpKernelTime,
                                                  out System.Runtime.InteropServices.ComTypes.FILETIME lpUserTime);

        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        private ulong _lastIdle;
        private ulong _lastKernel;
        private ulong _lastUser;
        private int _cachedCpu = 0;

        public SystemMonitor()
        {
            InitCpuTimes();
        }

        private void InitCpuTimes()
        {
            System.Runtime.InteropServices.ComTypes.FILETIME idle, kernel, user;
            if (GetSystemTimes(out idle, out kernel, out user))
            {
                _lastIdle = FileTimeToUInt64(idle);
                _lastKernel = FileTimeToUInt64(kernel);
                _lastUser = FileTimeToUInt64(user);
            }
        }

        private static ulong FileTimeToUInt64(System.Runtime.InteropServices.ComTypes.FILETIME ft)
        {
            return ((ulong)ft.dwHighDateTime << 32) | (uint)ft.dwLowDateTime;
        }

        public SystemMetrics GetCurrentMetrics()
        {
            var metrics = new SystemMetrics();

            // CPU
            System.Runtime.InteropServices.ComTypes.FILETIME idleTime, kernelTime, userTime;
            if (GetSystemTimes(out idleTime, out kernelTime, out userTime))
            {
                ulong currentIdle = FileTimeToUInt64(idleTime);
                ulong currentKernel = FileTimeToUInt64(kernelTime);
                ulong currentUser = FileTimeToUInt64(userTime);

                ulong idleDiff = currentIdle - _lastIdle;
                ulong kernelDiff = currentKernel - _lastKernel;
                ulong userDiff = currentUser - _lastUser;
                ulong totalDiff = kernelDiff + userDiff;

                if (totalDiff > 0)
                {
                    double cpu = 100.0 * (1.0 - ((double)idleDiff / (double)totalDiff));
                    if (cpu < 0) cpu = 0;
                    if (cpu > 100) cpu = 100;
                    _cachedCpu = (int)Math.Round(cpu);
                }

                _lastIdle = currentIdle;
                _lastKernel = currentKernel;
                _lastUser = currentUser;
            }
            metrics.CpuUsage = _cachedCpu;

            // RAM
            var memStatus = new MEMORYSTATUSEX { dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX)) };
            if (GlobalMemoryStatusEx(ref memStatus))
            {
                metrics.RamUsage = (int)memStatus.dwMemoryLoad;
                double total = memStatus.ullTotalPhys / (1024.0 * 1024.0 * 1024.0);
                double avail = memStatus.ullAvailPhys / (1024.0 * 1024.0 * 1024.0);
                metrics.RamTotalGb = Math.Round(total, 1);
                metrics.RamUsedGb = Math.Round(total - avail, 1);
            }

            // Uptime
            metrics.Uptime = TimeSpan.FromMilliseconds(Environment.TickCount & int.MaxValue);

            return metrics;
        }
    }
}
