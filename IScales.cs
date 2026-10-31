using System.Runtime.InteropServices;

namespace ScalesComponent
{
    /// <summary>
    /// COM-интерфейс для работы с весами из 1С
    /// </summary>
    [ComVisible(true)]
    [Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567890")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IScales
    {
        [DispId(1)]
        bool Connect(string portName, int baudRate, string scalesType);

        [DispId(2)]
        void Disconnect();

        [DispId(3)]
        double GetCurrentWeight();

        [DispId(4)]
        double GetStableWeight(int timeoutMs);

        [DispId(5)]
        bool IsConnected();

        [DispId(6)]
        bool IsStable();

        [DispId(7)]
        void Tare();

        [DispId(8)]
        string GetLastError();

        [DispId(9)]
        string[] GetAvailableScalesTypes();
    }
}