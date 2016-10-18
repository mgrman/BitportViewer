using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitportViewer.Common
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string text)
        {
            return string.IsNullOrEmpty(text);
        }

        public static bool IsNotNullOrEmpty(this string text)
        {
            return !string.IsNullOrEmpty(text);
        }

    }
}
