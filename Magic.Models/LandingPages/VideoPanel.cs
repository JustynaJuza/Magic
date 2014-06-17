using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magic.Models.LandingPages
{
    public class VideoPanel : LandingPagePanel
    {
        public virtual string VideoImageUrl { get; set; }
        public virtual string VideoUrl { get; set; }
        public virtual string VideoDescription { get; set; }
        
        public virtual IList<PictureTile.AnimationFrame> PictureBar { get; set; }

        public VideoPanel() {
            Type = PanelType.Video;
            PictureBar = Enumerable.Repeat(new PictureTile.AnimationFrame(), 6).ToList();
        }
    }
}