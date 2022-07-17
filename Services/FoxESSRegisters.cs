namespace Roses.SolarAPI.Services
{
    public enum FoxESSRegisters : ushort
    {
        /// <summary>
        /// Time period 1 should charge from grid (0 = No, or 1 = Yes)
        /// </summary>
        BATTERY_TIMEPERIOD1_CHARGE_FROM_GRID = 41001,

        /// <summary>
        /// Time Period 1 start time as Hours * 256 + Minutes. Set to 0 to disable force charge.
        /// </summary>
        BATTERY_TIMEPERIOD1_START_TIME = 41002,

        /// <summary>
        /// Time Period 1 end time as Hours * 256 + Minutes. Set to 0 to disable force charge.
        /// </summary>
        BATTERY_TIMEPERIOD1_END_TIME = 41003,

        /// <summary>
        /// Time period 2 should charge from grid (0 = No, or 1 = Yes)
        /// </summary>
        BATTERY_TIMEPERIOD2_CHARGE_FROM_GRID = 41004,

        /// <summary>
        /// Time Period 2 start time as Hours * 256 + Minutes. Set to 0 to disable force charge.
        /// </summary>
        BATTERY_TIMEPERIOD2_START_TIME = 41005,

        /// <summary>
        /// Time Period 2 end time as Hours * 256 + Minutes. Set to 0 to disable force charge.
        /// </summary>
        BATTERY_TIMEPERIOD2_END_TIME = 41006,

        /// <summary>
        /// Minimum battery state of charge. Minimum value 10.
        /// </summary>
        BATTERY_MIN_SOC = 41009,

        /// <summary>
        /// Minimum battery state of when on grid. Minimum value 10.
        /// </summary>
        BATTERY_MIN_SOC_ON_GRID = 41011,
    }
}
