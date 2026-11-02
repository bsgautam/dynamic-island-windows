using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace DynamicIslandLauncher
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string mainExe = Path.Combine(dir, "DynamicIsland.exe");
            if (File.Exists(mainExe))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = mainExe,
                    Arguments = "--settings",
                    WorkingDirectory = dir
                });
            }
            else
            {
                MessageBox.Show("Could not find DynamicIsland.exe in: " + dir, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
