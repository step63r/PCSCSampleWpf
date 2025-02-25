using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MinatoProject.PCSCSampleWpf.Common
{
    /// <summary>
    /// 選択されたデバイスを要求するメッセージ
    /// </summary>
    public class SelectedReaderRequestMessage : RequestMessage<string>
    {
    }
}
