﻿using System;
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

                            newstr = deleteWatermarkByProperty(newstr);

                            newstr = deleteWatermarkByPattern(newstr, wm);

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

        private static string deleteWatermarkByProperty(string origin)
        {
            string newstr = origin;
            string wmatch = "<</Subtype /Watermark ";
            int indWM = origin.IndexOf(wmatch);
            if (indWM >= 0)
            {
                int indBegin = origin.IndexOf("BDC", indWM);
                int indBegin2 = origin.LastIndexOf("\n", indBegin, indBegin);
                int indEnd = origin.IndexOf("EMC", indBegin);

                newstr = origin.Remove(indBegin2 + 1, indEnd - indBegin2 + 4);
            }
            return newstr;
        }

        private static string deleteWatermarkByPattern(string origin, string wm)
        {
            string newstr = origin;

            int indBT = newstr.IndexOf("BT");
            while(indBT >= 0)
            {
                int indET = newstr.IndexOf("ET", indBT);
                if(indET == -1)
                {
                    throw new Exception("BT ET Not Match Exception");
                }

                int indTj = newstr.IndexOf("Tj", indBT, indET - indBT);
                if (indTj != -1)
                {
                    int indEParenthesis = newstr.LastIndexOf(")", indTj, indTj - indBT);
                    int indEAngleBracket = newstr.LastIndexOf(">", indTj, indTj - indBT);
                    int indESquareBracket = newstr.LastIndexOf("]", indTj, indTj - indBT);
                    if (indEParenthesis > indEAngleBracket && indEParenthesis > indESquareBracket)
                    {
                        int indParenthesis = newstr.LastIndexOf("(", indEParenthesis, indEParenthesis - indBT);
                        if (indParenthesis == -1)
                        {
                            throw new Exception("Parenthesis Not Match Exception");
                        }
                        string text = newstr.Substring(indParenthesis + 1, indEParenthesis - indParenthesis - 1);
                        if (text.StartsWith(wm))
                        {
                            newstr = newstr.Remove(indBT, indET - indBT + 3);
                            indET = indBT;
                        }
                    }
                    else if (indEAngleBracket > indEParenthesis && indEAngleBracket > indESquareBracket)
                    {
                        int indAngleBracket = newstr.LastIndexOf("<", indEAngleBracket, indEAngleBracket - indBT);
                        if (indAngleBracket == -1)
                        {
                            throw new Exception("AngleBracket Not Match Exception");
                        }
                        //Decode Text //TBD

                    }
                }

                indBT = newstr.IndexOf("BT", indET);         
            }
            return newstr;
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
