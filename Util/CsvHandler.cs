using System.Collections.Generic;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using CsvHelper;
using PlantPassportGenerator;
using System.Globalization;
using WpfApp1.Model;
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
                        csv.Context.RegisterClassMap<PlantPassportMap>();
                        var records = csv.GetRecords<PlantPassport>().ToList();
                        _mainWindow.UpdatePlantPassports(records);

                        File.WriteAllText(jsonFilePath, JsonConvert.SerializeObject(records, Formatting.Indented));
                    }
                }
                catch (CsvHelperException ex)
                {
                    MessageBox.Show($"Error importing CSV: {ex.Message}\nPlease ensure all fields are filled, even with placeholder data.", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }



        private void ExportToCSV_Click(object sender, RoutedEventArgs e)
    {
            
        // Implement CSV export functionality here
    }

}


}
