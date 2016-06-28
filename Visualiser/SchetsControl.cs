using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SchetsEditor
{
    public class SchetsControl 
    {
        public Schets schets;
        private Color penkleur;

        public Color PenKleur
        {
            get { return penkleur; }
        }


        //EIGEN CODE
        public Bitmap Plaatje
        {
            get { return schets.Plaatje; }
        }
        //EINDE EIGEN CODE

        public SchetsControl(int[] size)
        {
            this.schets = new Schets(size);
        }

        public Graphics MaakBitmapGraphics()
        {
            Graphics g = schets.BitmapGraphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            return g;
        }

        //EIGEN CODE
        public void Lees(string naam)
        {
            schets.LeesIn(naam);
        }

        public void VoegElementToe(Element e)
        {
            schets.VoegToe(e);
        }

        public void VerwijderElement(Point p)
        {
            schets.Verwijder(p);
        }

        //EINDE EIGEN CODE

        public void Schoon(object o, EventArgs ea)
        {
            schets.Schoon();
        }
    }
}
