namespace NLog.Targets.Teams
{
    /// <summary>
    /// Public Interface for all Message Cards.<br/>
    /// Custom Layouts must Implement this.
    /// </summary>
    public interface IMessageCard
    {
        /// <summary>
        /// Creates the Card als String (in JSON Format).<br/>
        /// For more Info check out: https://docs.microsoft.com/en-us/outlook/actionable-messages/message-card-reference
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="applicationName"></param>
        /// <param name="environment"></param>
        /// <returns>string</returns>
        string CreateMessage(LogEventInfo logEvent, string applicationName, string environment);
    }
}
