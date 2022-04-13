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

namespace GTAOBusinesses
{
    public partial class MainWindow : Window
    {
        private readonly string stateDir = @"C:\Users\" + Environment.UserName + @"\AppData\Roaming\GTAOBusinesses\";
        private const string stateFilename = "state.txt";
        private readonly string statePath;

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
        private bool isRunning = true;

        public static double val = -1.0d;

        public MainWindow()
        {
            InitializeComponent();

            statePath = stateDir + stateFilename;

            businesses[0] = new Business(bunkerSup, bunkerProd, pbSupBunker, pbProdBunker, lbSupBunker, lbProdBunker, btResupplyBunker, btSellBunker);
            businesses[1] = new Business(cocaineSup, cocaineProd, pbSupCocaine, pbProdCocaine, lbSupCocaine, lbProdCocaine, btResupplyCocaine, btSellCocaine);
            businesses[2] = new Business(methSup, methProd, pbSupMeth, pbProdMeth, lbSupMeth, lbProdMeth, btResupplyMeth, btSellMeth);
            businesses[3] = new Business(counterSup, counterProd, pbSupCounterfeit, pbProdCounterfeit, lbSupCounterfeit, lbProdCounterfeit, btResupplyCounterfeit, btSellCounterfeit);
            
            btPause.Background = Brushes.MediumSeaGreen;

            saveTimer.Elapsed += tick;
            saveTimer.AutoReset = true;
            saveTimer.Start();

            load();
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
                File.Move(statePath, stateDir + "state.txt.bak", true);
                save();
                MessageBox.Show("The save file (" + statePath + ") could not be read, a clean one has been created and the old one renamed.", "Save file corrupted");
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

        private void tick(object source, ElapsedEventArgs e)
        {
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
            for (int i = 0; i < 4; i++)
            {
                if (isRunning)
                    businesses[i].Pause();
                else
                    businesses[i].Start();
            }

            isRunning = !isRunning;

            if (isRunning)
            {
                btPause.Background = Brushes.MediumSeaGreen;
            }
            else
            {
                btPause.Background = Brushes.MediumVioletRed;
            }
        }

        private void pbClick(object sender, RoutedEventArgs e)
        {
            SetValueDialog dialog = new SetValueDialog();
            dialog.ShowDialog();

            if (val < 0)
                return;

            Control c = (Control)sender;

            switch(c.Name)
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
            MessageBox.Show("Benjamin Goisser 2022\nhttps://github.com/Begus001", "About");
        }
    }
}
