﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitportViewer.Common
{

    public class BitPortResponse<T>
    {
        public Status status { get; set; }
        public T[] data { get; set; }
        public Error[] errors { get; set; }
    }


    public enum Status
    {
        Success,
        Error
    }

    public class Error
    {
        public string message { get; set; }
        public int code { get; set; }
    }
}
