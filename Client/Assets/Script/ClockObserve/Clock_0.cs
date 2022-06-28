using UnityEngine;
using System.Collections;

public class Clock_0 : MonoBehaviour 
{
    public class ClockDriver {
        readonly TimeSink sink;
        public ClockDriver(TimeSource source,TimeSink sink) {
            source.SetDriver(this);
            this.sink=sink;
        }

        public void Update(int hours,int minutes,int seconds) {
            sink.SetTime(hours,minutes,seconds);
        }
    }       

    public interface TimeSink{
        void SetTime(int hours,int minutes,int seconds);
    }

    public interface TimeSource{
       void SetDriver(ClockDriver driver);
    }

    public class MockTimeSource:TimeSource{
        ClockDriver itsDriver;
        public void SetTime(int hours,int minutes,int seconds) {
            itsDriver.Update(hours,minutes,seconds);
        }

        public void SetDriver(ClockDriver driver) {
            itsDriver=driver;
        }
    }

    public class MockTimeSink:TimeSink{
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
        public void SetTime(int hours,int minutes,int seconds) {
            itsHours=hours;
            itsMinutes=minutes;
            itsSeconds=seconds;
        }
    }

    void Start() {
        MockTimeSource source=new MockTimeSource();
        MockTimeSink sink=new MockTimeSink();
        ClockDriver driver=new ClockDriver(source,sink);
        source.SetTime(3,4,5);
        DEBUG.Log((3==sink.GetHours()).ToString());
        DEBUG.Log((4==sink.GetMinutes()).ToString());
        DEBUG.Log((5==sink.GetSeconds()).ToString());

    }



}


