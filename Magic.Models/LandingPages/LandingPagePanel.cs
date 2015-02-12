using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Magic.Models.LandingPages
{
    public enum PanelType
    {
        Tile = 0,
        Intro = 1,
        Picture = 2,
        Video = 3,
        Form = 4,
        Article = 5,
        PictureTile = 6,
        //AnimPictureTile = 6
    }

    // Factory class.
    public class LandingPagePanel
    {
        public virtual int Id { get; set; }
        public virtual PanelType Type { get; set; }
        public virtual string Title { get; set; }
        public virtual string Subtitle { get; set; }
        public virtual string Description { get; set; }
        public virtual string ImageUrl { get; set; }
        public virtual string ImageDescription { get; set; }

        public static SelectList GetAvailablePanelTypes()
        {
            return new SelectList(typeof(PanelType).GetEnumNames().Skip(1).Take(5));
        }

        public static LandingPagePanel Create(PanelType type = PanelType.Tile) {
            switch (type) { 
                case PanelType.Tile: return new LandingPagePanel();
                case PanelType.Intro: return new IntroPanel();
                case PanelType.Picture: return new PicturePanel();
                case PanelType.Video: return new VideoPanel();
                case PanelType.Form: return new FormPanel();
                case PanelType.Article: return new ArticlePanel();
                case PanelType.PictureTile: return new PictureTile();
                default: return new LandingPagePanel();
            }
        }

        public LandingPagePanel() { }
        public LandingPagePanel(PanelType type) {
            Type = type;
        }
    }
}