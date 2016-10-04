using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TramSim {

/*
    Prototype class, should never be instantiated.
    contains a few generic selfexplanatory functions
*/
    public class iHasRails {
        public Tram[] Spaces;
        public iHasRails Previous;
        public iHasRails Next;
        public string Name;
        protected Simulation Sim;


        //Cannot do this upon construction; not all iHasRails have been contructed by then.
        public virtual void initStuff(iHasRails prev, iHasRails next) {
            Previous = prev;
            Next = next;
        }

        public virtual void prepForEnd(StreamWriter totals) {
            return;
        }

        public bool AddTram (Tram tram) {
            int pos = firstOpenPos();
            if (pos != -1) {
                Spaces[pos] = tram;
                return true;
            }
            
            Console.WriteLine("Failure to add tram to station " + Name + ". Ending simulation" );
            Console.ReadLine(); Sim.EmergencyExit();;
            return false; 
        }

        public void RemoveTram(Tram tram){
            int pos = findTram(tram);
            if (pos != -1){
                Spaces[pos] = null;
            }
            else {
                Console.WriteLine("Tram " + tram.TramNo.ToString() + " not found at " + this.Name + ". Quiting simulation.");
                Console.ReadLine(); Console.ReadLine(); Sim.EmergencyExit();;;
            }
        }

        protected int firstOpenPos() {
            for (int i = 0; i < Spaces.Length; i++ ) {
                if (Spaces[i] == null) return i;
            }

            return -1;
        }

        public int TramWaitingCount() {
            int trams = 0;
            for (int i = 0; i <Spaces.Length; i++) {
                if (Spaces[i] != null && Spaces[i].busy == false) trams++;
            }
            return trams;
        }

        protected int findTram(Tram tram) {
            for (int i = 0; i < Spaces.Length; i++) {
                if (Spaces[i] == tram) return i;
            }
            return -1;
        }

        virtual public Tram ReadyTram() {    //returns tram if the first train in the queue is ready, null otherwise.  
            if (this.Spaces[0] != null && !this.Spaces[0].busy) return this.Spaces[0];
            else return null;
        }

        virtual public bool isStation() {
            return true;
        }


        //Handles everything needed for departure. 
        virtual public void handleDepart(Tram tram, int slot) { 
            Console.WriteLine("Attempted to call handleDepart from nonstation object. Crashing.");
            Console.ReadLine(); Console.ReadLine(); Sim.EmergencyExit();;;
        }
    
        //Handles the actual departing of trams from the station. Then checks if a tram can be put into the station in its place.
        virtual public void TramDepart (Tram tram) {
            Console.WriteLine("Attempted to call TramDepart from nonstation object. Crashing.");
            Console.ReadLine(); Console.ReadLine(); Sim.EmergencyExit();;;
        }

        //Executes on endTransit event. Handles arrival plus planning other events. 
        virtual public void handleArrival(Tram tram) {
            Console.WriteLine("Attempted to call handleArrival from nonstation object. Crashing.");
            Console.ReadLine(); Sim.EmergencyExit();;
        }

        //Handles actual departure. 
        virtual public void TramArrive (Tram tram) { 
            Console.WriteLine("Attempted to call TramArrive from nonstation object. Crashing.");
            Console.ReadLine(); Sim.EmergencyExit();;
        }



        //required method for InTransit objects. For all others: always return yes if the tram is there. Can add extra logic if stations are extended. 
        virtual public bool FirstinLine(Tram tram){
            Console.WriteLine("Attempted to call FirstinLine from station " + Name + "Crashing.");
            Console.ReadLine(); Sim.EmergencyExit();;
            return true; 
        }

        //Standard behaviour: throw tantrum, quit program. Only works for endstations. 
        virtual public void  LockTrack(int i) {
            Console.WriteLine("Attempted to call LockTrack from non-Endstation.");
            Console.ReadLine(); Sim.EmergencyExit();;
        }
    
        //See behaviour for LockTrack
        virtual public void UnlockTrack(int i) {
            Console.WriteLine("Attempted to call UnlockTrack from non-Endstation.");
            Console.ReadLine(); Sim.EmergencyExit();;
        }

        virtual protected int PassDepart ( int pass) {
	    	Console.WriteLine("Attempt to call PassDepart from Non-Station object. Crashing");
	    	Console.ReadLine(); Sim.EmergencyExit();;
            return -1;
        }

        virtual protected int TimePassExchange (int pin, int pout) {
	    	Console.WriteLine("Attempt to call TimePassExchange from Non-Station object. Crashing");
	    	Console.ReadLine(); Sim.EmergencyExit();;
            return -1;
        }

        virtual protected int TimeTransit() { 
	    	Console.WriteLine("Attempt to call TimeTransit from Non-Station object. Crashing");
	    	Console.ReadLine(); Sim.EmergencyExit();;
            return -1;
        }

        //moves trams at InTransits up to fill the first spot in the queue. Do nothing otherwise.
        public virtual void bumpTrams () {
            Console.WriteLine("Attempt to call bumpTrams from Station. Crashing");
		    Console.ReadLine(); Sim.EmergencyExit();;
        }
        
        virtual protected int updatePassWaiting() {
		    Console.WriteLine("Attempt to call updatePassWaiting from Non-Station object. Crashing");
		    Console.ReadLine(); Sim.EmergencyExit();;
            return -1;
        }

	    virtual public void TramRecruit () {
		    Console.WriteLine("Attempt to call TramRecruit from non-Endstation object.");
		    Console.ReadLine(); Sim.EmergencyExit();;
	    }
        virtual public void StartPassExchange (Tram tram) {
            Console.WriteLine ("Attempt to call StartPassExchange from Non-station object.");
            Console.ReadLine(); Sim.EmergencyExit();;
        }

        virtual public void EndPassExchange(Tram tram, int pin, int pout, bool iterate) {
            Console.WriteLine("Attempt to call EnPassExchange from Non-station object");
            Console.ReadLine(); Sim.EmergencyExit();;
        }


        virtual public void ItPassEmbarking (Tram tram) {
            Console.WriteLine("Attempt to call ItPassEmbarking from Non-station object");
            Console.ReadLine(); Sim.EmergencyExit();;
        }
    }

/*
    Regular stations
*/

    public class Station : iHasRails { 
        protected int lastpassupdate;
        protected int PassWaiting;
        protected TimeDistr PassArrival; //in passengers per second over time intervals
        protected TimeDistr PassOut; //chance per passenger in tram over time intervals
        protected int lastDeparture;
        protected StreamWriter logPassengersWaiting;
        protected StreamWriter logTramsWaiting;
        protected StreamWriter logInterDepartureTime;
        protected StreamWriter logDwelltime;
        protected StreamWriter logWaitingtime;
        protected double DriveAvg;
        protected double DriveVar;
        protected int TotalIn;
        protected int TotalOut;

        protected int transferPassNew;
        protected int transferTimeLast;
        protected int transferPassWaitStart;

        public Station (string name, TimeDistr inrate, TimeDistr outrate, TimeDistr indens, int pass, 
                        double driveavg, double drivevar, string datafolder, Simulation sim) { 
            if (Simulation.EXPLICIT) {
                Console.WriteLine("Initialising station " + name);
                if (Simulation.WAIT) Console.ReadLine();
            } 
            Name = name;
            Spaces = new Tram[1];
            lastpassupdate = 0;
            PassWaiting = 0;
            lastDeparture = 0;
            TotalIn = 0;
            TotalOut = 0;
            DriveAvg = driveavg;
            DriveVar = drivevar;
            Sim = sim;


            //reformulate distributions for incoming passengers to something sensible
            int[] times = indens.getTimes();
            double[] passengers = new double[times.Length];
            for (int i = 0; i < passengers.Length; i++) {
//                if (Simulation.EXPLICIT) Console.WriteLine("Calculating passengers at time " + times[i].ToString());
                passengers[i] = Convert.ToDouble(pass) * inrate.getRate(times[i]) * indens.getRate(times[i]);
//                if (Simulation.EXPLICIT) Console.WriteLine("Passengers from time " + times[i].ToString() + " : " + passengers[i].ToString());
            }
            PassArrival = new TimeDistr(times, passengers);

            PassArrival.WriteToFile(datafolder+Name+"PassDistr.csv");

            PassOut = outrate;
    
            logPassengersWaiting = new StreamWriter(datafolder + Name + "PassWaiting.csv");
            logPassengersWaiting.WriteLine("Time , Passengers waiting, passengers in tram");
            logInterDepartureTime = new StreamWriter(datafolder + Name + "IntDepTime.csv");
            logInterDepartureTime.WriteLine("Time , time since last Departure");
            logTramsWaiting = new StreamWriter(datafolder + Name + "TramsWaiting.csv");
            logTramsWaiting.WriteLine("Time , trams waiting , trams at station");
            logWaitingtime = new StreamWriter(datafolder + Name + "WaitingTime.csv");
            logWaitingtime.WriteLine("Time , passengers already waiting , time last departure , new passengers , time since last passenger update , passengers waiting after loading");
            logDwelltime = new StreamWriter(datafolder + name + "Dwelltime.csv");
            logDwelltime.WriteLine("Time , dwelltime of departing tram");

        }

        public override void prepForEnd(StreamWriter totals) {
            logPassengersWaiting.Close();
            logInterDepartureTime.Close();
            logTramsWaiting.Close();
            logDwelltime.Close();
            logWaitingtime.Close();

            totals.WriteLine(Name + " , " + TotalIn.ToString() + " , " + TotalOut.ToString());
        }   

        public override void initStuff(iHasRails prev, iHasRails next ) {            
            base.initStuff(prev, next);
        }


/*
    All RNG-based functions

*/

        override protected int updatePassWaiting() { 
            //get intervals from the PassArival which fall within lastupdate to Now, from there calculate the passengers waiting.

            if (Simulation.EXPLICIT) Console.WriteLine("Updating waiting passengers");
            int[] times = PassArrival.getIntervals(lastpassupdate, Time.Now());

            if (Simulation.EXPLICIT) Console.WriteLine("Calculating passengers at station " + Name + "over interval " + Time.Now().ToString() + " from " + lastpassupdate.ToString());

            int newpass = 0;
            for (int i = 0; i < times.Length -1; i++) {
                int time = times[i+1] - times[i];
                if (Simulation.EXPLICIT) Console.WriteLine("Calculatign for subinterval " + times[i].ToString() + " to " + times[i+1].ToString() + ". Passengers per second: " + PassArrival.getRate(times[i]).ToString());
                newpass += Math.Max(0, RNG.Poisson( PassArrival.getRate(times[i]) * Convert.ToDouble(time)));
                //newpass += Convert.ToInt32(PassArrival.getRate(Time.Now()) * Convert.ToDouble (Time.Now() - lastpassupdate)); //HOTFIX: TODO: FIX
            }


//            Console.WriteLine("passengers per second: " + PassArrival.getRate(Time.Now()).ToString() + " time since update: " + (Time.Now() - lastpassupdate).ToString() );
//            newpass = Convert.ToInt32(PassArrival.getRate(Time.Now()) * Convert.ToDouble (Time.Now() - lastpassupdate)); //hotfic, ugle as fuck.
            if (Simulation.EXPLICIT) Console.WriteLine("New passengers: " + newpass.ToString());
            lastpassupdate = Time.Now();
            PassWaiting += newpass;
            TotalIn += newpass;
            return newpass;
            
        }

        override protected int PassDepart ( int pass) {
            int pout = Math.Max(0, RNG.Bin(pass, PassOut.getRate(Time.Now())));
            TotalOut += pout;
            return pout;
        }

        override protected int TimePassExchange (int pin, int pout) {
            double delta = 12.5 + 0.13* Convert.ToDouble(pout) + 0.22 * Convert.ToDouble(pin);
            return (int)RNG.Gamma3(2, delta, 0.8* delta ); 
        }

        override protected int TimeTransit() { 
            return Convert.ToInt32(RNG.Norm(DriveAvg,DriveVar));
        }

/*
        Tram arrival methods
*/

        //Executes on endTransit event. Checks if tram is allow to enter the station
        override public void handleArrival(Tram tram) {
            tram.busy = false;
            tram.tramActivitylog.WriteLine(Time.Now() + " , Arrival , " + Previous.Name);

             if (firstOpenPos() != -1) {
                if (Previous.FirstinLine(tram)) TramArrive(tram);               
            }

            int trams =0;
            foreach (Tram place in Spaces) {
                if (place != null) {
                    trams++;
                }
            }
            logTramsWaiting.WriteLine(Time.Now().ToString() + " , " + Previous.TramWaitingCount().ToString() + " , " + trams.ToString());
            
        }

        //Handles actual arrival. 
        override public void TramArrive (Tram tram) { 
            if (this.firstOpenPos() == -1) {
                Console.WriteLine("Tried to move tram to station" + Name + " while there were no free spaces. Crashing");
                Console.ReadLine(); Sim.EmergencyExit();;
            }
            else {

                tram.moveToNext();
                tram.busy = false;
                Previous.bumpTrams();
                tram.arrivaltime = Time.Now();
                tram.tramActivitylog.WriteLine(Time.Now() + " , Arrival , " + Name);

    		    new StartPassTransfer(Time.Now(), tram, this );
                logInterDepartureTime.WriteLine((Time.Now()).ToString() + " , " + (Time.Now() - lastDeparture).ToString());
            }
        }


/*
        Passenger Embarkation/debarkation methods

*/
 
        override public void StartPassExchange(Tram tram) {
            int pin; int pout;
            tram.busy = true;

            transferPassNew = 0;
            transferTimeLast = lastpassupdate;
            transferPassWaitStart = PassWaiting;


            tram.tramActivitylog.WriteLine(Time.Now().ToString() + " , StartPassExchange , " + Name);

            int passnew = updatePassWaiting();
            pout = PassDepart(tram.PassCurr);
            pin = Math.Min(tram.PassMax - (tram.PassCurr - pout), PassWaiting);
            PassWaiting -= pin;

            transferPassNew += passnew;

            int extime = TimePassExchange(pin, pout);
            new EndPassTransfer(Time.Now() + extime, tram, this, pin, pout, true);
        }

        override public void EndPassExchange(Tram tram, int pin, int pout, bool iterate) {
            tram.busy = false;
            tram.tramActivitylog.WriteLine(Time.Now().ToString() + " , EndPassExchange , " + Name);
            tram.changePass(pin , pout);
	        if (iterate) ItPassEmbarking(tram);
            else {
                logWaitingtime.WriteLine(Time.Now() + " , " + transferPassWaitStart.ToString() + " , " + (Time.Now() - lastDeparture).ToString() 
                                         + " , " + transferPassNew + " , "  + (Time.Now() - transferTimeLast).ToString() + "  , " + PassWaiting );
                new StartTransit(Time.Now(), tram, this, 0);    
            }
        }

    	override public void ItPassEmbarking(Tram tram){ 
	    	int pnew = updatePassWaiting();
            transferPassNew += pnew;
            tram.busy = true;
            tram.tramActivitylog.WriteLine(Time.Now().ToString() + " , ItPassExchange , " + Name);
		    int pin = Math.Min(tram.PassMax - tram.PassCurr, PassWaiting);
            PassWaiting -= pin;
		    int deltat= (int)RNG.Gamma3(2, 0.2 * pin, 0.0);
		    new EndPassTransfer ( (Time.Now() + deltat), tram, this, pin, 0, false ) ;
	    }

/*
        Tram departure methods

*/

        //Handles everything needed for departure. 
        override public void handleDepart(Tram tram, int slot) { 
            tram.busy = true;
            TramDepart(tram);
        }
    
        //Handles the actual departing of trams from the station. Then checks if a tram can be put into the station in its place.
        override public void TramDepart (Tram tram) {
            lastDeparture = Time.Now();
            tram.moveToNext();
            logPassengersWaiting.WriteLine(Time.Now().ToString() + " , " + PassWaiting.ToString() + " , " + tram.PassCurr.ToString());
            logDwelltime.WriteLine(Time.Now().ToString() + " , " + (Time.Now() - tram.arrivaltime).ToString());
            new EndTransit(Time.Now() + this.TimeTransit(), tram, this.Next.Next);
            if (Previous.ReadyTram() != null) new EndTransit(Time.Now(), Previous.ReadyTram(), this);
        }


    }
    

/*
    Station with two spots for trams to load/unload, a diamondcrossing in front of it and possible access to a depot.


    note: some hardcoded parameters. Lock time (40), and turnaround time (4*60), for example.
*/
    public class Endstation : Station { 

        public Tram[] depot;
        private bool[] raillock;
    	private bool tramloading;
	    private int[] timetable;
        bool[] usedtimes;
        StreamWriter logPunctuality;
        private int Dweltime;
        private int RetireTime = Time.ConvertFromString("19:00") - Time.ConvertFromString("6:00"); //hardcoded. May need to be changed later.

        private int counter;

        public Endstation (string name, bool depot, int maxtrams, TimeDistr inrate, TimeDistr outrate, 
                           TimeDistr indens, int pass, double driveavg, double drivevar, TimeDistr schedule,
                           int timefromstrt, string datafolder, Simulation sim ) 
                           : base(name,inrate,outrate,indens, pass, driveavg, drivevar, datafolder, sim) {
            if (Simulation.EXPLICIT) {
                Console.WriteLine("Initialising Endstation " + name);
                if (Simulation.WAIT) Console.ReadLine();
            } 
            Spaces = new Tram[2];
            raillock = new bool[2];
            if (depot) this.depot = new Tram[maxtrams];
            Name = name;
            Dweltime = 4* 60;  //Parameterize this.
            //counter = 0;

            logPunctuality = new StreamWriter(datafolder + Name + "Punctuality.csv");
            logPunctuality.WriteLine("Time , Deviation from schedule");

            //create schedule
            int[] periods = schedule.getTimes();
            double[] intensity = schedule.getRates();

            if (Simulation.EXPLICIT) {
                Console.WriteLine("Creating timetable at Endstation " + name);
                if (Simulation.WAIT) Console.ReadLine();
            } 
            Queue<int> temptable = new Queue<int>();

            for (int i = 0; i < periods.Length ; i++) {
                if ( (i+1 == periods.Length + timefromstrt) || (intensity[i] == 0.0) ){
                    if (Simulation.EXPLICIT) Console.WriteLine("Scheduled leaving of tram at " + (periods[i]+ timefromstrt).ToString() );
                    temptable.Enqueue(periods[i] + timefromstrt);
                    continue;
                }
                int next = periods[i] + timefromstrt;
                
                int intdeptime = Convert.ToInt32( 3600.0/intensity[i]);
                while (next < periods[i+1] + timefromstrt) {
                    if (Simulation.EXPLICIT) Console.WriteLine("Scheduled event leaving at " + next.ToString() );

                    temptable.Enqueue(next);
                    next += intdeptime;
                }
            }

            timetable = temptable.ToArray();
            StreamWriter logTimeSchedule = new StreamWriter(datafolder + Name + "schedule.txt");
            
            foreach (int i in timetable) {
                logTimeSchedule.WriteLine(i.ToString());
            }

            usedtimes = new bool[timetable.Length];
            for (int i = 0; i < usedtimes.Length; i++) {
                usedtimes[i] = false;
            }
            logTimeSchedule.Close();

        }

        public override void initStuff (iHasRails prev, iHasRails next) {

            base.initStuff(prev, next);
        }

        public override void prepForEnd(StreamWriter totals) {
            logPunctuality.Close();
            bool allused = true;
            foreach (bool i in usedtimes) {
                if (!i) {
                    allused = false; break;
                }
            }
            if (!allused) {
                Console.WriteLine("Timetable at station " + Name + " not emptied at end of simulation.");
                //Console.ReadLine(); Sim.EmergencyExit();
            }
            base.prepForEnd(totals);
        }


/*
    Some methods pertaining to time tables.

*/

        public int[] showTimeTable() {
            return timetable;
        }

        private int NextDeparture(){
		    int first = -1;
            for (int i = 0; i < usedtimes.Length; i++) {
                if (!usedtimes[i]) {
                    first = i; break;
                }
            }
            if (first > -1) return timetable[first];
		    else return first;
	    }
    
        /*
            used to reserve a slot when planning a departure
            necessary so that other trams cannot use this scheduled departure for their decisions, but allows for the tram to release the slot in case it cannot leave at the scheduled time.
        */
        private int ReserveSlot() {
            int first = -1;
            for (int i = 0; i < usedtimes.Length; i++) {
                if (!usedtimes[i]) {
                    first = i; break;
                }
            }
            if (first == -1 ) {
                Console.WriteLine("Tried to reserve from an empty timetable at station " + Name);
                Console.ReadLine(); Sim.EmergencyExit();
            }
            usedtimes[first] = true;
            return first;
        }



/*
            TramLock methods

*/
        override public void LockTrack (int track){ 
		    if (track == 2) {
			    LockTrack (0);
			    LockTrack (1);
		    }
		    else {
			    if (raillock[track]){
				    Console.WriteLine("Attempt to lock locked track. Crashing");
				    Console.ReadLine(); Sim.EmergencyExit();;
                }
                raillock[track] = true;	
			}
				
		}

    	override public void UnlockTrack (int track){
            
            if (Simulation.EXPLICIT) Console.WriteLine("Releasing tracklocks at station " + Name);
    		if (track == 2){
    			raillock[0] = false;
    			raillock[1] = false;
    		}
    		else {
    			if (!raillock[track]){
    				Console.WriteLine("Attempt to unlock unlocked track. Crashing");
	    			Console.ReadLine(); Sim.EmergencyExit();;
	    		}
	    		raillock[track] = false;
    		}
    
    		//Check if tram can enter/leave station.

    		if (ReadyTram() != null && !raillock[0] ) {
                if (Simulation.EXPLICIT) Console.WriteLine("Planning departure of tram" + ReadyTram().TramNo.ToString() + " from station " + Name + " after lock has been released");
                //Console.ReadLine();
	    		planDeparture(ReadyTram());
    		}
	    	if (Previous.ReadyTram() != null && !raillock[1]) {
                if (Simulation.EXPLICIT) Console.WriteLine("Planning arrival of tram after lock has been released");
                //Console.ReadLine();
                new EndTransit(Time.Now(), Previous.ReadyTram(), this);
            }
	    }

        private int TimeLocked() { //currently hardcoded.
            return 40;
        }


/*
            Departure methods
*/


        //due to the scheduling at Termini which doesn't exist at regular stations, it's require to plan departures in advance.
        public void planDeparture(Tram tram) {  

            int next = NextDeparture();
            if (next == -1) {
                TramRetire(tram);
                return;
            }

            int slot = ReserveSlot();



            if (next < Time.Now() ) { 
                tram.busy = true;
                new StartTransit(Time.Now(), tram, this, slot);
            }
            else {
                tram.busy = true;
                new StartTransit(next, tram, this, slot);
            }

            if (tramloading && ReadyToLoad() != null) {
                Console.WriteLine("At planDeparture: tramloading == true, but also tram ready to load.");
                Console.ReadLine(); Sim.EmergencyExit();
            }
            if (ReadyToLoad() != null) {
                if (ReadyToLoad().TramNo == tram.TramNo) {
                    Console.WriteLine("Trying to start passenger loading departing tram");
                    Console.ReadLine(); Sim.EmergencyExit();
                }
                tramloading = true;
                new StartPassTransfer(Time.Now(), ReadyToLoad(), this);
            }
        }

    	override public void handleDepart(Tram tram, int slot) {

    		int pos = findTram(tram);
    		if (pos == 0) { // tram in line with departing rail
    			if (!raillock[0]) {
                    tram.busy = true;
	    			int time = TimeLocked();
		    		new TrackLock (Time.Now() + time, tram, this, 0, false);
                    logPunctuality.WriteLine ( Time.Now().ToString() + " , " + (Time.Now() - timetable[slot]).ToString()  );
                    counter ++;
                    //Console.WriteLine(counter.ToString());
                    
	       			base.handleDepart(tram, 0);
			    }
                else {
                     usedtimes[slot] = false;
                     tram.busy = false;
                }
    		}
	    	else if (pos == 1) { // tram not in line with departing rail
		    	if ((!raillock[0]) && (!raillock[1])) {
                    tram.busy = true;
			    	int time = TimeLocked();
                    logPunctuality.WriteLine ( Time.Now().ToString() + " , " + (Time.Now() - timetable[slot]).ToString()  );
                    counter++;
                    //Console.WriteLine(counter.ToString());
				    new TrackLock (Time.Now() + time, tram, this, 2, false);

    				base.handleDepart(tram, 0);
	    		}
                else {
                    usedtimes[slot] = false;
                    tram.busy = false;
                }
		    }
            else {
                Console.WriteLine("Event handleDepart at station " + Name + " at time " + Time.Now().ToString() + " attempted on tram not at the station." );
                Console.ReadLine(); Sim.EmergencyExit(); 
            }
    	}

        override public void TramDepart (Tram tram) {
            lastDeparture = Time.Now();
            
            tram.moveToNext();
            logPassengersWaiting.WriteLine(Time.Now().ToString() + " , " + PassWaiting.ToString() + " , " + tram.PassCurr.ToString());
            logDwelltime.WriteLine(Time.Now().ToString() + " , " + (Time.Now() - tram.arrivaltime).ToString());
            new EndTransit(Time.Now() + this.TimeTransit(), tram, Next.Next);
        }


/*
    Arrival methods

*/
	    override public void handleArrival(Tram tram){ 
		    int spot = firstOpenPos();
            tram.tramActivitylog.WriteLine(Time.Now() + " , Arrival , " + Previous.Name);

		    tram.busy = false;
            if (!Previous.FirstinLine(tram)) return;
		    if (spot == 0){
			    if ((!raillock[0]) && (!raillock[1])) {
				    int time = TimeLocked();
				    new TrackLock( Time.Now() + time, tram, this, 2, true);
		            TramArrive(tram); 
			    }
		    }
		    else if (spot == 1) {
			    if (!raillock[1]) {
				    int time = TimeLocked();
				    new TrackLock (Time.Now() + time, tram, this, 1, true);
		            TramArrive(tram);
			    }
		    }


            int trams =0;
            foreach (Tram place in Spaces) {
                if (place != null) {
                    trams++;
                }
            }
            logTramsWaiting.WriteLine(Time.Now().ToString() + " , " + Previous.TramWaitingCount().ToString() + " , " + trams.ToString());

	    }

    	override public void TramArrive (Tram tram) { 
	    	if (firstOpenPos() == -1){
		    	Console.WriteLine("Attempted to TramArrive at full endpoint. Crashing");
			    Console.ReadLine(); Sim.EmergencyExit();;
    		}
	    	else {
		    	tram.moveToNext();
                tram.busy = false;
                tram.hasloaded = false;
                Previous.bumpTrams();
                tram.arrivaltime = Time.Now();
                tram.tramActivitylog.WriteLine(Time.Now() + " , Arrival , " + Name);

                logInterDepartureTime.WriteLine((Time.Now()).ToString() + " , " + (Time.Now() - lastDeparture).ToString());

			    if (!TramRetire(tram)) {
				    if (!tramloading) {
                        new StartPassTransfer(Time.Now(), tram, this);
                        tramloading = true;
                    }
			    }
		    }
	    }

/*
        Passenger embarkation/debarkation methods
*/

	    override public void StartPassExchange(Tram tram) { 
	        tramloading = true; 
            tram.busy = true;
            if(Simulation.EXPLICIT) Console.WriteLine("starting passenger exchange");
            base.StartPassExchange(tram);
            
	    }

	    override public void EndPassExchange(Tram tram, int pin, int pout, bool iterate) {
            tram.changePass(pin , pout);
            tram.tramActivitylog.WriteLine(Time.Now().ToString() + " , EndPassExchange , " + Name);
            if(Simulation.EXPLICIT) Console.WriteLine("ending passenger exchange");
    	    if (iterate) { ItPassEmbarking(tram); }
            else {  
                tramloading = false;
                tram.busy = true;
                tram.cameFromDepot = false;
                tram.hasloaded = true;
                logWaitingtime.WriteLine(Time.Now() + " , " + transferPassWaitStart.ToString() + " , " + (Time.Now() - lastDeparture).ToString() 
                                         + " , " + transferPassNew + " , "  + (Time.Now() - transferTimeLast).ToString() + "  , " + PassWaiting );
                planDeparture(tram); 
            }  
	    }
	
	    override public void ItPassEmbarking(Tram tram) { 
		    int pnew = updatePassWaiting();
            transferPassNew += pnew;

            tram.tramActivitylog.WriteLine(Time.Now().ToString() + " , ItPassExchange , " + Name);

            if(Simulation.EXPLICIT) Console.WriteLine("iterative passenger embarkation");

		    int pin = Math.Min(tram.PassMax - tram.PassCurr, PassWaiting);
            PassWaiting -= pin;
            int deltat = (int)RNG.Gamma3(2, 0.2 * (double)pin, 0.0); 

            if (( !tram.cameFromDepot ) && (Time.Now() - tram.arrivaltime < Dweltime) ) {

                new PassEmbarkWait ( tram.arrivaltime + Dweltime, tram,  pin, this);
            }
            else if (NextDeparture() > Time.Now() + 5){
                new PassEmbarkWait ( NextDeparture() - 5 , tram, pin , this);
            }
            else {
                 new EndPassTransfer ( (Time.Now() + deltat), tram, this, pin, 0, false);
            }
        }


/*
        Tram Recruitment/retiring
*/
	    private bool TramRetire (Tram tram) { 
		    if (depot != null ) { //wrong endstation
                if ( ( (Time.Now() >= RetireTime ) && (firstOpenPos()  == -1) ) || NextDeparture() == -1) {
        		    RemoveTram (tram);
				    int i = 0;
				    while ( i < depot.Length ) {
					    if (depot[i] == null) {
                            EventList.checkForTram(tram);
                            int pout = PassDepart(tram.PassCurr );
                            TotalOut += pout;
                            tram.changePass(0, pout);
                            if(Simulation.EXPLICIT) Console.WriteLine("Retired tram to depot");
						    depot[i] = tram;
                            
                            
                            tram.tramActivitylog.WriteLine(Time.Now().ToString() + " , Retired , " + Name);
                            tram.indepot = true;
						    return true;
					    }
					    i++;
				    }
			    }   
            
/*          This was an idea, but not really necessary, since it ends after the even this belongs to is done anyhow.
            bool full = true;
            for (int i = 0; i < depot.Length; i++) {
                if (depot[i] == null) { full = false; break; }
            }
            if (full) //Find some way to end simulation from here...
*/
            }            
		    return false;
    	}

	    override public void TramRecruit() {
            
            if(NextDeparture() == -1) return;

/*
            //old recruitment. Retired because it caused high delays at high intensity scheduels.
            if (Spaces[1] != null || Spaces[0] != null) return;
            else Recruit();

*/
            //new recruitment
            if (Time.Now() + 30 > NextDeparture() && firstOpenPos() != -1) Recruit();
		    else if ((Spaces[0] != null || Spaces[1] != null )) { //At least one tram there
                if (firstOpenPos() != -1) {
                    int pos = 0;
                    if (Spaces[1] != null)  pos = 1;

                    if (Spaces[pos].arrivaltime + 4*60 > NextDeparture()) {   // the tram that's there can't possibly depart on time
                        Recruit();
                    }
                }
                else { // two trams at station, no possibility to recruit.
                    if (Simulation.EXPLICIT)Console.WriteLine("Recruitment unncessary, tram already there.");
			        return;
                }
		    }
		    else {				// recruit tram from depot
                Recruit();
		    }


	    }

        public void Recruit() {
    	    int  i = 0 ;
		    while (	i < depot.Length ) {
    			if (depot[i] != null) {
                    if (Simulation.EXPLICIT) Console.WriteLine("Recruited tram.");

	    		    Tram tram = depot[i];
                    tram.tramActivitylog.WriteLine(Time.Now().ToString() + " , Recruited , " + Name);
		    		AddTram(tram);
			    	depot[i] = null;
                    tram.indepot = false;
                    tram.cameFromDepot = true;
                    tram.arrivaltime = Time.Now();
                    tramloading = true;
				 	new StartPassTransfer(Time.Now(), tram, this);
				    return;
					
				}
				    i++;
			}
            if (Simulation.EXPLICIT) Console.WriteLine("No Tram to recruit.");           
        }


/*
    Some generic methods
*/

        // Needs an override, since there are two possible ready trams at an endstation.
    	override public Tram ReadyTram(){
    		if ( (Spaces[0] != null)  && ( Spaces[0].busy == false) && Spaces[0].hasloaded) return Spaces[0];
    		else if ( (Spaces[1] != null ) && ( Spaces[1].busy == false) && Spaces[1].hasloaded) return Spaces[1];
    		else return null;
    	}

        public Tram ReadyToLoad() {
            if ( (Spaces[0] != null) && (Spaces[0].busy == false) && Spaces[0].hasloaded == false) return Spaces[0];
            else if ( (Spaces[1] != null) && (Spaces[1].busy == false) && Spaces[1].hasloaded == false) return Spaces[1];
            else return null;
        }


    }


/*
    The bit of rails between stations.

*/

    class InTransit : iHasRails {
        public InTransit (int maxTrams, Simulation sim, string name) {
            Spaces = new Tram[maxTrams];
            Name = "intransitafter" + name;
            Sim = sim;
        }


        //move trains to fill empty spots in queue
        public override void bumpTrams() {
		    int i = 0;
		    while (i< Spaces.Length -1 && Spaces[i+1] != null){
    			Spaces[i] = Spaces[i+1];
	    		Spaces[i+1] = null;
                i++;
		    }
        }

	    public override bool isStation () {
		    return false;
	    }


    	override public bool FirstinLine(Tram tram){
	    	if (Spaces[0] == tram){
		    	return true;
		    }
		    return false;
	    }
    }

}
