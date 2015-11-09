using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovieMaster.ViewModels
{
    public enum TypeOfThing
    {
        Movie,
        TVShow
    }

    public class FileThing : ViewModelBase
    {
        public string Path
        {
            get;
            set;
        }

        public TypeOfThing TypeOfVideo
        {
            get;
            set;
        }                                                   
    }
}
