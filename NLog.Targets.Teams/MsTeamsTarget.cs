using NLog.Config;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NLog.Targets.MsTeams
{
    /// <summary>
    /// NLog Looging Target for MS Teams Incoming Webhook
    /// </summary>    
    [Target("MsTeams")]
    public sealed class MsTeamsTarget : AsyncTaskTarget
    {
        /// <summary>
        /// Ms Teams Incoming Webhook URL as string
        /// </summary>
        [RequiredParameter]
        public string Url { get; set; }

        /// <summary>
        /// flag if the default card should be used or if the card schold be created thru the specifies Layout.<br/>
        /// <see cref="TargetWithContext.Layout"/>
        /// true - NLog Layout will be user<br/>
        /// false - default card will be user
        /// </summary>
        [RequiredParameter]
        public bool UseLayout { get; set; }

        /// <summary>
        /// Name of the Accplication<br/>
        /// Will be displayed as Title in the default card layout
        /// </summary>
        [RequiredParameter]
        public string ApplicationName { get; set; }

        /// <summary>
        /// Environment / Stage your Application runs in (eg. develop, stage, production)<br/>
        /// NOT a System Environment variable
        /// </summary>
        [RequiredParameter]
        public string Environment { get; set; }

        /// <summary>
        /// Construction
        /// </summary>        
        public MsTeamsTarget()
        {
            IncludeEventProperties = true; // Include LogEvent Properties by default            
        }

        /// <summary>
        /// <see cref="AsyncTaskTarget.WriteAsyncTask(LogEventInfo, CancellationToken)"/>
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task WriteAsyncTask(LogEventInfo logEvent, CancellationToken cancellationToken)
        {
            await CreateAndSendMessage(logEvent);
        }

        /// <summary>
        /// <see cref="AsyncTaskTarget.Write(Common.AsyncLogEventInfo)"/>
        /// </summary>
        /// <param name="logEvent"></param>
        protected override void Write(LogEventInfo logEvent)
        {
            CreateAndSendMessage(logEvent).GetAwaiter().GetResult();

        }


        /// <summary>
        /// internal implementation for async / sync writing
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns></returns>
        private async Task CreateAndSendMessage(LogEventInfo logEvent)
        {
            string logMessage = CreateMessage(logEvent);
            var response = await SendMessage(logMessage);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Rest Call Failed - {response.ReasonPhrase}");
            }
        }

        /// <summary>
        /// posts the message to the url
        /// </summary>
        /// <param name="logMessage"></param>
        private async Task<HttpResponseMessage> SendMessage(string logMessage)
        {
            var messageContent = new StringContent(logMessage);
            messageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            using (var httpClient = new HttpClient())
            {
                var targetUrl = new Uri(Url);
                return await httpClient.PostAsync(targetUrl, messageContent);
            }
        }

        /// <summary>
        /// Creates the Message string
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns></returns>
        private string CreateMessage(LogEventInfo logEvent)
        {
            if (UseLayout)
            {
                return RenderLogEvent(Layout, logEvent);
            }

            return new DefaultCard().CreateMessage(logEvent, ApplicationName, Environment);
        }


    }
}
