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
    class SettingsManager
    {
        public double WindowWidth { get; set; }
        public double WindowHeight { get; set; }
        public double WindowX { get; set; }
        public double WindowY { get; set; }
        public bool Paused { get; set; }
        public string SaveLocation { get; set; }

        public SettingsManager(string saveLocation)
        {
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
            w.WriteLine(WindowWidth.ToString());
            w.WriteLine(WindowHeight.ToString());
            w.WriteLine(WindowX.ToString());
            w.WriteLine(WindowY.ToString());
            w.WriteLine(Paused.ToString());
            w.Close();
        }

        public void Load()
        {
            StreamReader r = new StreamReader(File.Open(SaveLocation, FileMode.Open));

            try
            {
                WindowWidth = Convert.ToDouble(r.ReadLine());
                WindowHeight = Convert.ToDouble(r.ReadLine());
                WindowX = Convert.ToDouble(r.ReadLine());
                WindowY = Convert.ToDouble(r.ReadLine());
                Paused = Convert.ToBoolean(r.ReadLine());
            }
            catch
            {
                r.Close();
                if (File.Exists(SaveLocation + ".bak"))
                    File.Delete(SaveLocation + ".bak");
                File.Move(SaveLocation, SaveLocation + ".bak");
                Reset();
                Save();
                MessageBox.Show("The settings file (" + SaveLocation + ") could not be read, a clean one has been created and the old one renamed.", "Settings file corrupted", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            r.Close();
        }

        public void Reset()
        {
            WindowWidth = 0;
            WindowHeight = 0;
            WindowX = 0;
            WindowY = 0;
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
