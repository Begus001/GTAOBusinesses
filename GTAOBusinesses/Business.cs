using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows;
using System.Diagnostics;

namespace GTAOBusinesses
{
    class Business
    {
        private int supplyFullSeconds;
        private int productFullSeconds;
        private int supplySeconds;
        private int productSeconds;

        private bool isBeingResupplied = false;
        private bool isPaused = false;

        private ProgressBar supplyBar;
        private ProgressBar productBar;
        private Label supplyLabel;
        private Label productLabel;
        private Button resupplyBtn;
        private Button sellBtn;

        private const int resupplyTime = 10 * 60 - 1;
        private int resupplyCounter = 0;

        private readonly Timer timer = new Timer(1000);
        private readonly Timer resupplyTimer = new Timer(1000);

        public Business(int supplyFullSeconds, int productFullSeconds, ProgressBar supplyBar, ProgressBar productBar, Label supplyLabel, Label productLabel, Button resupplyBtn, Button sellBtn)
        {
            this.supplyFullSeconds = supplyFullSeconds;
            this.productFullSeconds = productFullSeconds;
            productSeconds = productFullSeconds;

            timer.Elapsed += tick;
            timer.AutoReset = true;
            timer.Start();

            resupplyTimer.Elapsed += resupplyTick;
            resupplyTimer.AutoReset = true;
            resupplyTimer.Start();

            this.supplyBar = supplyBar;
            this.productBar = productBar;
            this.supplyLabel = supplyLabel;
            this.productLabel = productLabel;
            this.resupplyBtn = resupplyBtn;
            this.sellBtn = sellBtn;

            supplyBar.Maximum = supplyFullSeconds;
            productBar.Maximum = productFullSeconds;
        }

        private string clockFormat(int secs)
        {
            return string.Format("{0:D2}:{1:D2}:{2:D2}", secs / 3600, secs / 60 % 60, secs % 60);
        }

        private void updateUI()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                supplyBar.Value = supplySeconds;
                productBar.Value = productFullSeconds - productSeconds;

                supplyLabel.Content = clockFormat(supplySeconds);
                productLabel.Content = clockFormat(productSeconds);

                if (supplySeconds <= 0 && !isBeingResupplied)
                    resupplyBtn.Background = Brushes.DarkOrange;
                else
                    resupplyBtn.ClearValue(Control.BackgroundProperty);
                
                if (productSeconds <= 0)
                    sellBtn.Background = Brushes.MediumVioletRed;
                else
                    sellBtn.ClearValue(Control.BackgroundProperty);

                if (isBeingResupplied)
                {
                    supplyBar.Background = Brushes.LightGoldenrodYellow;
                    resupplyBtn.Background = Brushes.LightGoldenrodYellow;
                    resupplyBtn.Content = "Cancel";
                    supplyLabel.Content += " (Supplies in " + clockFormat(resupplyCounter) + ")";
                }
                else
                {
                    supplyBar.ClearValue(Control.BackgroundProperty);
                    resupplyBtn.Content = "Resupply";
                }
            });
        }

        private void tick(object source, ElapsedEventArgs e)
        {
            if (supplySeconds > 0 && productSeconds > 0)
            {
                supplySeconds--;
                productSeconds--;
            }
            updateUI();
        }

        private void resupplyTick(object source, ElapsedEventArgs e)
        {
            if (resupplyCounter <= 0 && isBeingResupplied)
            {
                resupplyTimer.Stop();

                supplySeconds = supplyFullSeconds;
                isBeingResupplied = false;

                updateUI();
            }
            else if (!isPaused && isBeingResupplied)
            {
                resupplyCounter--;
            }
        }

        public int GetSupplySeconds()
        {
            return supplySeconds;
        }

        public int GetProductSeconds()
        {
            return productSeconds;
        }

        public int GetResupplyTimeLeft()
        {
            return resupplyCounter;
        }

        public void SetSupplySeconds(int secs)
        {
            if (secs > supplyFullSeconds || secs < 0)
                return;

            supplySeconds = secs;

            updateUI();
        }

        public void SetProductSeconds(int secs)
        {
            if (secs > productFullSeconds || secs < 0)
                return;

            productSeconds = secs;

            updateUI();
        }

        public void SetSupplyBars(double bars)
        {
            SetSupplySeconds((int)(supplyFullSeconds * (bars / 5.0d)));
        }

        public void SetProductBars(double bars)
        {
            SetProductSeconds((int)(productFullSeconds - productFullSeconds * (bars / 5.0d)));
        }

        public void SetResupplyTimeLeft(int resupplyTimeLeft)
        {
            ToggleResupply();
            resupplyCounter = resupplyTimeLeft;
            updateUI();
        }

        public void ToggleResupply()
        {
            if (!isBeingResupplied)
            {
                resupplyCounter = resupplyTime;
                isBeingResupplied = true;
            }
            else
            {
                resupplyCounter = 0;
                isBeingResupplied = false;
            }
            updateUI();
        }

        public void SellProduct()
        {
            productSeconds = productFullSeconds;

            updateUI();
        }

        public void Pause()
        {
            timer.Stop();
            resupplyTimer.Stop();
            isPaused = true;
        }

        public void Start()
        {
            timer.Start();
            resupplyTimer.Start();
            isPaused = false;
        }
    }
}
