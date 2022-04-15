using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public partial class Settings : Window
    {
        private HotkeyManager hotkeyManager;
        private readonly List<Button> hotkeyButtons;
        private readonly Tuple<ModifierKey, VirtualKey>[] bindings;

        private Button beingEdited;

        private uint mod = 0;

        public Settings(HotkeyManager hotkeyManager)
        {
            InitializeComponent();

            this.hotkeyManager = hotkeyManager;
            hotkeyButtons = new List<Button>();

            bindings = new Tuple<ModifierKey, VirtualKey>[hotkeyManager.NumActions];

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(grKeymap); i++)
            {
                DependencyObject ctrl = VisualTreeHelper.GetChild(grKeymap, i);
                if (ctrl.GetType() == typeof(Button))
                {
                    hotkeyButtons.Add((Button)ctrl);
                }
            }

            assignHotkeyManagerBindings();
            updateUI();
        }

        private void assignHotkeyManagerBindings()
        {
            for (int i = 0; i < hotkeyManager.NumActions; i++)
            {
                if (hotkeyManager.Bindings[i] != null)
                    bindings[i] = hotkeyManager.Bindings[i];
            }
        }

        private void updateUI()
        {
            for (int i = 0; i < hotkeyManager.NumActions; i++)
            {
                if (bindings[i] != null)
                {
                    hotkeyButtons[i].Content = HotkeyManager.ModToString(bindings[i].Item1);
                    hotkeyButtons[i].Content += bindings[i].Item2.ToString();
                }
                else
                {
                    hotkeyButtons[i].Content = "";
                }
            }

            foreach (Button btn in hotkeyButtons)
            {
                if (btn.Equals(beingEdited))
                {
                    btn.Background = Brushes.LightGoldenrodYellow;
                    btn.Content = HotkeyManager.ModToString((ModifierKey)mod);
                }
                else
                {
                    btn.ClearValue(Control.BackgroundProperty);
                }
            }
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btShortcut_Click(object sender, RoutedEventArgs e)
        {
            if (beingEdited != null)
                return;

            beingEdited = (Button)sender;
            updateUI();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (beingEdited == null)
                return;

            switch (e.Key)
            {
                case Key.Escape:
                    for (int i = 0; i < bindings.Length; i++)
                    {
                        if (hotkeyButtons[i].Equals(beingEdited))
                        {
                            bindings[i] = null;
                        }
                    }
                    goto end;
                case Key.LeftAlt:
                case Key.RightAlt:
                    mod |= 1;
                    updateUI();
                    return;
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    mod |= 2;
                    updateUI();
                    return;
                case Key.LeftShift:
                case Key.RightShift:
                    mod |= 4;
                    updateUI();
                    return;
            }

            for (int i = 0; i < bindings.Length; i++)
            {
                if (hotkeyButtons[i].Equals(beingEdited))
                {
                    VirtualKey key;
                    Enum.TryParse(e.Key.ToString(), out key);
                    bindings[i] = Tuple.Create((ModifierKey)mod, key);
                }
            }

        end:

            beingEdited = null;
            mod = 0;

            updateUI();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            //if (beingEdited == null)
            //    return;
            //
            //switch (e.Key)
            //{
            //    case Key.LeftAlt:
            //    case Key.RightAlt:
            //        mod &= ~(uint)1;
            //        updateUI();
            //        return;
            //    case Key.LeftCtrl:
            //    case Key.RightCtrl:
            //        mod &= ~(uint)2;
            //        updateUI();
            //        return;
            //    case Key.LeftShift:
            //    case Key.RightShift:
            //        mod &= ~(uint)4;
            //        updateUI();
            //        return;
            //}
        }

        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < bindings.Length; i++)
            {
                if (bindings[i] != null)
                { 
                    if (!bindings[i].Equals(hotkeyManager.Bindings[i]))
                    {
                        hotkeyManager.Unset((HotkeyAction)i);
                        hotkeyManager.Set((HotkeyAction)i, bindings[i].Item1, bindings[i].Item2);
                    }
                }
                else
                {
                    if (hotkeyManager.Bindings[i] != null)
                    {
                        hotkeyManager.Unset((HotkeyAction)i);
                    }
                }
            }
            hotkeyManager.Save();
            Close();
        }
    }
}
