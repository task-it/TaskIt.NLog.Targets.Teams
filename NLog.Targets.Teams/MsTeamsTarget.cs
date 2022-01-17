using NLog.Config;
using NLog.Layouts;
using NLog.Targets.Teams;
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
        public string CardImpl
        {
            get => _cardImpl;
            set
            {
                _cardImpl = value;
                if (!string.IsNullOrEmpty(_cardImpl))
                    _messageCard = null;
            }
        }
        private string _cardImpl;

        /// <summary>
        /// fully qualified Assembly Name in which <see cref="CardImpl"/> is implemented.<br/>
        /// if ommitted, the Default Implementation in this assembly will be used.
        /// </summary>        
        public string CardAssembly
        {
            get => _cardAssembly;
            set
            {
                _cardAssembly = value;
                if (!string.IsNullOrEmpty(_cardAssembly))
                    _messageCard = null;
            }
        }
        private string _cardAssembly;
        
        /// <summary>
        /// Message Card reference
        /// </summary>
        private IMessageCard _messageCard;

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
            OptimizeBufferReuse = true;
        }

        /// <inheritdoc />
        protected override void InitializeTarget()
        {
            if (string.IsNullOrEmpty(_cardImpl) && string.IsNullOrEmpty(_cardAssembly))
            {
                if (ContextProperties.Count > 0)
                    _messageCard = new TargetJsonLayoutCard(this);
                else
                    _messageCard = new DefaultCard();
            }

            base.InitializeTarget();
        }

        /// <inheritdoc />
        protected override async Task WriteAsyncTask(LogEventInfo logEvent, CancellationToken cancellationToken)
        {
            var applicationName = RenderLogEvent(ApplicationName, logEvent);
            var environment = RenderLogEvent(Environment, logEvent);
            var urlAddress = RenderLogEvent(Url, logEvent);

            string logMessage = MessageCard.CreateMessage(logEvent, applicationName, environment);
            var response = await SendMessage(urlAddress, logMessage).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Rest Call Failed - {response.ReasonPhrase}");
            }
        }

        /// <summary>
        /// posts the message to the url
        /// </summary>
        private async Task<HttpResponseMessage> SendMessage(string urlAddress, string logMessage)
        {
            var messageContent = new StringContent(logMessage);
            messageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            using (var httpClient = new HttpClient())
            {
                var targetUrl = new Uri(urlAddress);
                return await httpClient.PostAsync(targetUrl, messageContent).ConfigureAwait(false);
            }
        }

        internal string RenderTargetLayout(LogEventInfo logEvent)
        {
            return RenderLogEvent(Layout, logEvent);
        }
    }
}
