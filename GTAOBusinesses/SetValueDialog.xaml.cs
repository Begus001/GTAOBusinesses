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
using System.Windows.Shapes;

namespace GTAOBusinesses
{
    /// <summary>
    /// Interaction logic for SetValue.xaml
    /// </summary>
    public partial class SetValueDialog : Window
    {
        public SetValueDialog()
        {
            InitializeComponent();
            tbValue.Focus();
        }

        private void btSet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double val = Convert.ToDouble(tbValue.Text);
                MainWindow.val = val;
                Close();
            }
            catch
            {
                tbValue.Focus();
                tbValue.SelectAll();
            }
        }

        private void tbValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                btSet_Click(sender, null);
            }
            else if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
