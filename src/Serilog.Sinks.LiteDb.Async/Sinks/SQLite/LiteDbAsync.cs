// Copyright 2016 Serilog Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiteDB.Async;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.Batch;
using Serilog.Sinks.Extensions;
using Serilog.Sinks.LiteDb.Async.Exceptions;
using Serilog.Sinks.Models;

namespace Serilog.Sinks.LiteDb.Async
{
    internal class LiteDbAsync : BatchProvider, ILogEventSink
    {
        private readonly string _liteDbConnectionString;
        private readonly TimeSpan? _retentionPeriod;
        private readonly Timer _retentionTimer;
        private const long BytesPerMb = 1_048_576;
        private const long MaxSupportedPages = 5_242_880;
        private const long MaxSupportedPageSize = 4096;
        private const long MaxSupportedDatabaseSize = unchecked(MaxSupportedPageSize * MaxSupportedPages) / 1048576;
        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public LiteDbAsync(
            string liteDbConnectionString,
            TimeSpan? retentionPeriod,
            TimeSpan? retentionCheckInterval,
            uint batchSize = 100,
            uint maxDatabaseSize = 10) : base((int)batchSize, 100_000)
        {
            _liteDbConnectionString = liteDbConnectionString;

            if (maxDatabaseSize > MaxSupportedDatabaseSize)
            {
                throw new LiteDbSinkException($"Database size greater than {MaxSupportedDatabaseSize} MB is not supported");
            }


            if (retentionPeriod.HasValue)
            {
                // impose a min retention period of 15 minute
                var retentionCheckMinutes = 15;
                if (retentionCheckInterval.HasValue)
                {
                    retentionCheckMinutes = Math.Max(retentionCheckMinutes, retentionCheckInterval.Value.Minutes);
                }

                // impose multiple of 15 minute interval
                retentionCheckMinutes = (retentionCheckMinutes / 15) * 15;

                _retentionPeriod = new[] { retentionPeriod, TimeSpan.FromMinutes(30) }.Max();

                // check for retention at this interval - or use retentionPeriod if not specified
                _retentionTimer = new Timer(
                    x => { ApplyRetentionPolicy().ConfigureAwait(false); },
                    null,
                    TimeSpan.FromMinutes(0),
                    TimeSpan.FromMinutes(retentionCheckMinutes));
            }
        }

        #region ILogEvent implementation

        public void Emit(LogEvent logEvent)
        {
            PushEvent(logEvent);
        }

        #endregion
        
        private LiteDatabaseAsync GetLiteDbConnection()
        {
            var sqlConString = new LiteDatabaseAsync(_liteDbConnectionString);

            return sqlConString;
        }

        private async Task ApplyRetentionPolicy()
        {
            var epoch = DateTimeOffset.Now.Subtract(_retentionPeriod.Value);
            using (var sqlConnection = GetLiteDbConnection())
            {
                var result = await sqlConnection.GetCollection<LiteDbLog>("LiteDbLog").DeleteManyAsync(x => x.Timestamp < epoch);
                SelfLog.WriteLine($"{result} records deleted");
            }
        }
        
        protected override async Task<bool> WriteLogEventAsync(ICollection<LogEvent> logEventsBatch)
        {
            if ((logEventsBatch == null) || (logEventsBatch.Count == 0))
                return true;
            await semaphoreSlim.WaitAsync().ConfigureAwait(false);
            try
            {
                try
                {
                    await WriteToDatabaseAsync(logEventsBatch).ConfigureAwait(false);
                    return true;
                }
                catch (Exception e)
                {
                    SelfLog.WriteLine(e.Message);
                    return false;
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private async Task WriteToDatabaseAsync(ICollection<LogEvent> logEventsBatch)
        {
            var listOfNewLogs = new List<LiteDbLog>();

            foreach (var logEvent in logEventsBatch)
            {
                listOfNewLogs.Add(new LiteDbLog
                {
                    Timestamp = logEvent.Timestamp.ToUniversalTime(),
                    LogDate = DateTime.Now,
                    Level = logEvent.Level,
                    Exception = logEvent.Exception?.ToString(),
                    RenderedMessage = logEvent.MessageTemplate.Text,
                    Properties = logEvent.Properties.Count > 0
                        ? logEvent.Properties.Json()
                        : string.Empty
                });
            }
            using (var sqlConnection = GetLiteDbConnection())
            {
                var result = await sqlConnection.GetCollection<LiteDbLog>("LiteDbLog").InsertBulkAsync(listOfNewLogs);
                SelfLog.WriteLine($"{result} records added");
            }
        }
    }
}
