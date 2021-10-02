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

using Serilog.Debugging;
using Serilog.Sinks.LiteDb.Async;

namespace Serilog
{
    using System;
    using Configuration;
    using Core;
    using Events;

    /// <summary>
    ///     Adds the WriteTo.LiteDb() extension method to <see cref="LoggerConfiguration" />.
    /// </summary>
    public static class LoggerConfigurationLiteDbExtensions
    {
        /// <summary>
        ///     Adds a sink that writes log events to a LiteDb database.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="liteDbConnectionString">LiteDb connection string.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="retentionPeriod">The maximum time that a log entry will be kept in the database, or null to disable automatic deletion of old log entries. Non-null values smaller than 30 minute will be replaced with 30 minute.</param>
        /// <param name="retentionCheckInterval">Time period to execute TTL process. Time span should be in 15 minutes increment</param>
        /// <param name="batchSize">Number of messages to save as batch to database. Default is 10, max 1000</param>
        /// <param name="levelSwitch">
        /// A switch allowing the pass-through minimum level to be changed at runtime.
        /// </param>
        /// <param name="maxDatabaseSize">Maximum database file size can grow in MB. Default 10 MB, maximum 20 GB</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException">A required parameter is null.</exception>
        public static LoggerConfiguration LiteDbAsync(
            this LoggerSinkConfiguration loggerConfiguration,
            string liteDbConnectionString,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            TimeSpan? retentionPeriod = null,
            TimeSpan? retentionCheckInterval = null,
            LoggingLevelSwitch levelSwitch = null,
            uint batchSize = 100,
            uint maxDatabaseSize = 10)
        {
            if (loggerConfiguration == null) {
                SelfLog.WriteLine("Logger configuration is null");

                throw new ArgumentNullException(nameof(loggerConfiguration));
            }

            if (string.IsNullOrEmpty(liteDbConnectionString)) {
                SelfLog.WriteLine("Invalid liteDbConnectionString");

                throw new ArgumentNullException(nameof(liteDbConnectionString));
            }


            try {
                return loggerConfiguration.Sink(
                    new LiteDbAsync(
                        liteDbConnectionString,
                        retentionPeriod,
                        retentionCheckInterval,
                        batchSize,
                        maxDatabaseSize),
                    restrictedToMinimumLevel,
                    levelSwitch);
            }
            catch (Exception ex) {
                SelfLog.WriteLine(ex.Message);

                throw;
            }
        }
    }
}
