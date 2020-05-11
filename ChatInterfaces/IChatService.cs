// ----------------------------------------------------------------------------
// Copyright 2010 Wyle
// ----------------------------------------------------------------------------
using System.ServiceModel;

namespace ChatInterfaces
{
    [ServiceContract(CallbackContract = typeof(IChatClient))]
    public interface IChatService
    {
        [OperationContract]
        void Login(string userName);

        [OperationContract(IsOneWay = true)]
        void Logout();

        [OperationContract(IsOneWay = true)]
        void SendMessage(string message);

        ChatUser[] LoggedInUsers
        { 
            [OperationContract]
            get;
        }
    }
}