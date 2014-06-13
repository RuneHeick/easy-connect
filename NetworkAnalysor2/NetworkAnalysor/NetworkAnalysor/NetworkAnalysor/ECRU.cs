using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkAnalysor
{
    public class ECRU: ViewModel.ViewModelBase
    {
        public ECRU()
        {
            ECMS = new ECMList(); 
        }

        private byte[] mac; 
        public byte[] Mac 
        {
            get
            {
                return mac; 
            }
            set
            {
                mac = value;
                OnPropertyChanged("Mac");
            }
        }

        private Timer TimeOutTimer = null;
        private DateTime lastSeen; 
        public DateTime LastSeen
        {
            get
            {
                return lastSeen;
            }
            set
            {
                lastSeen = value;
                OnPropertyChanged("LastSeen");
                OnPropertyChanged("IsOnline");
                if (TimeOutTimer == null)
                    TimeOutTimer = new Timer(TimeoutCallback, null, 30005 * 3, Timeout.Infinite);
                else
                    TimeOutTimer.Change(30005 * 3, Timeout.Infinite);
            }
        }

        private void TimeoutCallback(object state)
        {
            OnPropertyChanged("IsOnline");
        }

        private byte[] netstate; 
        public byte[] NetState
        {
            get
            {
                return netstate;
            }
            set
            {
                netstate = value;
                OnPropertyChanged("NetState");
            }
        }

        private string ipAddress; 
        public string IPAddres
        {
            get
            {
                return ipAddress;
            }
            set
            {
                ipAddress = value;
                OnPropertyChanged("IPAddres");
            }
        }

        public bool IsOnline
        {
            get
            {
                if (DateTime.Now < (lastSeen + new TimeSpan(0, 1, 30)))
                    return true;
                return false; 
            }
        }

        public ECMList ECMS { get; set; }

    }
}
