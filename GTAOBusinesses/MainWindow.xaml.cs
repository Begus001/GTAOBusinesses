using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;

namespace GTAOBusinesses
{
    [Flags]
    public enum ThreadAccess : int
    {
        TERMINATE = (0x0001),
        SUSPEND_RESUME = (0x0002),
        GET_CONTEXT = (0x0008),
        SET_CONTEXT = (0x0010),
        SET_INFORMATION = (0x0020),
        QUERY_INFORMATION = (0x0040),
        SET_THREAD_TOKEN = (0x0080),
        IMPERSONATE = (0x0100),
        DIRECT_IMPERSONATION = (0x0200)
    }

    public partial class MainWindow : Window
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        private static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        private static extern int ResumeThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr handle);

        private readonly Version version = new Version("1.6.1");

        private readonly string stateDir = @"C:\Users\" + Environment.UserName + @"\AppData\Roaming\GTAOBusinesses\";
        private const string stateFilename = "state.txt";
        private const string settingsFilename = "settings.txt";
        private const string keymapFilename = "keymap.txt";
        private readonly string statePath;
        private readonly string settingsPath;
        private readonly string keymapPath;
        private bool settingsOpen = false;

        private readonly SettingsManager settings;

        private const int numBusinesses = 4;

        private const int bunkerSup = 2 * 3600 + 24 * 60;
        private const int cocaineSup = 2 * 3600;
        private const int methSup = 2 * 3600 + 24 * 60;
        private const int counterSup = 2 * 3600 + 40 * 60;

        private const int bunkerProd = bunkerSup * 5;
        private const int cocaineProd = (int)(cocaineSup * 2.5);
        private const int methProd = (int)(methSup * 2.5);
        private const int counterProd = counterSup * 2;

        private readonly Business[] businesses = new Business[numBusinesses];

        private readonly Timer saveTimer = new Timer(1000);

        private readonly HotkeyManager hotkeyManager;

        bool suspended = false;
        System.Threading.Thread suspendThread;

        public static double val = -1.0d;

        private bool isGTAOpen = false;

        public MainWindow()
        {
            InitializeComponent();

            statePath = stateDir + stateFilename;
            settingsPath = stateDir + settingsFilename;
            keymapPath = stateDir + keymapFilename;

            businesses[0] = new Business(bunkerSup, bunkerProd, pbSupBunker, pbProdBunker, lbSupBunker, lbProdBunker, btResupplyBunker, btSellBunker);
            businesses[1] = new Business(cocaineSup, cocaineProd, pbSupCocaine, pbProdCocaine, lbSupCocaine, lbProdCocaine, btResupplyCocaine, btSellCocaine);
            businesses[2] = new Business(methSup, methProd, pbSupMeth, pbProdMeth, lbSupMeth, lbProdMeth, btResupplyMeth, btSellMeth);
            businesses[3] = new Business(counterSup, counterProd, pbSupCounterfeit, pbProdCounterfeit, lbSupCounterfeit, lbProdCounterfeit, btResupplyCounterfeit, btSellCounterfeit);

            btPause.Background = Brushes.MediumSeaGreen;

            saveTimer.Elapsed += tick;
            saveTimer.AutoReset = true;
            saveTimer.Start();

            load();

            settings = new SettingsManager(settingsPath);
            settings.RestoreWindowDimensions(this);
            settings.RestoreWindowLocation(this);

            hotkeyManager = new HotkeyManager(this, keymapPath);

            hotkeyManager.HotkeyPressed += globalKeyHandler;

            tick(null, null);

            checkUpdate(true);
        }

        private void globalKeyHandler(HotkeyEventArgs e)
        {
            if (settingsOpen)
                return;
            switch (e.action)
            {
                case HotkeyAction.Pause:
                    btPause_Click(null, null);
                    break;
                case HotkeyAction.SoloSession:
                    btSuspend_Click(null, null);
                    break;
                case HotkeyAction.ResupplyBunker:
                    btResupplyBunker_Click(null, null);
                    break;
                case HotkeyAction.ResupplyCocaine:
                    btResupplyCocaine_Click(null, null);
                    break;
                case HotkeyAction.ResupplyMeth:
                    btResupplyMeth_Click(null, null);
                    break;
                case HotkeyAction.ResupplyCounterfeit:
                    btResupplyCounterfeit_Click(null, null);
                    break;
                case HotkeyAction.SellBunker:
                    btSellBunker_Click(null, null);
                    break;
                case HotkeyAction.SellCocaine:
                    btSellCocaine_Click(null, null);
                    break;
                case HotkeyAction.SellMeth:
                    btSellMeth_Click(null, null);
                    break;
                case HotkeyAction.SellCounterfeit:
                    btSellCounterfeit_Click(null, null);
                    break;
                case HotkeyAction.KillProcess:
                    btKillProcess_Click(null, null);
                    break;
            }
        }

        private void load()
        {
            if (!File.Exists(statePath))
            {
                Directory.CreateDirectory(stateDir);
                save();
                return;
            }

            StreamReader file = new StreamReader(statePath);

            try
            {
                for (int i = 0; i < numBusinesses; i++)
                {
                    businesses[i].SetSupplySeconds(Convert.ToInt32(file.ReadLine()));
                    businesses[i].SetProductSeconds(Convert.ToInt32(file.ReadLine()));

                    int resupplyTime = Convert.ToInt32(file.ReadLine());
                    if (resupplyTime > 0)
                        businesses[i].SetResupplyTimeLeft(resupplyTime);
                }
            }
            catch
            {
                file.Close();
                if (File.Exists(stateDir + "state.txt.bak"))
                    File.Delete(stateDir + "state.txt.bak");
                File.Move(statePath, stateDir + "state.txt.bak");
                save();
                MessageBox.Show("The state file (" + statePath + ") could not be read, a clean one has been created and the old one renamed.", "State file corrupted", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            file.Close();
        }

        private void save()
        {
            StreamWriter file = new StreamWriter(statePath, false);

            for (int i = 0; i < numBusinesses; i++)
            {
                file.WriteLine(businesses[i].GetSupplySeconds().ToString());
                file.WriteLine(businesses[i].GetProductSeconds().ToString());
                file.WriteLine(businesses[i].GetResupplyTimeLeft().ToString());
            }

            file.Close();
        }

        private bool checkProcessOpen()
        {
            Process[] procs = Process.GetProcessesByName("GTA5");
            if (procs.Length > 0)
            {
                return true;
            }
            else
            { 
                return false;
            }
        }

        private void tick(object source, ElapsedEventArgs e)
        {
            if (checkProcessOpen() && !isGTAOpen)
            {
                isGTAOpen = true;
            }
            else if (!checkProcessOpen() && isGTAOpen && settings.PauseOnClose)
            {
                settings.Paused = true;
                isGTAOpen = false;
            }

            for (int i = 0; i < 4; i++)
            {
                if (settings.Paused)
                    businesses[i].Pause();
                else
                    businesses[i].Start();
            }
            Dispatcher.Invoke(() =>
            {
                if (settings.Paused)
                {
                    btPause.Background = Brushes.MediumVioletRed;
                }
                else
                {
                    btPause.Background = Brushes.MediumSeaGreen;
                }
            });
            save();
        }

        private void btResupplyBunker_Click(object sender, RoutedEventArgs e)
        {
            businesses[0].ToggleResupply();
            save();
        }

        private void btResupplyCocaine_Click(object sender, RoutedEventArgs e)
        {
            businesses[1].ToggleResupply();
            save();
        }

        private void btResupplyMeth_Click(object sender, RoutedEventArgs e)
        {
            businesses[2].ToggleResupply();
            save();
        }

        private void btResupplyCounterfeit_Click(object sender, RoutedEventArgs e)
        {
            businesses[3].ToggleResupply();
            save();
        }

        private void btSellBunker_Click(object sender, RoutedEventArgs e)
        {
            businesses[0].SellProduct();
            save();
        }

        private void btSellCocaine_Click(object sender, RoutedEventArgs e)
        {
            businesses[1].SellProduct();
            save();
        }

        private void btSellMeth_Click(object sender, RoutedEventArgs e)
        {
            businesses[2].SellProduct();
            save();
        }

        private void btSellCounterfeit_Click(object sender, RoutedEventArgs e)
        {
            businesses[3].SellProduct();
            save();
        }

        private void btPause_Click(object sender, RoutedEventArgs e)
        {
            settings.Paused = !settings.Paused;
            tick(null, null);
        }

        private void pbClick(object sender, RoutedEventArgs e)
        {
            SetValueDialog dialog = new SetValueDialog();
            dialog.ShowDialog();

            if (val < 0)
                return;

            Control c = (Control)sender;

            switch (c.Name)
            {
                case "pbSupBunker":
                    businesses[0].SetSupplyBars(val);
                    break;

                case "pbSupCocaine":
                    businesses[1].SetSupplyBars(val);
                    break;

                case "pbSupMeth":
                    businesses[2].SetSupplyBars(val);
                    break;

                case "pbSupCounterfeit":
                    businesses[3].SetSupplyBars(val);
                    break;

                case "pbProdBunker":
                    businesses[0].SetProductBars(val);
                    break;

                case "pbProdCocaine":
                    businesses[1].SetProductBars(val);
                    break;

                case "pbProdMeth":
                    businesses[2].SetProductBars(val);
                    break;

                case "pbProdCounterfeit":
                    businesses[3].SetProductBars(val);
                    break;

                default:
                    MessageBox.Show("Ajde");
                    break;
            }

            val = -1.0d;
        }

        private void btAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("GTAOBusinesses version " + version.ToString() + "\nBenjamin Goisser 2022\nhttps://github.com/Begus001", "About");
        }

        private void update()
        {
            string exepath = Assembly.GetEntryAssembly().Location;
            Process.Start("AutoUpdater.exe", '"' + exepath + '"');
            Environment.Exit(0);
        }

        private void checkUpdate(bool onStartup)
        {
            WebRequest req = WebRequest.CreateHttp("http://begus.ddns.net/gtaoupdate/version.txt");
            WebResponse resp = req.GetResponse();
            Version newVersion;
            StreamReader s = new StreamReader(resp.GetResponseStream());

            if (!Version.TryParse(s.ReadToEnd(), out newVersion))
            {
                MessageBox.Show("Couldn't check if new version available", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (version < newVersion)
            {
                MessageBoxResult res = MessageBox.Show(string.Format("New version available! ({0} -> {1})\nDo you want to update?", version.ToString(), newVersion.ToString()), "Update", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (res == MessageBoxResult.Yes)
                {
                    update();
                    return;
                }
            }
            else
            {
                if (!onStartup)
                    MessageBox.Show("Already up to date!", "No update", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            s.Close();
            resp.Close();
        }

        private void btUpdate_Click(object sender, RoutedEventArgs e)
        {
            checkUpdate(false);
        }

        private static bool SuspendProcess()
        {
            Process[] procs = Process.GetProcessesByName("GTA5");

            if (procs.Length < 1)
            {
                MessageBox.Show("Process GTA5.exe not found!", "Could not suspend", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            Process process = procs[0];

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                SuspendThread(pOpenThread);

                CloseHandle(pOpenThread);
            }
            return true;
        }

        public static bool ResumeProcess()
        {
            Process[] procs = Process.GetProcessesByName("GTA5");

            if (procs.Length < 1)
            {
                MessageBox.Show("Process GTA5.exe not found!", "Could not resume", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            Process process = procs[0];

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                var suspendCount = 0;
                do
                {
                    suspendCount = ResumeThread(pOpenThread);
                } while (suspendCount > 0);

                CloseHandle(pOpenThread);
            }
            return true;
        }

        private void btSuspend_Click(object sender, RoutedEventArgs e)
        {
            if (suspended)
            {
                suspendThread.Abort();
                ResumeProcess();
                btSuspend.Content = "Solo Session";
                btSuspend.ClearValue(Control.BackgroundProperty);
                suspended = false;
                return;
            }

            suspendThread = new System.Threading.Thread(() =>
            {
                if (!SuspendProcess())
                    return;
                suspended = true;

                Dispatcher.Invoke(() => btSuspend.Background = Brushes.LightGoldenrodYellow);
                for (int i = 9; i > 0; i--)
                {
                    Dispatcher.Invoke(() => btSuspend.Content = i.ToString());
                    System.Threading.Thread.Sleep(1000);
                }
                Dispatcher.Invoke(() =>
                {
                    btSuspend.Content = "Solo Session";
                    btSuspend.ClearValue(Control.BackgroundProperty);
                });

                ResumeProcess();

                suspended = false;
            });

            suspendThread.Start();
        }

        private void btSettings_Click(object sender, RoutedEventArgs e)
        {
            settingsOpen = true;
            Settings window = new Settings(hotkeyManager, settings);
            window.ShowDialog();
            isGTAOpen = false;
            settingsOpen = false;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            settings.SetWindowDimensions(this);
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            settings.SetWindowLocation(this);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            settings.Save();
        }

        private void btKillProcess_Click(object sender, RoutedEventArgs e)
        {
            Process[] procs = Process.GetProcessesByName("GTA5");
            if (procs.Length < 1)
            {
                MessageBox.Show("Process GTA5.exe not found!", "Could not kill", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            procs[0].Kill();
        }
    }
}
