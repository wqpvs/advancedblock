using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace qptech.src
{
    //Device to use up electricity
    class BEEDevice:BEElectric
    {
        //How many amps to run at maxVolts?
        protected int requiredAmps = 1;
        public int RequiredAmps { get { return requiredAmps; } }
        public bool isPowered { get { return capacitor >= requiredAmps; } }
        public override void OnTick(float par)
        {
            //override as we don't want to generate power
            UsePower();
        }
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            requiredAmps = Block.Attributes["requiredAmps"].AsInt(requiredAmps);
        }
        public virtual void UsePower()
        {
            if (isOn && IsPowered) { capacitor-=requiredAmps; }
        }
    }
}
