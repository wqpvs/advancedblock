using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;


namespace qptech.src.misc
{
    class ItemQuarryTool : Item
    {
        float nextactionat = 0;
        bool soundplayed = false;
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            //base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
            nextactionat = 2;
            

            handling = EnumHandHandling.Handled;
        }
        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            // - start particles
            // - first extension
            // - break top/bottom/side blocks (maybe add calculations to make it faster for less blocks
            // - second extension
            // - break back block
            // - reset
            if (blockSel == null) { return false; }
            if (!BlockFacing.HORIZONTALS.Contains(blockSel.Face)) { return false; } //not pointed at a block ahead, cancel
            if (secondsUsed>0.25f && !soundplayed)
            {
                //api.World.PlaySoundAt(new AssetLocation("sounds/quarrytemp"), blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, null, false, 8, 1);
                soundplayed = true;
            }
            if (secondsUsed > nextactionat)
            {

                IPlayer p = api.World.NearestPlayer(byEntity.Pos.X, byEntity.Pos.Y, byEntity.Pos.Z);
                foreach (BlockFacing bf in BlockFacing.ALLFACES)
                {
                    BlockPos bp = blockSel.Position.Copy().Offset(bf);
                    Block tb = api.World.BlockAccessor.GetBlock(bp);

                    if (tb == null) { continue; }
                    if (tb.FirstCodePart() == "rock")
                    {
                        tb.OnBlockBroken(api.World, bp, p, 1);
                    }
                }
                //nextactionat = secondsUsed + 2;
                soundplayed = false;
                return false;
            }
            return true;
        }
        public override bool OnHeldInteractCancel(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason)
        {
            
            return false;
        }
    }
}
