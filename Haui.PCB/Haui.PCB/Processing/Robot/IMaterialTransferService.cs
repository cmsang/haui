namespace Haui.PCB.Processing.Robot;

public interface IMaterialTransferService
{
    bool IsRunning { get; }

    void Cancel();

    Task TransferPassAsync(Action<string> reportStatus);

    Task TransferFailAsync(Action<string> reportStatus);
}
