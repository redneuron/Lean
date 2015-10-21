using System;
using System.Collections;
using System.Reflection;
using QuantConnect.Algorithm;
using QuantConnect.Data.Market;
using QuantConnect.Interfaces;
using QuantConnect.Orders;
using QuantConnect.Util;

namespace QuantConnect
{
    public class LimitOrderTestFramework : QCAlgorithm
    {

        OrderTicket _limitOrder;
        string _symbol = "EURUSD";
        decimal _price = 0;
        int _rebalancePeriod = 5;
        DateTime _submitted;
        DateTime _updatedTime;

        bool first = true;
        DateTime lastPlot;
        ICollection m_queue;
        Resolution resolution = Resolution.Tick;
        DateTime eurusdEndTime = DateTime.MinValue;

        public override void Initialize()
        {
            SetCash(25000);
            SetStartDate(2013, 1, 1);
            SetEndDate(DateTime.Now.Date.AddDays(-1));
            AddSecurity(SecurityType.Forex, _symbol, Resolution.Second);
        }

        public void OnData(TradeBars data)
        {
            if (first)
            {
                first = false;
                var _dataQueue = Composer.Instance.GetExportedValueByTypeName<IDataQueueHandler>("DataQueueCloud");
                var _queue = _dataQueue.GetType().GetField("_queue", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_dataQueue);
                var _consumer = _queue.GetType().GetField("_consumer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_queue);
                var Queue = _consumer.GetType().GetProperty("Queue", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_consumer);
                m_queue = (ICollection)Queue.GetType().GetField("m_queue", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Queue);
            }
            //Plot("QueueCount", "Count", m_queue.Count);

            //Plot("Memory", "RAM", (decimal)OS.TotalPhysicalMemoryUsed);
            Console.WriteLine(((decimal)OS.TotalPhysicalMemoryUsed).SmartRounding() +" MB Q Count: " + m_queue.Count);
            //var cpu = OS.CpuUsage.NextValue();
            //if (cpu > 0 && cpu <= 100)
            //{
            //    Plot("CPU", "CPU%", cpu);
            //}

            //TradeBar eurusd;
            //if (data.TryGetValue("EURUSD", out eurusd))
            //{
            //    Plot("EURUSD", "Price", eurusd.Close);
            //}

            _price = data[_symbol].Close;

            //Create the first order
            //if (_limitOrder == null)
            //{
            //    //Set our first order to less than SPY's price:
            //    var quantity = (int)(Portfolio.Cash / _price);
            //    _limitOrder = LimitOrder(_symbol, quantity, (_price * 0.95m));
            //    Debug("Created limit order with " + _symbol + " Price: " + _price.ToString("C") + " id: " + _limitOrder.OrderId);
            //    _submitted = Time;
            //}

            ////Update the limit price once per week:
            //if (_updatedTime < Time && !Portfolio.Invested)
            //{
            //    _updatedTime = Time.AddSeconds(5);

            //    var newLimitPrice = _price * 0.99m;
            //    var currentLimitPrice = _limitOrder.Get(OrderField.LimitPrice);

            //    Debug(Time.ToString("o") + " Scaning order for update: New limit: " + newLimitPrice);

            //    if (newLimitPrice > currentLimitPrice)
            //    {
            //        _limitOrder.Update(new UpdateOrderFields { LimitPrice = newLimitPrice });
            //        Debug(Time.ToString("o") + " Order Limit price updated: " + newLimitPrice);
            //    }

            //    if (Time > _submitted.AddSeconds(30))
            //    {
            //        _limitOrder.Update(new UpdateOrderFields { LimitPrice = _price });
            //        Debug(Time.ToString("o") + " Order Set to final price -- Price: " + _price);
            //    }
            //}
        }
    }
}