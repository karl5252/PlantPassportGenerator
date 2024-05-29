using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Newtonsoft.Json;
using System.IO;
using WpfApp1.Model;
using WpfApp1.Util;

namespace PlantPassportGenerator
{
    public partial class MainWindow : Window
    {
        private List<PlantPassport> _plantPassports;
        private List<string> _sectors;
        private List<BasketItem> _basket;
        private JsonDatabaseService _jsonDatabaseService;
        private CsvHandler _csvHandler;
        private const string DatabaseFilePath = "plant_passports.json";
        private const string SectorsFilePath = "sectors.json";
        private CollectionViewSource _collectionViewSource;

        public MainWindow()
        {
            InitializeComponent();
            _jsonDatabaseService = new JsonDatabaseService(DatabaseFilePath);
            _plantPassports = _jsonDatabaseService.LoadData();
            _basket = new List<BasketItem>();

            _csvHandler = new CsvHandler(this); // Pass the MainWindow instance to the CsvHandler

            _collectionViewSource = new CollectionViewSource { Source = _plantPassports };
            _collectionViewSource.Filter += CollectionViewSource_Filter;
            PlantDataGrid.ItemsSource = _collectionViewSource.View;

            LoadSectors();
            BasketListBox.ItemsSource = _basket;
        }

        private void LoadSectors()
        {
            if (File.Exists(SectorsFilePath))
            {
                string json = File.ReadAllText(SectorsFilePath);
                _sectors = JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
            }
            else
            {
                _sectors = new List<string>();
            }
            SectorComboBox.ItemsSource = _sectors;
            SectorListBox.ItemsSource = _sectors;
        }

        private void SaveSectors()
        {
            string json = JsonConvert.SerializeObject(_sectors, Formatting.Indented);
            File.WriteAllText(SectorsFilePath, json);
        }

        private void AddSectorButton_Click(object sender, RoutedEventArgs e)
        {
            string newSector = NewSectorTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(newSector) && !_sectors.Contains(newSector))
            {
                _sectors.Add(newSector);
                SaveSectors();
                LoadSectors();
            }
        }

        private void DeleteSectorButton_Click(object sender, RoutedEventArgs e)
        {
            if (SectorListBox.SelectedItem is string selectedSector)
            {
                _sectors.Remove(selectedSector);
                SaveSectors();
                LoadSectors();
            }
        }

        private void SectorListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Optional: Handle selection change if needed
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var newPassport = new PlantPassport
            {
                Id = IdTextBox.Text,
                PlantName = PlantNameTextBox.Text,
                Sector = SectorComboBox.SelectedItem?.ToString()
            };

            _plantPassports.Add(newPassport);
            _jsonDatabaseService.SaveData(_plantPassports);
            RefreshCollectionView();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlantDataGrid.SelectedItem is PlantPassport selectedPassport)
            {
                selectedPassport.PlantName = PlantNameTextBox.Text;
                selectedPassport.Sector = SectorComboBox.SelectedItem?.ToString();
                _jsonDatabaseService.SaveData(_plantPassports);
                RefreshCollectionView();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlantDataGrid.SelectedItem is PlantPassport selectedPassport)
            {
                _plantPassports.Remove(selectedPassport);
                _jsonDatabaseService.SaveData(_plantPassports);
                RefreshCollectionView();
            }
        }

        private void PlantDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlantDataGrid.SelectedItem is PlantPassport selectedPassport)
            {
                PlantNameTextBox.Text = selectedPassport.PlantName;
                IdTextBox.Text = selectedPassport.Id.ToString();
                SectorComboBox.SelectedItem = selectedPassport.Sector;
            }
        }

        private void PlantDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (PlantDataGrid.SelectedItem is PlantPassport selectedPassport)
            {
                var basketItem = _basket.FirstOrDefault(item => item.PlantName == selectedPassport.PlantName);
                if (basketItem != null)
                {
                    basketItem.Count++;
                }
                else
                {
                    _basket.Add(new BasketItem { PlantName = selectedPassport.PlantName, Count = 1 });
                }
                BasketListBox.Items.Refresh();
            }
        }

        private void IncrementCountButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is BasketItem basketItem)
            {
                basketItem.Count++;
                BasketListBox.Items.Refresh();
            }
        }

        private void DecrementCountButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is BasketItem basketItem)
            {
                if (basketItem.Count > 1)
                {
                    basketItem.Count--;
                }
                else
                {
                    _basket.Remove(basketItem);
                }
                BasketListBox.Items.Refresh();
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            var pdfGenerator = new PdfGenerator(_plantPassports, _basket);
            pdfGenerator.GeneratePdf();
        }


        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshCollectionView();
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is PlantPassport passport)
            {
                if (string.IsNullOrWhiteSpace(FilterTextBox.Text))
                {
                    e.Accepted = true;
                }
                else
                {
                    e.Accepted = passport.PlantName.IndexOf(FilterTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                 passport.Id.ToString().IndexOf(FilterTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                 passport.Sector.IndexOf(FilterTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
        }

        private void RefreshCollectionView()
        {
            if (_collectionViewSource?.View != null)
            {
                _collectionViewSource.View.Refresh();
            }
        }

        private void ImportFromCSV_Click(object sender, RoutedEventArgs e)
        {

            _csvHandler.ImportCsv(sender, e);
        }

        private void ExportToCSV_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
            // Implement CSV export functionality here
        }
    }


}
