# NLog.Targets.Teams

Simple [NLog](https://nlog-project.org/) Loging Target for [Microsoft TEAMS](https://products.office.com/en/microsoft-teams/group-chat-software?market=en).<br/>
The Target uses the TEAMS Incoming webhook. 
For more Information about webhhoks in Teams read:<br/>
https://docs.microsoft.com/en-us/microsoftteams/platform/webhooks-and-connectors/what-are-webhooks-and-connectors<br/> and <br/>
https://docs.microsoft.com/en-us/microsoftteams/platform/webhooks-and-connectors/how-to/add-incoming-webhook

The target has a built in layout for the Teams card so you don't have to design it by yourself.

## Installation
Simply add the nuget dependency. NLog will do th rest (see: [Configuration - NLog](#NLog)).


## Configuration

### NLog

See: https://github.com/NLog/NLog/wiki/Register-your-custom-component#separate-dll-and-auto-register-non-net-core

### MS Teams

Create a incoming Webhook in a Teams channel.<br/>
See: https://docs.microsoft.com/en-us/microsoftteams/platform/webhooks-and-connectors/how-to/add-incoming-webhook

### Your Application

Configure the target in your `nlog.config`.
The NLog type of the Target is:<br/>
`MsTeams`


### Parameters

Parameter | Required | Type | Description |
--------- | -------- | ---- | ----------- |
Url | true | string | Ms Teams incoming Webhook URL |
ApplicationName | true | string | the name of your application |
Environment | true | string | the stage your application runs in (e.g. develop, staging, production ) |
UseLayout | false | bool | Default = false <br/>Flag indicating whether the NLog layout property or the default Teams Card implementation will be used | 

### Sample Configuration
```
<target xsi:type="MsTeams" 
            name="whatever" 
            layout=""  
            Url="<your TEAMS incoming webhook url>"          
            ApplicationName="<your application name>"
            Environment="<Executing Environment>" />
```

### Default Teams Message Card

The screenshot shows the built in Teams message card.
![Built In Card](Screenshots/DefaultCard.png)
The color schema of the upper separator line will change according to the log level.
The used colors look like this:<br/>
<span style="background-color:black; color:#ffffff">TRACE</span>
<span style="color:#00ff00">DEBUG</span>
<span style="color:#0094FF">INFO</span>
<span style="color:#FFE97F">WARNING</span>
<span style="color:#ff0000">ERROR</span>
<span style="color:#000000">FATAL</span>

The exception section will only be visible, when an exception is logged.

### Custom Teams Message Card
To use your own message card, simply set the parameter `UseLayout` in your `nlog.config` to `true` and specify a NLog `layout`.<br/>
The target will render the message according to the layout pattern and send it to the URL.<br/>
This _should_ work, but I havn't tested this. All errors will be logged in the NLog internal log.<br/>
For more information about the Teams message card formatting please read https://docs.microsoft.com/en-us/outlook/actionable-messages/message-card-reference .


