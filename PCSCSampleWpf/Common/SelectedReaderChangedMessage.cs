using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MinatoProject.PCSCSampleWpf.Common
{
    /// <summary>
    /// 選択されたデバイス変更のメッセージ送受信クラス
    /// </summary>
    public class SelectedReaderChangedMessage(string deviceName) : ValueChangedMessage<string>(deviceName)
    {
    }
}
