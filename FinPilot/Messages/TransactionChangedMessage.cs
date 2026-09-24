using System;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace FinPilot.Messages
{
    public class TransactionChangedMessage : ValueChangedMessage<Guid>
    {
        public TransactionChangedMessage(Guid userId) : base(userId)
        {
        }
    }
}
