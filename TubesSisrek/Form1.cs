using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;


namespace TubesSisrek
{
    public partial class Form1 : Form
    {
        int[] npixel;
        public Image img;
        string filepath;
        PreProcessing pp = new PreProcessing();
        FeatureExtraction fe = new FeatureExtraction();
        
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string projectPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            string path = projectPath + @"/RawDataSet/Data-Latih.xlsx";
            Excel.Application ExcelApp = new Microsoft.Office.Interop.Excel.Application();

            Excel.Workbook wb = ExcelApp.Workbooks.Open(path);
            Excel.Worksheet sh = (Excel.Worksheet)wb.Sheets["Train"];

            string SavePath = projectPath + "/ImageDataSet/Data-Latih/";
            int width = 48;
            int height = 48;
            var b = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            int i = 3; //Inisialisasi Kolom nilai Pixel
            for (int j = 1; j <= 2178; j++)
            {
                for (int y = 0; y < 48; y++)
                {
                    for (int x = 0; x < 48; x++)
                    {
                        int rgb = Convert.ToInt32(sh.Cells[j, i].Value);
                        Color c = Color.FromArgb(rgb, rgb, rgb);
                        b.SetPixel(x, y, c);
                        i = i + 1;
                    }
                }
                string filename = "Latih_" + sh.Cells[j, 1].Value.ToString() + "_" + sh.Cells[j, 2].Value.ToString() + ".bmp";
                b.Save(SavePath + filename, ImageFormat.Bmp);
                i = 3;
            }

