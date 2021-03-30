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
            api.RegisterBlockEntityClass("BEEAssembler", typeof(BEEAssembler));
            api.RegisterBlockEntityClass("BEEGenerator", typeof(BEEGenerator));
            api.RegisterBlockClass("ElectricalBlock", typeof(ElectricalBlock));
            //api.RegisterBlockClass("BlockElectricMotor", typeof(BlockElectricMotor));
            //api.RegisterBlockEntityClass("BEEMotor", typeof(BEEMotor));

        }
    }
}
