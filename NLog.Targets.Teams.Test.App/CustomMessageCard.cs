using System.Collections.Generic;

namespace NLog.Targets.Teams.Test.App
{
    /// <summary>
    /// Just for demostration purpose<br/>
    /// The messages look like the default Card Implementation, but all levels use black as color
    /// </summary>
    class CustomMessageCard : IMessageCard
    {
        /// <summary>
        /// Card part for the Exception visualization
        /// </summary>
        public const string JSON_EXCEPTION = @", { ""activityTitle""	: ""${exception}"", ""activitySubtitle"": ""${exceptionMessage}"", ""activityText"": ""${stacktrace}""}";
        /// <summary>
        /// JSON String for the main Part of the Card
        /// </summary>
        public const string JSON_MAIN_CARD = @"""@type"": ""MessageCard"", ""@context"": ""http://schema.org/extensions"", ""themeColor"": ""${color}"", ""title"": ""${application}"",  ""text"": ""${environment}"", ""sections"": [{""facts"": [{""name"": ""${level}"",""value"": ""${message}"" }]}";

        /// <summary>
        /// JSON String for a card with Exception
        /// </summary>        
        public const string JSON_CARD_WITH_EXCEPTION = "{" + JSON_MAIN_CARD + JSON_EXCEPTION + "]}";

        /// <summary>
        /// JSON String for a Card without exception
        /// </summary>
        public const string JSON_CARD_NO_EXCEPTION = "{" + JSON_MAIN_CARD + "]}";

        /// <summary>
        /// Colors for the different Log Levels
        /// </summary>
        public static readonly Dictionary<LogLevel, string> ColorMap = new Dictionary<LogLevel, string>()
        {
            { LogLevel.Trace, "000000" },
            { LogLevel.Debug, "000000" },
            { LogLevel.Info, "000000" },
            { LogLevel.Warn, "000000" },
            { LogLevel.Error, "000000" },
            { LogLevel.Fatal, "000000" },
            { LogLevel.Off, "ffffff" }
        };

        /// <summary>
        /// Creates the Message
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="applicationName"></param>
        /// <param name="environment"></param>
        /// <returns>string</returns>
        public string CreateMessage(LogEventInfo logEvent, string applicationName, string environment)
        {
            // get the resulting card json
            string result = JSON_CARD_WITH_EXCEPTION;
            if (logEvent.Exception == null)
            {
                result = JSON_CARD_NO_EXCEPTION;
            }

            // format / replace contents in Card
            //color
            ColorMap.TryGetValue(logEvent.Level, out string color);
            result = result.Replace("${color}", color ?? "ffffff");
            // level
            result = result.Replace("${level}", logEvent.Level.ToString().ToUpper());
            // message
            result = result.Replace("${message}", logEvent.FormattedMessage);
            // application
            result = result.Replace("${application}", applicationName);
            // application
            result = result.Replace("${environment}", environment);
            // stacktrace
            if (logEvent.Exception != null)
            {
                result = result.Replace("${exception}", logEvent.Exception.GetType().Name);
                result = result.Replace("${exceptionMessage}", logEvent.Exception.Message);
                result = result.Replace("${stacktrace}", logEvent.Exception.StackTrace?.ToString());
            }

            // globale bereinigung
            result = result.Replace("\\", "/");

            return result;
        }
    }
}
