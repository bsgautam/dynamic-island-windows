using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace DynamicIsland.Services
{
    public class MediaInfo
    {
        public bool IsPlaying { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string AppName { get; set; }
    }

    public class MediaMonitor
    {
        private const byte VK_MEDIA_NEXT_TRACK = 0xB0;
        private const byte VK_MEDIA_PREV_TRACK = 0xB1;
        private const byte VK_MEDIA_PLAY_PAUSE = 0xB3;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private string _lastTitle = "";
        public event Action<MediaInfo> TrackChanged;

        public static void SendPlayPause()
        {
            keybd_event(VK_MEDIA_PLAY_PAUSE, 0, 0, UIntPtr.Zero);
            keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        public static void SendNextTrack()
        {
            keybd_event(VK_MEDIA_NEXT_TRACK, 0, 0, UIntPtr.Zero);
            keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        public static void SendPrevTrack()
        {
            keybd_event(VK_MEDIA_PREV_TRACK, 0, 0, UIntPtr.Zero);
            keybd_event(VK_MEDIA_PREV_TRACK, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        public MediaInfo PollActiveMedia()
        {
            var info = new MediaInfo
            {
                IsPlaying = false,
                Title = "No Track Playing",
                Artist = "Apple Music / Spotify",
                AppName = "Media"
            };

            try
            {
                // Check Spotify
                Process[] spotifyProcesses = Process.GetProcessesByName("Spotify");
                foreach (var p in spotifyProcesses)
                {
                    string title = p.MainWindowTitle;
                    if (!string.IsNullOrEmpty(title) && !title.Equals("Spotify", StringComparison.OrdinalIgnoreCase)
                        && !title.Equals("Spotify Free", StringComparison.OrdinalIgnoreCase)
                        && !title.Equals("Spotify Premium", StringComparison.OrdinalIgnoreCase))
                    {
                        info.IsPlaying = true;
                        info.AppName = "Spotify";
                        ParseArtistTitle(title, info);
                        CheckTrackChange(info);
                        return info;
                    }
                }

                // Check Browsers (YouTube / Web Media)
                string[] browserNames = { "chrome", "msedge", "brave", "firefox" };
                foreach (var name in browserNames)
                {
                    Process[] procs = Process.GetProcessesByName(name);
                    foreach (var p in procs)
                    {
                        string title = p.MainWindowTitle;
                        if (!string.IsNullOrEmpty(title) && (title.Contains("YouTube") || title.Contains("SoundCloud")))
                        {
                            info.IsPlaying = true;
                            info.AppName = name.ToUpper();
                            // Clean up browser title
                            string cleaned = Regex.Replace(title, @" - (YouTube|SoundCloud|Google Chrome|Microsoft​ Edge|Brave).*", "", RegexOptions.IgnoreCase);
                            ParseArtistTitle(cleaned, info);
                            CheckTrackChange(info);
                            return info;
                        }
                    }
                }

                // Check VLC
                Process[] vlcProcs = Process.GetProcessesByName("vlc");
                if (vlcProcs.Length > 0 && !string.IsNullOrEmpty(vlcProcs[0].MainWindowTitle))
                {
                    string title = vlcProcs[0].MainWindowTitle.Replace(" - VLC media player", "").Trim();
                    if (!string.IsNullOrEmpty(title))
                    {
                        info.IsPlaying = true;
                        info.AppName = "VLC";
                        ParseArtistTitle(title, info);
                        CheckTrackChange(info);
                        return info;
                    }
                }
            }
            catch (Exception)
            {
            }

            return info;
        }

        private void ParseArtistTitle(string raw, MediaInfo info)
        {
            if (raw.Contains(" - "))
            {
                var parts = raw.Split(new string[] { " - " }, 2, StringSplitOptions.None);
                info.Artist = parts[0].Trim();
                info.Title = parts[1].Trim();
            }
            else
            {
                info.Title = raw.Trim();
                info.Artist = "Playing Media";
            }
        }

        private void CheckTrackChange(MediaInfo info)
        {
            if (info.IsPlaying && !string.IsNullOrEmpty(info.Title) && info.Title != _lastTitle)
            {
                _lastTitle = info.Title;
                if (TrackChanged != null)
                {
                    TrackChanged(info);
                }
            }
        }
    }
}
