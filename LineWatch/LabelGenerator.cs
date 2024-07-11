using iText.Barcodes;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace LineWatch
{
    /// <summary>
    /// Генератор бирок для ящиков
    /// </summary>
    public static class LabelGenerator
    {

        static Label label = new();
        static List<Material> MaterialList = new();
        static LabelGenerator()
        {
            var filename = @"./conf/materials.txt";
            if (File.Exists(@filename))
            {
                var listFile = File.ReadLines(@filename);
                foreach (string line in listFile)
                {
                    string[] items = line.Split(",");
                    Material m = new(items[0], items[1], items[2], items[3], float.Parse(items[4]), float.Parse(items[5]));
                    MaterialList.Add(m);
                }
            }
            else
            {
                List<Material> manualList = new()
                {
                    new Material("C23443-114", "8450045950", "", "", 2, 20),
                    new Material("C23442-114", "8450045951", "", "", 2, 20),
                    new Material("C61999-112", "8450111766", "", "", 2, 20),
                    new Material("C61998-112", "8450111767", "", "", 2, 20),
                    new Material("C36388-114", "8450087580", "", "", 2, 20),
                    new Material("C36387-114", "8450087581", "", "", 2, 20),

                    new Material("E68985-103", "8450040355", "", "8USP0010", 2, 20),
                    new Material("E68986-103", "8450040354", "", "8USP0010", 2, 20),
                    //vesta
                    new Material("LF1400-100", "8450034709", "08329", "8USP0010", 2, 20),
                    new Material("LF1300-100", "8450034710", "08329", "8USP0010", 2, 20),
                    new Material("LF2400-100", "8450044658", "08329", "8USP0010", 2, 20),
                    new Material("LF2300-100", "8450044659", "08329", "8USP0010", 2, 20),
                    new Material("LF1200-100", "8450045950", "08329", "8USP0010", 2, 20),
                    new Material("LF1100-100", "8450045951", "08329", "8USP0010", 2, 20),
                    //granta
                    new Material("LD1200-100", "8450111766", "", "8USP0010", 2, 20),
                    new Material("LD1100-100", "8450111767", "", "8USP0010", 2, 20),
                    new Material("LD2400-100", "8450111764", "", "8USP0010", 2, 20),
                    new Material("LD2300-100", "8450111765", "", "8USP0010", 2, 20),
                    //niva
                    new Material("LL1200-100", "8450087580", "08P10", "8USP0010", 2, 20),
                    new Material("LL1100-100", "8450087581", "08P09", "8USP0010", 2, 20),
                    //iskra
                    new Material("LO2200-100", "8450160090", "", "8USP0010", 2, 20),
                    new Material("LO2100-100", "8450160091", "", "8USP0010", 2, 20),
                    new Material("LO2400-100", "8450160088", "", "8USP0010", 2, 20),
                    new Material("LO2300-100", "8450160089", "", "8USP0010", 2, 20),
                };
                MaterialList = manualList;
            }
        }
        /// <summary>
        /// Формирует PDF файл бирки с номером бирки
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public static string MakeLabel(HandlingUnit box)
        {
            DateTime DT = DateTime.Now;
            string filename = @"./pdf/" + box.Number + ".pdf";

            foreach (Material material in MaterialList)
            {
                if (material.Internal == box.Type)
                {
                    label.labelFields[0].Value = "АвтоВАЗ";
                    label.labelFields[1].Value = material.Dock;
                    label.labelFields[4].Value = (material.Weight * box.Amount).ToString();
                    label.labelFields[5].Value = (material.Weight * box.Amount + material.PackWeight).ToString();
                    label.labelFields[6].Value = "1";
                    label.labelFields[7].Value = material.Customer;
                    label.labelFields[13].Value = material.Package;
                    break;
                }
            }
            label.labelFields[2].Value = "992410";
            label.labelFields[8].Value = box.Amount.ToString();
            label.labelFields[9].Value = box.Type;
            label.labelFields[10].Value = box.Number.ToString();
            label.labelFields[11].Value = "M0FAP";
            label.labelFields[12].Value = DT.ToString("dd.MM.yyyy HH:mm:ss");

            PdfFontFactory.RegisterSystemDirectories();
            var fontLibrary = PdfFontFactory.GetRegisteredFonts();
            PdfFont font = PdfFontFactory.CreateRegisteredFont("arial");
            PdfDocument pdfDoc = new(new PdfWriter(filename));
            pdfDoc.SetDefaultPageSize(PageSize.A5);
            Document doc = new(pdfDoc);
            PdfPage page = pdfDoc.AddNewPage()
                .SetIgnorePageRotationForContent(true)
                .SetRotation(90);
            PdfCanvas canvas = new(page);
            Barcode128 barcode = new(pdfDoc);
            double mm = label.MM;

            foreach (int[] b in label.borders)
            {
                canvas
                    .MoveTo(b[2] * mm, b[3] * mm)
                    .LineTo(b[0] * mm, b[1] * mm)
                    .ClosePathStroke();
            }

            foreach (Field field in label.labelFields)
            {
                //   НАЗВАНИЕ ПОЛЯ
                doc.ShowTextAligned(new Paragraph(field.Name).SetFontSize(8).SetFont(font),
                    (float)(field.X * mm), (float)(field.Y * mm), TextAlignment.LEFT, VerticalAlignment.TOP);
                //ЗНАЧЕНИЕ
                doc.ShowTextAligned(new Paragraph(field.Value).SetFontSize(18).SetFont(font),
                    (float)(field.X * mm + 20), (float)(field.Y * mm - 9), TextAlignment.LEFT, VerticalAlignment.TOP);
                //   ШТРИХКОД
                if (field.Barcode == true)
                {
                    barcode.SetCode(field.Code + field.Value);
                    barcode.SetSize(1);
                    barcode.SetBarHeight(32);
                    PdfFormXObject img = barcode.CreateFormXObject(ColorConstants.BLACK, ColorConstants.WHITE, pdfDoc);
                    canvas.AddXObjectAt(img, (float)(field.X * mm + 60), (float)(field.Y * mm - 70));
                }
            }
            canvas.Release();
            doc.Close();
            return (filename);
        }
    }

}
