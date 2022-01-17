using System.Collections.Generic;
using NLog.Layouts;

namespace NLog.Targets.Teams
{
    internal class TargetJsonLayoutCard : IMessageCard
    {
        private readonly MsTeams.MsTeamsTarget _target;

        public TargetJsonLayoutCard(MsTeams.MsTeamsTarget target)
        {
            _target = target;

            var messageCardLayout = new JsonLayout() { EscapeForwardSlash = false };
            messageCardLayout.Attributes.Add(new JsonAttribute("@type", "MessageCard"));
            messageCardLayout.Attributes.Add(new JsonAttribute("context", "http://schema.org/extensions"));
            messageCardLayout.Attributes.Add(new JsonAttribute("themeColor", "${when:when=level==LogLevel.Trace:inner=ffffff}"));
            messageCardLayout.Attributes.Add(new JsonAttribute("themeColor", "${when:when=level==LogLevel.Debug:inner=00ff00}"));
            messageCardLayout.Attributes.Add(new JsonAttribute("themeColor", "${when:when=level==LogLevel.Info:inner=0094FF}"));
            messageCardLayout.Attributes.Add(new JsonAttribute("themeColor", "${when:when=level==LogLevel.Warn:inner=FFE97F}"));
            messageCardLayout.Attributes.Add(new JsonAttribute("themeColor", "${when:when=level==LogLevel.Error:inner=ff0000}"));
            messageCardLayout.Attributes.Add(new JsonAttribute("themeColor", "${when:when=level==LogLevel.Fatal:inner=000000}"));
            messageCardLayout.Attributes.Add(new JsonAttribute("themeColor", "${when:when=level==LogLevel.Off:inner=ffffff}"));
            messageCardLayout.Attributes.Add(new JsonAttribute("title", target.ApplicationName));
            messageCardLayout.Attributes.Add(new JsonAttribute("text", target.Environment));

            // Using CompoundLayout for building a JSON-Array
            var facts = new CompoundLayout();
            if (target.ContextProperties.Count > 0)
            {
                facts.Layouts.Add(new SimpleLayout("[ "));
                for (int i = 0; i < target.ContextProperties.Count; ++i)
                {
                    if (i != 0)
                        facts.Layouts.Add(new SimpleLayout(", "));

                    var fact = target.ContextProperties[i];
                    facts.Layouts.Add(new JsonLayout()
                    {
                        Attributes = {
                            new JsonAttribute("name", fact.Name) { EscapeForwardSlash = false },
                            new JsonAttribute("value", fact.Layout) { EscapeForwardSlash = false, Encode = !(fact.Layout is JsonLayout) },
                        }
                    });
                }
                facts.Layouts.Add(new SimpleLayout(" ]"));
            }

            var sections = new JsonLayout()
            {
                RenderEmptyObject = false,
                Attributes = {
                    new JsonAttribute("activityTitle", "${exception:format=type}"),
                    new JsonAttribute("activitySubtitle", "${exception:format=message}"),
                    new JsonAttribute("activityText", "${exception:format=tostring}"),
                    new JsonAttribute("facts", facts) { Encode = false },
                }
            };
            messageCardLayout.Attributes.Add(new JsonAttribute("sections", sections) { Encode = false });

            _target.Layout = messageCardLayout;
        }

        public string CreateMessage(LogEventInfo logEvent, string applicationName, string environment)
        {
            return _target.RenderTargetLayout(logEvent);
        }
    }
}
