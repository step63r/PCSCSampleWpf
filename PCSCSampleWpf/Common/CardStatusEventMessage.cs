using CommunityToolkit.Mvvm.Messaging.Messages;
using PCSC.Monitoring;

namespace MinatoProject.PCSCSampleWpf.Common
{
    /// <summary>
    /// CardStatusEventArgsのメッセージ送受信クラス
    /// </summary>
    /// <param name="args">CardStatusEventArgs</param>
    public class CardStatusEventMessage(CardStatusEventArgs args) : ValueChangedMessage<CardStatusEventArgs>(args)
    {
    }
}
