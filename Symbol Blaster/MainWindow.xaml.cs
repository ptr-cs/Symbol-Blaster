using SymbolBlaster.Game;
using SymbolBlaster.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SymbolBlaster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly MainViewModel mainViewModel;

        public MainWindow()
        {
            InitializeComponent();

            mainViewModel = new MainViewModel();

            this.DataContext = mainViewModel;

            mainViewModel.GameViewModel.SetGameContainer(canvas);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mainViewModel.GameViewModel.ResizeGameArea(interfaceContainer);
            mainViewModel.GameViewModel.StartGame();
            // mainViewModel.GameViewModel.GameOver();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            mainViewModel.GameViewModel.ResizeGameArea(interfaceContainer);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (mainViewModel.GameViewModel.GameState == GameState.SHOW_SCORES && e.Key == Key.Enter)
            {
                mainViewModel.GameViewModel.RestartGame();
            }

            if (GameViewModel.IsKeyInUserInputSet(e) && !configurationNameEntryTextBox.IsFocused)
                mainViewModel.GameViewModel.HandleKeyDown(e);

            //if (mainViewModel.GameViewModel.GameState == GameState.GAME_ACTIVE && e.Key == Key.OemTilde)
            //{
            //    mainViewModel.GameViewModel.ConfigurationPresetBuiltInCollectionView.MoveCurrentToFirst();
            //    mainViewModel.GameViewModel.LoadConfigurationPreset?.Execute(PresetType.BuiltIn);
            //    mainViewModel.GameViewModel.RestartGame();
            //}
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (GameViewModel.IsKeyInUserInputSet(e))
                mainViewModel.GameViewModel.HandleKeyUp(e);
        }

        private void MenuButton_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            IgnoreSpacePress(sender, e);
        }

        private void IgnoreSpacePress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && !configurationNameEntryTextBox.IsFocused)
                e.Handled = true;
        }

        private void NameEntryTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                mainViewModel.GameViewModel.AddHighScore();
                mainViewModel.GameViewModel.ShowHighScores();
            }
        }

        private void ResetFocus(object sender, EventArgs e)
        {
            resetFocusElement.Focus();
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            mainViewModel.GameViewModel.ResizeGameArea(interfaceContainer);
        }

        private void CloseSaveConfigurationButton_Click(object sender, RoutedEventArgs e)
        {
            addConfigurationPopup.IsOpen = false;
        }

        private void AddConfigurationPopup_Opened(object sender, EventArgs e)
        {
            popupCloseHitTestBlocker.IsHitTestVisible = true;
            configurationNameEntryTextBox.Focus();
            if (mainViewModel.GameViewModel.SaveConfigurationPresetName.Length > 0)
                configurationNameEntryTextBox.SelectAll();
        }

        private void PopupCloseHitTestBlocker_Click(object sender, RoutedEventArgs e)
        {
            popupCloseHitTestBlocker.IsHitTestVisible = false;
        }

        private void AddConfigurationPopup_Closed(object sender, EventArgs e)
        {
            // WPF hack for dismissing Popup with StaysOpen="False"
            // when clicking a corresponding ToggleButton for showing/hiding the Popup;
            // Default behavoir results in "double triggering" the ToggleButton.
            if (!addConfigurationToggleButton.IsPressed)
                popupCloseHitTestBlocker.IsHitTestVisible = false;
        }

        private void SaveConfigurationButton_Click(object sender, RoutedEventArgs e)
        {
            addConfigurationPopup.IsOpen = false;
        }

        private void ConfigurationNameEntryTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && mainViewModel.GameViewModel.SaveConfigurationPresetName.Length > 0)
            {
                if (mainViewModel.GameViewModel.SaveConfigurationPreset is null)
                    return;

                if (mainViewModel.GameViewModel.SaveConfigurationPreset.CanExecute(null))
                    mainViewModel.GameViewModel.SaveConfigurationPreset?.Execute(null);
                addConfigurationPopup.IsOpen = false;
            }
            else if (e.Key == Key.Escape)
                addConfigurationPopup.IsOpen = false;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            resetFocusElement.Focus();
        }
    }
}
