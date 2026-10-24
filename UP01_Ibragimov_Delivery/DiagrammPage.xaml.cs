using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;

// === ПСЕВДОНИМЫ ДЛЯ WORD, ЧТОБЫ ИЗБЕЖАТЬ КОНФЛИКТА С EXCEL ===
using WordParagraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using WordRun = DocumentFormat.OpenXml.Wordprocessing.Run;
using WordText = DocumentFormat.OpenXml.Wordprocessing.Text;
using WordBold = DocumentFormat.OpenXml.Wordprocessing.Bold;
using WordTable = DocumentFormat.OpenXml.Wordprocessing.Table;
using WordTableRow = DocumentFormat.OpenXml.Wordprocessing.TableRow;
using WordTableCell = DocumentFormat.OpenXml.Wordprocessing.TableCell;
using WordTableProperties = DocumentFormat.OpenXml.Wordprocessing.TableProperties;
using WordTableBorders = DocumentFormat.OpenXml.Wordprocessing.TableBorders;
using WordTopBorder = DocumentFormat.OpenXml.Wordprocessing.TopBorder;
using WordBottomBorder = DocumentFormat.OpenXml.Wordprocessing.BottomBorder;
using WordLeftBorder = DocumentFormat.OpenXml.Wordprocessing.LeftBorder;
using WordRightBorder = DocumentFormat.OpenXml.Wordprocessing.RightBorder;
using WordInsideHorizontalBorder = DocumentFormat.OpenXml.Wordprocessing.InsideHorizontalBorder;
using WordInsideVerticalBorder = DocumentFormat.OpenXml.Wordprocessing.InsideVerticalBorder;
using WordRunProperties = DocumentFormat.OpenXml.Wordprocessing.RunProperties;
using WordColor = DocumentFormat.OpenXml.Wordprocessing.Color;
using WordJustification = DocumentFormat.OpenXml.Wordprocessing.Justification;
using WordParagraphProperties = DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties;
using WordBreak = DocumentFormat.OpenXml.Wordprocessing.Break;

namespace UP01_Ibragimov_Delivery
{
    public partial class DiagrammPage : System.Windows.Controls.Page
    {
        public DiagrammPage()
        {
            InitializeComponent();

            ChartPayments.ChartAreas.Add(new ChartArea("Main"));
            var currentSeries = new Series("Платежи") { IsValueShownAsLabel = true };
            ChartPayments.Series.Add(currentSeries);

            CmbUser.ItemsSource = Entities.GetContext().User.ToList();
            CmbDiagram.ItemsSource = Enum.GetValues(typeof(SeriesChartType));
        }

        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (CmbUser.SelectedItem is User currentUser && CmbDiagram.SelectedItem is SeriesChartType currentType)
            {
                Series currentSeries = ChartPayments.Series.FirstOrDefault();
                currentSeries.ChartType = currentType;
                currentSeries.Points.Clear();

                using (var db = new Entities())
                {
                    var categoriesList = db.Category.ToList();
                    foreach (var category in categoriesList)
                    {
                        var sum = db.Payment.ToList()
                            .Where(p => p.UserID == currentUser.ID && p.CategoryID == category.ID)
                            .Sum(p => p.Price * p.Num);

                        currentSeries.Points.AddXY(category.Name, sum);
                    }
                }
            }
        }

        // ==========================================
        // ЭКСПОРТ В WORD (используем псевдонимы Word...)
        // ==========================================
        private void BtnExportWord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filePath = @"C:\Users\Давид\Payments.docx";

