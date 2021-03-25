using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace qptech.src
{
    public class BEElectric : BlockEntity
    {
        /*base class to handle electrical devices*/
        protected int maxAmps=1;    //how many packets that can move at once
        protected int maxVolts=16;  //how many volts it can handle before exploding
        protected int capacitance=1;//how many packets it can store
        protected int capacitor;  //packets currently stored (the ints store the volts for each packet)
        protected bool isOn=true;        //if it's not on it won't do any power processing
        protected Dictionary<BlockFacing, BEElectric> connections;
        public int MaxAmps { get { return maxAmps; } }
        public int MaxVolts { get { return maxVolts; } }

        public bool IsPowered { get { return false; } }
        public bool IsOn { get { return isOn; } }
        //Used for other power devices to offer this device some energy
        public virtual int ReceivePacketOffer (int volt, int amp)
        { 
            if (!isOn) { return 0; }
            if (volt > maxVolts) { DoOverload();return 0; }
            if (volt < maxVolts) { return 0; }
            if (capacitance == capacitor) { return 0; }
            amp = Math.Min(amp, MaxAmps); //can only move a certain amount of amps
            int useamps = Math.Min(amp, capacitance - capacitor);
            capacitor += useamps;
            
            return useamps;
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            capacitor = 0;
            connections = new Dictionary<BlockFacing, BEElectric>();
            maxAmps = Block.Attributes["maxAmps"].AsInt(maxAmps);
            maxVolts = Block.Attributes["maxVolts"].AsInt(maxVolts);
            capacitance = Block.Attributes["capacitance"].AsInt(capacitance);
            RegisterGameTickListener(OnTick, 100);
        }

        public virtual void OnTick(float par)
        {
            if (isOn) { DistributePower(); }
        }
        //Attempt to send out power (can be overridden for devices that only use power)
        public virtual void DistributePower()
        {
            if (!isOn) { return; } //can't generator power if off
            
            if (connections == null) { return; } //nothing hooked up
            if (connections.Count==0) { return; }
            int ampsMoved = 0;
                        
            foreach (BEElectric bee in connections.Values)
            {
                if (capacitor == 0) { break; } //no power to give
                
                ampsMoved=bee.ReceivePacketOffer(MaxVolts,Math.Min(capacitor,MaxAmps));
                capacitor -= ampsMoved;
            }
            
        }
        
        public virtual void DoOverload()
        {
            //BOOOOM!
        }
    }
}
