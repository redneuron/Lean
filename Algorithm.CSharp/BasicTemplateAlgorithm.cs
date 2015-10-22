using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using QuantConnect.Algorithm;
using QuantConnect.Data.Market;
using QuantConnect.Interfaces;
using QuantConnect.Securities;
using QuantConnect.Securities.Forex;
using QuantConnect.Util;

namespace QuantConnect
{
    /*
    *   QuantConnect University: Full Basic Template:
    *
    *   The underlying QCAlgorithm class is full of helper methods which enable you to use QuantConnect.
    *   We have explained some of these here, but the full algorithm can be found at:
    *   https://github.com/QuantConnect/QCAlgorithm/blob/master/QuantConnect.Algorithm/QCAlgorithm.cs
    */
    public class BasicTemplateAlgorithm : QCAlgorithm
    {
        bool first = true;
        System.Timers.Timer _timer;
        ICollection m_queue;

        Resolution resolution = Resolution.Daily;
        DateTime eurusdEndTime = DateTime.MinValue;

        //Initialize the data and resolution you require for your strategy:
        public override void Initialize()
        {
            //Start and End Date range for the backtest:
            SetStartDate(2013, 1, 1);
            SetEndDate(2013, 1, 5);

            //Cash allocation
            SetCash(25000);

            // added to force ondata to trigger
            AddSecurity(SecurityType.Equity, "SPY", Resolution.Second);

            foreach (var pair in Forex.CurrencyPairs)
            {
                AddSecurity(SecurityType.Forex, pair, resolution);
                Debug(string.Format("{0}: {1}", pair, Securities[pair].Exchange.TimeZone));
            }

            if (first)
            {
                _timer = new System.Timers.Timer(1000);
                _timer.Enabled = true;
                _timer.AutoReset = true;
                _timer.Elapsed += (sender, args) =>
                {
                    try
                    {
                        if (m_queue == null)
                        {
                            var _dataQueue = Composer.Instance.GetExportedValueByTypeName<IDataQueueHandler>("DataQueueCloud");
                            var _queue = _dataQueue.GetType().GetField("_queue", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_dataQueue);
                            var _consumer = _queue.GetType().GetField("_consumer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_queue);
                            var Queue = _consumer.GetType().GetProperty("Queue", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_consumer);
                            m_queue = (ICollection)Queue.GetType().GetField("m_queue", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Queue);
                        }
                        Plot("QueueCount", "Count", m_queue.Count);

                        Plot("Memory", "RAM", (double)OS.TotalPhysicalMemoryUsed);
                        var cpu = OS.CpuUsage.NextValue();
                        if (cpu > 0 && cpu <= 100)
                        {
                            Plot("CPU", "CPU%", cpu);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("_timer.Elapsed threw an error: " + ex);
                    }
                };
                first = false;
            }
        }

        //Data Event Handler: New data arrives here. "TradeBars" type is a dictionary of strings so you can access it by symbol.
        public void OnData(TradeBars data)
        {
            Security eurusd;
            if (Securities.TryGetValue("EURUSD", out eurusd))
            {
                Plot("EURUSD", "Price", eurusd.Close);
                if (eurusd.Cache.GetData() is TradeBar)
                    Plot("EURUSD_V", "Volume", ((TradeBar)eurusd.Cache.GetData()).Volume);
            }

            var delta = (DateTime.UtcNow - UtcTime).TotalMilliseconds.ToString("00.0000");
            
            Console.WriteLine("{0}ms AlgoTime: {1} data.EndTime: {2}", delta, Time, data.Max(x => x.Value.EndTime));
        }

        //Data Event Handler: New data arrives here. "TradeBars" type is a dictionary of strings so you can access it by symbol.
        public void OnData(Ticks data)
        {
            Plot("Memory", "RAM", (double)OS.TotalPhysicalMemoryUsed);
            Plot("CPU", "CPU%", OS.CpuUsage.NextValue());

            List<Tick> eurusd;
            if (data.TryGetValue("EURUSD", out eurusd) && eurusd.Count > 0)
            {
                Plot("EURUSD", "Price", eurusd.Last().Value);
                eurusdEndTime = eurusd.Last().EndTime;
            }

            var delta = (DateTime.UtcNow - UtcTime).TotalMilliseconds.ToString("00.0000");
            Console.WriteLine("{0}ms AlgoTime: {1} TickCount: {2} date.EndTime: {3}", delta, Time, string.Join(",", data.Select(x => x.Value.Count)), data.Max(x => x.Value.Max(y => y.EndTime)));
        }
    }
}