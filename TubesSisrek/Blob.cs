using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TubesSisrek
{
    public class Blob:CollectionBase
    {
        private int mark = 0;
        private int startX = 0, startY = 0;
        private int finalX = 0, finalY = 0;
        //public _1103120009_Tugas2Tahap1.MomentClass mc;

        public Blob()
        {
            //mc = new MomentClass();
        }

        public void Add(Blob i)
        {
            List.Add(i);
        }

        //public int getWidth() { return finalX - startX; }
        public int getWidth() { return startX - finalX; }
        //public int getHeight() { return finalY - startY; }
        public int getHeight() { return startY - finalY; }


        public int StartX
        {
            get { return startX; }
            set { startX = value; }
        }

        public int StartY
        {
            get { return startY; }
            set { startY = value; }
        }

        public int FinalX
        {
            get { return finalX; }
            set { finalX = value; }
        }
        public int FinalY
        {
            get { return finalY; }
            set { finalY = value; }
        }
        public int Mark
        {
            get { return mark; }
            set { mark = value; }
        }
    }
}
