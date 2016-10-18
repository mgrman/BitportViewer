using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitportViewer.Common
{
    public static class BitportAppTokenExtensions
    {
        public static bool IsValid(this BitportAppToken tokenResponse)
        {
            return (tokenResponse?.access_token).IsNotNullOrEmpty();
        }

        public static bool IsInvalid(this BitportAppToken tokenResponse)
        {
            return !tokenResponse.IsValid();
        }
    }
}
