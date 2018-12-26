﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace EchoApp.Hubs
{
    public class EchoHub : Hub
    {
        private readonly ILogger _logger;
        public EchoHub(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<EchoHub>();
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("SignalR client {@1} connected", Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, "SignalR Users");
            await base.OnConnectedAsync();            
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SignalR Users");
            await base.OnDisconnectedAsync(exception);
            _logger.LogInformation("SignalR client {@1} disconnected", Context.ConnectionId);
        }

#pragma warning disable CS1998
        // ReSharper disable UnusedMember.Global
        public async Task<string> EchoWithJsonFormat(string message)
        // ReSharper restore UnusedMember.Global
#pragma warning restore CS1998
        {
            var structedBuffer = new MyStructedLog() { Buffer = message };
            _logger.LogInformation("buffer= {@1}", structedBuffer);
            var sendStr = $"{{\"recv\": \"{message}\"}}";

            return sendStr;
        }

        // ReSharper disable UnusedMember.Global
        public ChannelReader<char> Reverse(string input)
        // ReSharper restore UnusedMember.Global
        {
            var cancellationToken = Context.ConnectionAborted;
            var channel = Channel.CreateBounded<char>(1);

#pragma warning disable 4014
            DoReverse(input, channel.Writer, new TimeSpan(0, 0, 1), cancellationToken);
#pragma warning restore 4014

            return channel.Reader;
        }

        private async Task DoReverse(string input, ChannelWriter<char> channelWriter, TimeSpan delay, CancellationToken cancellationToken)
        {
            try
            {
                for (var i = input.Length - 1; i >= 0; i--)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var theChar = input[i];
                    _logger.LogInformation("write {0}", theChar);
                    await channelWriter.WriteAsync(theChar, cancellationToken);
                    await Task.Delay(delay, cancellationToken);
                }

                channelWriter.TryComplete();
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("channel writer canncelled due to client disconnected.");
            }
        }
    }
}
