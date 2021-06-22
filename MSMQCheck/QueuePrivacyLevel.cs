using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSMQCheck
{
    enum QueuePrivacyLevel
    {
        MQ_PRIV_LEVEL_NONE = 0,
        MQ_PRIV_LEVEL_OPTIONAL = 1,
        MQ_PRIV_LEVEL_BODY = 2,
    }
}
