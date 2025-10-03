namespace LabsQueueBot.Core.Enums;

[Flags]
public enum WeekDays
{
    None      = 0b0000000,  // 0
    Monday    = 0b0000001,  // 1 (1 << 0)
    Tuesday   = 0b0000010,  // 2 (1 << 1)
    Wednesday = 0b0000100,  // 4 (1 << 2)
    Thursday  = 0b0001000,  // 8 (1 << 3)
    Friday    = 0b0010000,  // 16 (1 << 4)
    Saturday  = 0b0100000,  // 32 (1 << 5)
    Sunday    = 0b1000000,  // 64 (1 << 6)
    
    Weekdays  = Monday | Tuesday | Wednesday | Thursday | Friday,   // 31 (0b0011111)
    Weekend   = Saturday | Sunday,                                  // 96 (0b1100000)
    All       = Weekdays | Weekend                                  // 127 (0b1111111)
}