using System;

namespace TradeOnSda.Data;

public class SdaSettings    
{
    public bool IsEnabledAutoConfirm { get; set; }
    
    public TimeSpan AutoConfirmDelay { get; set; }

    public SdaSettings(bool isEnabledAutoConfirm, TimeSpan autoConfirmDelay)
    {
        IsEnabledAutoConfirm = isEnabledAutoConfirm;
        AutoConfirmDelay = autoConfirmDelay;
    }
}