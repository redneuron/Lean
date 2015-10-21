/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Basic template algorithm simply initializes the date range and cash
    /// </summary>
    public class BasicTemplateAlgorithm : QCAlgorithm
    {
        //Initialize the data and resolution you require for your strategy:
        public override void Initialize()
        {
            UniverseSettings.Resolution = Resolution.Minute;

            //Start and End Date range for the backtest:
            SetStartDate(2013, 1, 1);
            SetEndDate(DateTime.Now.Date.AddDays(-1));

            //Cash allocation
            SetCash(25000);

            //Add as many securities as you like. All the data will be passed into the event handler:
            AddSecurity(SecurityType.Forex, "EURUSD", Resolution.Minute);

            AddData<RemoteFileBaseData>("SPY", Resolution.Minute);
        }

        //Data Event Handler: New data arrives here. "TradeBars" type is a dictionary of strings so you can access it by symbol.
        public void OnData(Slice data)
        {
            Debug(Time.ToString("o") + ": RemoteFileCallCount: " + RemoteFileBaseData.RemoteFileCallCount);
            Debug(string.Join(",", data.Keys));

            if (!Portfolio.HoldStock)
            {
                //Order function places trades: enter the string symbol and the quantity you want:
                SetHoldings("EURUSD", 1);
                Log("This is a log message");
            }
        }
    }

    /// <summary>
    /// Custom data type that uses a remote file download
    /// </summary>
    public class RemoteFileBaseData : BaseData
    {
        public static int RemoteFileCallCount;

        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, bool isLiveMode)
        {
            var csv = line.Split(',');
            if (csv[1].ToLower() != config.Symbol.ToLower())
            {
                // this row isn't for me
                return null;
            }

            var time = QuantConnect.Time.UnixTimeStampToDateTime(double.Parse(csv[0])).ConvertFromUtc(config.TimeZone).Subtract(config.Increment);
            return new RemoteFileBaseData
            {
                Symbol = config.Symbol,
                Time = time,
                EndTime = time.Add(config.Increment),
                Value = decimal.Parse(csv[3])

            };
        }

        public override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, bool isLiveMode)
        {
            RemoteFileCallCount++;

            // this file is only a few seconds worth of data, so it's quick to download
            var remoteFileSource = @"http://www.quantconnect.com/live-test?type=file&symbols=" + config.Symbol;
            remoteFileSource = @"http://beta.quantconnect.com/live-test?type=file&symbols=" + config.Symbol;
            return new SubscriptionDataSource(remoteFileSource, SubscriptionTransportMedium.RemoteFile, FileFormat.Csv);
        }
    }
}