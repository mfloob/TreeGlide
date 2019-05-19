using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TreeGlide.Managers
{
    public class TimerManager
    {
        private List<DispatcherTimer> timerList;

        public TimerManager()
        {
            this.timerList = new List<DispatcherTimer>();
        }

        public DispatcherTimer CreateTimer(int delay, bool addToTimerList)
        {
            DispatcherTimer timer = new DispatcherTimer();
            if (addToTimerList)
                this.timerList.Add(timer);
            timer.Interval = new TimeSpan(0, 0, 0, 0, delay);
            return timer;
        }

        public void StopTimers()
        {
            foreach (DispatcherTimer timer in this.timerList)
                timer.Stop();
        }
    }
}
