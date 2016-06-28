/* Deze klassen beschrijven de mogelijke elementen waaruit een schets kan bestaan.
 * Dit zijn uiteindelijk LetterElement, RechteLijn, KrommeLijn, Rechthoek, VolRechthoek, Ovaal en VolOvaal.
 * 
 * De abstracte klasse Element is hierarchisch handig, omdat alle elementen een aantal eigenschappen gemeen hebben,
 * en is handig omdat nu in Schets een list<Element> kan worden gemaakt en bijgehouden (alle verschillende elementen kunnen dus 
 * in 1 list worden opgeslagen). De abstracte klasse TweepuntElement heeft alleen hierarchisch nut, en voorkomt code-duplicatie.
 */

using System;
using System.Drawing;
using System.Collections.Generic;


namespace SchetsEditor
{
    public abstract class Element
    {
        //Elk element wordt gekarakteriseerd door een beginpunt, een kleur en een string die aangeeft welk type element het is.
        protected Point beginpunt;
        protected Brush kleur;
        public string soort;

        //De methode maak zet vast een paar membervariabelen op de juiste waarden. Deze methode kan gebruikt worden in constructors,
        //zodat je niet voortdurend dezelfde toekenningen hoeft te typen.
        protected virtual void maak(Point punt, Brush kwast)
        {
            beginpunt = punt;
            kleur = kwast;
        }

        /* De methode raak gaat voor een gegeven punt bepalen of er raak geklikt is op het gegeven element, en geeft dat terug als bool.
         * De methode teken gaat het element op het gegeven Graphics-object g tekenen. Deze methoden moeten bestaan voor elk element,
         * maar omdat de uitwerking van deze 2 methodes erg afhangt van het type element zijn ze hier nog abstract.
         */
        public abstract bool raak(Point klik);
        public abstract void teken(Graphics g);

    }

    public class LetterElement : Element
    {
        //Een LetterElement heeft naast een beginpunt, kleur en soort ook een daadwerkelijke letter, en een grootte.
        char letter;
        SizeF grootte;

        //Constructor
        public LetterElement(Point punt, Brush kwast, char c)
        {
            soort = "Letter";
            this.maak(punt, kwast);
            letter = c;
        }
        
        // De methode teken tekent de letter op het Graphics-object, en bepaalt de grootte van de letter.
        public override void teken(Graphics g)
        {
            Font font = new Font("Tahoma", 40);
            string tekst = letter.ToString();
            grootte =
            g.MeasureString(tekst, font, this.beginpunt, StringFormat.GenericTypographic);
            g.DrawString(tekst, font, kleur,
                                          this.beginpunt, StringFormat.GenericTypographic);
        }
        
        //Read-only property om de grootte van de letter op te vragen (LetterTool heeft deze size nodig)
        public SizeF Grootte
        {
            get { return grootte; }
        }
        
        // Als de gebruiker in de rechthoek rondom de letter klikt, beschouwen we de klik als raak.
        public override bool raak(Point klik)
        {
            if (beginpunt.X <= klik.X && klik.X <= beginpunt.X + grootte.Width &&
                beginpunt.Y <= klik.Y && klik.Y <= beginpunt.Y + grootte.Height)
                return true;
            else
                return false;
        }
    }

    public abstract class TweePuntElement : Element
    {
        // Een TweePuntElement heeft naast een beginpunt ook een eindpunt
        protected Point eindpunt;

        //Nieuwe methode maak, die ook direct eindpunt de juiste waarde geeft.
        protected void maak(Point begin, Point einde, Brush kwast)
        {
            this.maak(begin, kwast);
            eindpunt = einde;
        }
        
        /* De methode coordinaten geeft een int-array terug met daarin de coordinaten van de linkerbovenhoek
         * en de rechteronderhoek van de rechthoek die gevormd wordt van beginpunt tot eindpunt (vergelijkbaar met
         * de Punten2Rechthoek in de TweepuntTool, maar het gaat nu echt om de coordinaten);
         * Volgorde van uitvoer is linksboven.x, linksboven.y, rechtsonder.x, rechtsonder.y.
         */ 
        protected int[] coordinaten()
        {
            int[] antwoord = {
                                Math.Min(beginpunt.X, eindpunt.X),
                                Math.Max(beginpunt.X, eindpunt.X),
                                Math.Min(beginpunt.Y, eindpunt.Y),
                                Math.Max(beginpunt.Y, eindpunt.Y)
                             };
            return antwoord;
        }
    }
    
    
    public class RechteLijn : TweePuntElement
    {
        //Constructor
        public RechteLijn(Point begin, Point eind, Brush kwast)
        {
            soort = "RechteLijn";
            this.maak(begin, eind, kwast);
        }

        // teken tekent de lijn. Er wordt hier gebruik gemaakt van de methode MaakPen uit de klasse TweepuntTool,
        // dit kan omdat die methode statisch is.
        public override void teken(Graphics g)
        {
            g.DrawLine(TweepuntTool.MaakPen(this.kleur, 3), beginpunt.X, beginpunt.Y, eindpunt.X, eindpunt.Y);
        }
        
