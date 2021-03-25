using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace qptech.src
{
    class BEEGenerator:BEElectric
    {
        //how many power packets we can generate - will see if every more than one
        protected int genAmps = 1;      
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            api.World.Logger.Event("GENERATOR IS HERE!");
            genAmps = Block.Attributes["genAmps"].AsInt(genAmps);
        }
        public override void OnTick(float par)
        {
            if (isOn) {
                GeneratePower(); //Create power packets if possible, base.Ontick will handle distribution attempts
            }
            base.OnTick(par);
        }
        //Attempts to generate power
        public virtual void GeneratePower()
        {
            if (!CanGeneratePower()) { return; }
            capacitor = Math.Min(capacitance, capacitor + MaxAmps);
            return;
        }

        public virtual bool CanGeneratePower()
        {
            if (!isOn) { return false; }
            
            return true;
        }
        
    }
}
