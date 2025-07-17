using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TrainsOfTheRim
{
    internal class RailComp : ThingComp
    {
        public RailCompProperties Props => (RailCompProperties)this.props;
    }
}
