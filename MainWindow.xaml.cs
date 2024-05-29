using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using WpfApp1.Model;
using WpfApp1;

namespace PlantPassportGenerator
{
    public partial class MainWindow : Window
    {
        private List<PlantPassport> _plantPassports;
        private JsonDatabaseService _jsonDatabaseService;
        private const string DatabaseFilePath = "plant_passports.json";

        public MainWindow()
        {
            InitializeComponent();
            _jsonDatabaseService = new JsonDatabaseService(DatabaseFilePath);
            _plantPassports = _jsonDatabaseService.LoadData();
            PlantDataGrid.ItemsSource = _plantPassports;

            LoadSectors();
        }

        private void LoadSectors()
        {
            // Load sectors from the multiline TextBox
            var sectors = SectorTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            SectorComboBox.ItemsSource = sectors;
        }

        private void RefreshSectorsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSectors();
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
            PlantDataGrid.Items.Refresh();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlantDataGrid.SelectedItem is PlantPassport selectedPassport)
            {
                selectedPassport.PlantName = PlantNameTextBox.Text;
                selectedPassport.Sector = SectorComboBox.SelectedItem?.ToString();
                _jsonDatabaseService.SaveData(_plantPassports);
                PlantDataGrid.Items.Refresh();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlantDataGrid.SelectedItem is PlantPassport selectedPassport)
            {
                _plantPassports.Remove(selectedPassport);
                _jsonDatabaseService.SaveData(_plantPassports);
                PlantDataGrid.Items.Refresh();
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

            foreach (PlantPassport passport in PlantDataGrid.SelectedItems)
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
    }
}
