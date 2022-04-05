using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GTAOBusinesses
{
    class Business
    {
        private int supplyFullSeconds;
        private int productFullSeconds;
        private int supplySeconds;
        private int productSeconds;

        private Timer timer = new Timer(1000);

        public Business(int supplyFullSeconds, int productFullSeconds)
        {
            this.supplyFullSeconds = supplyFullSeconds;
            this.productFullSeconds = productFullSeconds;
            productSeconds = productFullSeconds;
            timer.Elapsed += tick;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        private void tick(Object source, ElapsedEventArgs e)
        {
            if (supplySeconds > 0 && productSeconds > 0)
            {
                supplySeconds--;
                productSeconds--;
            }
        }

        public void SetSupplyBars(double bars)
        {
            SetSupplySeconds((int)(supplyFullSeconds * (bars / 5.0d)));
        }

        public void SetProductBars(double bars)
        {
            SetProductSeconds((int)(productFullSeconds - productFullSeconds * (bars / 5.0d)));
        }

        public int GetSupplySeconds()
        {
            return supplySeconds;
        }

        public int GetProductSeconds()
        {
            return productSeconds;
        }

        public void SetSupplySeconds(int secs)
        {
            if (secs > supplyFullSeconds || secs < 0)
                return;

            supplySeconds = secs;
        }

        public void SetProductSeconds(int secs)
        {
            if (secs > productFullSeconds || secs < 0)
                return;

            productSeconds = secs;
        }

        public void Resupply()
        {
            supplySeconds = supplyFullSeconds;
        }

        public void SellProduct()
        {
            productSeconds = productFullSeconds;
        }

        public void Pause()
        {
            timer.Enabled = false;
        }

        public void Start()
        {
            timer.Enabled = true;
        }
    }
}
