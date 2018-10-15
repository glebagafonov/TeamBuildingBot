using System;
using System.Globalization;
using Bot.Infrastructure.Exceptions;
using Bot.Infrastructure.Services.Interfaces;
using BotService.Model.Dialog;
using BotService.Requests;
using MediatR;

namespace BotService.Model.Dialogs
{
    public class ScheduleGameRequestDialog : Dialog<ScheduleGameRequest>
    {
        private readonly IMediator _mediator;

        public ScheduleGameRequestDialog(ILogger logger, IMediator mediator) : base(logger)
        {
            _mediator = mediator;
            Add("Введи дату игры в формате \"dd.MM.yyyy HH:mm\"");
            Add((message, scheduleGameRequest) =>
            {
                if (!DateTime.TryParseExact(message, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dateTime))
                {
                    throw new InvalidInputException("Не могу понять. Введи время по маске \"dd.MM.YYYY HH:mm\"");
                }
                scheduleGameRequest.DateTime = dateTime;
                return scheduleGameRequest;
            });
            Add("Игра запланирована");
        }
        
        protected override string CommandName => "Планирование игры";
        public override void ProcessDialogEnded()
        {
            _mediator.Send(DialogData);

        }
    }
}