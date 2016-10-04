using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


/*
    This file contains the main simulation looping and all functions necessary to read in parameters and build the required objects.
*/

namespace TramSim { 
    
    class Program {

        static void Main(string[] args) {
            Console.WriteLine("give folder name \n");
            string folder = Console.ReadLine();
            Simulation sim = new Simulation(folder); 

        }
    }

    public class Simulation {
        Tram[] trams;
        iHasRails[] railway;
        public static int SEED = 32; //standard value. NOTE: Seed is used for the entire batch of simulations.
        public int Runs;
        public string FOLDER = "..\\..\\..\\..\\test\\"; //standard value. 
        public string BATCHNAME;

        //These two parameters should only be used in bugfixing. If EXPLICIT: shows debug messages. If also WAIT: pauses simulation after each message until user presses enter.
        public static bool EXPLICIT = false;
        public static bool WAIT = false;

        public string DataFolder;

        //Parameters needed for initialisation.
        int StartTime;
        string[] Stations;
        bool[] isEndstation;
        bool[] hasDepot;
        TimeDistr[] PassInrates; //NOTE: unconverted in and out rates.
        TimeDistr[] PassOutrates;
        double[] DrivTimVar;
        double[] DrivTimAvg;
        public int EndTime;
        int PassDir0;
        int PassDir1;
        int NoTrams;
        int MaxPassTram;
        int PerStationLeeway; 
        TimeDistr TramSchedule;
        TimeDistr PassDens_0;
        TimeDistr PassDens_1;
        
        

        public Simulation (string folder) {

            FOLDER = folder;

            ReadData("initfile.ini"); //hardcoded .ini file filename.


            for (int i = 0; i < Runs; i++) {
                Time.NewDay(EndTime);
                DataFolder = FOLDER + BATCHNAME + i.ToString() + "\\";
                Directory.CreateDirectory(DataFolder);
                initSim();
       //         if (Simulation.EXPLICIT) {
                    Console.WriteLine("Initialisation complete. Starting simulations");
                    if (WAIT) Console.ReadLine();
      //          }
                Looper();
    //            if (Simulation.EXPLICIT) {
                    Console.WriteLine("Finished simulation " + i.ToString());
     //           }
            }

            Console.WriteLine("Simulations complete. Press enter to exit.");
            Console.ReadLine();
        }

        /*
            Main event loop
        */
        public void Looper() {
            Event current;
            while( (current = EventList.nextEvent()) != null) {
                
                Time.SetClock(current.ETime);
                current.execEvent();
            }
        }

        /*
            Only called if catastrophic error occurs.
        */
        public void EmergencyExit() {
            endSim();
            Environment.Exit(1);
        }


        /*
            Ends the simulation. Gathers some additional metadata for each station and trams and tells them to close up all datafiles.
        */
        public void endSim() {
            if (EventList.allowEnding(this)) {
                Console.WriteLine("Ending Simulation");
                StreamWriter stationtotals = new StreamWriter(DataFolder + "stationtotals.csv");
                stationtotals.WriteLine("Station , Passengers in , passengers out");
                StreamWriter tramtotals = new StreamWriter(DataFolder + "tramtotals.csv");
                tramtotals.WriteLine("TramNo, total in, total out");

                
                foreach (iHasRails place in railway) {
                    place.prepForEnd(stationtotals);
                }
                foreach (Tram tram in trams) {
                    tram.prepEnd(tramtotals);
                }

                stationtotals.Close();
                tramtotals.Close();
                EventList.prepEnd();
            }
        }

        /*  LOADING FUNCTIONS
            All functions in the block below are required for reading the data necessary to run a simulation.
        */

        /*
            Main loading runction.
        */
        public void ReadData (string filename) {
            StreamReader initfile = new StreamReader ( FOLDER + filename);

            string line;
            while ( (line = initfile.ReadLine()) != null) {
                if (line.Length == 0 ) continue;
                if (line[0] == '#') continue;
                if (line == "START_PARAMS") ReadParams(initfile);
                if (line == "START_STATION_DATA") ReadStationData(initfile);
                
            }
        }