        // raak kijkt of de afstand van het geklikte punt tot de lijn klein genoeg is.
        public override bool raak(Point klik)
        {
            double afstand = this.afstandtotlijn(klik);
            if (afstand <= 3)
                return true;
            else
                return false;

        }

        /* De methode afstandtotLijn doet het eigenlijke rekenwerk voor de methode raak, en bepaalt de afstand van een
         * punt tot de lijn. Dit wordt gedaan met behulp van lineaire algebra. Laat k het geklikte punt voorstellen, en
         * a het beginpunt en b het eindpunt van onze lijn. De vector van k naar a wordt gegeven door (k-a), en de richtingsvector
         * van de oneindige lijn door a en b wordt gegeven door (b-a). Noem de genormaliseerde richtingsvector (die ontstaat door te delen
         * door de lengte van de lijn) v. Alle punten x op de lijn worden nu beschreven door x = a + t*v, waar t een reeel getal is.
         * We bepalen nu het projectiepunt p van k op de lijn door p = a + s*v, met s het inproduct van (k-a) en v (dat hier het projectiepunt uitkomt
         * volgt direct uit de geometrische definitie (met hoeken en afstanden) van het inproduct). Echter, omdat wij niet met een oneidige lijn te maken
         * hebben, maar met een lijnstuk, moeten we ingrijpen als het projectiepunt buiten het lijnstuk zou komen te liggen. We nemen dan het dichtsbijzijnde
         * uiteinde als projectiepunt. Als laatste stap bepalen we de afstand van het punt k tot het projectiepunt p, "gewoon" met Pythagoras.
         * (Bron: de eerstejaars wiskundecursus Lineaire Algebra in 2008, en het daarbij gebruikte boek "Vectoren en Matrices" van Jan van der Craats).
         */
        protected double afstandtotlijn(Point klik)
        {
            //lengte van het lijnstuk
            double lengtelijn = Math.Sqrt(Math.Pow((beginpunt.X - eindpunt.X), 2) + Math.Pow((beginpunt.Y - eindpunt.Y), 2));

            //richtingsvector van de lijn
            double richtingx = (eindpunt.X - beginpunt.X) / lengtelijn;
            double richtingy = (eindpunt.Y - beginpunt.Y) / lengtelijn;

            double inproduct = (klik.X - beginpunt.X) * richtingx + (klik.Y - beginpunt.Y) * richtingy;

            //bepalen van het projectiepunt
            double projectiex, projectiey;

            if (inproduct < 0)
            {
                projectiex = beginpunt.X;
                projectiey = beginpunt.Y;
            }
            else if (inproduct > lengtelijn)
            {
                projectiex = eindpunt.X;
                projectiey = eindpunt.Y;
            }
            else
            {
                projectiex = beginpunt.X + inproduct * richtingx;
                projectiey = beginpunt.Y + inproduct * richtingy;
            }

            //afstand van het punt tot de lijn opleveren
            return Math.Sqrt(Math.Pow((klik.X - projectiex), 2) + Math.Pow((klik.Y - projectiey), 2));
        }

    }
    
    public class KrommeLijn : TweePuntElement
    {
        // We vatten een KrommeLijn of als een grote verzameling van allemaal korte rechte lijnen.
        // Deze list is public omdat we er vanuit de Tool-klasse er dingen aan toe willen kunnen voegen.
        public List<RechteLijn> lijn;

        //Constructor
        public KrommeLijn()
        {
            soort = "KrommeLijn";
            lijn = new List<RechteLijn>();
        }
        
        //De tekenmethode tekent alle rechte lijntjes
        public override void teken(Graphics g)
        {
            foreach (RechteLijn lijnstuk in lijn)
            {
                lijnstuk.teken(g);
            }
        }
        
        //De methode raak kijkt of er minstens 1 recht lijntje is waarop raak is geklikt
        public override bool raak(Point klik)
        {
            foreach (RechteLijn lijnstuk in lijn)
            {
                if (lijnstuk.raak(klik))
                    return true;
            }
            return false;
        }

    }
    
    public class Rechthoek : TweePuntElement
    {
        //Constructor
        public Rechthoek(Point begin, Point eind, Brush kwast)
        {
            soort = "Rechthoek";
            this.maak(begin, eind, kwast);
        }

        public override void teken(Graphics g)
        {
            g.DrawRectangle(TweepuntTool.MaakPen(kleur, 3), TweepuntTool.Punten2Rechthoek(beginpunt, eindpunt));
        }

