using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

/*
    Contains definition of the Tram object and all functions pertaining to it.
*/

namespace TramSim
{
    public class Tram
    {

        public int PassMax;
        public int PassCurr;
        public int TramNo;
        public int arrivaltime;
        public bool busy;
        public iHasRails Position;
        public bool indepot;
        public bool hasloaded;
        protected StreamWriter logger;
        public StreamWriter tramActivitylog;
        protected StreamWriter logPassengers;
        public int TotIn;
        public int TotOut;
        public bool cameFromDepot;
        private Simulation Sim;                 //Only used to call emergency exit.

        public Tram(int number, Endstation place, int max, string datafolder, Simulation sim)
        {
            PassMax = max;
            PassCurr = 0;
            TramNo = number;
            indepot = true;
            busy = false;
            Position = place;
            hasloaded = false;
            TotIn = 0; TotOut = 0;
            cameFromDepot = false;
            Sim = sim;

            int i = 0;
            while (i < place.depot.Length) {
                if (place.depot[i] == null) {
                    place.depot[i] = this;
                    break;
                }
                i++;
            }
            logger = new StreamWriter(datafolder + "Tram" + TramNo.ToString() + ".csv");
            logger.WriteLine("time , place of departure , place of arrival");
            tramActivitylog = new StreamWriter(datafolder+"activityLogTram" + TramNo.ToString() + ".txt");
            logPassengers = new StreamWriter(datafolder + "Tram" +TramNo.ToString() + "PassLog.csv");
            logPassengers.WriteLine("time, location, passengers, passengers in, passengers out");
        }

        public void changePass(int pin, int pout) {
            PassCurr += pin - pout;
/*            if (PassCurr < 0 || pin < 0 || pout < 0) {
                Console.WriteLine("Oh shit, we have anti-passengers!");
                Console.ReadLine(); Sim.EmergencyExit();
            }
*/
            TotIn += pin;
            TotOut += pout;
            logPassengers.WriteLine(Time.Now().ToString() + " , " + Position.Name + " , " + PassCurr.ToString() + " , " + pin.ToString() + " , " + pout.ToString());
        }

        public void prepEnd(StreamWriter logtotals) {
            logtotals.WriteLine(TramNo.ToString() + " , " + TotIn.ToString() + " , " + TotOut.ToString());
            logger.Flush();
            logger.Close();
            tramActivitylog.Flush();
            tramActivitylog.Close();
            logPassengers.Flush();
            logPassengers.Close();
        }

        /*
            In addition to moving the tram: logs some data on tram activity.
        */
        private void moveTo(iHasRails place) {
            if (Simulation.EXPLICIT) Console.WriteLine("Moving tram " + TramNo.ToString() + " from " + Position.Name + " to " + place.Name);
            
            if (!place.AddTram(this)) {
                Console.WriteLine("Error trying to move Tram" + TramNo.ToString() + " to station " + place.Name);
                Console.ReadLine(); Sim.EmergencyExit();
            }
            Position.RemoveTram(this);
            logger.WriteLine(Time.Now().ToString() + " , " + Position.Name + " , " + place.Name);
            tramActivitylog.WriteLine(Time.Now().ToString() + " , Move , " + Position.Name + " , " + place.Name);
            Position = place;
            
        }

        /*
            name says it all.
        */
        public void moveToNext() {
            moveTo(Position.Next);
        }

        /*
            Currently unused.
        */
        public void moveToDepot (Endstation place) {
            if (indepot) return;
            Position.RemoveTram(this);
            int i = 0;
            while (i < place.depot.Length) {
                if (place.depot[i] == null) {
                    place.depot[i] = this;
                    break;
                }
                i++;
            }
            indepot = true;
            Position = place;
            if (!place.Name.Equals(Position.Name)) {
                Console.WriteLine("WTF");
                Console.ReadLine();
            }
        }
        


    }
}
