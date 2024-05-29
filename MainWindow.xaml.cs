using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Newtonsoft.Json;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PlantPassportGenerator
{
    public partial class MainWindow : Window
    {
        private List<PlantPassport> _plantPassports;
        private List<string> _sectors;
        private JsonDatabaseService _jsonDatabaseService;
        private const string DatabaseFilePath = "plant_passports.json";
        private const string SectorsFilePath = "sectors.json";
        private CollectionViewSource _collectionViewSource;

        public MainWindow()
        {
            InitializeComponent();
            _jsonDatabaseService = new JsonDatabaseService(DatabaseFilePath);
            _plantPassports = _jsonDatabaseService.LoadData();

            _collectionViewSource = new CollectionViewSource { Source = _plantPassports };
            _collectionViewSource.Filter += CollectionViewSource_Filter;
            PlantDataGrid.ItemsSource = _collectionViewSource.View;

            LoadSectors();

            // Ensure FilterTextBox is empty or has only placeholder text
            FilterTextBox.Text = string.Empty;

            // Refresh the CollectionViewSource
            _collectionViewSource.View.Refresh();
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

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Plant Passports";

            int passportsPerPage = 15; // 5 rows * 3 columns
            int passportCounter = 0;

            PdfPage page = null;
            XGraphics gfx = null;
            XFont font = new XFont("Verdana", 10, XFontStyleEx.Regular);
            XFont boldFont = new XFont("Verdana", 10, XFontStyleEx.Bold);

            foreach (PlantPassport passport in BasketListBox.Items)
            {
                if (passportCounter % passportsPerPage == 0)
                {
                    page = document.AddPage();
                    page.Size = PdfSharp.PageSize.A4;
                    gfx = XGraphics.FromPdfPage(page);
                }

                int row = (passportCounter % passportsPerPage) / 3;
                int column = (passportCounter % passportsPerPage) % 3;

                double x = 40 + column * (210 + 20); // 2.5cm (around 70 points) + 5cm (around 140 points)
                double y = 40 + row * (100 + 20); // 2.5cm (around 70 points) + 5cm (around 140 points)

                gfx.DrawString("EU", boldFont, XBrushes.Black, new XRect(x, y, 70, 20), XStringFormats.TopLeft);
                gfx.DrawString("Paszport rolin / Plant passport", font, XBrushes.Black, new XRect(x + 70, y, 140, 20), XStringFormats.TopLeft);

                gfx.DrawString($"A: {passport.PlantName}", font, XBrushes.Black, new XRect(x, y + 20, 210, 20), XStringFormats.TopLeft);
                gfx.DrawString($"B: {passport.Id}", font, XBrushes.Black, new XRect(x, y + 40, 210, 20), XStringFormats.TopLeft);
                gfx.DrawString($"C: {passport.Sector} / {DateTime.Now.Year}", font, XBrushes.Black, new XRect(x, y + 60, 210, 20), XStringFormats.TopLeft);
                gfx.DrawString("D: PL", font, XBrushes.Black, new XRect(x, y + 80, 210, 20), XStringFormats.TopLeft);

                passportCounter++;
            }

            string filename = "PlantPassports.pdf";
            document.Save(filename);
            MessageBox.Show($"PDF saved as {filename}");
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
                                 passport.Sector?.IndexOf(FilterTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
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
    }

    public class PlantPassport
    {
        public string Id { get; set; }
        public string PlantName { get; set; }
        public string Sector { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
    }

    public class JsonDatabaseService
    {
        private readonly string _filePath;

        public JsonDatabaseService(string filePath)
        {
            _filePath = filePath;
        }

        public List<PlantPassport> LoadData()
        {
            if (File.Exists(_filePath))
            {
                string json = File.ReadAllText(_filePath);
                return JsonConvert.DeserializeObject<List<PlantPassport>>(json) ?? new List<PlantPassport>();
            }

            return new List<PlantPassport>();
        }

        public void SaveData(List<PlantPassport> data)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }
    }
}