        // De methode raak geeft true terug als in 1 van de twee richtingen (x en y) op voldoende kleine afstand van de grenswaarden
        // (beginpunt en eindpunt) is geklikt, en in de andere richting (bijna) binnen de rechthoek.
        public override bool raak(Point klik)
        {
            int xafstand = Math.Min(Math.Abs(klik.X - beginpunt.X), Math.Abs(klik.X - eindpunt.X));
            int yafstand = Math.Min(Math.Abs(klik.Y - beginpunt.Y), Math.Abs(klik.Y - eindpunt.Y));

            int[] coo = this.coordinaten();

            if ((xafstand <= 3 && (klik.Y >= coo[2] - 3) && (klik.Y <= coo[3] + 3))
                || (yafstand <= 3 && (klik.X >= coo[0] - 3) && (klik.X <= coo[1] + 3)))
                return true;
            else
                return false;
        }

    }
    public class VolleRechthoek : Rechthoek
    {
        //Constructor, die een uitbreiding is van de constructor voor Rechthoek
        public VolleRechthoek(Point begin, Point eind, Brush kwast)
            : base(begin, eind, kwast)
        {
            soort = "VolleRechthoek";
        }

        public override void teken(Graphics g)
        {
            g.FillRectangle(kleur, TweepuntTool.Punten2Rechthoek(beginpunt, eindpunt));
        }

        // De methode raak geeft true terug als er binnen de rechthoek is geklikt, en false als het punt daarbuiten ligt.
        public override bool raak(Point klik)
        {
            int[] coo = this.coordinaten();

            if ((klik.X >= coo[0]) && (klik.X <= coo[1])
                  && (klik.Y >= coo[2]) && (klik.Y <= coo[3]))
                return true;
            else
                return false;
        }

    }
    public class Ovaal : TweePuntElement
    {
        //Constructor
        public Ovaal(Point begin, Point eind, Brush kwast)
        {
            soort = "Ovaal";
            this.maak(begin, eind, kwast);

        }

        public override void teken(Graphics g)
        {
            Pen p = TweepuntTool.MaakPen(kleur, 3);
            Rectangle rect = TweepuntTool.Punten2Rechthoek(beginpunt, eindpunt);
            g.DrawEllipse(p, rect);
        }

        /* Een ellips rond het punt (middenx, middeny) met breedte b en hoogte h wordt beschreven door alle punten (x,y) die voldoen aan
         * de formule ((x-middenx)/b)^2 + ((y-middeny)/h)^2 = 1. De methode meetPunt berekent voor een punt (x,y) de waarden xmaat en ymaat, 
         * die worden gegeven door xmaat = (x-middenx)/b)^2, ymaat = ((y-middeny)/h)^2. We kunnen vervolgens kijken of de som van deze twee 
         * maten voldoende dicht bij 1 ligt. 
         * Als dat zo is, is het punt dicht bij de ellips, en geeft raak true terug. Anders wordt er false teruggegeven.
         */
        public override bool raak(Point klik)
        {
            float xmaat, ymaat;
            xmaat = this.meetPunt(klik)[0];
            ymaat = this.meetPunt(klik)[1];

            if (Math.Abs((xmaat + ymaat) - 1) <= 0.2)
                return true;
            else
                return false;
        }
        
        protected float[] meetPunt(Point klik)
        {
            int x1, x2, y1, y2, ellhoogte, ellbreedte;
            float xmaat, ymaat;

            x1 = this.coordinaten()[0];
            x2 = this.coordinaten()[1];
            y1 = this.coordinaten()[2];
            y2 = this.coordinaten()[3];

            //bepaal het midden en de hoogte en breedte van de ellips
            int middenX = (x1 + x2) / 2;
            int middenY = (y1 + y2) / 2;
            ellhoogte = (y2 - y1) / 2;
            ellbreedte = (x2 - x1) / 2;

            xmaat = ((float)Math.Pow(klik.X - middenX, 2)) / ((float)Math.Pow(ellbreedte, 2));
            ymaat = ((float)Math.Pow(klik.Y - middenY, 2)) / ((float)Math.Pow(ellhoogte, 2));

            float[] antwoord = { xmaat, ymaat };
            return antwoord;
        }
    }


    public class VolleOvaal : Ovaal
    {
        //Constructor, die een uitbreiding is van de constructor van Ovaal
        public VolleOvaal(Point begin, Point eind, Brush kwast)
            : base(begin, eind, kwast)
        {
            soort = "VolleOvaal";
            this.maak(begin, eind, kwast);
        }

        public override void teken(Graphics g)
        {
            Rectangle rect = TweepuntTool.Punten2Rechthoek(beginpunt, eindpunt);
            g.FillEllipse(kleur, rect);
        }
        
        // In de methode raak gebruiken we hetzelfde principe als bij het bepalen van de afstand tot de gewone Ovaal.
        // Voor een gevulde ellips geldt dat alle punten in de ellips gegeven worden door ((x-middenx)/b)^2 + ((y-middeny)/h)^2 <= 1,
        // en dat is precies wat hier als criterium voor raak klikken wordt gebruikt.
        public override bool raak(Point klik)
        {
            float xmaat, ymaat;
            xmaat = this.meetPunt(klik)[0];
            ymaat = this.meetPunt(klik)[1];

            if (xmaat + ymaat <= 1)
                return true;
            else
                return false;
        }
    }
}