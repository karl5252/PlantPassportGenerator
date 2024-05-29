using System.Collections.Generic;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using CsvHelper;
using PlantPassportGenerator;
using System.Globalization;
using WpfApp1.Model;
using System;
using System.Linq;

namespace WpfApp1.Util
{
    class CsvHandler
    {
        private readonly MainWindow _mainWindow;

        public CsvHandler(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public void ImportCsv(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV files (*.csv)|*.csv";
            if (openFileDialog.ShowDialog() == true)
            {
                string csvFilePath = openFileDialog.FileName;
                string jsonFilePath = Path.ChangeExtension(csvFilePath, ".json");

                try
                {
                    using (var reader = new StreamReader(csvFilePath))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        var records = csv.GetRecords<PlantPassport>();
                        List<PlantPassport> data = new List<PlantPassport>(records);
                        _mainWindow.UpdatePlantPassports(data);
                        // we need to extract from data Sectors and update them
                        _mainWindow.UpdatePlantSectors(data.Select(p => p.Sector).Distinct().ToList());

                        File.WriteAllText(jsonFilePath, JsonConvert.SerializeObject(data, Formatting.Indented));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"The CSV file could not be processed. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }


        private void ExportToCSV_Click(object sender, RoutedEventArgs e)
        {
            // Implement CSV export functionality here
        }

    }


}