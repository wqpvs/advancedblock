using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace qptech.src
{
    class Electricity:ModSystem
    {
        public static List<BEElectric> electricalDevices;
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockEntityClass("BEEWire", typeof(BEEWire));
            api.RegisterBlockEntityClass("BEEDevice", typeof(BEEDevice));
            api.RegisterBlockEntityClass("BEEGenerator", typeof(BEEGenerator));
            api.RegisterBlockClass("ElectricalBlock", typeof(ElectricalBlock));
            /*protected int maxAmps=1;    //how many packets that can move at once
        protected int maxVolts=16;  //how many volts it can handle before exploding
        protected int capacitance=1;*/
        }
    }
}