        /*
            Function which reads all parameters given in initfile.ini. Passes on density fiels to their respective functions.
        */
        private void ReadParams(StreamReader initfile) {
            string line;
            while ( (line = initfile.ReadLine()) != "END_PARAMS") {
                if (line.Length == 0) continue;
                string[] words = splitLine(line);
                switch (words[0]) {
                    case "start_time":
                        StartTime = Time.ConvertFromString(words[1]);
                        break;
                    case "end_time":
                        EndTime = Time.ConvertFromString(words[1]) - StartTime;
                        break;
                    case "runs":
                        Runs = Int32.Parse(words[1]);
                        break;
                    case "trams":
                        NoTrams = Int32.Parse(words[1]);
                        break;
                    case "trams_hour":
                        ReadSchedule(line);
                        break;
                    case "seed":
                        SEED = Int32.Parse(words[1]);
                        break;
                    case "pass_0":
                        PassDir0 = Int32.Parse(words[1]);
                        break;
                    case "pass_1":
                        PassDir1 = Int32.Parse(words[1]);
                        break;
                    case "dir0_density":
                        ReadPassDensity (0, words[1]);
                        break;
                    case "dir1_density":
                        ReadPassDensity (1, words[1]);
                        break;
                    case "maxpasstram":
                        MaxPassTram = Int32.Parse(words[1]);
                        break;
                    case "batchname":
                        BATCHNAME = words[1];
                        break;
                    case "perstationleeway":
                        PerStationLeeway = Int32.Parse(words[1]);
                        break;
                    default:
                        Console.WriteLine("Parameter " + words[0] + " not recognized. Error found, quitting.");
                        Console.ReadLine(); Environment.Exit(1);
                        break;

                }
            }
        }

        /*
            Main function for reading station data. Firstly: reads out the list of stations. Then reads drivetime/passenger arrival rates/exit rates from their respective files.            
        */
        private void ReadStationData(StreamReader initfile) {
            if (EXPLICIT) {
                Console.WriteLine ("Reading station data");
                if (WAIT) Console.ReadLine();
            }
            ReadStations(initfile);
            string line;
            while ( (line = initfile.ReadLine()) != "END_STATION_DATA") {
                if (line.Length == 0) continue;
                if (line.Split()[0] == "DRIVETIME_FILE") ReadDriveTimes(line.Split()[1]);
                if (line.Split()[0] == "PASS_DISTR_FILE") ReadStationDistr(line.Split()[1] );
                if (line.Split()[0] == "PASS_EXIT_FILE") ReadExitDistr(line.Split()[1] );
            }
         }

        
        /*
            Reads the exit distribution from the file given.
            Optional: functions exactely the same as ReadStationDistr. Should be possible to make these two into a single generic function
        */
        private void ReadExitDistr(string filename) { 
            if (EXPLICIT) {
                Console.WriteLine ("Reading exit data");
                if (WAIT) Console.ReadLine();
            }
            PassOutrates = new TimeDistr[Stations.Length];
            StreamReader data = new StreamReader(FOLDER + filename);
            string line = data.ReadLine(); //read time data on this line
            string[] words = line.Split(',');
            int[] times = new int[words.Length -1];
            for (int i =0; i < times.Length; i++) {
                times[i] = Time.ConvertFromString(words[i+1]) - StartTime;
            }
            while ( (line = data.ReadLine()) != null) {
                if (line.Length == 0) continue;
                //find the right station.
                words = splitLine(line, ',');
                int pos = -1;
                for (int i = 0; i < Stations.Length; i++) {
                    if (Stations[i] == words[0] ) {
                        pos = i; 
                        break;
                    }
                }
                if (pos == -1) {
                    Console.WriteLine("Error in ReadExitDistr: found station not in Stations list. Check spelling.");
                    Console.ReadLine(); Environment.Exit(1);
                }

                double[] rates = new double[times.Length];
                //read distribution data
                for (int i = 1; i < words.Length; i++) { //NOTE: stating at i = 1 because first word is the station name
                    rates[i-1] = Double.Parse(words[i]);
                }
                PassOutrates[pos] = new TimeDistr(times,rates);                
            }

            for (int i = 0; i < PassOutrates.Length; i++) {
                if (PassOutrates[i] == null) {
                    Console.WriteLine("Data missing from ExitRates from station " + Stations[i] + ". Check files.");
                    Console.ReadLine(); Environment.Exit(1);
                }
            }
        }
    
