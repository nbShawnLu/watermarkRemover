using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;


namespace watermarkRemover
{
    class Program
    {
        static void Main(string[] args)
        {
            string SourceName = args[0].ToString();
            string DecName = SourceName + ".dec.pdf";
            string DestName = SourceName + ".unwmed.pdf";
 
            try
            {
                deletePDFEncrypt(SourceName, DecName);
            }
            catch (Exception ex)
            {
                Console.Write("deletePDFEncrypt Failed: " + ex.Message);
                return;
            }
            Console.WriteLine("Decrypt success, enter the text of watermark: ");
            string wm = Console.ReadLine();
            try
            {
                deletePDFWatermark(DecName, DestName, wm);
            }
            catch (Exception ex)
            {
                Console.Write("deletePDFWatermark Failed: " + ex.Message);
                return;
            }
            
            Console.Write("Success");
        }

        private static void deletePDFWatermark(string SourceName, string DestName, string wm)
        {
            if (string.IsNullOrEmpty(SourceName) || string.IsNullOrEmpty(DestName))
            {
                throw new Exception("Null Name Exception");
            }

            try
            {
                PdfReader src = new PdfReader(SourceName);
                PdfWriter dest = new PdfWriter(DestName);
                src.SetUnethicalReading(true);
                PdfDocument pdfDoc = new PdfDocument(src, dest);

                int n = pdfDoc.GetNumberOfPages();

                for (int i = 1; i <= n; i++)
                {
                    PdfPage page = pdfDoc.GetPage(i);
                    PdfCanvas canvas = new PdfCanvas(page);
                    int m = page.GetContentStreamCount();

                    for (int j = 1; j <=m; j++)
                    {
                        try
                        {
                            PdfStream st = page.GetContentStream(j);
                            byte[] bst = st.GetBytes();
                            string str = System.Text.Encoding.UTF8.GetString(bst);
                            string newstr = str;
                            string wmatch = "<</Subtype /Watermark ";                 
                            int indWM = str.IndexOf(wmatch);
                            if (indWM >= 0)
                            {
                                int indBegin = str.IndexOf("BDC", indWM);
                                int indBegin2 = str.LastIndexOf("\n", indBegin, indBegin);
                                if (indBegin2 == -1) indBegin2 = 0;
                                int indEnd = str.IndexOf("EMC", indBegin);

                                newstr = str.Remove(indBegin2, indEnd - indBegin2 + 4);
                            }
                            byte[] newbst = System.Text.Encoding.UTF8.GetBytes(newstr);
                            st.SetData(newbst);

                        }
                        catch (Exception ex) {;}
                     }
                }
                pdfDoc.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private static void deletePDFEncrypt(string SourceName, string DestName)
        {
            if (string.IsNullOrEmpty(SourceName) || string.IsNullOrEmpty(DestName))
            {
                throw new Exception("Null Name Exception");
            }

            try
            {
                PdfReader src = new PdfReader(SourceName);
                PdfWriter dest = new PdfWriter(DestName);
                src.SetUnethicalReading(true);
                PdfDocument pdfDoc = new PdfDocument(src, dest);

                pdfDoc.Close();
               
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
