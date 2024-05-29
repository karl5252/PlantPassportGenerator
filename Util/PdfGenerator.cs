using System;
using System.Collections.Generic;
using System.Linq;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Windows;
using PlantPassportGenerator;
using WpfApp1.Model;
using System.IO;

public class PdfGenerator
{
    private List<PlantPassport> _plantPassports;
    private List<BasketItem> _basket;
    private XImage _euFlag;

    public PdfGenerator(List<PlantPassport> plantPassports, List<BasketItem> basket)
    {
        _plantPassports = plantPassports;
        _basket = basket;
        string euFlagPath = Path.Combine(Directory.GetCurrentDirectory(), "Images", "EU.png");
        _euFlag = XImage.FromFile(euFlagPath);
    }

   /* public PdfGenerator(List<PlantPassport> plantPassports, List<BasketItem> basket)
    {
        PlantPassports = plantPassports;
        Basket = basket;
    }
    */
    public List<PlantPassport> PlantPassports { get; }
    public List<BasketItem> Basket { get; }

    public void GeneratePdf()
    {
        PdfDocument document = new PdfDocument();
        document.Info.Title = "Plant Passports";

        int passportsPerPage = 10; // 5 rows * 3 columns
        int passportCounter = 0;

        PdfPage page = null;
        XGraphics gfx = null;
        XFont font = new XFont("Verdana", 10, XFontStyleEx.Regular);
        XFont boldFont = new XFont("Verdana", 10, XFontStyleEx.Bold);

        foreach (var basketItem in _basket)
        {
            for (int i = 0; i < basketItem.Count; i++)
            {
                var passport = _plantPassports.FirstOrDefault(p => p.PlantName == basketItem.PlantName);

                if (passport == null) continue;

                if (passportCounter % passportsPerPage == 0)
                {
                    page = document.AddPage();
                    page.Size = PdfSharp.PageSize.A4;
                    gfx = XGraphics.FromPdfPage(page);
                }
                int row = (passportCounter % passportsPerPage) / 3;
                int column = (passportCounter % passportsPerPage) % 2;

                double x = 40 + column * (230 + 30); // 2.5cm (around 70 points) + 5cm (around 140 points) + margin
                double y = 40 + row * (100 + 55); // 2.5cm (around 70 points) + 5cm (around 140 points) + margin
                
                gfx.DrawImage(_euFlag, x, y, 80, 50); // Draw flag

                gfx.DrawString("Paszport roślin / Plant passport", font, XBrushes.Black, new XRect(x + 85, y, 140, 20), XStringFormats.TopLeft);

                gfx.DrawString($"A {passport.PlantName}", font, XBrushes.Black, new XRect(x + 5, y + 70, 210, 20), XStringFormats.TopLeft);
                gfx.DrawString($"B {passport.Id}", font, XBrushes.Black, new XRect(x + 5, y + 90, 210, 20), XStringFormats.TopLeft);
                gfx.DrawString($"C {passport.Sector} /  {DateTime.Now.ToString("yy")}", font, XBrushes.Black, new XRect(x + 5, y + 110, 210, 20), XStringFormats.TopLeft);
                gfx.DrawString("D PL", font, XBrushes.Black, new XRect(x + 5, y + 130, 210, 20), XStringFormats.TopLeft);

                gfx.DrawRectangle(new XPen(XColors.Black, 1), x, y, 245, 150); // Draw border 
                passportCounter++;
            }
        }

        string filename = "PlantPassports.pdf";
        document.Save(filename);
        MessageBox.Show($"PDF saved as {filename}");
    }
}
