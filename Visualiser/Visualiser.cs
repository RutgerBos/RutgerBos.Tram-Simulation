using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace Visualiser {
    public class Program {
        static void Main(string[] args) {
            int trams;
            Console.WriteLine("State number of trams:");
            string word = "14";
            word = Console.ReadLine();
            trams = Int32.Parse(word);

            int[] GFXsize = { 1600,900};
            Color[] tramcolors = {
                                    Color.Blue, Color.Red, Color.Green, Color.Brown, Color.Cyan,
                                    Color.BlueViolet, Color.Gray, Color.Gold, Color.GreenYellow, Color.IndianRed,
                                    Color.DarkTurquoise, Color.DarkOliveGreen, Color.Magenta, Color.Coral, Color.Orange,
                                    Color.Orchid, Color.Salmon, Color.SeaGreen, Color.SlateBlue, Color.Maroon
                                 };

            //firstly: make dict of all stations and overall driving times from start converted to pixel position.

            Dictionary<string, int> statpos = new Dictionary<string, int>();
            statpos.Add("uithof", 50);
            statpos.Add("wkz", 151);
            statpos.Add("umc", 223);
            statpos.Add("heidelberglaan", 299);
            statpos.Add("padualaan" , 354);
            statpos.Add("krommerijn", 446);
            statpos.Add("galgenwaard", 501);
            statpos.Add("vaartscherijn", 725);
            statpos.Add("centralstation", 850);

            string[] stations = {"uithof", "wkz", "umc", "heidelberglaan" , "padualaan" , "krommerijn", "galgenwaard", "vaartscherijn", "centralstation"};
            
            //Read in all tram activity logs, starting at 0 and ending at trams-1

            List<int[]>[] timeposdata = new List<int[]>[trams];
            
            for (int i = 0; i < trams; i++) {
                StreamReader tram = new StreamReader("activityLogTram" + i.ToString() + ".txt");
                timeposdata[i] = new List<int[]>();
                string line;
                while ( (line = tram.ReadLine()) != null) {
//                    Console.WriteLine(line);

                    string[] words = line.Split(',');
                    string station = words[2].Trim();
                    int position;
                    if (! station.StartsWith("intransitafter")) { // start/end at station
                        station = station.Substring(0, station.Length - 2); //removing the directional element
//                        Console.WriteLine(station);

                        position  = statpos[station];

     

                    }
                    else {                                          // start/end at intransit.
                        //Console.WriteLine(station);
                        station = words[2].Trim().Substring("intransitafter".Length );
                        //Console.WriteLine(station);
                        if ( !(station == "wkz_1" ) ) { //not endstation
                            if (station[station.Length -1] == '0') {
                                station = station.Substring(0, station.Length - 2);
                          //      Console.WriteLine(station);
                                int pos = Array.IndexOf(stations, station) +1;
                                position = statpos[stations[pos]];
                                position -= 10;
                            }
                            else {
                                station = station.Substring(0, station.Length - 2);
                            //    Console.WriteLine(station);
                                int pos = Array.IndexOf(stations, station) -1;
                              //  Console.WriteLine(pos);
                                position = statpos[ stations[pos ] ];
                                
                                position += 10;
                            }
                        }
                        else { 
                            position = statpos["uithof"] + 10;
                            
                        }
                    }

                        int[] data = { Int32.Parse(words[0]), position };
                        timeposdata[i].Add( data  );

                }
            }


            Directory.CreateDirectory("IMG");

            //make a per-hour image showing the data of trams in that interval.
            int windowstart = 0;
            int windowend = 3600;
    
            while (windowend < (22.5  -6)*3600) {
                SchetsEditor.SchetsControl sc = new SchetsEditor.SchetsControl(GFXsize);
                for (int i = 0; i < trams; i++) {

                    List<Point> datapoints = new List<Point>();
                    for (int j = 0; j < timeposdata[i].Count; j++) { //get data, create points from that data
                        if (timeposdata[i][j][0] >= windowstart && timeposdata[i][j][0] < windowend ) {
                            int x = 250 + (1600-350) * (timeposdata[i][j][0]- windowstart) / 3600;
                            datapoints.Add( new Point( x, timeposdata[i][j][1]) );
                        }
                    }

                    SchetsEditor.KrommeLijn tramline = new SchetsEditor.KrommeLijn();
                    Brush brush = new SolidBrush(tramcolors[i]);

                    for (int k = 0; k < datapoints.Count -1; k++) {
                        tramline.lijn.Add( new SchetsEditor.RechteLijn(datapoints[k], datapoints[k+1],brush));
                    }

                    sc.VoegElementToe(tramline);
                }


                
                sc.schets.maakBitmapuitLijst();
                Bitmap img = sc.schets.Plaatje;

                Graphics gr = Graphics.FromImage(img);

                //Add text to the image.
                
                foreach (string station in stations) {
                                        
                    gr.DrawString(station, new Font("Tahoma",12), Brushes.Black, new Point (50, statpos[station]));
                    gr.DrawLine(Pens.Black, 250, statpos[station], 1600 -100, statpos[station]);
                }

                for (int i = 0; i <= 3600; i+=60) {
                    int start = 10;
                    if (i%300 == 0) start = 25;
                    int xcoord  = 250 + ( (1600 -350)* i)/3600;
                    gr.DrawLine(Pens.Black, xcoord, 50 - start, xcoord, 50);
                    gr.DrawLine(Pens.Black, xcoord, 850, xcoord, 850 + start);
                }
                
                Point topleft = new Point(250, 50);
                Point topright = new Point(1600 - 100, 50);
                Point botleft = new Point(250,850);
                Point botright = new Point(1600- 100, 850);
                gr.DrawLine(Pens.Black,topleft, topright);
                gr.DrawLine(Pens.Black, topleft, botleft);
                gr.DrawLine(Pens.Black, botleft, botright);
                gr.DrawLine(Pens.Black, topright, botright);

                img.Save("IMG\\HOUR" + (windowstart / 3600).ToString() + ".jpg");

                windowstart += 3600;
                windowend += 3600;
            }
            
            
        }

    }
}
