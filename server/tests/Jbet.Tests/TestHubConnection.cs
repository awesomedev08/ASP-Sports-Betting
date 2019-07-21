﻿using Jbet.Tests.Extensions;
using Microsoft.AspNetCore.SignalR.Client;
using Moq;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Jbet.Tests
{
    public class TestHubConnection<TEvent>
    {
        private readonly HubConnection _connection;
        private readonly string _expectedEventToReceive;
        private readonly Mock<Action<TEvent>> _handlerMock;
        private readonly int _verificationTimeout;

        internal TestHubConnection(HubConnection connection, string expectedEventToReceive, int verificationTimeout = 10000)
        {
            if (connection.State == HubConnectionState.Connected)
            {
                throw new ArgumentException($"You shouldn't pass open connections. Use {nameof(OpenAsync)}" +
                                            "to open the connection before verifying that the message was received.");
            }

            _handlerMock = new Mock<Action<TEvent>>();
            _expectedEventToReceive = expectedEventToReceive;
            _connection = connection;
            _verificationTimeout = verificationTimeout;
        }

        public async Task OpenAsync()
        {
            await _connection.StartAsync();
            _connection.On(_expectedEventToReceive, _handlerMock.Object);
        }

        public void VerifyMessageReceived(Expression<Func<TEvent, bool>> predicate, Times times)
        {
            _handlerMock.VerifyWithTimeout(x => x(It.Is<TEvent>(predicate)), times, _verificationTimeout);
        }

        public void VerifyNoMessagesWereReceived()
        {
            _handlerMock.VerifyWithTimeout(x => x(It.IsAny<TEvent>()), Times.Never(), _verificationTimeout);
        }

    }
}