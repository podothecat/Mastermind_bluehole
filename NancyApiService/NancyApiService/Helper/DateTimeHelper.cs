using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NancyApiService.Helper
{
    public static class DateTimeHelper
    {
        public static DateTime ToDateTime(this int date)
        {
            return new DateTime(date / 10000, (date % 10000) / 100, date % 100);
        }
    }
}