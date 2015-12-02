using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
namespace TubesSisrek
{
    public class PreProcessing
    {
        //Histogram Equilization
        public Bitmap HistEq(Bitmap b, double blackPointPercent = 0.02, double whitePointPercent = 0.01)
        {
            BitmapData bmpData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Bitmap resultImage = new Bitmap(b.Width, b.Height);
            BitmapData destData = resultImage.LockBits(new Rectangle(0, 0, resultImage.Width, resultImage.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            int stride = bmpData.Stride;
            IntPtr srcScan0 = bmpData.Scan0;
            IntPtr destScan0 = destData.Scan0;
            var freq = new int[256];

            unsafe
            {
                byte* src = (byte*)srcScan0;
                for (int y = 0; y < b.Height; ++y)
                {
                    for (int x = 0; x < b.Width; ++x)
                    {
                        ++freq[src[y * stride + x * 4]];
                    }
                }

                int numPixels = b.Width * b.Height;
                int minI = 0;
                var blackPixels = numPixels * blackPointPercent;
                int accum = 0;

                while (minI < 255)
                {
                    accum += freq[minI];
                    if (accum > blackPixels) break;
                    ++minI;
                }

                int maxI = 255;
                var whitePixels = numPixels * whitePointPercent;
                accum = 0;

                while (maxI > 0)
                {
                    accum += freq[maxI];
                    if (accum > whitePixels) break;
                    --maxI;
                }
                double spread = 255d / (maxI - minI);
                byte* dst = (byte*)destScan0;
                for (int y = 0; y < b.Height; ++y)
                {
                    for (int x = 0; x < b.Width; ++x)
                    {
                        int i = y * stride + x * 4;

                        byte val = (byte)Clamp(Math.Round((src[i] - minI) * spread), 0, 255);
                        dst[i] = val;
                        dst[i + 1] = val;
                        dst[i + 2] = val;
                        dst[i + 3] = 255;
                    }
                }
            }

            b.UnlockBits(bmpData);
            resultImage.UnlockBits(destData);

            return resultImage;
        }

        static double Clamp(double val, double min, double max)
        {
            return Math.Min(Math.Max(val, min), max);
        }

        public Bitmap Binarize(Bitmap b)
        {
            int w = b.Width;
            int h = b.Height; 
            Rectangle r = new Rectangle(0, 0, w, h);
            if (b.PixelFormat != PixelFormat.Format32bppPArgb)
            {
                Bitmap temp = new Bitmap(w, h, PixelFormat.Format32bppPArgb);
                Graphics g = Graphics.FromImage(temp);
                g.DrawImage(b, r, 0, 0, w, h, GraphicsUnit.Pixel);
                g.Dispose(); b = temp;
            }

            BitmapData bmpData = b.LockBits(r, ImageLockMode.ReadOnly, b.PixelFormat);
            Bitmap b0 = new System.Drawing.Bitmap(w, h, PixelFormat.Format1bppIndexed);
            BitmapData b0dat = b0.LockBits(r, ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int index = y * bmpData.Stride + (x * 4);
                    if (Color.FromArgb(Marshal.ReadByte(bmpData.Scan0, index + 2), Marshal.ReadByte(bmpData.Scan0, index + 1), Marshal.ReadByte(bmpData.Scan0, index)).GetBrightness() > 0.5f)
                    {
                        int index0 = y * b0dat.Stride + (x >> 3);
                        byte p = Marshal.ReadByte(b0dat.Scan0, index0);
                        byte mask = (byte)(0x80 >> (x & 0x7));
                        Marshal.WriteByte(b0dat.Scan0, index0, (byte)(p | mask));
                    }
                }
            }
            b0.UnlockBits(b0dat);
            b.UnlockBits(bmpData);
            return b0;
        }


        public Bitmap EdgeDetection(Bitmap sourceBitmap, double[,] filterMatrix, double factor = 1, int bias = 0)
        {
            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            byte[] resultBuffer = new byte[sourceData.Stride * sourceData.Height];

            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            sourceBitmap.UnlockBits(sourceData);

            double blue = 0.0;
            double green = 0.0;
            double red = 0.0;

            int filterWidth = filterMatrix.GetLength(1);
            int filterHeight = filterMatrix.GetLength(0);

            int filterOffset = (filterWidth - 1) / 2;
            int calcOffset = 0;

            int byteOffset = 0;

            for (int offsetY = filterOffset; offsetY < sourceBitmap.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX < sourceBitmap.Width - filterOffset; offsetX++)
                {
                    blue = 0;
                    green = 0;
                    red = 0;

                    byteOffset = offsetY * sourceData.Stride + offsetX * 4;

                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset + (filterX * 4) + (filterY * sourceData.Stride);

                            blue += (double)(pixelBuffer[calcOffset]) * filterMatrix[filterY + filterOffset, filterX + filterOffset];

                            green += (double)(pixelBuffer[calcOffset + 1]) * filterMatrix[filterY + filterOffset, filterX + filterOffset];

                            red += (double)(pixelBuffer[calcOffset + 2]) * filterMatrix[filterY + filterOffset, filterX + filterOffset];
                        }
                    }

                    blue = factor * blue + bias;
                    green = factor * green + bias;
                    red = factor * red + bias;

                    blue = (blue > 255 ? 255 : (blue < 0 ? 0 : blue));

                    green = (green > 255 ? 255 : (green < 0 ? 0 : green));

                    red = (red > 255 ? 255 : (red < 0 ? 0 : red));

                    resultBuffer[byteOffset] = (byte)(blue);
                    resultBuffer[byteOffset + 1] = (byte)(green);
                    resultBuffer[byteOffset + 2] = (byte)(red);
                    resultBuffer[byteOffset + 3] = 255;
                }
            }

            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);

            BitmapData resultData = resultBitmap.LockBits(new Rectangle(0, 0,
                                     resultBitmap.Width, resultBitmap.Height),
                                                      ImageLockMode.WriteOnly,
                                                 PixelFormat.Format32bppArgb);

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            resultBitmap.UnlockBits(resultData);

            return resultBitmap;
        }

        public Bitmap DetectEdge(Bitmap sourceBitmap)
        {
            Bitmap resultBitmap = null;
            resultBitmap = EdgeDetection(sourceBitmap,Matrix.SobelMask, 1.0 / 1.0, 0);
    
            return resultBitmap;
        }
    }
}
