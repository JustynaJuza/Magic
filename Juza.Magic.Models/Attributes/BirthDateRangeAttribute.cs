using System;
using System.ComponentModel.DataAnnotations;

namespace Juza.Magic.Models
{
    public class BirthDateRangeAttribute : RangeAttribute
    {
        private static string startDate = DateTime.Now.AddYears(-100).ToString();
        private static string endDate = DateTime.Today.ToString();

        public BirthDateRangeAttribute()
            : base(typeof(DateTime), startDate, endDate) { }
    }
}