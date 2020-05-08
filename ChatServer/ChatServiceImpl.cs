// ----------------------------------------------------------------------------
// Copyright 2010 Wyle
// ----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using ChatInterfaces;

namespace ChatServer
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    public class ChatServiceImpl : IChatService
    {
        private readonly Dictionary<IChatClient, ChatUser> _users = new Dictionary<IChatClient, ChatUser>();

        #region Implementation of IChatService

        public void Login(string userName)
        {
            var connection = OperationContext.Current.GetCallbackChannel<IChatClient>();
            var user = new ChatUser {UserName = userName, LogInTime = DateTime.Now};
            _users[connection] = user;

            Console.WriteLine("{0} logged in.", userName);
        }

        public void Logout()
        {
            var connection = OperationContext.Current.GetCallbackChannel<IChatClient>();
            ChatUser user;
            if(_users.TryGetValue(connection, out user))
            {
                Console.WriteLine("{0} logged out.", user.UserName);
                _users.Remove(connection);
            }
        }

        public void SendMessage(string message)
        {
            var connection = OperationContext.Current.GetCallbackChannel<IChatClient>();
            ChatUser user;
            if (!_users.TryGetValue(connection, out user))
                return;

            Console.WriteLine("{0} sent message {1}", user.UserName, message);

            // a foreach collection is immutable and can't be cdhanged from within the loop
            var usersToBeRemovedDueToErrors = new List<IChatClient>();

            foreach (var otherConnection in _users.Keys)
            {
                if (otherConnection == connection)
                    continue;
                try
                {
                    otherConnection.ReceiveMessage(user.UserName, message);
                }
                catch (CommunicationException)
                {
                    // if a client closed without logging out, then this is a CommunicationObjectAbortedException
                    Console.WriteLine(
                        $"The connection {otherConnection} for user is not responsive. Eliminating from clients list");

                    usersToBeRemovedDueToErrors.Add(otherConnection);
                }
            }

            foreach (var remove in usersToBeRemovedDueToErrors)
            {
                _users.Remove(remove);
            }
        }

        public ChatUser[] LoggedInUsers
        {
            get
            {
                return _users.Values.ToArray();
            }
        }

        #endregion
    }
}