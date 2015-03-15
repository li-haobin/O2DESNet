using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpSimulator
{
    public class EventRecorder<T>
    {
        class EventRecord
        {
            internal int StatusIndex;
            internal T Load;
            internal DateTime ClockTime;
        }
        private DESModel _desModel;
        private Dictionary<string, int> _eventIndices;
        private List<EventRecord> _recordList;
        private List<T> _loadList;
        public bool Active { get; set; }
        public EventRecorder(DESModel desModel)
        {
            _desModel = desModel;
            _eventIndices = new Dictionary<string, int>();
            _recordList = new List<EventRecord>();
            _loadList = new List<T>();
            Active = true;
        }
        public void CheckIn(T load, string eventKey, bool consoleDisplay = false)
        {
            if (!Active) return;
            if (!_loadList.Contains(load)) _loadList.Add(load);
            _recordList.Add(new EventRecord { StatusIndex = GetIndex(eventKey), Load = load, ClockTime = _desModel.ClockTime });
            if (consoleDisplay) Console.WriteLine("{0}: {1} of {2}.", _desModel.ClockTime, eventKey, load.ToString());
        }
        public Dictionary<DateTime, int> Counts(string countInEventKeys, string countOutEventKeys)
        {
            _recordList = _recordList.OrderBy(r => r.ClockTime).ToList();
            List<T> counted = new List<T>();
            var timeSeries = new Dictionary<DateTime, int>();
            var countInIndices = countInEventKeys.Split(',').Select(s => GetIndex(s)).ToArray();
            var countOutIndices = countOutEventKeys.Split(',').Select(s => GetIndex(s)).ToArray();
            foreach (var record in _recordList)
            {
                if (countInIndices.Contains(record.StatusIndex) && !counted.Contains(record.Load)) counted.Add(record.Load);
                else if (countOutIndices.Contains(record.StatusIndex)) counted.Remove(record.Load);
                if (!timeSeries.ContainsKey(record.ClockTime)) timeSeries.Add(record.ClockTime, counted.Count);
                else timeSeries[record.ClockTime] = counted.Count;
            }
            return timeSeries;
        }
        public double AverageCount(string countInEventKeys, string countOutEventKeys, DateTime? startTime = null, DateTime? endTime = null)
        {
            var timeSeries = Counts(countInEventKeys, countOutEventKeys);
            if (startTime == null) startTime = _recordList.First().ClockTime;
            if (endTime == null) endTime = _recordList.Last().ClockTime;
            DateTime current = startTime.Value;
            var totalTimeSpan = endTime.Value - startTime.Value;
            double averageCount = 0;
            foreach (var item in timeSeries)
            {
                if (item.Key > current)
                {
                    if (item.Key < endTime)
                    {
                        averageCount += 1.0 * item.Value * (item.Key - current).TotalMinutes / totalTimeSpan.TotalMinutes;
                        current = item.Key;
                    }
                    else
                    {
                        averageCount += 1.0 * item.Value * (endTime.Value - current).TotalMinutes / totalTimeSpan.TotalMinutes;
                        break;
                    }
                }
            }
            return averageCount;
        }
        private int GetIndex(string eventKey)
        {
            if (!_eventIndices.ContainsKey(eventKey)) _eventIndices.Add(eventKey, _eventIndices.Count);
            return _eventIndices[eventKey];
        }

    }
}
