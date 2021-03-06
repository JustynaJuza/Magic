﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Magic.Models.LandingPages
{
    public class ArticlePanel : LandingPagePanel
    {
        public virtual IList<LandingPagePanel> Tiles { get; set; }

        public virtual string LeftQuote { get; set; }
        public virtual string LeftQuoteAuthor { get; set; }

        public virtual string RightQuote { get; set; }
        public virtual string RightQuoteAuthor { get; set; }

        public ArticlePanel() {
            Type = PanelType.Article;
            Tiles = Enumerable.Repeat(new LandingPagePanel(), 2).ToList();
        }
    }
}