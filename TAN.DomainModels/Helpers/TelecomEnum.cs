using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Helpers
{
 
    public enum CallDirection
    {
        [Description("INBOUND")]
        INBOUND,
        [Description("INBOUND-FORWARDED")]
        INBOUND_FORWARDED,
        [Description("OUTBOUND")]
        OUTBOUND,
        [Description("OUTBOUND-FORWARDED")]
        OUTBOUND_FORWARDED
    }

    public enum CallType
    {
        [Description("EMERGENCY")]
        EMERGENCY,
        [Description("INBOUND-TFOOS")]
        INBOUND_TFOOS,
        [Description("INFORMATION")]
        INFORMATION,
        [Description("INTERNATIONAL")]
        INTERNATIONAL,
        [Description("INTERNATIONAL-INTERNAL")]
        INTERNATIONAL_INTERNAL,
        [Description("INTERSTATE")]
        INTERSTATE,
        [Description("INTRASTATE")]
        INTRASTATE,
        [Description("INTL-BLOCK")]
        INTL_BLOCK,
        [Description("LOCAL")]
        LOCAL,
        [Description("OPERATOR")]
        OPERATOR,
        [Description("OTHER-N11")]
        OTHER_N11,     
        [Description("SIPURI-EXT")]
        SIPURI_EXT,       
        [Description("TOLLFREE-IN")]
        TOLLFREE_IN,
        [Description("TOLLFREE-OUT")]
        TOLLFREE_OUT,
        [Description("UNDETERMINED")]
        UNDETERMINED
    }

    public enum CallResult
    {
        [Description("COMPLETED")]
        COMPLETED,
        [Description("INCOMPLETE")]
        INCOMPLETE
    }

    public enum HangUpSource
    {
        [Description("BANDWIDTH_INTERNAL")]
        BANDWIDTH_INTERNAL,
        [Description("CALLED_PARTY")]
        CALLED_PARTY,
        [Description("CALLING_PARTY")]
        CALLING_PARTY
    }

    public enum DataServiceStatusCode {
        INVALID_FILE = 1001,
        FILE_EXIST = 1002,
        SUCCESS = 1003,
        FAILED = 1004,
        ACCOUNTNUMBER_EXIST = 1005,
        UPLOAD_FILE_ERROR = 1006
    }

}
