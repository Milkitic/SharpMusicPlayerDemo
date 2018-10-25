using SharpMusicPlayerDemo.Models;
using SharpMusicPlayerDemo.PlayerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharpMusicPlayerDemo
{
    internal static class Program
    {
        #region Field

        private static MusicPlayer _player;
        private static readonly object LockObj = new object();

        #endregion

        [STAThread]
        public static void Main(string[] args)
        {
            Task.Run(() =>
            {
                int preT = 0;
                while (true)
                {
                    Thread.Sleep(5);
                    if (Player == null) continue;
                    if (Player?.PlayerStatus != PlayerStatus.Playing) continue;
                    if (preT == Player.PlayTime) continue;

                    Debug.WriteLine($"{Player.PlayTime}/{Player.Duration}");
                    preT = Player.PlayTime;
                }
            });
            LoadByArgument(args);

            while (true)
            {
                try
                {
                    var str = Console.ReadLine();
                    if (string.IsNullOrEmpty(str))
                        continue;
                    if (str.Equals("load"))
                    {
                        BrowseNewFile();
                    }
                    else if (str.StartsWith("load "))
                    {
                        var value = string.Join(" ", str.Split(' ').Skip(1));
                        if (!File.Exists(value))
                            throw new FileNotFoundException("Cannot locate file.", value);
                        Player?.Dispose();
                        LoadNewFile(value);
                    }
                    else if (str.Equals("play"))
                    {
                        Player.Play();
                    }
                    else if (str.Equals("pause"))
                    {
                        Player.Pause();
                    }
                    else if (str.Equals("play"))
                    {
                        Player.Play();
                    }
                    else if (str.Equals("stop"))
                    {
                        Player.Stop();
                    }
                    else if (str.Equals("exit") || str.Equals("quit"))
                    {
                        Player.Dispose();
                        break;
                    }
                    else if (str.StartsWith("settime "))
                    {
                        var value = int.Parse(str.Split(' ')[1]);
                        Player.SetTime(value);
                    }
                    else if (str.StartsWith("rate "))
                    {
                        var value = float.Parse(str.Split(' ')[1]);
                        Player.RateValue = value;
                    }
                    else if (str.StartsWith("tempo "))
                    {
                        var value = float.Parse(str.Split(' ')[1]);
                        Player.TempoValue = value;
                    }
                    else if (str.StartsWith("pitch "))
                    {
                        var value = float.Parse(str.Split(' ')[1]);
                        Player.PitchValue = value;
                    }
                    else if (str.Equals("dt"))
                    {
                        Reset();
                        Player.TempoValue = 1.5f;
                    }
                    else if (str.Equals("ht"))
                    {
                        Reset();
                        Player.TempoValue = 0.75f;
                    }
                    else if (str.Equals("dc"))
                    {
                        Reset();
                        Player.RateValue = 0.75f;
                    }
                    else if (str.Equals("nc"))
                    {
                        Reset();
                        Player.RateValue = 1.5f;
                    }
                    else if (str.Equals("reset"))
                    {
                        Reset();
                    }
                    else
                        Console.WriteLine($"{str.Split(' ')[0]}: command not found");
                }
                catch (NullReferenceException)
                {
                    BrowseNewFile();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private static void Reset()
        {
            Player.RateValue = 1;
            Player.TempoValue = 1;
            Player.PitchValue = 0;
        }

        private static void LoadByArgument(IReadOnlyList<string> args)
        {
            bool handled = false;
            if (args != null && args.Count == 1)
            {
                try
                {
                    if (File.Exists(args[0]))
                    {
                        LoadNewFile(args[0]);
                        handled = true;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            if (!handled) BrowseNewFile();
        }

        private static void BrowseNewFile()
        {
            var odf = new Microsoft.Win32.OpenFileDialog();
            var result = odf.ShowDialog();
            if (result != true)
                return;
            Player?.Dispose();
            LoadNewFile(odf.FileName);
        }

        private static void LoadNewFile(string path)
        {
            Player = new MusicPlayer(path);
            Player.Play();
        }

        #region Properties

        private static MusicPlayer Player
        {
            get => _player;
            set
            {
                lock (LockObj)
                {
                    _player = value;
                }
            }
        }

        #endregion
    }
}
