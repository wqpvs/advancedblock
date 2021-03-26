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
        protected List<BEElectric>connections; //what we are connected to
        protected List<BEElectric> usedconnections; //track if already traded with in a given turn
        public int MaxAmps { get { return maxAmps; } }
        public int MaxVolts { get { return maxVolts; } }

        public bool IsPowered { get { return false; } }
        public bool IsOn { get { return isOn; } }
        
       

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            if (Electricity.electricalDevices == null) { Electricity.electricalDevices = new List<BEElectric>(); }
            Electricity.electricalDevices.Add(this);
            capacitor = 0;
            if (connections == null) { connections = new List<BEElectric>(); }
            if (Block.Attributes == null) { api.World.Logger.Error("ERROR BEE INITIALIZE HAS NO BLOCK");return; }
            maxAmps = Block.Attributes["maxAmps"].AsInt(maxAmps);
            maxVolts = Block.Attributes["maxVolts"].AsInt(maxVolts);
            capacitance = Block.Attributes["capacitance"].AsInt(capacitance);
            if (connections == null) { connections = new List<BEElectric>(); }
            RegisterGameTickListener(OnTick, 100);
            FindConnections();
        }
        //look for neighbors to connect to - TODO add a face check
        BlockEntity checkblock;
        public virtual void FindConnections()
        {
            //BlockFacing probably has useful stuff to do this right
            BlockPos[] checksides = {
                    new BlockPos(Pos.X - 1, Pos.Y, Pos.Z),
                    new BlockPos(Pos.X+1,Pos.Y,Pos.Z),
                    new BlockPos(Pos.X, Pos.Y, Pos.Z+1),
                    new BlockPos(Pos.X, Pos.Y, Pos.Z-1) };

            foreach (BlockPos checkPos in checksides)
            {
                checkblock = Api.World.BlockAccessor.GetBlockEntity(checkPos);
                 var bee = checkblock as BEElectric;
                 if (bee == null) { continue; }
                 if (bee.TryConnection(this)&&!connections.Contains(bee)) { connections.Add(bee); MarkDirty(); }

            }
        }
        
        //Allow devices to connection to each other
        //TODO add way to connect valid faces, especially if placed in different directions
        public virtual bool TryConnection(BEElectric connectto)
        {
            if (connections == null) { connections = new List<BEElectric>(); }
            if (!connections.Contains(connectto)) { connections.Add(connectto); MarkDirty(); }
            return true;
        }

        //Tell a connection to remove itself
        public virtual void RemoveConnection(BEElectric disconnect)
        {
            connections.Remove(disconnect);
        }

        public override void OnBlockBroken()
        {
            base.OnBlockBroken();
            Electricity.electricalDevices.Remove(this);
            foreach (BEElectric bee in connections) { bee.RemoveConnection(this); }
        }
        public virtual void OnTick(float par)
        {
            if (isOn) { DistributePower(); }
            usedconnections = new List<BEElectric>(); //clear record of connections for next tick
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);
            dsc.AppendLine("   On:" + isOn.ToString());
            dsc.AppendLine("Volts:"+MaxVolts.ToString()+"V");
            dsc.AppendLine("Power:" + capacitor.ToString() + "/" + capacitance.ToString());
        }
        
        //Used for other power devices to offer this device some energy
        public virtual int ReceivePacketOffer(BEElectric from, int inVolt, int inAmp) //eg 2
        {
            if (usedconnections == null) { usedconnections = new List<BEElectric>(); }
            if (!isOn) { return 0; }//Not even on
            if (inVolt > maxVolts) { DoOverload(); return 0; }//!TOO MANY VOLTS!
            if (inVolt < maxVolts) { return 0; }// not enough volts
            if (capacitor>=capacitance) { return 0; }//already full
            inAmp = Math.Min(inAmp, MaxAmps); //can only move a certain amount of amps - eg 2
            int useamps = Math.Min(inAmp, capacitance - capacitor); //2
            capacitor += useamps;//capacitor=2
            usedconnections.Add(from);
            if (useamps != 0) { MarkDirty(); }//not zero should be dirty
            return useamps;//return 2
        }
        
        //Attempt to send out power (can be overridden for devices that only use power)
        public virtual void DistributePower()
        {
            if (usedconnections == null) { usedconnections = new List<BEElectric>(); }
            if (!isOn) { return; } //can't generator power if off
            
            if (connections == null) { return; } //nothing hooked up
            if (connections.Count==0) { return; }
            
            int ampsMoved = 0;
                        
            foreach (BEElectric bee in connections)
            {
                if (capacitor == 0) { break; } //no power to give 
                if (bee == null) { continue; }
                if (usedconnections.Contains(bee)) { continue; } //already traded with this bee
                int powerOffer = Math.Min(capacitor, MaxAmps); //offer as much as possible 
                int powerUsed = bee.ReceivePacketOffer(this,MaxVolts, powerOffer);
                ampsMoved += powerUsed;
                capacitor -= powerUsed;
                
            }
            if (ampsMoved != 0) { MarkDirty(); }
            
        }
        
        public virtual void DoOverload()
        {
            //BOOOOM!
        }

        public virtual void TogglePower()
        {
            isOn = !isOn;
        }
    }
}
