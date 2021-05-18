using NLog.Config;
using NLog.Layouts;
using NLog.Targets.Teams;
using System;
using System.Net;
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
        public Layout Url { get; set; }

        /// <summary>
        /// Name of the Accplication<br/>
        /// Will be displayed as Title in the default card layout
        /// </summary>
        [RequiredParameter]
        public Layout ApplicationName { get; set; }

        /// <summary>
        /// Environment / Stage your Application runs in (eg. develop, stage, production)<br/>
        /// NOT a System Environment variable
        /// </summary>
        [RequiredParameter]
        public Layout Environment { get; set; }

        /// <summary>
        /// fully qualified Name of the (custom) <see cref="IMessageCard"/> Implementation Class.<br/>
        /// if ommitted, the default Implementation will be used.
        /// </summary>        
        public string CardImpl { get; set; } = typeof(DefaultCard).FullName;

        /// <summary>
        /// fully qualified Assembly Name in which <see cref="CardImpl"/> is implemented.<br/>
        /// if ommitted, the Default Implementation in this assembly will be used.
        /// </summary>        
        public string CardAssembly { get; set; } = typeof(DefaultCard).Assembly.GetName().Name;

        /// <summary>
        /// Proxy to be used<br/>
        /// Will be displayed as Title in the default card layout
        /// </summary>
        public Layout ProxyUrl { get; set; }

        /// <summary>
        /// Proxy user<br/>
        /// Used only if ProxyUrl is provided
        /// </summary>
        public Layout ProxyUser { get; set; }

        /// <summary>
        /// Proxy pass<br/>
        /// Used only if ProxyUrl is provided
        /// </summary>
        public Layout ProxyPassword { get; set; }

        /// <summary>
        /// Proxy bypass on local ip<br/>
        /// Used only if ProxyUrl is provided
        /// </summary>
        public Layout ProxyBypassOnLocal { get; set; }

        /// <summary>
        /// Message Card reference
        /// </summary>
        private IMessageCard _messageCard = null;

        /// <summary>
        /// Message Card Implementation Reference
        /// </summary>
        private IMessageCard MessageCard
        {
            // lazy initialization for the Card Implementation
            get
            {
                if (_messageCard == null)
                {
                    _messageCard = CreateMessageCard();
                }
                return _messageCard;
            }
        }

        /// <summary>
        /// Creates the Card Implementation from the Parameters <see cref="CardImpl"/> and <see cref="CardAssembly"/>
        /// </summary>
        /// <returns></returns>
        private IMessageCard CreateMessageCard()
        {
            string cardToInstantiate = $"{CardImpl}, {CardAssembly}";
            var cardType = Type.GetType(cardToInstantiate);

            return Activator.CreateInstance(cardType) as IMessageCard;
        }

        /// <summary>
        /// Construction
        /// </summary>        
        public MsTeamsTarget()
        {
            // do nothing
        }

        /// <summary>
        /// <see cref="AsyncTaskTarget.WriteAsyncTask(LogEventInfo, CancellationToken)"/>
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task WriteAsyncTask(LogEventInfo logEvent, CancellationToken cancellationToken)
        {
            var applicationName = RenderLogEvent(ApplicationName, logEvent);
            var environment = RenderLogEvent(Environment, logEvent);
            var urlAddress = RenderLogEvent(Url, logEvent);
            var proxyUrl = RenderLogEvent(ProxyUrl, logEvent);
            var proxyUser = RenderLogEvent(ProxyUser, logEvent);
            var proxyPassword = RenderLogEvent(ProxyPassword, logEvent);
            var proxyBypassOnLocal = RenderLogEvent(ProxyBypassOnLocal, logEvent).Equals("true");

            string logMessage = MessageCard.CreateMessage(logEvent, applicationName, environment);
            var response = await SendMessage(urlAddress, proxyUrl, proxyUser, proxyPassword, proxyBypassOnLocal, logMessage).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Rest Call Failed - {response.ReasonPhrase}");
            }
        }

        /// <summary>
        /// posts the message to the url
        /// </summary>
        private async Task<HttpResponseMessage> SendMessage(string urlAddress, string proxyUrl, string proxyUser, string ProxyPass, bool proxyBypassOnLocal, string logMessage)
        {
            var messageContent = new StringContent(logMessage);
            messageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var proxiedHttpClientHandler = new HttpClientHandler();

            if (!string.IsNullOrEmpty(proxyUrl))
            {
                proxiedHttpClientHandler = new HttpClientHandler() { UseProxy = true };
                proxiedHttpClientHandler.Proxy = new WebProxy(proxyUrl);
            }

            using (var httpClient = new HttpClient(proxiedHttpClientHandler))
            {
                var targetUrl = new Uri(urlAddress);
                return await httpClient.PostAsync(targetUrl, messageContent).ConfigureAwait(false);
            }
        }
    }
}
