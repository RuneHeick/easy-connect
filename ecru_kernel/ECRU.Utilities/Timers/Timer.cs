using System.Threading; 

namespace ECRU.Utilities.Timers
{
   
    public class ECTimer
    {
        Timer timer;
        int startTime, period;
        TimerCallback callback;
        object state;

        bool isrunning = false; 

        public ECTimer(TimerCallback callback, object state, int startTime, int period)
        {
            this.callback = callback;
            this.state = state;
            this.startTime = startTime;
            this.period = period;
        }

        public void Start()
        {
            if (isrunning)
                Stop();

            isrunning = true; 
            timer = new Timer(TimeCall, state, startTime, period); 
        }

        public void Stop()
        {
            isrunning = false;
            if (timer != null)
            {
                timer.Dispose();
                timer = null; 
            }
        }

        private void TimeCall(object obj)
        {
            if (isrunning)
                callback(obj); 
        }

    }
}