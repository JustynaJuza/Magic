using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magic.Models.LandingPages
{
    public class PicturePanel : LandingPagePanel
    {
        public virtual IList<PictureTile> Tiles { get; set; }

        public PicturePanel() {
            Type = PanelType.Picture;
            Tiles = Enumerable.Repeat(new PictureTile(), 6).ToList();
        }
    }

    public class PictureTile : LandingPagePanel
    {
        public class AnimationFrame {
            protected virtual int Id { get; set; }
            //protected virtual PictureTile ParentTile { get; set; }
            public virtual string ImageUrl { get; set; }

            public AnimationFrame() { }
            public AnimationFrame(string imageUrl)
            {
                ImageUrl = imageUrl;
            }

            //public static explicit operator string(AnimationFrame obj)
            //{
            //    return obj.ImageUrl;
            //}
            //public static explicit operator AnimationFrame(String str)
            //{
            //    return new AnimationFrame(str);
            //}
            public static implicit operator string(AnimationFrame obj)
            {
                return obj.ImageUrl;
            }
            public static implicit operator AnimationFrame(String str)
            {
                return new AnimationFrame(str);
            }
        }

        public virtual string Colour { get; set; }
        public virtual bool IsAnimated
        {
            get { return AnimationFrames.Count() > 0; }
        }

        public virtual IList<AnimationFrame> AnimationFrames { get; set; }

        public PictureTile() {
            Type = PanelType.PictureTile;
            AnimationFrames = new List<AnimationFrame>();
        }
    }
}