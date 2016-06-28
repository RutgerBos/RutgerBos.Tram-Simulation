using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
    Contains all classes/methods pertaining to Events.
*/
namespace TramSim {

    /*
        Contains functions to build the eventlist at the start and return the next event.
    */
    public static class EventList { 
        static List<Event> list;
        public static Simulation Sim;
        static EventList() {
            list = new List<Event>();
        }
        
        public static void addEvent(Event happening) {
            foreach (Event oud in list) {
                if ( (happening.Subject!= null) && oud.Subject != null && (happening.Name == oud.Name) && (happening.Subject.TramNo == oud.Subject.TramNo) ) {
                    Console.WriteLine("Duplicate event " + happening.Name + " detected at station " +happening.Place.Name + " for tram " + happening.Subject.TramNo.ToString());
                    Console.ReadLine(); Sim.EmergencyExit();
                }
                if ( (happening.Subject!= null) && oud.Subject != null && (happening.Subject.TramNo == oud.Subject.TramNo) ) {
                    Console.WriteLine("Dual event found for tram" + happening.Subject.TramNo.ToString()+ ": " +happening.Name + " at " + happening.Place.Name + "  and " + oud.Name + " at " + oud.Place.Name);
                    Console.ReadLine(); Sim.EmergencyExit();
                }
            }

     
            int time = happening.ETime;
            for (int i =0; i < list.Count(); i++) {
                if (time < list[i].ETime) {              
                    list.Insert(i, happening);
                    if (Simulation.EXPLICIT) Console.WriteLine("Inserted event " + happening.Name + " at station " + happening.Place.Name + " for time " + happening.ETime.ToString() + " at time " + Time.Now().ToString());
                    return;
                }
                else if (time == list[i].ETime && happening.Priority > list[i].Priority) {
                    list.Insert(i, happening);
                    if (Simulation.EXPLICIT) Console.WriteLine("Inserted event " + happening.Name + " at station " + happening.Place.Name + " for time " + happening.ETime.ToString() + " at time " + Time.Now().ToString());
                    return;
                }
            }
            list.Add(happening);            
        }

        // note: also removes event from the list.
        public static Event nextEvent() {
            if (list.Count() > 0) {
                Event next = list[0];
                list.RemoveAt(0);
                return next;
            }
            else return null;
        }

        /*
            Creates the eventlist.
            Events added upon creation:
                EndSim event
                RecruitTram event (planned 30 seconds before each planned departure at the endstation containing the depot)
        */
        public static void buildEventList (int endtime, int[] timetable, Endstation place, Simulation sim) {
            Sim = sim;
            if (Simulation.EXPLICIT) {
                Console.WriteLine("Initialising event list");
                if (Simulation.WAIT) Console.ReadLine();
            }
            list.Add(new EndSim(endtime, sim));
            for (int i = 0; i < timetable.Length; i++) {
                int time = Math.Max(0, timetable[i] - 30);
                new RecruitTram(time, null, place);
            }
        }
        
        /*
            called when the EndSim event is called. Checks if the eventlist is empty. If it's not, it creates a new EndSim event and forces continuation of the main loop.
        */
        public static bool allowEnding(Simulation sim) {
            Console.WriteLine("Checking if allowing to end");
            if (list.Count() > 0) {
                Console.WriteLine("Simulation would've ended while not all events were done");
                if (Simulation.WAIT) Console.ReadLine();
                list.Add( new EndSim ( list[list.Count() -1].ETime + 600, sim) );
                return false;
            }
                
            else {
                Console.WriteLine("Ending Simulation Allowed at time." );
                return true;
            }
        }

        //There shouldn't be two events for the same tram in the event queue
        public static void checkForTram(Tram tram) {
            foreach (Event e in list) {
                if (e.Subject == tram) {
                    Console.WriteLine("Tram already has event in the list");
                    Console.ReadLine(); Sim.EmergencyExit();
                }
            }
        }
        

        public static void prepEnd() {
            list.Clear();
        }
    }

    /*
        Prototype class. Should never be created.
    */
    public class Event {
        public int ETime;
        public Tram Subject;
        public string Name;
        public int Priority;
        public iHasRails Place; //Place where event will take place. IE: if Arrival at station, this should be the station where the tram arrives, not where it leaves from.

