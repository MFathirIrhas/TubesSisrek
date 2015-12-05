using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;


namespace TubesSisrek
{
    public class FeatureExtraction
    {
        private BitmapData data;
        private int stride;
        private System.IntPtr ptr;
        private int nOffset;
        public int line = 0;
        private int startX, finalX;
        private int startY = int.MaxValue;
        private int finalY = int.MinValue;

        public Blob ConnectedComponents = new Blob();
        public readonly static int neighborhoodV = 1;

        private Bitmap image;
        public Bitmap BinaryImage;
        private int Rows;
        private int Cols;
        private int mark;
        public void Segment(Bitmap b)
        {
            this.Rows = b.Height;//-(2*(b.Height/3));
            this.Cols = b.Width;//- (2* (b.Width / 3));
            data = b.LockBits(new Rectangle(0, 0, this.Cols, this.Rows), ImageLockMode.ReadWrite, b.PixelFormat);
            stride = data.Stride;
            ptr = data.Scan0;
            nOffset = stride - b.Width * 3;

            //MomentClass.data = data;
            //MomentClass.stride = data.Stride;
            //MomentClass.ptr = data.Scan0;
            //MomentClass.nOffset = nOffset;
            unsafe
            {
                byte* p;

                p = (byte*)(void*)ptr;
                //p+=stride;
                for (int y = 1; y < Rows /*- 1*/; y++)
                {
                    //p+=3;											
                    for (int x = 1; x < Cols/*3 - 3*/; x++)
                    {
                        //if (p[0] == 0)
                        if ((p + stride * y + x * 3)[0] == 0)
                        {
                            mark++;
                            try
                            {
                                startY = 0;//int.MaxValue;//10000;
                                finalY = b.Height;//int.MaxValue;
                                startX = 0;//int.MaxValue;
                                finalX = b.Width;//int.MaxValue;
                                
                                search(mark, y, x);
                                Blob blob = new Blob();
                                blob.StartX = startX;
                                blob.StartY = startY;
                                blob.FinalX = finalX;
                                blob.FinalY = finalY;
                                blob.Mark = mark;
                                ConnectedComponents.Add(blob); //array yang menampung seluruh connected-component yang ditemukan selama proses scanning image.
                            }
                            catch (System.StackOverflowException e)
                            {
                                //MessageBox.Show("Sorry, Cannot Extract Image", "Error");
                                return;
                            }
                        }

                        //p += 1;
                    }
                    //p += 3;
                    //p += nOffset;
                }
                //int t = 0;
                //t++;
            }
            b.UnlockBits(data);
        }

        

        #region | Hitung Objek Segmentasi |
        private void Segmentasi(Bitmap b)
        {
            for (int xx = 0; xx < 3; xx++)
            {
                   
            }
        }
        #endregion
        //Mencari pixel-pixel tetangga
        private void search(int mark, int r, int c)
        {
            
            if (r > startY)
                startY = r;
            if (r < finalY)
                finalY = r;
            if (c < finalX)
                finalX = c;
            if (c > startX)
                startX = c;


            try
            {

                int[] nb = { r, c };
                ArrayList nbList = findNeighbours(nb);
                unsafe
                {
                    byte* p = (byte*)(void*)ptr;
                    (p + r * stride + c * 3)[0] = (byte)mark;//blue
                    (p + r * stride + c * 3)[1] = (byte)mark;//green
                    (p + r * stride + c * 3)[2] = (byte)mark;//red 

                    for (int i = 0; i < nbList.Count; i++)
                    {
                        int[] pos = (int[])nbList[i];
                        if ((p + pos[0] * stride + pos[1] * 3)[0] == 0)
                            search(mark, pos[0], pos[1]);
                    }
                }
            }
            catch (System.StackOverflowException e)
            {
                return;
            }
        }

        private ArrayList findNeighbours(int[] pos)
        {
            ArrayList nbList;
            //nbList = find4Neighbours(pos);
            nbList = find8ConnectedN(pos);
            //nbList = findDNeighbours(pos);
            return nbList;
        }

        //Mencari 8 tetangga dari suatu pixel yang dikunjungi 
        private ArrayList find8ConnectedN(int[] pos)
        {
            ArrayList nbList = new ArrayList();

            if ((pos[0] > 0) && (pos[1] > 0))
                addNeighbour(pos[0] - 1, pos[1] - 1, nbList); //barat laut
            if (pos[0] > 0)
                addNeighbour(pos[0] - 1, pos[1], nbList); //utara
            if ((pos[0] > 0) && (pos[1] < Cols - 1))
                addNeighbour(pos[0] - 1, pos[1] + 1, nbList); //timur laut
            if (pos[1] > 0)
                addNeighbour(pos[0], pos[1] - 1, nbList);//barat
            if (pos[1] < Cols - 1)
                addNeighbour(pos[0], pos[1] + 1, nbList); // timur
            if ((pos[0] < Rows - 1) && (pos[1] > 0))
                addNeighbour(pos[0] + 1, pos[1] - 1, nbList); //barat daya
            if (pos[0] < Rows - 1)
                addNeighbour(pos[0] + 1, pos[1], nbList); //selatan
            if ((pos[0] < Rows - 1) && (pos[1] < Cols - 1))
                addNeighbour(pos[0] + 1, pos[1] + 1, nbList); //tenggara

            return nbList;
        }

        private void addNeighbour(int r, int c, ArrayList list)
        {
            int[] nb = { r, c };
            list.Add(nb);
        }


    }
}
