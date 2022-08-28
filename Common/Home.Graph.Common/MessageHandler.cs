using Home.Common.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common
{

    public delegate MessageResponse MessageHandler(MessageOrigin origin, string topic, string messageBody);
    
}
