using System;
using System.Collections.Generic;
using System.Linq;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Windows;
using PlantPassportGenerator;

public class PdfGenerator
{
    private List<PlantPassport> _plantPassports;
    private List<BasketItem> _basket;

    public PdfGenerator(List<PlantPassport> plantPassports, List<BasketItem> basket)
    {
        _plantPassports = plantPassports;
        _basket = basket;
    }

    public void GeneratePdf()
    {
        PdfDocument document = new PdfDocument();
        document.Info.Title = "Plant Passports";

        int passportsPerPage = 15; // 5 rows * 3 columns
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
        }

        string filename = "PlantPassports.pdf";
        document.Save(filename);
        MessageBox.Show($"PDF saved as {filename}");
    }
}
