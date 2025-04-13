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
        private bool isBoosted = false;

        private ProgressBar supplyBar;
        private ProgressBar productBar;
        private Label supplyLabel;
        private Label productLabel;
        private Button resupplyBtn;
        private Button sellBtn;
        private Button boostBtn;

        private const int resupplyTime = 10 * 60 - 1;
        private const int boostTime = 2 * 60 * 60 + 24 * 60 - 1;
        private int resupplyCounter = 0;
        private int boostCounter = 0;

        private readonly Timer timer = new Timer(1000);
        private readonly Timer resupplyTimer = new Timer(1000);

        public Business(int supplyFullSeconds, int productFullSeconds, ProgressBar supplyBar, ProgressBar productBar, Label supplyLabel, Label productLabel, Button resupplyBtn, Button sellBtn, Button boostBtn = null)
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
            this.boostBtn = boostBtn;

            supplyBar.Maximum = supplyFullSeconds;
            productBar.Maximum = productFullSeconds;
        }

        private string clockFormat(int secs)
        {
            return string.Format("{0:D2}:{1:D2}:{2:D2}", secs / 3600, secs / 60 % 60, secs % 60);
        }

        private void updateUI()
        {
			Application.Current.Dispatcher.Invoke((Action)(() =>
            {
				supplyBar.Maximum = supplyFullSeconds;
				productBar.Maximum = productFullSeconds;
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

                if (isAcid())
                {
					if (isBoosted)
					{
						boostBtn.Background = Brushes.LightGoldenrodYellow;
						boostBtn.Content = clockFormat(boostCounter);
					}
					else
					{
						boostBtn.ClearValue(Control.BackgroundProperty);
						boostBtn.Content = "Boost";
					}
				}
			}));
        }

        private void tick(object source, ElapsedEventArgs e)
        {
            if (supplySeconds > 0 && productSeconds > 0)
            {
                supplySeconds--;
                productSeconds--;
				if (boostCounter <= 0 && isBoosted)
				{
                    ToggleBoostAcid();
				}
				else if (!isPaused && isBoosted)
				{
					boostCounter--;
				}
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

        public int GetBoostTimeLeft()
        {
            return boostCounter;
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

        public void SetBoostTimeLeft(int boostTime)
        {
            ToggleBoostAcid();
            boostCounter = boostTime;
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

        public bool isAcid()
        {
            return boostBtn != null;
        }

        public void ToggleBoostAcid()
        {
            if (!isAcid()) return;

            if (!isBoosted)
            {
                supplyFullSeconds = (int)(supplyFullSeconds * 0.6);
                supplySeconds = (int)(supplySeconds * 0.6);
                productFullSeconds = (int)(productFullSeconds * 0.75);
                productSeconds = (int)(productSeconds * 0.75);
                boostCounter = boostTime;
            }
            else
            {
                supplyFullSeconds = (int)(supplyFullSeconds / 0.6);
                supplySeconds = (int)(supplySeconds / 0.6);
                productFullSeconds = (int)(productFullSeconds / 0.75);
                productSeconds = (int)(productSeconds / 0.75);
                boostCounter = 0;
            }
            isBoosted = !isBoosted;
            updateUI();
        }
    }
}
