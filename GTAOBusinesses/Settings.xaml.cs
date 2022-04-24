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
        private SettingsManager settingsManager;
        private readonly List<Button> hotkeyButtons;
        private readonly Tuple<ModifierKey, VirtualKey>[] bindings;

        private bool settingPauseOnClose;
        private int settingAFKKey;

        private Button beingEdited;

        private uint mod = 0;

        public Settings(HotkeyManager hotkeyManager, SettingsManager settingsManager, bool keymapPage)
        {
            InitializeComponent();

            this.hotkeyManager = hotkeyManager;
            this.settingsManager = settingsManager;

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

            settingPauseOnClose = settingsManager.PauseOnClose;
            settingAFKKey = settingsManager.AFKKey;

            hotkeyManager.UnregisterAll();

            if (keymapPage)
                tcTab.SelectedItem = tiKeymap;

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

            cbPauseOnGameClose.IsChecked = settingPauseOnClose;
            tbAFKKey.Text = settingAFKKey.ToString("X");
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            hotkeyManager.ReregisterAll();
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

            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                mod |= 1;
                updateUI();
                return;
            }

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
                    Tuple<ModifierKey, VirtualKey> t = Tuple.Create((ModifierKey)mod, key);
                    if (!bindings.Contains(t))
                        bindings[i] = t;
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
            hotkeyManager.ReregisterAll();
            settingsManager.PauseOnClose = settingPauseOnClose;
            try 
            { 
                settingAFKKey = Convert.ToInt32(tbAFKKey.Text, 16);
                settingsManager.AFKKey = settingAFKKey;
            }
            catch { MessageBox.Show("AFK key value invalid, value was not saved!", "Invalid AFK Key", MessageBoxButton.OK, MessageBoxImage.Error); }
            settingsManager.Save();
            Close();
        }

        private void cbPauseOnGameClose_Checked(object sender, RoutedEventArgs e)
        {
            settingPauseOnClose = true;
        }

        private void cbPauseOnGameClose_Unchecked(object sender, RoutedEventArgs e)
        {
            settingPauseOnClose = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            hotkeyManager.ReregisterAll();
        }

        private void tbAFKKey_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
                btOK_Click(tbAFKKey, null);
        }
    }
}
