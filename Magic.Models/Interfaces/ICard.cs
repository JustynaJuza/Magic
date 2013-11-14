using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magic.Models.Interfaces
{
    public interface ICard
    {
        bool Play();
        void Tap();
        void UnTap();
    }
}
