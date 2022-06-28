using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Clock_2:MonoBehaviour {
    public interface ClockObserver {
        void Update(int hours,int minutes,int secs);
    }


    public interface TimeSource {
        void RegisterObserver(ClockObserver observer);
    }

    public class MockTimeSource:TimeSource {
        List<ClockObserver> itsObservers=new List<ClockObserver>();
        public void SetTime(int hours,int minutes,int seconds) {
            itsObservers.ForEach(observer => observer.Update(hours,minutes,seconds));
        }

        public void RegisterObserver(ClockObserver observer) {
            itsObservers.Add(observer);
        }
    }

    public class MockTimeSink:ClockObserver {
        int itsHours;
        int itsMinutes;
        int itsSeconds;

        public int GetHours() {
            return itsHours;
        }

        public int GetMinutes() {
            return itsMinutes;
        }
        public int GetSeconds() {
            return itsSeconds;
        }
        public void Update(int hours,int minutes,int seconds) {
            itsHours=hours;
            itsMinutes=minutes;
            itsSeconds=seconds;
        }
    }

    void Start() {
        MockTimeSource source=new MockTimeSource();
        MockTimeSink sink=new MockTimeSink();
        source.RegisterObserver(sink);
        //ClockDriver driver=new ClockDriver(source,sink);
        //source.SetObserver(sink);
        source.SetTime(3,4,5);
        DEBUG.Log((3==sink.GetHours()).ToString());
        DEBUG.Log((4==sink.GetMinutes()).ToString());
        DEBUG.Log((5==sink.GetSeconds()).ToString());

    }



}


