using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace GTAOBusinesses
{
    public class SettingsManager
    {
        private enum Settings : int
        {
            Width,
            Height,
            X,
            Y,
            Paused,
            PauseOnClose,
            AFKKey
        }
        public double WindowWidth
        {
            get
            {
                return (double)settings[(int)Settings.Width];
            }
            set
            {
                settings[(int)Settings.Width] = value;
            }
        }
        public double WindowHeight
        {
            get
            {
                return (double)settings[(int)Settings.Height];
            }
            set
            {
                settings[(int)Settings.Height] = value;
            }
        }
        public double WindowX
        {
            get
            {
                return (double)settings[(int)Settings.X];
            }
            set
            {
                settings[(int)Settings.X] = value;
            }
        }
        public double WindowY
        {
            get
            {
                return (double)settings[(int)Settings.Y];
            }
            set
            {
                settings[(int)Settings.Y] = value;
            }
        }
        public bool Paused
        {
            get
            {
                return (bool)settings[(int)Settings.Paused];
            }
            set
            {
                settings[(int)Settings.Paused] = value;
            }
        }
        public bool PauseOnClose
        {
            get
            {
                return (bool)settings[(int)Settings.PauseOnClose];
            }
            set
            {
                settings[(int)Settings.PauseOnClose] = value;
            }
        }
        public int AFKKey
        {
            get
            {
                return (int)settings[(int)Settings.AFKKey];
            }
            set
            {
                settings[(int)Settings.AFKKey] = value;
            }
        }

        public string SaveLocation { get; set; }


        private readonly List<object> settings = new List<object>();
        private readonly List<object> defaults = new List<object>();
        private readonly List<Type> types = new List<Type>();

        public SettingsManager(string saveLocation)
        {
            types.Add(typeof(double));
            types.Add(typeof(double));
            types.Add(typeof(double));
            types.Add(typeof(double));
            types.Add(typeof(bool));
            types.Add(typeof(bool));
            types.Add(typeof(int));

            defaults.Add(0.0d);
            defaults.Add(0.0d);
            defaults.Add(0.0d);
            defaults.Add(0.0d);
            defaults.Add(false);
            defaults.Add(true);
            defaults.Add(0x4e);  // NumPad+ (Add)

            Reset();

            SaveLocation = saveLocation;

            if (!File.Exists(saveLocation))
            {
                Save();
            }
            else
            {
                Load();
            }
        }

        public void Save()
        {
            StreamWriter w = new StreamWriter(SaveLocation, false);
            for (int i = 0; i < settings.Count; i++)
            {
                w.WriteLine(Convert.ChangeType(settings[i], types[i]));
            }
            w.Close();
        }

        public void Load()
        {
            StreamReader r = new StreamReader(File.Open(SaveLocation, FileMode.Open));
            bool error = false;

            for (int i = 0; i < settings.Count; i++)
            { 
                try
                {
                    settings[i] = Convert.ChangeType(r.ReadLine(), types[i]);
                }
                catch
                {
                    error = true;
                    settings[i] = defaults[i];
                }
            }

            r.Close();

            if (error)
            {
                if (File.Exists(SaveLocation + ".bak"))
                    File.Delete(SaveLocation + ".bak");
                File.Move(SaveLocation, SaveLocation + ".bak");
                Save();
                MessageBox.Show(string.Format("Some settings ({0}) could not be loaded and have been reset to their default setting! A backup has been created.", SaveLocation), "Settings corrupted", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void Reset()
        {
            settings.Clear();
            defaults.ForEach(x => settings.Add(x));
        }

        public void RestoreWindowDimensions(Window win)
        {
            if (WindowWidth >= win.MinWidth && WindowWidth <= win.MaxWidth)
                win.Width = WindowWidth;
            if (WindowHeight >= win.MinHeight && WindowHeight <= win.MaxHeight)
                win.Height = WindowHeight;
        }

        public void RestoreWindowLocation(Window win)
        {
            win.Left = WindowX;
            win.Top = WindowY;
        }

        public void SetWindowDimensions(Window win)
        {
            WindowWidth = win.Width;
            WindowHeight = win.Height;
        }

        public void SetWindowLocation(Window win)
        {
            WindowX = win.Left;
            WindowY = win.Top;
        }
    }
}
