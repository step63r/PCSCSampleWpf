using CommunityToolkit.Mvvm.Messaging.Messages;
using PCSC.Monitoring;

namespace MinatoProject.PCSCSampleWpf.Common
{
    /// <summary>
    /// StateChangeEventArgsのメッセージ送受信クラス
    /// </summary>
    /// <param name="args">StatusChangeEventArgs</param>
    public class StatusChangeEventMessage(StatusChangeEventArgs args) : ValueChangedMessage<StatusChangeEventArgs>(args)
    {
    }
}
