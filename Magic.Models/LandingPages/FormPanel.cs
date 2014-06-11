using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magic.Models.LandingPages
{
    public class FormPanel : LandingPagePanel
    {
        public virtual string RegistrationCode { get; set; }

        public FormPanel() {
            Type = PanelType.Form;
        }
    }
}