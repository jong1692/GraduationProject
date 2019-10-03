using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Message
{
    public enum MessageType
    {
        DAMAGED = 0,
        DEAD,
        RESPAWN,
    }

    public interface IMessageReceiver
    {
        void receiveMessage(MessageType type, object sender, object msg);
    }
}