                using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
                {
                    MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    Body body = mainPart.Document.AppendChild(new Body());

                    var allUsers = Entities.GetContext().User.ToList();
                    var allCategories = Entities.GetContext().Category.ToList();

                    foreach (var user in allUsers)
                    {
                        // Заголовок
                        WordParagraph para = body.AppendChild(new WordParagraph());
                        WordRun run = para.AppendChild(new WordRun());
                        run.AppendChild(new WordText("Пользователь: " + user.FIO));
                        run.PrependChild(new WordRunProperties(new WordBold()));
                        para.ParagraphProperties = new WordParagraphProperties(new WordJustification() { Val = JustificationValues.Center });

                        // Таблица
                        WordTable table = new WordTable();
                        WordTableProperties tblProp = new WordTableProperties(
                            new WordTableBorders(
                                new WordTopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                new WordBottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                new WordLeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                new WordRightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                new WordInsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                new WordInsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 }
                            )
                        );
                        table.AppendChild(tblProp);

                        WordTableRow headerRow = new WordTableRow();
                        headerRow.AppendChild(CreateCell("Категория", true));
                        headerRow.AppendChild(CreateCell("Сумма расходов", true));
                        table.AppendChild(headerRow);

                        foreach (var category in allCategories)
                        {
                            WordTableRow row = new WordTableRow();
                            row.AppendChild(CreateCell(category.Name, false));
                            var sum = user.Payment.ToList().Where(p => p.CategoryID == category.ID).Sum(p => p.Num * p.Price);
                            row.AppendChild(CreateCell(sum.ToString("N2") + " руб.", false));
                            table.AppendChild(row);
                        }
                        body.AppendChild(table);

                        // Самый дорогой / дешевый
                        var maxPayment = user.Payment.OrderByDescending(p => p.Price * p.Num).FirstOrDefault();
                        var minPayment = user.Payment.OrderBy(p => p.Price * p.Num).FirstOrDefault();

                        if (maxPayment != null)
                        {
                            WordParagraph maxPara = body.AppendChild(new WordParagraph());
                            WordRun maxRun = maxPara.AppendChild(new WordRun());
                            maxRun.AppendChild(new WordText($"Самый дорогостоящий заказ - {maxPayment.Name} за {(maxPayment.Price * maxPayment.Num).ToString("N2")} руб. от {maxPayment.Date:dd.MM.yyyy}"));
                            maxRun.PrependChild(new WordRunProperties(new WordColor() { Val = "C00000" }));
                        }

                        if (minPayment != null)
                        {
                            WordParagraph minPara = body.AppendChild(new WordParagraph());
                            WordRun minRun = minPara.AppendChild(new WordRun());
                            minRun.AppendChild(new WordText($"Самый дешевый заказ - {minPayment.Name} за {(minPayment.Price * minPayment.Num).ToString("N2")} руб. от {minPayment.Date:dd.MM.yyyy}"));
                            minRun.PrependChild(new WordRunProperties(new WordColor() { Val = "008000" }));
                        }

                        if (user != allUsers.LastOrDefault())
                        {
                            body.AppendChild(new WordParagraph(new WordRun(new WordBreak() { Type = BreakValues.Page })));
                        }
                    }
                }

                MessageBox.Show($"Документ Word успешно создан по пути:\n{filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при создании документа: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Вспомогательный метод для Word (используем псевдонимы)
        private WordTableCell CreateCell(string text, bool isBold)
        {
            WordTableCell cell = new WordTableCell();
            WordParagraph p = new WordParagraph();
            WordRun r = new WordRun();
            r.AppendChild(new WordText(text));
            if (isBold) r.PrependChild(new WordRunProperties(new WordBold()));
            p.AppendChild(r);
            cell.AppendChild(p);
            return cell;
        }

        // ==========================================
        // ЭКСПОРТ В EXCEL (без конфликтов, используем классы без приставок)
        // ==========================================
        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filePath = @"C:\Users\Давид\Payments.xlsx";

                using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
                {
                    WorkbookPart workbookPart = spreadsheetDocument.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(new SheetData());

                    Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());
                    Sheet sheet = new Sheet() { Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Отчет" };
                    sheets.Append(sheet);

                    SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                    var allUsers = Entities.GetContext().User.ToList();
                    var allCategories = Entities.GetContext().Category.ToList();

                    uint rowIndex = 1;

                    foreach (var user in allUsers)
                    {
                        Row userRow = new Row() { RowIndex = rowIndex };
                        userRow.Append(CreateTextCell("Пользователь: " + user.FIO, true));
                        sheetData.Append(userRow);
                        rowIndex++;

                        Row headerRow = new Row() { RowIndex = rowIndex };
                        headerRow.Append(CreateTextCell("Категория", true));
                        headerRow.Append(CreateTextCell("Сумма расходов", true));
                        sheetData.Append(headerRow);
                        rowIndex++;

                        foreach (var category in allCategories)
                        {
                            Row dataRow = new Row() { RowIndex = rowIndex };
                            dataRow.Append(CreateTextCell(category.Name, false));
                            var sum = user.Payment.ToList().Where(p => p.CategoryID == category.ID).Sum(p => p.Num * p.Price);
                            dataRow.Append(CreateTextCell(sum.ToString("N2") + " руб.", false));
                            sheetData.Append(dataRow);
                            rowIndex++;
                        }

                        var maxPayment = user.Payment.OrderByDescending(p => p.Price * p.Num).FirstOrDefault();
                        var minPayment = user.Payment.OrderBy(p => p.Price * p.Num).FirstOrDefault();

                        if (maxPayment != null)
                        {
                            Row maxRow = new Row() { RowIndex = rowIndex };
                            maxRow.Append(CreateTextCell($"Самый дорогостоящий заказ: {maxPayment.Name} за {(maxPayment.Price * maxPayment.Num).ToString("N2")} руб.", false));
                            sheetData.Append(maxRow);
                            rowIndex++;
                        }

                        if (minPayment != null)
                        {
                            Row minRow = new Row() { RowIndex = rowIndex };
                            minRow.Append(CreateTextCell($"Самый дешевый заказ: {minPayment.Name} за {(minPayment.Price * minPayment.Num).ToString("N2")} руб.", false));
                            sheetData.Append(minRow);
                            rowIndex++;
                        }

                        rowIndex++;
                    }

                    workbookPart.Workbook.Save();
                }

                MessageBox.Show($"Файл Excel успешно создан по пути:\n{filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при создании Excel: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Cell CreateTextCell(string text, bool isBold)
        {
            Cell cell = new Cell();
            cell.DataType = CellValues.String;
            cell.CellValue = new CellValue(text);
            return cell;
        }
    }
}