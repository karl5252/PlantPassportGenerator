using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Model;
using WpfApp1;
using System.Windows.Media;
using System.Xml.Linq;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

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
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var newPassport = new PlantPassport
            {
                //Id = _plantPassports.Count > 0 ? _plantPassports.Max(p => p.Id) + 1 : 1,
                PlantName = PlantNameTextBox.Text,
                Sector = SectorTextBox.Text,
                Id = IdTextBox.Text,
                DateAdded = DateTime.Now
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
                selectedPassport.Sector = SectorTextBox.Text;
                selectedPassport.Id = IdTextBox.Text;
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
                SectorTextBox.Text = selectedPassport.Sector;
                IdTextBox.Text = selectedPassport.Id;
            }
        }



        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlantDataGrid.SelectedItems.Count > 0)
            {
                PdfDocument document = new PdfDocument();
                document.Info.Title = "Plant Passports";

                foreach (PlantPassport passport in PlantDataGrid.SelectedItems)
                {
                    PdfPage page = document.AddPage();
                    XGraphics gfx = XGraphics.FromPdfPage(page);
                    XFont font = new XFont("Verdana", 20, XFontStyleEx.Bold);
                    gfx.DrawString("Plant Passport", font, XBrushes.Black, new XRect(0, 0, page.Width, 50), XStringFormats.TopCenter);
                    gfx.DrawString($"ID: {passport.Id}", font, XBrushes.Black, new XRect(40, 60, page.Width, 0));
                    gfx.DrawString($"Plant Name: {passport.PlantName}", font, XBrushes.Black, new XRect(40, 100, page.Width, 0));
                    gfx.DrawString($"Species: {passport.Id}", font, XBrushes.Black, new XRect(40, 140, page.Width, 0));
                    gfx.DrawString($"Origin: {passport.Sector}", font, XBrushes.Black, new XRect(40, 180, page.Width, 0));
                    gfx.DrawString($"Date Added: {passport.DateAdded}", font, XBrushes.Black, new XRect(40, 220, page.Width, 0));

                }

                string filename = "PlantPassports.pdf";
                document.Save(filename);
                MessageBox.Show($"PDF saved as {filename}");
            }
        }
    }   
    
}
