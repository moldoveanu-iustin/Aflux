using UglyToad.PdfPig;
using System.Text;

namespace CheltuieliApp.Helpers;

public static class PdfTextExtractor
{
    public static string ExtractText(Stream pdfStream)
    {
        using var document = PdfDocument.Open(pdfStream);
        var sb = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            sb.AppendLine(page.Text);
            sb.AppendLine();
        }

        return sb.ToString();
    }
}