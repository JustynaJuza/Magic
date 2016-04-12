using System;

namespace Juza.Magic.Areas.Admin.Controllers
{
    public class SuccessResult
    {
        public bool Success { get; set; }
        public Exception Exception { get; set; }
        public string Description { get; set; }
    }
}