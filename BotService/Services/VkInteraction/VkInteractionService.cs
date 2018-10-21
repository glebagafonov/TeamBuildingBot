using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BotService.Mediator.Requests;
using BotService.Services.Interfaces;
using MediatR;
using Newtonsoft.Json.Linq;
using VkNet;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Model.RequestParams;
using ILogger = Bot.Infrastructure.Services.Interfaces.ILogger;

namespace BotService.Services.VkInteraction
{
    public class VkInteractionService
    {
        private static string LongPollFailedKey  = "failed";
        private static string LongPollUpdatesKey = "updates";

        private static   int                     LongPollMessageValue               = 4;
        private static   int                     LongPollOutcomingMessageValue      = 3;
        private static   int                     LongPollIncomingMessageValue       = 17;
        private static   int                     LongPollMobileIncomingMessageValue = 1;
        private readonly ServiceConfiguration    _serviceConfiguration;
        private readonly ILogger                 _logger;
        private readonly IUserInteractionService _userInteractionService;
        private readonly IMediator               _mediator;
        private readonly VkApi                   _vkApi;
        private          CancellationTokenSource _cts;
        private          object                  _login;
        private          HttpClient              _longPolling;

        public VkInteractionService(ServiceConfiguration serviceConfiguration, ILogger logger,
            IUserInteractionService userInteractionService, IMediator mediator)
        {
            _serviceConfiguration   = serviceConfiguration;
            _logger                 = logger;
            _userInteractionService = userInteractionService;
            _mediator               = mediator;

            _login       = new object();
            _vkApi       = new VkApi();
            _longPolling = new HttpClient();
            InitLongPoll();
        }

        private async void InitLongPoll()
        {
            lock (_login)
                if (_cts != null)
                    return;
            _cts = new CancellationTokenSource();

            LongPollServerResponse longPollingServer = null;
            try
            {
                longPollingServer = _vkApi.Messages.GetLongPollServer(true);
            }
            catch (AccessTokenInvalidException ex)
            {
                try
                {
                    RegistrationExplicity();
                    longPollingServer = _vkApi.Messages.GetLongPollServer(true);
                }
                catch (Exception)
                {
                    return;
                }
            }

            CancellationToken ct = _cts.Token;
            string            ts;

            while (true)
            {
                try
                {
                    var answer = await _longPolling
                        .GetStringAsync(
                            $"https://{longPollingServer.Server}?act=a_check&key={longPollingServer.Key}&ts={longPollingServer.Ts}&wait=90")
                        .ConfigureAwait(false);
                    if (ct.IsCancellationRequested) return;

                    JObject jAnswer = JObject.Parse(answer);
                    try
                    {
                        ThrowIfFailed(jAnswer);
                    }
                    catch (VkLongPollingException ex)
                    {
                        if (ex.Error == 1)
                        {
                            ts                   = jAnswer.SelectToken("$.ts").ToString();
                            longPollingServer.Ts = ts;
                        }
                        else if (ex.Error == 2 || ex.Error == 3)
                            longPollingServer = _vkApi.Messages.GetLongPollServer(true);

                        else if (ex.Error == 4)
                        {
                            return;
                        }

                        continue;
                    }

                    ts                   = jAnswer.SelectToken("$.ts").ToString();
                    longPollingServer.Ts = ts;

                    MessageHandler(jAnswer);
                }
                catch
                {
                    // ignored
                }
            }
        }

        private void RegistrationExplicity()
        {
            _vkApi.Authorize(new ApiAuthParams
            {
                AccessToken = _serviceConfiguration.VkToken
            });
        }

        private void ThrowIfFailed(JObject jAnswer)
        {
            if (jAnswer.ContainsKey(LongPollFailedKey))
                throw new VkLongPollingException((int) jAnswer[LongPollFailedKey]);
        }

        private async void MessageHandler(JObject jAnswer)
        {
            if (jAnswer.ContainsKey(LongPollUpdatesKey))
            {
                var updates = jAnswer[LongPollUpdatesKey];
                foreach (var updateArray in updates)
                {
                    if (!(updateArray is JArray update))
                        continue;

                    if (int.TryParse(update[0].Value<string>(), out var actionType) &&
                        actionType == LongPollMessageValue                          &&
                        int.TryParse(update[2].Value<string>(), out var direction)  &&
                        (direction == LongPollIncomingMessageValue || direction == LongPollMobileIncomingMessageValue) &&
                        long.TryParse(update[3].Value<string>(), out var vkId))
                    {
                        _logger.Info($"[{vkId}]: Vk: Got message");
                        var communicator = GetCommunicator(vkId);
                        var user         = await _mediator.Send(new AuthorizeRequest(communicator));
                        _userInteractionService.ProcessMessage(user, communicator, update[6].Value<string>());
                    }
                }
            }
        }

        private VkCommunicator GetCommunicator(long vkId)
        {
            return new VkCommunicator(vkId, this);
        }

        public void SendMessage(long vkId, string text)
        {
            _logger.Trace($"[{vkId}]: Vk: Send message");
            _vkApi.Messages.Send(new MessagesSendParams()
            {
                UserId  = vkId,
                Message = text
            });
        }
    }
}