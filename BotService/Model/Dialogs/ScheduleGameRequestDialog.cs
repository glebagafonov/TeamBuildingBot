using System;
using System.Collections.Generic;
using System.Globalization;
using Bot.Infrastructure.Exceptions;
using Bot.Infrastructure.Helpers;
using Bot.Infrastructure.Services.Interfaces;
using BotService.Mediator.Requests;
using BotService.Model.Dialog;
using BotService.Services.Interfaces;
using MediatR;

namespace BotService.Model.Dialogs
{
    public class ScheduleGameRequestDialog : Dialog<ScheduleGameRequest>
    {
        private readonly IMediator _mediator;
        private readonly IServiceConfiguration _configuration;

        public ScheduleGameRequestDialog(IEnumerable<ICommunicator> communicators, Guid userId, ScheduleGameRequest dialogData,
            ILogger logger, IMediator mediator, IServiceConfiguration configuration) : base(logger, communicators,
            userId, dialogData)
        {
            _mediator = mediator;
            _configuration = configuration;

            WarningDialog();
            GetDateDialog();
        }

        private void GetDateDialog()
        {
            Add((message, scheduleGameRequest) =>
            {
                if (!DateTime.TryParseExact(message, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal, out var dateTime))
                {
                    throw new InvalidInputException("Не могу понять. Введи время по маске \"dd.MM.YYYY HH:mm\"");
                }

                var startScheduledEventsDateTime = DateTime.Now.Add(_configuration.GameScheduleThreshold, _configuration.StartDayTime, _configuration.EndDayTime); 
                
                if (dateTime < startScheduledEventsDateTime)
                {
                    WarningDialog();
                    GetDateDialog();
                }
                else
                {
                    Add("Игра запланирована");
                }

                scheduleGameRequest.DateTime = dateTime;
                return scheduleGameRequest;
            });
        }

        private void WarningDialog()
        {
            Add($"Игра планируется за 24 рабочих часа до игры.");
            var dateTime = DateTime.Now.Add(_configuration.GameScheduleThreshold, _configuration.StartDayTime,
                _configuration.EndDayTime);
            Add($"Дата должна быть не раньше {dateTime.ToString(DateTimeHelper.DateFormat)}. Или собирай вручную :)");
            Add("Введи дату игры в формате \"dd.MM.yyyy HH:mm\" или используй команду - /cancel");
        }

        protected override string CommandName => "Планирование игры";
        public override void ProcessDialogEnded()
        {
            _mediator.Send(DialogData);

        }
    }
}