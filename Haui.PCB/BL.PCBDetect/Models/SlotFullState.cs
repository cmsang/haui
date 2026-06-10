namespace BL.PCBDetect.Models;

public static class SlotFullState
{
    public const string Empty = "EMPTY";
    public const string Full = "FULL";

    public static bool IsEmpty(string? state)
        => string.IsNullOrWhiteSpace(state)
           || state.Equals(Empty, StringComparison.OrdinalIgnoreCase);
}
