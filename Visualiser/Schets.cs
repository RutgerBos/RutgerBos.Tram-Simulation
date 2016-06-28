using System;
using System.Collections.Generic;
using System.Drawing;

namespace SchetsEditor
{
    public class Schets
    {
        private Bitmap bitmap;
        //Een schets bestaat naast een bitmap ook uit een lijst met elementen.
        private List<Element> elementen;

        public Bitmap Plaatje
        {
            get
            {
                return bitmap;
            }
        }
        //EIND EIGEN CODE

        public Schets(int[] size)
        {
            bitmap = new Bitmap(size[0], size[1]);
            elementen = new List<Element>();
        }
        public Graphics BitmapGraphics
        {
            get { return Graphics.FromImage(bitmap); }
        }

        //EIGEN CODE
        public void LeesIn(string naam)
        {
            bitmap = (Bitmap)Image.FromFile(naam);
        }

        public void VoegToe(Element e)
        {
            this.elementen.Add(e);
        }

        public void Verwijder(Point p)
        {
            int lengte = elementen.Count;
            bool veranderd = false;
            for (int t = lengte - 1; t >= 0; t--)
                if (elementen[t].raak(p))
                {
                    elementen.RemoveAt(t);
                    veranderd = true;
                    break;
                }
            if (veranderd)
                this.maakBitmapuitLijst();
        }

        public void maakBitmapuitLijst()
        {
            bitmap = new Bitmap(bitmap.Width, bitmap.Height);
            Graphics gr = Graphics.FromImage(bitmap);
            gr.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
            foreach (Element e in elementen)
                e.teken(gr);
        }

        //EINDE EIGEN CODE

        public void VeranderAfmeting(Size sz)
        {
            if (sz.Width > bitmap.Size.Width || sz.Height > bitmap.Size.Height)
            {
                Bitmap nieuw = new Bitmap(Math.Max(sz.Width, bitmap.Size.Width)
                                         , Math.Max(sz.Height, bitmap.Size.Height)
                                         );
                Graphics gr = Graphics.FromImage(nieuw);
                gr.FillRectangle(Brushes.White, 0, 0, nieuw.Size.Width, nieuw.Size.Height);
                gr.DrawImage(bitmap, 0, 0);
                bitmap = nieuw;
            }
        }
        public void Teken(Graphics gr)
        {
            gr.DrawImage(bitmap, 0, 0);
        }
        public void Schoon()
        {
            Graphics gr = Graphics.FromImage(bitmap);
            gr.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
        }
        public void Roteer()
        {
            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
        }
    }
}
