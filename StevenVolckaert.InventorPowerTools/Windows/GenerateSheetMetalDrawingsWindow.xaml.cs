﻿namespace StevenVolckaert.InventorPowerTools.Windows
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using Inventor;

    public partial class GenerateSheetMetalDrawingsWindow : Window
    {
        private GenerateSheetMetalDrawingsViewModel ViewModel
        {
            get { return (GenerateSheetMetalDrawingsViewModel)DataContext; }
            set { DataContext = value; }
        }

        public GenerateSheetMetalDrawingsWindow()
        {
            InitializeComponent();
            ViewModel = new GenerateSheetMetalDrawingsViewModel();

            // TODO find a better solution: don't reuse the window, then there's also no need to intercept the closing event.
            // see http://msdn.microsoft.com/en-us/library/aa972163.aspx
            // and http://stackoverflow.com/a/848106/2314596

            Closing +=
                (sender, e) =>
                {
                    e.Cancel = true;
                    Hide();
                };
        }

        public void Show(AssemblyDocument assembly, List<Part> parts)
        {
            if (parts == null || parts.Count == 0)
                return;

            ViewModel.Assembly = assembly;
            ViewModel.Parts = parts.OrderByOrdinal(x => x.Name).ToList();
            Show();
        }

        private void Hide(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ComputeIsEverythingSelected();
        }
    }
}