        /*
            Reads the passenger arrival distribution over time file. 
            Note: see note at ReadExitDistr
        */
        private void ReadStationDistr(string filename) {
            if (EXPLICIT) {
                Console.WriteLine ("Reading distribution data");
                if (WAIT) Console.ReadLine();
            }
            TimeDistr[] stationdistrs = new TimeDistr[Stations.Length];
            StreamReader data = new StreamReader(FOLDER + filename);
            string line = data.ReadLine(); //read time data on these lines;
            string[] words = line.Split(',');
            int[] times = new int[words.Length -1];
            for (int i =0; i < times.Length; i++) {
                times[i] = Time.ConvertFromString(words[i+1]) - StartTime;
            }
            while ( (line = data.ReadLine()) != null) {
                if (line.Length == 0) continue;
                //find the right station.
                words = splitLine(line, ',');
                int pos = -1;
                for (int i = 0; i < Stations.Length; i++) {
                    if (Stations[i] == words[0] ) {
                        pos = i; 
                        break;
                    }
                }
                if (pos == -1) {
                    Console.WriteLine("Error in StationRates: found station not in Stations list. Check spelling.");
                    Console.ReadLine(); Environment.Exit(1);
                }

                double[] rates = new double[times.Length];
                //read distribution data
                for (int i = 1; i < words.Length; i++) { //NOTE: stating at i = 1 because first word is the station name
                    rates[i-1] = Double.Parse(words[i]);
                }
                stationdistrs[pos] = new TimeDistr(times,rates);                
            }
            for (int i = 0; i < stationdistrs.Length; i++) {
                if (stationdistrs[i] == null) {
                    Console.WriteLine("Data missing from StationRates from station " + Stations[i] + ". Check files.");
                    Console.ReadLine(); Environment.Exit(1);
                }
            }
            PassInrates = stationdistrs;
            
        }

        /*
            Reads average drive times and variances from file.
            Note: [stationname] , avg, var 
            means: driving from [stationname] to the next station takes avg seconds with a variance of var
        */
        private void ReadDriveTimes(string filename) {
            if (EXPLICIT) {
                Console.WriteLine ("Reading drivetime data");
                if (WAIT) Console.ReadLine();
            }
            StreamReader data = new StreamReader(FOLDER + filename);
            string line = data.ReadLine(); //get rid of comment line.
            string[] words;
            if (Stations == null) {
                Console.WriteLine("Stations not loaded in at ReadDrivesTimes. Check code");
                Console.ReadLine(); Environment.Exit(1);
            }
            DrivTimVar = new double[Stations.Length];
            DrivTimAvg = new double[Stations.Length];

            for (int i = 0; i < Stations.Length; i++) {
                DrivTimVar[i] = -1.0;
                DrivTimAvg[i] = -1.0;
            }
            while ( (line= data.ReadLine()) != null) {
                if (line.Length == 0) continue;
                words = splitLine(line, ',');
                int pos = -1;
                for (int i = 0; i < Stations.Length; i++) {
                    if (Stations[i] == words[0] ) {
                        pos = i; 
                        break;
                    }
                }
                if (pos == -1) {
                    Console.WriteLine("Error in ReadDriveTimes: found station " + words[0] + " not in Stations list. Check spelling.");
                    Console.ReadLine(); Environment.Exit(1);
                }
                DrivTimVar[pos] = Double.Parse(words[2]);
                DrivTimAvg[pos] = Double.Parse(words[1]);
            }
            for (int i = 0; i < Stations.Length; i++) {
                if (DrivTimVar[i] == -1.0 || DrivTimAvg[i] == -1.0) {
                    Console.WriteLine("Missing drive time variables for station " + Stations[i] + ". Check files.");
                    Console.ReadLine(); Environment.Exit(1);
                }
            }
        }

        /*
            Reads the list and ordering of stations and some additional metadata
        */
        private void ReadStations(StreamReader initfile) {
            string line;
            if (EXPLICIT) {
                Console.WriteLine ("Reading stations");
                if (WAIT) Console.ReadLine();
            }
            List<string> stations = new List<string>();
            List<bool> isendstation = new List<bool>();
            List<bool> hasdepot = new List<bool>();
            while ( (line= initfile.ReadLine()) != "END_STATIONS") {
                if (line.Length == 0) continue;
                string[] words = splitLine(line);
                if (words[0] == "endstation") {
                    stations.Add(words[1]);
                    isendstation.Add(true);
                    hasdepot.Add(bool.Parse(words[2]));
                }
                if (words[0] == "station") {
                    stations.Add (words[1]);
                    isendstation.Add(false);
                    hasdepot.Add(false);
                }
            }
            Stations = stations.ToArray();  
            isEndstation = isendstation.ToArray();
            hasDepot = hasdepot.ToArray();
        }
        

        /*
            Reads the scheduled times of how many trams leave per hour.
        */       
        private void ReadSchedule (string line) {
            int start = 0;
            int end = 0;
            for (int i = 0; i < line.Length; i++) {
                if (line[i] == '/') break;
                if (line[i] == '{') start = i +1;
                if (line[i] == '}') end = i - 1;
            }

            string datastring = line.Substring(start, end-start + 1);
            string[] pairs = datastring.Split(';');
            int[] times= new int[pairs.Length];
            double[] rates = new double[pairs.Length];
            string[] pair;
            
            for (int i =0 ; i <pairs.Length; i++) {
                pair = pairs[i].Split(',');
                times[i] = Time.ConvertFromString(pair[0]) - StartTime;
                double trams = Double.Parse(pair[1]);
               
                rates[i] = trams;
            }

            TramSchedule = new TimeDistr(times, rates);
        }