        virtual public void execEvent() {
            Console.WriteLine("If you see this event, you have forgotten to create an event handler. You fucking donkey.");
            EventList.Sim.EmergencyExit();
        }

        protected void addToList() {
            EventList.addEvent(this);
        }
       
    }



    class StartTransit : Event { 
        int number;
        public StartTransit(int time, Tram tram, iHasRails place, int timenumber) {
            EventList.checkForTram(tram);
            Priority = 5;
            ETime = time; Subject = tram; Place = place; number = timenumber;
            Subject.busy = true;
            if (Place != tram.Position) {
                 Console.WriteLine("At StartTransit event: place and subject.Position did not match. " + Place.Name + " vs " + Subject.Position.Name);
                 Console.ReadLine(); EventList.Sim.EmergencyExit();
            }

            if (Simulation.EXPLICIT) Console.WriteLine("Planned Starttransit of tram"  + Subject.TramNo.ToString() + " at station " + Place.Name + " at time " + Time.Now().ToString());
            Name = "StartTransit";
            base.addToList();
        }

        override public void execEvent() {
            if (Simulation.EXPLICIT) {
                Console.WriteLine("Starting Transit of tram " + Subject.TramNo.ToString() + " at station " + Place.Name + "planned for" + ETime.ToString() +" at time " + Time.Now().ToString());
                if (Simulation.WAIT) Console.ReadLine();
            }
            if ( !Place.Name.Trim().Equals( Subject.Position.Name.Trim())) {
                Console.WriteLine("At Execution of starttransit: place and subject.position do not match. " +Place.Name + " vs " + Subject.Position.Name);
                Console.ReadLine(); EventList.Sim.EmergencyExit();
            }
            Place.handleDepart(Subject, number);
        }
    }



    class EndTransit : Event {

        public EndTransit(int time, Tram tram, iHasRails place) {
            EventList.checkForTram(tram);
            Priority = 5;
            ETime = time; Subject = tram; Place = place;
            tram.busy = true;
            if ( !place.Name.Trim().Equals( tram.Position.Next.Name.Trim())) {
                Console.WriteLine("AT EndTransit: place and subject.position.next do not match. \"" + Place.Name + "\" vs \"" + Subject.Position.Next.Name + "\"");
                 Console.ReadLine(); EventList.Sim.EmergencyExit();
            }
            Name = "Endtransit";
            base.addToList();
        }


        override public void execEvent() { 
            if (Simulation.EXPLICIT) {
                Console.WriteLine("Ending of tram " + Subject.TramNo.ToString() + " at station " + Place.Name + " at time " + Time.Now().ToString());
                if (Simulation.WAIT) Console.ReadLine();
            }
            Place.handleArrival(Subject); //handle crossover locking for endstations here. For other stations: just start loading passengers and shit. 
            

        }      
    }

    class StartPassTransfer : Event {
        public StartPassTransfer(int time, Tram tram, iHasRails place) {
            EventList.checkForTram(tram);
            Priority = 2;
            tram.busy = true; 
            ETime = time; Subject = tram; Place = place;
            Name = "StartPassTransfer";
            base.addToList();            
        }

        public override void execEvent() {
            if (Simulation.EXPLICIT) {
                Console.WriteLine("Starting passenger transfor of tram " + Subject.TramNo.ToString() + " at station " + Place.Name + " at time " + Time.Now().ToString());
                if (Simulation.WAIT) Console.ReadLine();
            }
            if ( !Place.Name.Trim().Equals( Subject.Position.Name.Trim())) {
                Console.WriteLine("At Execution of starttransfer: place and subject.position do not match. " +Place.Name + " vs " + Subject.Position.Name);
                Console.ReadLine(); EventList.Sim.EmergencyExit();
            }
            Place.StartPassExchange(Subject);
        }
    }
    class EndPassTransfer : Event {
        int PassIn, PassOut; bool Cont;
        public EndPassTransfer(int time, Tram tram, iHasRails place, int In, int Out, bool cont) {
            EventList.checkForTram(tram);

            Priority = 4;
            PassIn = In; PassOut = Out;
            ETime = time; Subject = tram; Place = place;
            Cont = cont;
            Name = "EndPassTransfer";
            base.addToList();
        }

