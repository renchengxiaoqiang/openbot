using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BotLib.FileSerializer
{
    public interface ISeriableObject
    {
        void OnAppClose();
    }
}
