﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using MathWorks.MATLAB.NET.Utility;
using MathWorks.MATLAB.NET.Arrays;
using MatlabProject;

namespace ImageProcessingAssignment1
{
    class ImageClass
    {
        //Open Image
        public void openBmpJpeg(string picPath, ref PictureInfo newPictureItem, string PictureName, PictureBox picBox)
        {
            Bitmap Bmp = new Bitmap(picPath);
            int width = Bmp.Width;
            int height = Bmp.Height;
            newPictureItem = new PictureInfo(width, height, PictureName, picBox, new byte[height, width], new byte[height, width], new byte[height, width]);
            BitmapData bmpData = Bmp.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, Bmp.PixelFormat);
            unsafe
            {
                byte* p = (byte*)bmpData.Scan0;
                if (Bmp.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    int space = bmpData.Stride - width * 3;
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            newPictureItem.bluePixels[i, j] = p[0];
                            newPictureItem.greenPixels[i, j] = p[1];
                            newPictureItem.redPixels[i, j] = p[2];
                            p += 3;
                        }
                        p += space;
                    }
                }
                else if (Bmp.PixelFormat == PixelFormat.Format32bppArgb || Bmp.PixelFormat == PixelFormat.Format32bppRgb)
                {
                    int space = bmpData.Stride - width * 4;
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            newPictureItem.bluePixels[i, j] = p[0];
                            newPictureItem.greenPixels[i, j] = p[1];
                            newPictureItem.redPixels[i, j] = p[2];
                            p += 4;
                        }
                        p += space;
                    }
                }
                else if (Bmp.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    int space = bmpData.Stride - width;
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            newPictureItem.bluePixels[i, j] = p[0];
                            newPictureItem.greenPixels[i, j] = p[0];
                            newPictureItem.redPixels[i, j] = p[0];
                            p++;
                        }
                        p += space;
                    }
                }
            }
            Bmp.UnlockBits(bmpData);
        }
        public void openPPM(string picPath, ref PictureInfo newPictureItem, string PictureName, PictureBox picBox)
        {
            FileStream FS = new FileStream(picPath, FileMode.Open);
            StreamReader SR = new StreamReader(FS);
            string type = SR.ReadLine();
            int Size_P6 = type.Length + 1;
            string temp;
            string MetaData = "";
            while (true)
            {
                temp = SR.ReadLine();
                if (temp[0] == '#')
                {
                    MetaData += temp;
                    Size_P6 += temp.Length + 1;
                }
                else
                    break;
            }
            string Width_Height = temp;
            Size_P6 += temp.Length + 2;
            int FirstIndex = Width_Height.IndexOf(' ');
            int width = int.Parse(Width_Height.Substring(0, FirstIndex));
            int height = int.Parse(Width_Height.Substring(FirstIndex + 1, Width_Height.Length - FirstIndex - 1));
            int Size = width * height * 3;
            temp = SR.ReadLine();
            int MaxValue = int.Parse(temp);
            Size_P6 += type.Length + 1;
            newPictureItem = new PictureInfo(width, height, PictureName, picBox, new byte[height, width], new byte[height, width], new byte[height, width]);
            if (type.ToLower() == "p3")
            {
                string Values = SR.ReadToEnd();
                string[] tempValues = Values.Split(new char[] { ' ' });
                int ind = 0;
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        newPictureItem.redPixels[i, j] = new byte();
                        newPictureItem.redPixels[i, j] = byte.Parse(tempValues[ind++]);
                        newPictureItem.greenPixels[i, j] = new byte();
                        newPictureItem.greenPixels[i, j] = byte.Parse(tempValues[ind++]);
                        newPictureItem.bluePixels[i, j] = new byte();
                        newPictureItem.bluePixels[i, j] = byte.Parse(tempValues[ind++]);
                    }
                }
            }
            else if (type.ToLower() == "p6")
            {
                FS.Position = Size_P6;
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        newPictureItem.redPixels[i, j] = new byte();
                        newPictureItem.redPixels[i, j] = (byte)FS.ReadByte();
                        newPictureItem.greenPixels[i, j] = new byte();
                        newPictureItem.greenPixels[i, j] = (byte)FS.ReadByte();
                        newPictureItem.bluePixels[i, j] = new byte();
                        newPictureItem.bluePixels[i, j] = (byte)FS.ReadByte();
                    }
                }
            }
            FS.Close();
        }

        //=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*

        //Save as
        public void SaveAsPPM(string PicturePath, PictureInfo pic, int FilterIndex)
        {
            FileStream FS = new FileStream(PicturePath, FileMode.Create);
            StreamWriter SW = new StreamWriter(FS);
            int height = pic.height;
            int width = pic.width;
            int MaxValue = 255;
            if (FilterIndex == 3)
                SaveAsP3(SW, height, width, MaxValue, pic);
            else
                SaveAsP6(FS, height, width, MaxValue, pic);
            FS.Close();

        }
        public void SaveAsP3(StreamWriter SW, int height, int width, int MaxValue, PictureInfo pic)
        {
            try
            {
                SW.WriteLine("P3");
                SW.WriteLine(width.ToString() + " " + height.ToString());
                SW.WriteLine(MaxValue.ToString());
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        string PixString = pic.redPixels[i, j].ToString() + " ";
                        PixString += (pic.greenPixels[i, j].ToString() + " ");
                        PixString += (pic.bluePixels[i, j].ToString() + " ");
                        SW.Write(PixString);
                    }
                }
                SW.Close();
            }
            catch { }
        }
        public void SaveAsP6(FileStream FS, int height, int width, int MaxValue, PictureInfo pic)
        {
            try
            {
                string str = "P6\n" + width.ToString() + " " + height.ToString() + "\n" + MaxValue.ToString() + "\n";
                int Length = str.Length;
                byte[] Header = new byte[Length];
                for (int i = 0; i < Length; i++)
                {
                    Header[i] = (byte)(str[i]);
                }
                FS.Write(Header, 0, Length);

                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        FS.WriteByte(pic.redPixels[i, j]);
                        FS.WriteByte(pic.greenPixels[i, j]);
                        FS.WriteByte(pic.bluePixels[i, j]);
                    }
                }
            }
            catch { }
        }

        //=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*
        //Image operations

        public void TranslateImage(PictureInfo pic, int xTranslation, int yTranslation)
        {
            int height = pic.height;
            int width = pic.width;
            byte[,] Red = new byte[height, width];
            byte[,] Green = new byte[height, width];
            byte[,] Blue = new byte[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int newYCoor = (i + yTranslation) % height;
                    int newXCoor = (j + xTranslation) % width;
                    if (newYCoor < 0) newYCoor += height;
                    if (newXCoor < 0) newXCoor += width;
                    Red[newYCoor, newXCoor] = pic.redPixels[i, j];
                    Green[newYCoor, newXCoor] = pic.greenPixels[i, j];
                    Blue[newYCoor, newXCoor] = pic.bluePixels[i, j];
                }
            }
            pic.redPixels = Red;
            pic.greenPixels = Green;
            pic.bluePixels = Blue;
        }
        public void RotateImage(PictureInfo pic, double _Theta)
        {
            int height = pic.height;
            int width = pic.width;
            _Theta *= -1;
            double Theta = _Theta * Math.PI / 180;
            double cosTheta = Math.Cos(Theta);
            double sinTheta = Math.Sin(Theta);
            int newHeight = 0, newWidth = 0;
            int xValue = 0, yValue = 0;
            if (_Theta < 0)
            {
                xValue = -(int)(width * sinTheta);
                newHeight = (int)(height * cosTheta) + xValue + 1;
                newWidth = (int)(-height * sinTheta + width * cosTheta);
            }
            else
            {
                yValue = (int)(height * sinTheta);
                newHeight = (int)(height * cosTheta + width * sinTheta);
                newWidth = (int)(width * cosTheta) + yValue + 1;
            }
            byte[,] Red = new byte[newHeight, newWidth];
            byte[,] Green = new byte[newHeight, newWidth];
            byte[,] Blue = new byte[newHeight, newWidth];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int _X = (int)Math.Round(i * cosTheta + j * sinTheta) + xValue;
                    int _Y = (int)Math.Round(-i * sinTheta + j * cosTheta) + yValue;
                    Red[_X, _Y] = pic.redPixels[i, j];
                    Green[_X, _Y] = pic.greenPixels[i, j];
                    Blue[_X, _Y] = pic.bluePixels[i, j];
                }
            }
            pic.height = newHeight;
            pic.width = newWidth;
            pic.redPixels = Red;
            pic.greenPixels = Green;
            pic.bluePixels = Blue;
        }
        public void ShearImage(PictureInfo pic, int Shear)
        {
            int height = pic.height;
            int width = pic.width;
            int newHeight = 0;
            int newWidth = 0;
            int index = 0;
            newHeight = height;
            if (Shear < 0) newWidth = height * (-Shear) + width;
            else newWidth = height * Shear + width;
            byte[,] Red = new byte[newHeight, newWidth];
            byte[,] Green = new byte[newHeight, newWidth];
            byte[,] Blue = new byte[newHeight, newWidth];
            if (Shear > 0)
            {
                for (int i = height - 1; i >= 0; i--)
                {
                    for (int j = 0; j < width; j++)
                    {
                        int newX = i;
                        int newY = j;
                        newY = index * Shear + j;
                        Red[newX, newY] = pic.redPixels[i, j];
                        Green[newX, newY] = pic.greenPixels[i, j];
                        Blue[newX, newY] = pic.bluePixels[i, j];
                    }
                    index++;
                }
            }
            else
            {
                Shear *= -1;
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        int newX = i;
                        int newY = j;
                        newY = index * Shear + j;
                        Red[newX, newY] = pic.redPixels[i, j];
                        Green[newX, newY] = pic.greenPixels[i, j];
                        Blue[newX, newY] = pic.bluePixels[i, j];
                    }
                    index++;
                }
            }
            pic.height = newHeight;
            pic.width = newWidth;
            pic.redPixels = Red;
            pic.greenPixels = Green;
            pic.bluePixels = Blue;
        }
        public void FlipImage(PictureInfo pic, int flipType)
        {
            int height = pic.height;
            int width = pic.width;
            byte[,] Red = new byte[height, width];
            byte[,] Green = new byte[height, width];
            byte[,] Blue = new byte[height, width];
            if (flipType == 0)
            {
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        Red[i, width - j - 1] = pic.redPixels[i, j];
                        Green[i, width - j - 1] = pic.greenPixels[i, j];
                        Blue[i, width - j - 1] = pic.bluePixels[i, j];
                    }
                }
            }
            else
            {
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        Red[height - i - 1, j] = pic.redPixels[i, j];
                        Green[height - i - 1, j] = pic.greenPixels[i, j];
                        Blue[height - i - 1, j] = pic.bluePixels[i, j];
                    }
                }
            }
            pic.redPixels = Red;
            pic.greenPixels = Green;
            pic.bluePixels = Blue;
        }
        public void ResizeImage(PictureInfo PicInfo, int NewH, int NewW)
        {
            int height = PicInfo.height;
            int width = PicInfo.width;
            byte[,] Red = new byte[NewH, NewW];
            byte[,] Green = new byte[NewH, NewW];
            byte[,] Blue = new byte[NewH, NewW];
            double W_Ratio = (double)width / (double)NewW;
            double H_Ratio = (double)height / (double)NewH;
            for (int j = 0; j < NewH; j++)
            {
                for (int i = 0; i < NewW; i++)
                {
                    //Calculate the corresponding location in the original buffer
                    double OldX = i * W_Ratio;
                    double OldY = j * H_Ratio;

                    //Find 4 adjacent pixels
                    int X_1 = (int)Math.Floor(OldX);
                    int X_2 = Math.Min((int)Math.Ceiling(OldX), width - 1);
                    int Y_1 = (int)Math.Floor(OldY);
                    int Y_2 = Math.Min((int)Math.Ceiling(OldY), height - 1);

                    Pixel Point_1 = new Pixel(PicInfo.redPixels[Y_1, X_1], PicInfo.greenPixels[Y_1, X_1], PicInfo.bluePixels[Y_1, X_1]);
                    Pixel Point_3 = new Pixel(PicInfo.redPixels[Y_1, X_2], PicInfo.greenPixels[Y_1, X_2], PicInfo.bluePixels[Y_1, X_2]);
                    Pixel Point_2 = new Pixel(PicInfo.redPixels[Y_2, X_1], PicInfo.greenPixels[Y_2, X_1], PicInfo.bluePixels[Y_2, X_1]);
                    Pixel Point_4 = new Pixel(PicInfo.redPixels[Y_2, X_2], PicInfo.greenPixels[Y_2, X_2], PicInfo.bluePixels[Y_2, X_2]);

                    //Calculate X, Y fractions
                    double X_Fraction = OldX - X_1;
                    double Y_Fraction = OldY - Y_1;

                    //Interpolate in X-Direction
                    //float Z_1 = Point_1 * (1 - X_Fraction) + Point_2 * X_Fraction;
                    //float Z_2 = Point_3 * (1 - X_Fraction) + Point_4 * X_Fraction;
                    double Z_1_R = Point_1.GetR() * (1 - X_Fraction) + Point_2.GetR() * X_Fraction;
                    double Z_1_G = Point_1.GetG() * (1 - X_Fraction) + Point_2.GetG() * X_Fraction;
                    double Z_1_B = Point_1.GetB() * (1 - X_Fraction) + Point_2.GetB() * X_Fraction;

                    //Interpolate in Y-Direction
                    //int NewPixel = Z_1 * (1 - Y_Fraction) + Z_2 * Y_Fraction;
                    double Z_2_R = Point_3.GetR() * (1 - X_Fraction) + Point_4.GetR() * X_Fraction;
                    double Z_2_G = Point_3.GetG() * (1 - X_Fraction) + Point_4.GetG() * X_Fraction;
                    double Z_2_B = Point_3.GetB() * (1 - X_Fraction) + Point_4.GetB() * X_Fraction;

                    //Add the New Pixel
                    Red[j, i] = (byte)(Z_1_R * (1 - Y_Fraction) + Z_2_R * Y_Fraction);
                    Green[j, i] = (byte)(Z_1_G * (1 - Y_Fraction) + Z_2_G * Y_Fraction);
                    Blue[j, i] = (byte)(Z_1_B * (1 - Y_Fraction) + Z_2_B * Y_Fraction);
                }
            }
            PicInfo.pictureBox.Width = PicInfo.width = NewW;
            PicInfo.pictureBox.Height = PicInfo.height = NewH;
            PicInfo.redPixels = Red;
            PicInfo.greenPixels = Green;
            PicInfo.bluePixels = Blue;
        }
        public void ResizeImage(PictureInfo PicInfo, int NewH, int NewW, ref byte[,] Red, ref byte[,] Green, ref byte[,] Blue)
        {
            int height = PicInfo.height;
            int width = PicInfo.width;
            double W_Ratio = (double)width / (double)NewW;
            double H_Ratio = (double)height / (double)NewH;
            for (int j = 0; j < NewH; j++)
            {
                for (int i = 0; i < NewW; i++)
                {
                    //Calculate the corresponding location in the original buffer
                    double OldX = i * W_Ratio;
                    double OldY = j * H_Ratio;

                    //Find 4 adjacent pixels
                    int X_1 = (int)Math.Floor(OldX);
                    int X_2 = Math.Min((int)Math.Ceiling(OldX), width - 1);
                    int Y_1 = (int)Math.Floor(OldY);
                    int Y_2 = Math.Min((int)Math.Ceiling(OldY), height - 1);

                    Pixel Point_1 = new Pixel(PicInfo.redPixels[Y_1, X_1], PicInfo.greenPixels[Y_1, X_1], PicInfo.bluePixels[Y_1, X_1]);
                    Pixel Point_3 = new Pixel(PicInfo.redPixels[Y_1, X_2], PicInfo.greenPixels[Y_1, X_2], PicInfo.bluePixels[Y_1, X_2]);
                    Pixel Point_2 = new Pixel(PicInfo.redPixels[Y_2, X_1], PicInfo.greenPixels[Y_2, X_1], PicInfo.bluePixels[Y_2, X_1]);
                    Pixel Point_4 = new Pixel(PicInfo.redPixels[Y_2, X_2], PicInfo.greenPixels[Y_2, X_2], PicInfo.bluePixels[Y_2, X_2]);

                    //Calculate X, Y fractions
                    double X_Fraction = OldX - X_1;
                    double Y_Fraction = OldY - Y_1;

                    //Interpolate in X-Direction
                    //float Z_1 = Point_1 * (1 - X_Fraction) + Point_2 * X_Fraction;
                    //float Z_2 = Point_3 * (1 - X_Fraction) + Point_4 * X_Fraction;
                    double Z_1_R = Point_1.GetR() * (1 - X_Fraction) + Point_2.GetR() * X_Fraction;
                    double Z_1_G = Point_1.GetG() * (1 - X_Fraction) + Point_2.GetG() * X_Fraction;
                    double Z_1_B = Point_1.GetB() * (1 - X_Fraction) + Point_2.GetB() * X_Fraction;

                    //Interpolate in Y-Direction
                    //int NewPixel = Z_1 * (1 - Y_Fraction) + Z_2 * Y_Fraction;
                    double Z_2_R = Point_3.GetR() * (1 - X_Fraction) + Point_4.GetR() * X_Fraction;
                    double Z_2_G = Point_3.GetG() * (1 - X_Fraction) + Point_4.GetG() * X_Fraction;
                    double Z_2_B = Point_3.GetB() * (1 - X_Fraction) + Point_4.GetB() * X_Fraction;

                    //Add the New Pixel
                    Red[j, i] = (byte)(Z_1_R * (1 - Y_Fraction) + Z_2_R * Y_Fraction);
                    Green[j, i] = (byte)(Z_1_G * (1 - Y_Fraction) + Z_2_G * Y_Fraction);
                    Blue[j, i] = (byte)(Z_1_B * (1 - Y_Fraction) + Z_2_B * Y_Fraction);
                }
            }
        }
        public void ReverseColors(PictureInfo pic)
        {
            int height = pic.height;
            int width = pic.width;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    pic.redPixels[i, j] = (byte)(255 - (int)pic.redPixels[i, j]);
                    pic.greenPixels[i, j] = (byte)(255 - (int)pic.greenPixels[i, j]);
                    pic.bluePixels[i, j] = (byte)(255 - (int)pic.bluePixels[i, j]);
                }
            }
        }
        public void GrayScale(PictureInfo pic)
        {
            int width = pic.width;
            int height = pic.height;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int temp = ((int)pic.redPixels[i, j] + (int)pic.greenPixels[i, j] + (int)pic.bluePixels[i, j]) / 3;
                    pic.redPixels[i, j] = (byte)temp;
                    pic.greenPixels[i, j] = (byte)temp;
                    pic.bluePixels[i, j] = (byte)temp;
                }
            }
        }
        public void histogramEqualization(PictureInfo pic, PictureInfo picSelected)
        {
            int height = pic.height;
            int width = pic.width;
            double[] RedHistogram = new double[256];
            double[] GreenHistogram = new double[256];
            double[] BlueHistogram = new double[256];
            GetHistogram(ref RedHistogram, ref GreenHistogram, ref BlueHistogram, picSelected);
            RunningSumToRound(ref RedHistogram);
            RunningSumToRound(ref GreenHistogram);
            RunningSumToRound(ref BlueHistogram);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    pic.redPixels[i, j] = (byte)RedHistogram[pic.redPixels[i, j]];
                    pic.greenPixels[i, j] = (byte)GreenHistogram[pic.greenPixels[i, j]];
                    pic.bluePixels[i, j] = (byte)BlueHistogram[pic.bluePixels[i, j]];
                }
            }
        }

        //Brigthness
        public void ChangeBrightness(int height, int width, double lastBrightness, byte[,] tempRPixelArray, byte[,] tempGPixelArray, byte[,] tempBPixelArray, byte[,] modifiedRPixelArray, byte[,] modifiedGPixelArray, byte[,] modifiedBPixelArray)
        {
            modifiedRPixelArray = BrightnessHelp(height, width, lastBrightness, tempRPixelArray);
            modifiedGPixelArray = BrightnessHelp(height, width, lastBrightness, tempGPixelArray);
            modifiedBPixelArray = BrightnessHelp(height, width, lastBrightness, tempBPixelArray);
        }
        private byte[,] BrightnessHelp(int height, int width, double lastBrightness, byte[,] tempPixelArray)
        {
            double BrightnessTemp = 0;
            byte[,] modifiedPixelArray = new byte[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    BrightnessTemp = ((double)tempPixelArray[i, j] + lastBrightness);
                    modifiedPixelArray[i, j] = (byte)(CutOffValue(BrightnessTemp));
                }
            }
            return modifiedPixelArray;
        }

        //Contrast
        public void ChangeContrastint(int height, int width, double lastContrast, byte[,] tempRPixelArray, byte[,] tempGPixelArray, byte[,] tempBPixelArray, byte[,] modifiedRPixelArray, byte[,] modifiedGPixelArray, byte[,] modifiedBPixelArray)
        {
            modifiedRPixelArray = ContrastHelp(height, width, lastContrast, tempRPixelArray);
            modifiedGPixelArray = ContrastHelp(height, width, lastContrast, tempGPixelArray);
            modifiedBPixelArray = ContrastHelp(height, width, lastContrast, tempBPixelArray);
        }
        private byte[,] ContrastHelp(int height, int width, double lastContrast, byte[,] tempPixelArray)
        {
            double oldMin = double.MaxValue, oldMax = double.MinValue;
            double[,] Contrast = new double[height, width];
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    Contrast[i, j] = (double)tempPixelArray[i, j];
            getMaxAndMin(height, width, ref oldMin, ref oldMax, Contrast);
            double newMin = oldMin - lastContrast, newMax = oldMax + lastContrast;
            byte[,] modifiedPixelArray = new byte[height, width];
            Normalization(height, width, oldMin, oldMax, newMax, newMin, Contrast);
            CutOff(height, width, 255, 0, Contrast);
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    modifiedPixelArray[i, j] = (byte)Contrast[i, j];
            return modifiedPixelArray;
        }

        //Gamma Correction
        public void GammaCorrection(int height, int width, double Gamma, byte[,] tempRPixelArray, byte[,] tempGPixelArray, byte[,] tempBPixelArray, byte[,] modifiedRPixelArray, byte[,] modifiedGPixelArray, byte[,] modifiedBPixelArray)
        {
            modifiedRPixelArray = GammaHelp(height, width, Gamma, tempRPixelArray);
            modifiedGPixelArray = GammaHelp(height, width, Gamma, tempGPixelArray);
            modifiedBPixelArray = GammaHelp(height, width, Gamma, tempBPixelArray);
        }
        private byte[,] GammaHelp(int height, int width, double lastGamma, byte[,] tempPixelArray)
        {
            double oldMin = double.MaxValue, oldMax = double.MinValue;
            double newMin = 0, newMax = 255;
            double[,] Gamma = new double[height, width];
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    Gamma[i, j] = Math.Pow((double)tempPixelArray[i, j], 1 / lastGamma);
            getMaxAndMin(height, width, ref oldMin, ref oldMax, Gamma);
            byte[,] modifiedPixelArray = new byte[height, width];
            Normalization(height, width, oldMin, oldMax, newMax, newMin, Gamma);
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    modifiedPixelArray[i, j] = (byte)Gamma[i, j];
            return modifiedPixelArray;
        }

        public void HistogramMatching(int height, int width, PictureInfo FirstPic, PictureInfo SecondPic, List<PictureInfo> picList)
        {
            double[] RedHistogram1 = new double[256];
            double[] GreenHistogram1 = new double[256];
            double[] BlueHistogram1 = new double[256];
            double[] RedHistogram2 = new double[256];
            double[] GreenHistogram2 = new double[256];
            double[] BlueHistogram2 = new double[256];
            int length = picList.Count - 1;
            GetHistogram(ref RedHistogram1, ref GreenHistogram1, ref BlueHistogram1, FirstPic);
            GetHistogram(ref RedHistogram2, ref GreenHistogram2, ref BlueHistogram2, SecondPic);
            RunningSumToRound(ref RedHistogram1);
            RunningSumToRound(ref GreenHistogram1);
            RunningSumToRound(ref BlueHistogram1);
            RunningSumToRound(ref RedHistogram2);
            RunningSumToRound(ref GreenHistogram2);
            RunningSumToRound(ref BlueHistogram2);
            picList[length].redPixels = new byte[height, width];
            picList[length].greenPixels = new byte[height, width];
            picList[length].bluePixels = new byte[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    picList[length].redPixels[i, j] = (byte)GetClose(RedHistogram1[FirstPic.redPixels[i, j]], RedHistogram2);
                    picList[length].greenPixels[i, j] = (byte)GetClose(GreenHistogram1[FirstPic.greenPixels[i, j]], GreenHistogram2);
                    picList[length].bluePixels[i, j] = (byte)GetClose(BlueHistogram1[FirstPic.bluePixels[i, j]], BlueHistogram2);
                }
            }
        }
        private double GetClose(double Value, double[] Array)
        {
            int count = Array.Count();
            for (int i = 0; i < count; i++)
                if (Array[i] == Value || Array[i] == Value - 1 || Array[i] == Value + 1)
                    return i;
            return Value;
        }

        public void ApplyQuantization(int height, int width, int binary, byte[,] modifiedRPixelArray, byte[,] modifiedGPixelArray, byte[,] modifiedBPixelArray,PictureInfo PicParent)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    modifiedRPixelArray[i, j] = (byte)((int)PicParent.redPixels[i, j] & binary);
                    modifiedGPixelArray[i, j] = (byte)((int)PicParent.greenPixels[i, j] & binary);
                    modifiedBPixelArray[i, j] = (byte)((int)PicParent.bluePixels[i, j] & binary);
                }
            }
        }

        //=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*

        //Arithmetic Operations
        public void AddSubtractTwoPictures(int height, int width, byte[,] fRed, byte[,] fGreen, byte[,] fBlue, byte[,] sRed, byte[,] sGreen, byte[,] sBlue, PictureInfo pic, int operation)
        {
            double rMin = double.MaxValue, rMax = double.MinValue, newMin = 0, newMax = 255;
            double gMin = double.MaxValue, gMax = double.MinValue;
            double bMin = double.MaxValue, bMax = double.MinValue;

            double[,] TempR = new double[height, width];
            double[,] TempG = new double[height, width];
            double[,] TempB = new double[height, width];
            if (operation == 0)//Add
            {
                Add(height, width, ref TempR, ref fRed, ref sRed);
                Add(height, width, ref TempG, ref fGreen, ref sGreen);
                Add(height, width, ref TempB, ref fBlue, ref sBlue);
            }
            else if (operation == 1)//Subtract
            {
                Subract(height, width, ref TempR, ref fRed, ref sRed);
                Subract(height, width, ref TempG, ref fGreen, ref sGreen);
                Subract(height, width, ref TempB, ref fBlue, ref sBlue);
            }
            else if (operation == 2)//Product
            {
                Product(height, width, ref TempR, ref fRed, ref sRed);
                Product(height, width, ref TempG, ref fGreen, ref sGreen);
                Product(height, width, ref TempB, ref fBlue, ref sBlue);
            }
            else if (operation == 3)//AND
            {
                AND(height, width, ref TempR, ref fRed, ref sRed);
                AND(height, width, ref TempG, ref fGreen, ref sGreen);
                AND(height, width, ref TempB, ref fBlue, ref sBlue);
            }
            else if (operation == 4)//OR
            {
                OR(height, width, ref TempR, ref fRed, ref sRed);
                OR(height, width, ref TempG, ref fGreen, ref sGreen);
                OR(height, width, ref TempB, ref fBlue, ref sBlue);
            }
            getMaxAndMin(height, width, ref rMin, ref rMax, TempR);
            getMaxAndMin(height, width, ref gMin, ref gMax, TempG);
            getMaxAndMin(height, width, ref bMin, ref bMax, TempB);
            Normalization(height, width, rMin, rMax, newMax, newMin, TempR);
            Normalization(height, width, gMin, gMax, newMax, newMin, TempG);
            Normalization(height, width, bMin, bMax, newMax, newMin, TempB);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    pic.redPixels[i, j] = (byte)TempR[i, j];
                    pic.greenPixels[i, j] = (byte)TempG[i, j];
                    pic.bluePixels[i, j] = (byte)TempB[i, j];
                }
            }
        }
        private void Add(int height, int width, ref double[,] temp, ref byte[,] first, ref byte[,] second)
        {
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    temp[i, j] = (double)first[i, j] + (double)second[i, j];
        }
        private void Subract(int height, int width, ref double[,] temp, ref byte[,] first, ref byte[,] second)
        {
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    temp[i, j] = (double)first[i, j] - (double)second[i, j];
        }
        private void Product(int height, int width, ref double[,] temp, ref byte[,] first, ref byte[,] second)
        {
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    temp[i, j] = (double)first[i, j] * (double)second[i, j];
        }
        private void AND(int height, int width, ref double[,] temp, ref byte[,] first, ref byte[,] second)
        {
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    temp[i, j] = first[i, j] & second[i, j];
        }
        private void OR(int height, int width, ref double[,] temp, ref byte[,] first, ref byte[,] second)
        {
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    temp[i, j] = first[i, j] | second[i, j];
        }

        //=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*
        //Histogram

        public void GetHistogram(ref double[] R, ref double[] G, ref double[] B, PictureInfo pic)
        {
            int NR, NG, NB;
            int width = pic.width;
            int height = pic.height;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    NR = (int)++R[pic.redPixels[i, j]];
                    NG = (int)++G[pic.greenPixels[i, j]];
                    NB = (int)++B[pic.bluePixels[i, j]];
                }
            }
        }
        public int GetMin(double[] A)
        {
            for (int i = 0; i < 256; i++)
                if (A[i] > 0) return i;
            return 0;
        }
        public int GetMax(double[] A)
        {
            for (int i = 255; i >= 0; i--)
                if (A[i] > 0) return i;
            return 0;
        }
        public void RunningSumToRound(ref double[] Array)
        {
            int length = Array.Count();
            for (int i = 1; i < length; i++)
                Array[i] += Array[i - 1];
            for (int i = 0; i < length; i++)
                Array[i] = Math.Round((Array[i] / Array[length - 1]) * 255);
        }

        private void Normalization(int height, int width, double oldMin, double oldMax, double newMax, double newMin, double[,] Array)
        {
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    Array[i, j] = ((Array[i, j] - oldMin) / (oldMax - oldMin)) * (newMax - newMin) + newMin;
        }
        private void CutOff(int height, int width, double Max, double Min, double[,] Array)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (Array[i, j] < Min) Array[i, j] = Min;
                    if (Array[i, j] > Max) Array[i, j] = Max;
                }
            }
        }
        private double CutOffValue(double Value)
        {
            if (Value > 255) return 255;
            if (Value < 0) return 0;
            return Value;
        }
        private void getMaxAndMin(int height, int width, ref double oldMin, ref double oldMax, double[,] doublePixels)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (doublePixels[i, j] > oldMax && doublePixels[i, j] < double.MaxValue) oldMax = doublePixels[i, j];
                    if (doublePixels[i, j] < oldMin) oldMin = doublePixels[i, j];
                }
            }
        }
        
        //=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*
        //Converting between Spatial & frequency domain

        public void ConverttoSpatialDomain(PictureInfo pic)
        {
            int height = pic.height;
            int width = pic.width;
            pic.frequency = false;
            MatlabClass matlabClass = new MatlabClass();
            double[,] NewR = (double[,])((MWNumericArray)(matlabClass.ConvertToSpatial((MWNumericArray)pic.redReal, (MWNumericArray)pic.redImag))).ToArray(MWArrayComponent.Real);
            double[,] NewG = (double[,])((MWNumericArray)(matlabClass.ConvertToSpatial((MWNumericArray)pic.greenReal, (MWNumericArray)pic.greenImag))).ToArray(MWArrayComponent.Real);
            double[,] NewB = (double[,])((MWNumericArray)(matlabClass.ConvertToSpatial((MWNumericArray)pic.blueReal, (MWNumericArray)pic.blueImag))).ToArray(MWArrayComponent.Real);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (NewR[i, j] > 255) NewR[i, j] = 255;
                    else if (NewR[i, j] < 0) NewR[i, j] = 0;
                    pic.redPixels[i, j] = (byte)NewR[i, j];
                    if (NewG[i, j] > 255) NewG[i, j] = 255;
                    else if (NewG[i, j] < 0) NewG[i, j] = 0;
                    pic.greenPixels[i, j] = (byte)NewG[i, j];
                    if (NewB[i, j] > 255) NewB[i, j] = 255;
                    else if (NewB[i, j] < 0) NewB[i, j] = 0;
                    pic.bluePixels[i, j] = (byte)NewB[i, j];
                }
            }
        }
        public void convertToFrequencyDomain(PictureInfo pic)
        {
            int height = pic.height;
            int width = pic.width;
            pic.frequency = true;
            MatlabClass matlabClass = new MatlabClass();
            MWArray[] NewR = (MWArray[])(matlabClass.ConvertToFrequencyDomain(2, (MWNumericArray)pic.redPixels));
            MWArray[] NewG = (MWArray[])(matlabClass.ConvertToFrequencyDomain(2, (MWNumericArray)pic.greenPixels));
            MWArray[] NewB = (MWArray[])(matlabClass.ConvertToFrequencyDomain(2, (MWNumericArray)pic.bluePixels));
            pic.redReal = (double[,])((MWNumericArray)(NewR[0])).ToArray(MWArrayComponent.Real);
            pic.greenReal = (double[,])((MWNumericArray)(NewG[0])).ToArray(MWArrayComponent.Real);
            pic.blueReal = (double[,])((MWNumericArray)(NewB[0])).ToArray(MWArrayComponent.Real);
            pic.redImag = (double[,])((MWNumericArray)(NewR[1])).ToArray(MWArrayComponent.Real);
            pic.greenImag = (double[,])((MWNumericArray)(NewG[1])).ToArray(MWArrayComponent.Real);
            pic.blueImag = (double[,])((MWNumericArray)(NewB[1])).ToArray(MWArrayComponent.Real);
            //Normalization
            double[,] tempR = new double[height, width];
            double[,] tempG = new double[height, width];
            double[,] tempB = new double[height, width];
            double rMax = double.MinValue;
            double gMax = double.MinValue;
            double bMax = double.MinValue;
            double rMin = double.MaxValue;
            double gMin = double.MaxValue;
            double bMin = double.MaxValue;
            double cnst = 0.1;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    tempR[i, j] = Math.Sqrt((pic.redReal[i, j] * pic.redReal[i, j]) + (pic.redImag[i, j] * pic.redImag[i, j]));
                    tempR[i, j] = Math.Log(tempR[i, j] + cnst);
                    if (tempR[i, j] > rMax) rMax = tempR[i, j];
                    else if (tempR[i, j] < rMin) rMin = tempR[i, j];

                    tempG[i, j] = Math.Sqrt((pic.greenReal[i, j] * pic.greenReal[i, j]) + (pic.greenImag[i, j] * pic.greenImag[i, j]));
                    tempG[i, j] = Math.Log(tempG[i, j] + cnst);
                    if (tempG[i, j] > gMax) gMax = tempG[i, j];
                    else if (tempG[i, j] < gMin) gMin = tempG[i, j];

                    tempB[i, j] = Math.Sqrt((pic.blueReal[i, j] * pic.blueReal[i, j]) + (pic.blueImag[i, j] * pic.blueImag[i, j]));
                    tempB[i, j] = Math.Log(tempB[i, j] + cnst);
                    if (tempB[i, j] > bMax) bMax = tempB[i, j];
                    else if (tempB[i, j] < bMin) bMin = tempB[i, j];
                }
            }
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    pic.redPixels[i, j] = (byte)(((tempR[i, j] - rMin) / (rMax - rMin)) * 255);
                    pic.greenPixels[i, j] = (byte)(((tempG[i, j] - gMin) / (gMax - gMin)) * 255);
                    pic.bluePixels[i, j] = (byte)(((tempB[i, j] - bMin) / (bMax - bMin)) * 255);
                }
            }
        }

        //=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*

        //Add Noise
        public void AddSaltPepperNoise(PictureInfo pic, double salt, double pepper)
        {
            if ((salt + pepper) > 100)
                MessageBox.Show("Summation of Salt and Pepper Percentages should be less than 100!");
            else
            {
                int height = pic.height;
                int width = pic.width;
                List<Point> pointList = new List<Point>();
                for (int i = 0; i < height; i++)
                    for (int j = 0; j < width; j++)
                        pointList.Add(new Point(i, j));


                int Size = width * height;
                int saltNumber = (int)(salt * Size / 100);
                int pepperNumber = (int)(pepper * Size / 100);

                Random rand = new Random();
                int count = Size - 1;

                for (int i = 0; i < saltNumber; i++)
                {
                    int ChosenIndex = rand.Next(0, count--);
                    int x = pointList[ChosenIndex].X;
                    int y = pointList[ChosenIndex].Y;
                    pic.redPixels[x, y] = 255;
                    pic.greenPixels[x, y] = 255;
                    pic.bluePixels[x, y] = 255;
                    pointList.RemoveAt(ChosenIndex);//el remove at mebatta2 el 3amalayyeea awiiiii
                }
                for (int i = 0; i < pepperNumber; i++)
                {
                    int ChosenIndex = rand.Next(0, count--);
                    int x = pointList[ChosenIndex].X;
                    int y = pointList[ChosenIndex].Y;
                    pic.redPixels[x, y] = 0;
                    pic.greenPixels[x, y] = 0;
                    pic.bluePixels[x, y] = 0;
                    pointList.RemoveAt(ChosenIndex);
                }
            }
        }
        public void AddUnifromNoise(PictureInfo pic, int a, int b, double NoisePercentage)
        {
            if (NoisePercentage > 100)
                MessageBox.Show("The percentage should be less than 100!");
            else
            {
                int height = pic.height;
                int width = pic.width;
                List<Point> pointList = new List<Point>();
                for (int i = 0; i < height; i++)
                    for (int j = 0; j < width; j++)
                        pointList.Add(new Point(i, j));

                int Size = width * height;
                int NoiseNumber = (int)((double)(1.0 / (b - a)) * Size * (NoisePercentage / 100.0));
                Random rand = new Random();
                int count = Size - 1;
                for (int j = a; j <= b; j++)
                {
                    for (int i = 0; i < NoiseNumber; i++)
                    {
                        int ChosenIndex = rand.Next(0, count--);
                        int x = pointList[ChosenIndex].X;
                        int y = pointList[ChosenIndex].Y;
                        pic.redPixels[x, y] = (byte)j;
                        pic.greenPixels[x, y] = (byte)j;
                        pic.bluePixels[x, y] = (byte)j;
                        pointList.RemoveAt(ChosenIndex);
                    }
                }
            }
        }
        public void AddGaussianNoise(PictureInfo pic, int mu, int sigma, double NoisePercentage)
        {
            if (NoisePercentage > 100)
                MessageBox.Show("The percentage should be less than 100!");
            else
            {
                int height = pic.height;
                int width = pic.width;
                List<Point> pointList = new List<Point>();
                for (int i = 0; i < height; i++)
                    for (int j = 0; j < width; j++)
                        pointList.Add(new Point(i, j));

                int Size = width * height;
                Random rand = new Random();
                int count = Size - 1;
                int min = 0, max = 255;
                for (int i = min; i <= max / 2; i++)
                {
                    double Const = Math.Pow(((i - mu) / sigma), 2) * (-0.5);
                    double denominator = (Math.Sqrt(2 * Math.PI) * sigma);
                    double NoiseNumber = ((1 / denominator) * (Math.Pow(Math.E, (Const))));
                    NoiseNumber *= (Size * (NoisePercentage / 100.0));
                    for (int k = 0; k < 2; k++)
                    {
                        int color = i;
                        if (k == 1)
                            color = max - i;
                        for (int j = 0; j < NoiseNumber; j++)
                        {
                            int ChosenIndex = rand.Next(0, count--);
                            int x = pointList[ChosenIndex].X;
                            int y = pointList[ChosenIndex].Y;
                            pic.redPixels[x, y] = (byte)color;
                            pic.greenPixels[x, y] = (byte)color;
                            pic.bluePixels[x, y] = (byte)color;
                            pointList.RemoveAt(ChosenIndex);
                        }
                    }
                }
            }
        }
        public void AddRayleighNoise(PictureInfo pic, int a, int b, double NoisePercentage)
        {
            if (NoisePercentage > 100)
                MessageBox.Show("The percentage should be less than 100!");
            else
            {
                int height = pic.height;
                int width = pic.width;
                List<Point> pointList = new List<Point>();
                for (int i = 0; i < height; i++)
                    for (int j = 0; j < width; j++)
                        pointList.Add(new Point(i, j));

                int Size = width * height;
                Random rand = new Random();
                int count = Size - 1;
                for (int j = a; j <= 255; j++)
                {
                    double Const = (-(j - a) * (j - a)) / b;
                    int NoiseNumber = (int)(((2.0 / b) * (j - a) * (Math.Pow(Math.E, Const)) * Size * (NoisePercentage / 100.0)));
                    for (int i = 0; i < NoiseNumber; i++)
                    {
                        int ChosenIndex = rand.Next(0, count--);
                        int x = pointList[ChosenIndex].X;
                        int y = pointList[ChosenIndex].Y;
                        pic.redPixels[x, y] = (byte)j;
                        pic.greenPixels[x, y] = (byte)j;
                        pic.bluePixels[x, y] = (byte)j;
                        pointList.RemoveAt(ChosenIndex);
                    }
                }
            }
        }
        public void AddGammaNoise(PictureInfo pic, int a, int b, double NoisePercentage)
        {
            if (NoisePercentage > 100)
                MessageBox.Show("The percentage should be less than 100!");
            else
            {
                int height = pic.height;
                int width = pic.width;
                List<Point> pointList = new List<Point>();
                for (int i = 0; i < height; i++)
                    for (int j = 0; j < width; j++)
                        pointList.Add(new Point(i, j));

                int Size = width * height;
                Random rand = new Random();
                int count = Size - 1;
                for (int j = a; j <= 255; j++)
                {
                    double t1 = Math.Pow(a, b);
                    double t2 = Math.Pow(j, b - 1);
                    double t3 = Factorial(b - 1);
                    double t4 = Math.Pow(Math.E, (-a * j));
                    int NoiseNumber = (int)((((t1 * t2 * t4) / t3) * Size * (NoisePercentage / 100.0)));
                    for (int i = 0; i < NoiseNumber; i++)
                    {
                        int ChosenIndex = rand.Next(0, count--);
                        int x = pointList[ChosenIndex].X;
                        int y = pointList[ChosenIndex].Y;
                        pic.redPixels[x, y] = (byte)j;
                        pic.greenPixels[x, y] = (byte)j;
                        pic.bluePixels[x, y] = (byte)j;
                        pointList.RemoveAt(ChosenIndex);
                    }
                }
            }
        }
        public double Factorial(int num)
        {
            double result = 1;
            for (int i = num; i >= 2; i--)
            {
                result *= i;
            }
            return result;
        }
        public void AddExponentialNoise(PictureInfo pic, int a, double NoisePercentage)
        {
            if (NoisePercentage > 100)
                MessageBox.Show("The percentage should be less than 100!");
            else
            {
                int height = pic.height;
                int width = pic.width;
                List<Point> pointList = new List<Point>();
                for (int i = 0; i < height; i++)
                    for (int j = 0; j < width; j++)
                        pointList.Add(new Point(i, j));

                int Size = width * height;
                Random rand = new Random();
                int count = Size - 1;
                for (int j = 0; j <= 255; j++)
                {
                    double t1 = Math.Pow(Math.E, (-a * j));
                    int NoiseNumber = (int)(((a * t1) * Size * (NoisePercentage / 100.0)));
                    for (int i = 0; i < NoiseNumber; i++)
                    {
                        int ChosenIndex = rand.Next(0, count--);
                        int x = pointList[ChosenIndex].X;
                        int y = pointList[ChosenIndex].Y;
                        pic.redPixels[x, y] = (byte)j;
                        pic.greenPixels[x, y] = (byte)j;
                        pic.bluePixels[x, y] = (byte)j;
                        pointList.RemoveAt(ChosenIndex);
                    }
                }
            }
        }

        //=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*

    }
}