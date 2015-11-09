using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovieMaster.Helpers
{
    public interface IListen
    {
        void Notify(object msg);
    }
    
    public class EventHelper
    {
        private static EventHelper _instance;
        private Dictionary<Type, object> _map;
        private List<IListen> Listeners = new List<IListen>();

        private EventHelper()
        {
        }

        public static EventHelper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EventHelper();

                return _instance;
            }
        }

        public void Subscribe(IListen listener)
        {
            Listeners.Add(listener);
        }

        public void Unsubscribe(IListen listener)
        {
            Listeners.Remove(listener);
        }

        public void Publish(object msg)
        {
            Broadcast(msg);
        }

        private void Broadcast(object msg)
        {
            foreach (var listener in Listeners)
            {
                (listener as IListen).Notify(msg);
            }
        }        
    }
}
