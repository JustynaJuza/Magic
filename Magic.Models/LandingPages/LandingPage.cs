using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Magic.Models.LandingPages
{
    public class LandingPage
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public virtual DateTime Created { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(NullDisplayText = "Unpublished", ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public virtual DateTime? Published { get; set; }

        [DisplayName("Shortlink")]
        public virtual string ShortUrl { get; set; }
        [DisplayName("Social Share Image")]
        public virtual string ShareImageUrl { get; set; }
        [DisplayName("Social Share Description")]
        public virtual string ShareDescription { get; set; }

        public virtual IList<LandingPagePanel> Panels { get; set; }

        public LandingPage() {
            Panels = new List<LandingPagePanel>();
            //Created = DateTime.Now;
        }

        public virtual LandingPage AddDefaultPanels() {
            this.Panels = new List<LandingPagePanel>() { 
                LandingPagePanel.Create(PanelType.Intro),
                LandingPagePanel.Create(PanelType.Picture),
                LandingPagePanel.Create(PanelType.Video),
                LandingPagePanel.Create(PanelType.Form),
                LandingPagePanel.Create(PanelType.Article)
            };

            return this;
        }

        public virtual LandingPage CopyFromEdit(LandingPage m)
        {
            this.Name = m.Name;
            this.Created = m.Created;
            this.Published = m.Published;
            this.ShortUrl = m.ShortUrl;
            this.ShareDescription = m.ShareDescription;
            this.ShareImageUrl = m.ShareImageUrl;
            //foreach(var x in this.Panels)
            //    this.Panels.
            this.Panels = new List<LandingPagePanel>() { 
                LandingPagePanel.Create(PanelType.Intro),
                LandingPagePanel.Create(PanelType.Picture),
                LandingPagePanel.Create(PanelType.Video),
                LandingPagePanel.Create(PanelType.Form),
                LandingPagePanel.Create(PanelType.Article)
            };

            return this;
        }
    }
}