﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using Vintagestory.API.Server;
using Vintagestory.API;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Util;
using Vintagestory.ServerMods;

namespace qptech.src
{
    /*
     * Firepit Stoker by WQP
     * will search for firepits to the sides and attempt to fill
     * from a container placed above it
     * Should only add fuel when the firepit is processing
     * Will not attempt to validate valid fuel types
     */
    public class FirepitStoker : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            
        }

        public class FirepitStokerEntity : BlockEntity
        {
            public override void Initialize(ICoreAPI api)
            {
                base.Initialize(api);
                RegisterGameTickListener(OnTick, 500);
            }
            BlockEntity checkblock;
            
            public void OnTick(float par)
            {

                //Check for container
                BlockPos contPos = new BlockPos(Pos.X, Pos.Y + 1, Pos.Z);
                checkblock = Api.World.BlockAccessor.GetBlockEntity(contPos);
                //if (checkblock == null) { return; } //no container no point running
                
                var inputContainer = checkblock as BlockEntityContainer;
                if (inputContainer == null) {
                    
                    return;
                }
                
                BlockPos[] checksides = {
                    new BlockPos(Pos.X - 1, Pos.Y, Pos.Z),
                    new BlockPos(Pos.X+1,Pos.Y,Pos.Z),
                    new BlockPos(Pos.X, Pos.Y, Pos.Z+1),
                    new BlockPos(Pos.X, Pos.Y, Pos.Z-1) };

                foreach (BlockPos checkPos in checksides)
                { 
                    //try to find a firepit
                    checkblock = Api.World.BlockAccessor.GetBlockEntity(checkPos);
                    //Nothing at firepit location, nothing we can do, there is no point, good day sir!
                    
                    var firepit = checkblock as BlockEntityFirepit;
                    //NO Firepit then you musta quit
                    if (firepit == null) {continue; }
                    
                    if (firepit.inputSlot == null) { continue; }
                    //Don't bother trying to add fuel if nothing is being cooked
                    if (firepit.inputSlot.StackSize == 0) { continue; }
                    if (firepit.fuelSlot==null) { continue; }
                    //IF fuel is in the fuelSlot don't bother
                    if (firepit.fuelSlot.StackSize > 0) { 
                        
                        continue;
                    }
                    //TODO figure out how to keep firepit lit, also figure out how to verify fuel
                    //OK looks like we need fuel, attempt to add a piece
                    ItemSlot sourceSlot = inputContainer.Inventory.GetAutoPullFromSlot(BlockFacing.DOWN);
                    if (sourceSlot == null) { continue; }
                    int quantity = 1;
                    ItemStackMoveOperation op = new ItemStackMoveOperation(Api.World, EnumMouseButton.Left, 0, EnumMergePriority.DirectMerge, quantity);

                    int qmoved = sourceSlot.TryPutInto(firepit.fuelSlot, ref op);
                    firepit.fuelSlot.MarkDirty();
                    sourceSlot.MarkDirty();

                    
                    
                }
            }
        }
    }
}
