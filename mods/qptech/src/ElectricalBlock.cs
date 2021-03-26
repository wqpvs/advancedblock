using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using Vintagestory.API.MathTools;

namespace qptech.src
{
    class ElectricalBlock:Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            
            var bee = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEElectric;
            if (bee != null) { bee.TogglePower(); }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
    }
}