        /*
            Reads how the distribution of passenger arrival rates over time per station.
        */
        private void ReadPassDensity(int dir, string filename) { 
            List<int> times = new List<int>();
            List<double> rates = new List<double>();

            StreamReader file = new StreamReader (FOLDER + filename);
            file.ReadLine(); //get rid of comment line;
            string line;
            string[] words;
            while ( (line = file.ReadLine() ) != null ) {
                if (line.Length == 0) continue;
                words = line.Split(',');
                times.Add(Time.ConvertFromString(words[0]) - StartTime);
                rates.Add(double.Parse(words[2], System.Globalization.NumberStyles.Float));
            }
            if (dir == 0) PassDens_0 = new TimeDistr(times.ToArray(), rates.ToArray());
            if (dir == 1) PassDens_1 = new TimeDistr(times.ToArray(), rates.ToArray());
        }
    

        /*  BUILDING FUNCTIONS
            All functions in the following block are used to initialise the next day. 
            They create the new railway and trams, reset the clock and build the eventlist.
        */

        /*
            main function. Calls all other intitialisation functions.
        */
        private void initSim() {

            if (EXPLICIT) {
                Console.WriteLine("Initialising simulation");
                if (WAIT) Console.ReadLine();
            }
            buildRailRoad();
            buildTrams();
            EventList.buildEventList(EndTime, ((Endstation)railway[0]).showTimeTable(), (Endstation)railway[0], this);
        }


        /*
            Creates the new track layout
        */  
        private void buildRailRoad() {
            if (EXPLICIT) {
                Console.WriteLine("Initialising railway");
                if (WAIT) Console.ReadLine();
            }
            railway = new iHasRails[2 *Stations.Length ];
            for (int i = 0; i < Stations.Length; i ++) {
                TimeDistr passdens;
                int pass;
                if (Stations[i][Stations[i].Length -1] == '0') {
                    passdens = PassDens_0;
                    pass = PassDir0;
                }
                else {
                    passdens = PassDens_1;
                    pass= PassDir1;
                }

                if (isEndstation[i]) {
                    // This block: calculates the offset necesary in the timeschedule. 
                    int drivetime = 0;
                    for (int j = 0; j < i; j++) {
                        drivetime += Convert.ToInt32(DrivTimAvg[j]) + PerStationLeeway;
                    }
                    if (drivetime > 0) drivetime += 4*60;

                    railway[2*i] = new Endstation( Stations[i], hasDepot[i], NoTrams, PassInrates[i], 
                                                   PassOutrates[i], passdens, pass, DrivTimAvg[i], 
                                                   DrivTimVar[i], TramSchedule, drivetime , DataFolder, this
                                                  );
                }
                else railway[2*i] = new Station(Stations[i], PassInrates[i], PassOutrates[i], passdens, pass,
                                                DrivTimAvg[i], DrivTimVar[i], DataFolder, this);
                
                railway[2*i + 1] = new InTransit(NoTrams, this, Stations[i]);
            }

            for (int i = 0; i < railway.Length; i++) {
                railway[i].initStuff( railway[(railway.Length + i - 1)%railway.Length], railway[(railway.Length + i + 1)%railway.Length]
                                                                          );
            }
        }


        /*
            Instantiates the trams and places them in the Endstation containing the depot. The latter is currently hardcoded to be the first entry in railway
        */              
        private void buildTrams() {
            if (EXPLICIT) {
                Console.WriteLine("Initialising trams");
                if (WAIT) Console.ReadLine();
            }            trams = new Tram[NoTrams];

            for (int i = 0; i < NoTrams; i++) {
                trams[i] = new Tram(i, (Endstation)railway[0], MaxPassTram, DataFolder, this);
            }

        }


        /*
            Some functions used during the reading of all parameter files.
        */

        private string[] splitLine(string line) {

            line = Cull(line.ToLower());
            
            string[] words = line.Split();
            return words;

        }

        private string[] splitLine(string line, char split) {
            line = Cull(line.ToLower());
            string[] words = line.Split(split);
            return words;
        }

        private string Cull (string line) {
            int i ;
            for (i= 0; i < line.Length; i ++) {
                if (line[i] == '/') break;
            }
            line = line.Substring(0,i);

            return line;
        }

    }
}
