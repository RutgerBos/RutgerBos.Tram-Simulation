using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SchetsEditor
{
    public interface ISchetsTool
    {
        void Letter(SchetsControl s, char c);
    }

    public abstract class StartpuntTool : ISchetsTool
    {
        protected Point startpunt;
        protected Brush kwast;

        public abstract void Letter(SchetsControl s, char c);
    }

    public class TekstTool : StartpuntTool
    {
        public override string ToString() { return "tekst"; }

        public override void Letter(SchetsControl s, char c)
        {
            LetterElement letter;
            if (c >= 32)
            {
                letter = new LetterElement(startpunt, kwast, c);
                s.VoegElementToe(letter);
                Graphics gr = s.MaakBitmapGraphics();
                letter.teken(gr);
                SizeF sz = letter.Grootte;
                startpunt.X += (int)sz.Width;
            }
        }
    }

    public abstract class TweepuntTool : StartpuntTool
    {
        public static Rectangle Punten2Rechthoek(Point p1, Point p2)
        {
            return new Rectangle(new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y))
                                , new Size(Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y))
                                );
        }
        public static Pen MaakPen(Brush b, int dikte)
        {
            Pen pen = new Pen(b, dikte);
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;
            return pen;
        }
        public override void Letter(SchetsControl s, char c)
        {
        }
        public abstract void Bezig(Graphics g, Point p1, Point p2);

        public abstract void Compleet(SchetsControl s, Point p1, Point p2);

        protected void VoerCompleetUit(Element e, SchetsControl s)
        {
            s.VoegElementToe(e);
            e.teken(s.MaakBitmapGraphics());
        }

    }

    public class RechthoekTool : TweepuntTool
    {
        public override string ToString() { return "kader"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {
            g.DrawRectangle(MaakPen(kwast, 3), TweepuntTool.Punten2Rechthoek(p1, p2));
        }
        public override void Compleet(SchetsControl s, Point p1, Point p2)
        {
            Rechthoek e = new Rechthoek(p1, p2, kwast);
            base.VoerCompleetUit(e, s);
        }
    }

    public class VolRechthoekTool : RechthoekTool
    {
        public override string ToString() { return "vlak"; }

        public override void Compleet(SchetsControl s, Point p1, Point p2)
        {
            VolleRechthoek e = new VolleRechthoek(p1, p2, kwast);
            base.VoerCompleetUit(e, s);
        }
    }

    //EIGEN CODE//
    public class OvaalTool : TweepuntTool
    {
        Ovaal ovaal;
        public override string ToString() { return "ovaal"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {
            g.DrawEllipse(MaakPen(kwast, 3), TweepuntTool.Punten2Rechthoek(p1, p2));
        }

        public override void Compleet(SchetsControl s, Point p1, Point p2)
        {
            ovaal = new Ovaal(p1, p2, kwast);
            base.VoerCompleetUit(ovaal, s);
        }
    }

    public class VolOvaalTool : OvaalTool
    {
        VolleOvaal volleovaal;

        public override string ToString() { return "schijf"; }

        public override void Compleet(SchetsControl s, Point p1, Point p2)
        {
            volleovaal = new VolleOvaal(p1, p2, kwast);
            base.VoerCompleetUit(volleovaal, s);

        }
    }
    //EIND EIGEN CODE//

    public class LijnTool : TweepuntTool
    {
        public override string ToString() { return "lijn"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {
            g.DrawLine(MaakPen(this.kwast, 3), p1.X, p1.Y, p2.X, p2.Y);
        }
        public override void Compleet(SchetsControl s, Point p1, Point p2)
        {
            RechteLijn e = new RechteLijn(p1, p2, kwast);
            base.VoerCompleetUit(e, s);
        }
    }

    public class PenTool : LijnTool
    {

        KrommeLijn pen;

        public override string ToString() { return "pen"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {
            pen.lijn.Add(new RechteLijn(p1, p2, kwast));
            pen.teken(g);
        }

        public override void Compleet(SchetsControl s, Point p1, Point p2)
        {
            this.Bezig(s.MaakBitmapGraphics(), p1, p2);
            s.VoegElementToe(pen);
        }
    }

}
