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
        private const int bunkerSup = 2 * 3600 + 34 * 60;
        private const int cocaineSup = 2 * 3600 + 10 * 60;
        private const int methSup = 2 * 3600 + 34 * 60;
        private const int counterSup = 2 * 3600 + 50 * 60;

        private const int bunkerProd = (bunkerSup - 10 * 60) * 5;
        private const int cocaineProd = (int)((cocaineSup - 10 * 60) * 2.5);
        private const int methProd = (int)((methSup - 10 * 60) * 2.5);
        private const int counterProd = (counterSup - 10 * 60) * 2;

        private readonly Business[] businesses = new Business[4];

        private readonly Timer uiUpdater = new Timer(1000);
        private bool isRunning = true;

        public static double val = -1.0d;

        public MainWindow()
        {
            InitializeComponent();
            businesses[0] = new Business(bunkerSup, bunkerProd);
            businesses[1] = new Business(cocaineSup, cocaineProd);
            businesses[2] = new Business(methSup, methProd);
            businesses[3] = new Business(counterSup, counterProd);

            pbSupBunker.Maximum = bunkerSup;
            pbSupCocaine.Maximum = cocaineSup;
            pbSupMeth.Maximum = methSup;
            pbSupCounterfeit.Maximum = counterSup;

            pbProdBunker.Maximum = bunkerProd;
            pbProdCocaine.Maximum = cocaineProd;
            pbProdMeth.Maximum = methProd;
            pbProdCounterfeit.Maximum = counterProd;

            uiUpdater.Elapsed += tick;
            uiUpdater.AutoReset = true;
            uiUpdater.Enabled = true;
            
            btPause.Background = Brushes.MediumSeaGreen;

            load();
            updateUI();
        }

        private void load()
        {
            StreamReader file = new StreamReader("state.txt");

            for (int i = 0; i < 4; i++)
            {
                file.ReadLine();
                businesses[i].SetSupplySeconds(Convert.ToInt32(file.ReadLine()));
                businesses[i].SetProductSeconds(Convert.ToInt32(file.ReadLine()));
            }

            file.Close();
        }

        private void save()
        {
            StreamWriter file = new StreamWriter("state.txt", false);

            file.WriteLine("Bunker");
            file.WriteLine(businesses[0].GetSupplySeconds().ToString());
            file.WriteLine(businesses[0].GetProductSeconds().ToString());
            file.WriteLine("Cocaine");
            file.WriteLine(businesses[1].GetSupplySeconds().ToString());
            file.WriteLine(businesses[1].GetProductSeconds().ToString());
            file.WriteLine("Meth");
            file.WriteLine(businesses[2].GetSupplySeconds().ToString());
            file.WriteLine(businesses[2].GetProductSeconds().ToString());
            file.WriteLine("Counterfeit");
            file.WriteLine(businesses[3].GetSupplySeconds().ToString());
            file.WriteLine(businesses[3].GetProductSeconds().ToString());

            file.Close();
        }

        private string clockFormat(int secs)
        {
            return string.Format("{0:D2}:{1:D2}:{2:D2}", secs / 3600, secs / 60 % 60, secs % 60);
        }

        private void updateUI()
        {
            Dispatcher.Invoke(() =>
            {
                pbSupBunker.Value = businesses[0].GetSupplySeconds();
                pbSupCocaine.Value = businesses[1].GetSupplySeconds();
                pbSupMeth.Value = businesses[2].GetSupplySeconds();
                pbSupCounterfeit.Value = businesses[3].GetSupplySeconds();

                pbProdBunker.Value = bunkerProd - businesses[0].GetProductSeconds();
                pbProdCocaine.Value = cocaineProd - businesses[1].GetProductSeconds();
                pbProdMeth.Value = methProd - businesses[2].GetProductSeconds();
                pbProdCounterfeit.Value = counterProd - businesses[3].GetProductSeconds();

                lbSupBunker.Content = clockFormat(businesses[0].GetSupplySeconds());
                lbSupCocaine.Content = clockFormat(businesses[1].GetSupplySeconds());
                lbSupMeth.Content = clockFormat(businesses[2].GetSupplySeconds());
                lbSupCounterfeit.Content = clockFormat(businesses[3].GetSupplySeconds());

                lbProdBunker.Content = clockFormat(businesses[0].GetProductSeconds());
                lbProdCocaine.Content = clockFormat(businesses[1].GetProductSeconds());
                lbProdMeth.Content = clockFormat(businesses[2].GetProductSeconds());
                lbProdCounterfeit.Content = clockFormat(businesses[3].GetProductSeconds());

                if (businesses[0].GetSupplySeconds() <= 0)
                {
                    btResupplyBunker.Background = Brushes.DarkOrange;
                }

                if (businesses[0].GetProductSeconds() <= 0)
                {
                    btSellBunker.Background = Brushes.MediumVioletRed;
                }

                if (businesses[1].GetSupplySeconds() <= 0)
                {
                    btResupplyCocaine.Background = Brushes.DarkOrange;
                }

                if (businesses[1].GetProductSeconds() <= 0)
                {
                    btSellCocaine.Background = Brushes.MediumVioletRed;
                }

                if (businesses[2].GetSupplySeconds() <= 0)
                {
                    btResupplyMeth.Background = Brushes.DarkOrange;
                }

                if (businesses[2].GetProductSeconds() <= 0)
                {
                    btSellMeth.Background = Brushes.MediumVioletRed;
                }

                if (businesses[3].GetSupplySeconds() <= 0)
                {
                    btResupplyCounterfeit.Background = Brushes.DarkOrange;
                }

                if (businesses[3].GetProductSeconds() <= 0)
                {
                    btSellCounterfeit.Background = Brushes.MediumVioletRed;
                }
            });
        }

        private void tick(Object source, ElapsedEventArgs e)
        {
            if (!isRunning)
                return;

            updateUI();
            
            save();
        }

        private void btResupplyBunker_Click(object sender, RoutedEventArgs e)
        {
            pbSupBunker.Value = bunkerSup;
            businesses[0].Resupply();
            btResupplyBunker.ClearValue(Button.BackgroundProperty);
        }

        private void btResupplyCocaine_Click(object sender, RoutedEventArgs e)
        {
            pbSupCocaine.Value = cocaineSup;
            businesses[1].Resupply();
            btResupplyCocaine.ClearValue(Button.BackgroundProperty);
        }

        private void btResupplyMeth_Click(object sender, RoutedEventArgs e)
        {
            pbSupMeth.Value = methSup;
            businesses[2].Resupply();
            btResupplyMeth.ClearValue(Button.BackgroundProperty);
        }

        private void btResupplyCounterfeit_Click(object sender, RoutedEventArgs e)
        {
            pbSupCounterfeit.Value = counterSup;
            businesses[3].Resupply();
            btResupplyCounterfeit.ClearValue(Button.BackgroundProperty);
        }

        private void btSellBunker_Click(object sender, RoutedEventArgs e)
        {
            pbProdBunker.Value = 0;
            businesses[0].SellProduct();
            btSellBunker.ClearValue(Button.BackgroundProperty);
        }

        private void btSellCocaine_Click(object sender, RoutedEventArgs e)
        {
            pbProdCocaine.Value = 0;
            businesses[1].SellProduct();
            btSellCocaine.ClearValue(Button.BackgroundProperty);
        }

        private void btSellMeth_Click(object sender, RoutedEventArgs e)
        {
            pbProdMeth.Value = 0;
            businesses[2].SellProduct();
            btSellMeth.ClearValue(Button.BackgroundProperty);
        }

        private void btSellCounterfeit_Click(object sender, RoutedEventArgs e)
        {
            pbProdCounterfeit.Value = 0;
            businesses[3].SellProduct();
            btSellCounterfeit.ClearValue(Button.BackgroundProperty);
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
                load();
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
    }
}