        public override void execEvent() {
            if (Simulation.EXPLICIT) {
                Console.WriteLine("Ending passenger transfer of tram " + Subject.TramNo.ToString() + " at station " + Place.Name + " at time " + Time.Now().ToString());
                Console. WriteLine (" In : " + PassIn.ToString() + " out : " + PassOut.ToString());
                if (Simulation.WAIT) Console.ReadLine();
            }
            if ( !Place.Name.Trim().Equals( Subject.Position.Name.Trim())) {
                Console.WriteLine("At Execution of endpasstransfer: place and subject.position do not match. " +Place.Name + " vs " + Subject.Position.Name);
                Console.ReadLine(); EventList.Sim.EmergencyExit();
            }
            Place.EndPassExchange(Subject, PassIn, PassOut, Cont);
        }
    }


/*
    Only applicable for endstations.
*/
    class TrackLock : Event { 
        int Track;
        bool Incoming;

        public TrackLock(int time, Tram tram, iHasRails place, int track, bool incoming ) {
            Priority = 6;
            ETime = time; Place = place; Track = track; Incoming = incoming;
            Place.LockTrack(track);
            Name = "TrackLock";
            if (Simulation.EXPLICIT) {
                Console.WriteLine("Starting tracklock of  at station " + Place.Name + " at time " + Time.Now().ToString());
                if (Simulation.WAIT) Console.ReadLine();
            }

            base.addToList();
        }

        public override void execEvent() {
            if (Simulation.EXPLICIT) {
                Console.WriteLine("Ending tracklock at station " + Place.Name + " at time " + Time.Now().ToString());
                if (Simulation.WAIT) Console.ReadLine();
            }
            Place.UnlockTrack(Track); 
        }
    }



/*
    Does what it says on the tin.
*/
    class EndSim : Event {
        Simulation Sim;
        public EndSim(int time, Simulation sim) {
            Priority = -1;
            ETime = time;
            Name = "Endsim";
            Sim = sim;
            if (Simulation.EXPLICIT) Console.WriteLine("End of simulation planned for " + ETime.ToString());
        }

        public override void execEvent() {
            if (Simulation.EXPLICIT) Console.WriteLine("End of simulation");
            Sim.endSim();
        }
    }


/*
    Should only be scheduled at initiation.
*/
	class RecruitTram : Event {
		public RecruitTram (int time, Tram tram, iHasRails place){
            Priority = 0;
			ETime = time; Subject = tram; Place= place;
            Name = "RecruitTram";
			base.addToList();		
		}

		public override void execEvent() {
            if (Simulation.EXPLICIT) {
                Console.WriteLine("Recruiting tram at station " + Place.Name + " at time " + Time.Now().ToString());
                if (Simulation.WAIT) Console.ReadLine();
            }
			Place.TramRecruit();
		}
	}


/*
    Only used at endstations if they're still turning around or if they're waiting for the next scheduled departure.
*/
	class PassEmbarkWait : Event {
        int Pin;
        public PassEmbarkWait(int time, Tram tram, int pin, iHasRails place){
            EventList.checkForTram(tram);
            Priority = 3;
			ETime = time; Subject = tram; Place= place;
            Subject.busy = true;
            Pin = pin;
            Name = "PassEmbarkWait";
            if ( !Place.Name.Trim().Equals( Subject.Position.Name.Trim())) {
                Console.WriteLine("At creation of passembarkwait: place and subject.position do not match. " +Place.Name + " vs " + Subject.Position.Name);
                Console.ReadLine(); EventList.Sim.EmergencyExit();
            }
			base.addToList();		
		}

		public override void execEvent() {
            if (Simulation.EXPLICIT) {
                Console.WriteLine("Waiting further passenger embarkation of tram " + Subject.TramNo.ToString() + " at station " + Place.Name + " at time " + Time.Now().ToString());
                if (Simulation.WAIT) Console.ReadLine();
            }
            if ( !Place.Name.Trim().Equals( Subject.Position.Name.Trim())) {
                Console.WriteLine("At Execution of passembarkwait: place and subject.position do not match. " +Place.Name + " vs " + Subject.Position.Name);
                Console.ReadLine(); EventList.Sim.EmergencyExit();
            }
			Place.EndPassExchange(Subject, Pin, 0, true);
		}
	}
}
