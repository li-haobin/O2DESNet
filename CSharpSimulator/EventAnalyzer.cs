using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpSimulator
{
    [Serializable]
    public class EventAnalyzer<T>
    {
        [Serializable]
        class EventRecord
        {
            internal int StatusIndex;
            internal T Load;
            internal DateTime ClockTime;
        }
        private DESModel _desModel;
        private Dictionary<string, int> _eventIndices;
        private List<EventRecord> _recordList;
        private Dictionary<T, List<EventRecord>> _recordListByLoad;
        public bool Active { get; set; }
        public EventAnalyzer(DESModel desModel)
        {
            _desModel = desModel;
            _eventIndices = new Dictionary<string, int>();
            _recordList = new List<EventRecord> { new EventRecord { ClockTime = _desModel.ClockTime, StatusIndex = -1 } };
            _recordListByLoad = new Dictionary<T, List<EventRecord>>();
            Active = true;            
        }
        public void CheckIn(T load, string eventKey, bool consoleDisplay = false)
        {
            if (!Active) return;
            if (!_recordListByLoad.ContainsKey(load)) _recordListByLoad.Add(load, new List<EventRecord>());
            _recordList.Add(new EventRecord { StatusIndex = GetIndex(eventKey), Load = load, ClockTime = _desModel.ClockTime });
            _recordListByLoad[load].Add(_recordList.Last());
            if (consoleDisplay) Console.WriteLine("{0}: {1} of {2}.", _desModel.ClockTime, eventKey, load.ToString());
        }
        public Dictionary<DateTime, int> Counts(string countInEventKeys, string countOutEventKeys)
        {
            var counted = new HashSet<T>();
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
            if (startTime == null) startTime = InitialClockTime;
            if (endTime == null) endTime = LastChekInClockTime;
            var timeSeries = Counts(countInEventKeys, countOutEventKeys);
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
        public Dictionary<T, TimeSpan> Durations(string inEvent, string outEvent, DateTime? startTime = null, DateTime? endTime = null)
        {
            if (startTime == null) startTime = InitialClockTime;
            if (endTime == null) endTime = LastChekInClockTime;
            var inIndex = GetIndex(inEvent);
            var outIndex = GetIndex(outEvent);
            var durations = new Dictionary<T, TimeSpan>();
            foreach (var item in _recordListByLoad)
            {
                var inRecord = item.Value.FirstOrDefault(r => r.StatusIndex == inIndex);
                var outRecord = item.Value.FirstOrDefault(r => r.StatusIndex == outIndex);
                if (inRecord != null && outRecord != null) durations.Add(item.Key, outRecord.ClockTime - inRecord.ClockTime);
            }
            return durations;
        }
        public TimeSpan AverageDuration(string inEvent, string outEvent, DateTime? startTime = null, DateTime? endTime = null)
        {
            var durations = Durations(inEvent, outEvent, startTime, endTime);
            if (durations.Count < 1) return TimeSpan.FromMinutes(0);
            return TimeSpan.FromMinutes(durations.Values.Average(ts => ts.TotalMinutes));
        }

        private int GetIndex(string eventKey)
        {
            if (!_eventIndices.ContainsKey(eventKey)) _eventIndices.Add(eventKey, _eventIndices.Count);
            return _eventIndices[eventKey];
        }
        public DateTime InitialClockTime { get { return _recordList.First().ClockTime; } }
        public DateTime LastChekInClockTime { get { return _recordList.Last().ClockTime; } }

    }
}
