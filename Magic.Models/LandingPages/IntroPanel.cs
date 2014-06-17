using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magic.Models.LandingPages
{
    public class IntroPanel : LandingPagePanel
    {
        public virtual string ButtonDescription { get; set; }

        public virtual string UpperImageUrl { get; set; }
        public virtual string UpperImageDescription { get; set; }
        public virtual string LowerImageUrl { get; set; }
        public virtual string LowerImageDescription { get; set; }

        public IntroPanel() {
            Type = PanelType.Intro;
        }
    }
}