            MessageBox.Show("Image Saved!", "Success", MessageBoxButtons.OK);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string projectPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));

            string path = projectPath + @"/ImageDataSet/Data-Latih";
            string path2 = projectPath + @"/RawDataSet2/testing.xlsx";
            //var files = Directory.GetFiles(path, "1_3.bmp");

            Excel.Application ExcelApp = new Microsoft.Office.Interop.Excel.Application();

            Excel.Workbook wb = ExcelApp.Workbooks.Open(path2);
            Excel.Worksheet sh = (Excel.Worksheet)wb.Sheets["Sheet1"];

            DirectoryInfo di = new DirectoryInfo(path);

            FileInfo[] Images = di.GetFiles("*.bmp");
            FileInfo[] sortedImages = Images.OrderBy(f => f.CreationTime).ToArray();
            
            
            int i = 3;
            int img = 0;
            for (int j = 1; j <= 4; j++)
            {
                Image image = Image.FromFile(path + "/" + sortedImages[img].Name);
                Bitmap b = new Bitmap(image);
                string kl2 = Path.GetFileNameWithoutExtension(sortedImages[img].Name);
                string l = kl2.Substring(kl2.Length-1,1);
                sh.Cells[j, 1].Value2 = j;
                sh.Cells[j, 2].Value2 = l;

                for (int y = 0; y < 48; y++)
                {
                    for (int x = 0; x < 48; x++)
                    {
                        
                        
                        Color c = b.GetPixel(x, y);
                        int rgb = c.R;
                        
                        sh.Cells[j, i].Value = rgb;
                        i++;
                        
                    }
                }
                img = img + 1;
                i = 3;
                wb.Save();
                //wb.Close();
            }

                MessageBox.Show("Succeed", "Succeed", MessageBoxButtons.OK);
        }

        

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Title = "Select Image";
            open.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.bmp) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.bmp";

            if (open.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = new Bitmap(open.FileName);
                pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
                textBox1.Text = open.FileName.ToString();
                img = pictureBox1.Image;
                filepath = open.FileName;
                //b = new Bitmap(pictureBox1.Image);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string projectPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));

            string path = projectPath + @"/ImageDataSet/Data-Latih";
            string SavePath = projectPath + "/PreProcessedDataSet/Data-Latih/";
            //Bitmap bmp = new Bitmap(pictureBox1.Image);
            DirectoryInfo di = new DirectoryInfo(path);

            FileInfo[] Images = di.GetFiles("*.bmp");
            FileInfo[] sortedImages = Images.OrderBy(f => f.CreationTime).ToArray();
            for (int i = 0; i < 2178; i++)
            {
                Image image = Image.FromFile(path + "/" + sortedImages[i].Name);
                Bitmap b = new Bitmap(image);
                Bitmap newBmp = pp.HistEq(b);
                string name = Path.GetFileNameWithoutExtension(sortedImages[i].Name);
                string lname = name.Substring(name.Length - 1, 1);
                int j = i + 1;
                string filename = "Latih_" + j+ "_" + lname +".bmp";
                newBmp.Save(SavePath + filename, ImageFormat.Bmp);

            }
            MessageBox.Show("Selesai", "Done", MessageBoxButtons.OK);
                //pictureBox1.Image = pp.HistEq(bmp);

            //string filename = "Latih_" + sh.Cells[j, 1].Value.ToString() + "_" + sh.Cells[j, 2].Value.ToString() + ".bmp";
            //b.Save(SavePath + filename, ImageFormat.Bmp);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(pictureBox1.Image);
            pictureBox1.Image = pp.Binarize(bmp);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(pictureBox1.Image);
            pictureBox1.Image = pp.EdgeDetection(bmp, Matrix.SobelMask, 1.0 / 1.0, 0);

        }

        private void button7_Click(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(pictureBox1.Image);
            fe.Segment(bmp);
            pictureBox1.Image = bmp;
            
        }

        private void button8_Click(object sender, EventArgs e) //Ngetes doank
        {
            string projectPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            string path2 = projectPath + @"/RawDataSet2/testing.xlsx";
            //var files = Directory.GetFiles(path, "1_3.bmp");

            Excel.Application ExcelApp = new Microsoft.Office.Interop.Excel.Application();

            Excel.Workbook wb = ExcelApp.Workbooks.Open(path2);
            Excel.Worksheet sh = (Excel.Worksheet)wb.Sheets["Sheet1"];

            
            int i = 1;
            //int img = 0;
            for (int j = 1; j <= 1; j++)
            {
                
                for (int y = 0; y < 48; y++)
                {
                    for (int x = 0; x < 48; x++)
                    {

                        Bitmap b = new Bitmap(pictureBox1.Image);
                        Color c = b.GetPixel(x, y);
                        int rgb = c.R;

                        sh.Cells[j, i].Value = rgb;
                        i++;

                    }
                }
                //img = img + 1;
                i = 1;
                wb.Save();
                //wb.Close();
            }

            MessageBox.Show("Succeed", "Succeed", MessageBoxButtons.OK);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            
            FeatureExtraction fe = new FeatureExtraction();
            for (int i = 0; i < 5; i++)
            {
                int[] j;
                //fe.VectorImage1D(ref i);

               
            }
            //richTextBox1.Text = fe.VectorImage1D(ref )
            
        }

        private void button10_Click(object sender, EventArgs e) //Matrix R
        {
            string projectPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));

            string path = projectPath + @"/HistEqDataSet/Data-Latih";
            //string path = projectPath + @"/ImageDataSet/Data-Latih";
            string path2 = projectPath + @"/pcaProcess/MatrixR.xlsx";

            Excel.Application ExcelApp = new Microsoft.Office.Interop.Excel.Application();

            Excel.Workbook wb = ExcelApp.Workbooks.Open(path2);
            Excel.Worksheet sh = (Excel.Worksheet)wb.Sheets["Sheet1"];

            DirectoryInfo di = new DirectoryInfo(path);

            FileInfo[] Images = di.GetFiles("*.bmp");
            FileInfo[] sortedImages = Images.OrderBy(f => f.CreationTime).ToArray();
            

            int i = 1;
            int img = 0;
            for (int j = 1; j <=2178; j++)
            {
                Image image = Image.FromFile(path + "/" + sortedImages[img].Name);
                Bitmap b = new Bitmap(image);
                

                for (int y = 0; y < 48; y++)
                {
                    for (int x = 0; x < 48; x++)
                    {
                        Color c = b.GetPixel(x, y);
                        int rgb = c.R;

                        sh.Cells[i, j].Value = rgb;
                        i++;

                    }
                }
                img = img + 1;
                i = 1;
            }
            wb.Save();
            wb.Close();
            MessageBox.Show("Matrix R Selesai", "Done!", MessageBoxButtons.OK);
        }

        private void button11_Click(object sender, EventArgs e) //Mean Image
        {
            string projectPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));

            string path  = projectPath + @"/pcaProcess/MatrixR.xlsx";
            string path2 = projectPath + @"/pcaProcess/MeanImage.xlsx";
            Excel.Application ExcelApp = new Microsoft.Office.Interop.Excel.Application();

            Excel.Workbook wb = ExcelApp.Workbooks.Open(path);
            Excel.Worksheet sh = (Excel.Worksheet)wb.Sheets["Sheet1"]; //Matrix R

            Excel.Workbook wb2 = ExcelApp.Workbooks.Open(path2);
            Excel.Worksheet sh2 = (Excel.Worksheet)wb2.Sheets["Sheet1"]; //Matrix Mean Image

            int sum = 0;
            for (int i = 1; i <= 2304; i++)
            {
                for (int j = 1; j <= 2178; j++)
                {
                    sum += sh.Cells[i, j].Value;
                }
                int avg = sum / 2178;
                sh2.Cells[i, 1].Value = avg;
                sum = 0;
            }

            wb.Save();
            wb2.Save();
            wb.Close();
            wb2.Close();


        }

        private void button12_Click(object sender, EventArgs e)
        {
            string projectPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));

            string path = projectPath + @"/pcaProcess/MatrixR.xlsx";
            string path2 = projectPath + @"/pcaProcess/MeanImage.xlsx";
            string path3 = projectPath + @"/pcaProcess/CenteredMean.xlsx";
            Excel.Application ExcelApp = new Microsoft.Office.Interop.Excel.Application();

            Excel.Workbook wb = ExcelApp.Workbooks.Open(path);
            Excel.Worksheet sh = (Excel.Worksheet)wb.Sheets["Sheet1"]; //Matrix R

            Excel.Workbook wb2 = ExcelApp.Workbooks.Open(path2);
            Excel.Worksheet sh2 = (Excel.Worksheet)wb2.Sheets["Sheet1"]; //Matrix Mean Image

            Excel.Workbook wb3 = ExcelApp.Workbooks.Open(path3);
            Excel.Worksheet sh3 = (Excel.Worksheet)wb3.Sheets["Sheet1"]; //Matrix Centered Mean

            for (int i = 1; i <= 2178; i++)
            {
                for (int j = 1; j <= 2304; j++)
                {
                    sh3.Cells[j, i].Value = sh.Cells[j, i].Value - sh2.Cells[j, 1].Value;
                }
            }

            wb.Save();
            wb2.Save();
            wb3.Save();
            wb.Close();
            wb2.Close();
            wb3.Close();

        }

        private void button13_Click(object sender, EventArgs e)
        {
            
            string projectPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));

            string path = projectPath + @"/pcaProcess/CenteredMean.xlsx";
            string path2 = projectPath + @"/pcaProcess/TCenteredMean.xlsx";
            Excel.Application ExcelApp = new Microsoft.Office.Interop.Excel.Application();


            Excel.Workbook wb = ExcelApp.Workbooks.Open(path);
            Excel.Worksheet sh = (Excel.Worksheet)wb.Sheets["Sheet1"];

            Excel.Workbook wb2 = ExcelApp.Workbooks.Open(path2);
            Excel.Worksheet sh2 = (Excel.Worksheet)wb2.Sheets["Sheet1"];

            for (int i = 1; i <= 2304; i++)
            {
                for (int j = 1; j <= 2178; j++)
                {
                    sh2.Cells[j, i].Value = sh.Cells[i, j].Value;
                }
            }

            wb.Save();
            wb2.Save();
            wb.Close();
            wb2.Close();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            string projectPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));

            string path = projectPath + @"/pcaProcess/TCenteredMean.xlsx";
            string path2 = projectPath + @"/pcaProcess/CenteredMean.xlsx";
            string path3 = projectPath + @"/pcaProcess/CovMatrix.xlsx";
            Excel.Application ExcelApp = new Microsoft.Office.Interop.Excel.Application();


            Excel.Workbook wb = ExcelApp.Workbooks.Open(path);
            Excel.Worksheet sh = (Excel.Worksheet)wb.Sheets["Sheet1"];

            Excel.Workbook wb2 = ExcelApp.Workbooks.Open(path2);
            Excel.Worksheet sh2 = (Excel.Worksheet)wb2.Sheets["Sheet1"];

            Excel.Workbook wb3 = ExcelApp.Workbooks.Open(path2);
            Excel.Worksheet sh3 = (Excel.Worksheet)wb3.Sheets["Sheet1"];

            for (int i = 1; i <= 2178; i++)
            {
                for (int j = 1; j <= 2178; j++)
                {

                }
            }


            
        }

    }
}

