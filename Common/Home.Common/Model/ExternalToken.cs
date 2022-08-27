using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public class ExternalToken
    {
        public string Id { get; set; }

        public string UserName { get; set; }
        public string TokenType { get; set; }

        public string Token { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
        public string RefreshToken { get; set; }
    }
}
