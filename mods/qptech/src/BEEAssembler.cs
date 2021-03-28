﻿using System;
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
    //Finds items in a container and using power, assembles into new items
    class BEEAssembler:BEEDevice
    {
        protected string recipe = "game:bowl-raw";
        protected int outputQuantiy = 1;
        protected string ingredient = "game:clay-blue";
        protected int inputQuantity = 4;
        protected int internalQuantity = 0; //will store ingredients virtually
        protected List<BlockFacing> rmInputFaces; //what faces will be checked for input containers
        protected List<BlockFacing> rmOutputFaces; //what faces will be checked for output containers
        
        /// </summary>
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            rmInputFaces = new List<BlockFacing>();
            rmOutputFaces = new List<BlockFacing>();
            //TEMP CODE TO ADD faces, should be loaded from attributes
            rmInputFaces.Add(BlockFacing.UP);
            rmOutputFaces.Add(BlockFacing.DOWN);
        }
        protected override void DoDeviceStart()
        {
            //TODO
            //Check for power
            //Check for supplies
            //If ok - begin process, use up supplies
            if (capacitor < requiredAmps) { return; }//not enough power
            FetchMaterial();
                  
            if (internalQuantity<inputQuantity) { deviceState = enDeviceState.MATERIALHOLD; return; }//check for and extract the required RM
            //TODO - do we make sure there's an output container?
            if (capacitor >= requiredAmps)
            {
                internalQuantity = 0;
                tickCounter = 0;
                deviceState = enDeviceState.RUNNING;

                if (Api.World.Side == EnumAppSide.Client && animUtil != null)
                {
                    if (!animInit)
                    {
                        float rotY = Block.Shape.rotateY;
                        animUtil.InitializeAnimator("process", new Vec3f(0, rotY, 0));
                        animInit = true;
                    }
                    animUtil.StartAnimation(new AnimationMetaData()
                    {
                        Animation = "process",
                        Code = "process",
                        AnimationSpeed = 1f,
                        EaseInSpeed = 1,
                        EaseOutSpeed = 1,
                        Weight = 1,
                        BlendMode = EnumAnimationBlendMode.Add
                    });
                    Api.World.PlaySoundAt(new AssetLocation("sounds/doorslide"), Pos.X, Pos.Y, Pos.Z, null, false, 8, 1);
                }

                //sounds/blocks/doorslide.ogg
                DoDeviceProcessing();
            }
            else { DoFailedStart(); }
        }
        protected override void DoDeviceComplete()
        {
            deviceState = enDeviceState.IDLE;
            Block block = Api.World.GetBlock(new AssetLocation(recipe));
            ItemStack outputStack = new ItemStack(block);

            Vec3d pos = Pos.ToVec3d();
            pos.Y += 0.5f;
            Vec3d vel = new Vec3d(0, 0.25f, 0);
            Api.World.SpawnItemEntity(outputStack, pos, vel);
            //Api.World.SpawnItemEntity(grindedStack, this.Pos.ToVec3d().Add(0.5 + face.Normalf.X * 0.7, 0.75, 0.5 + face.Normalf.Z * 0.7), new Vec3d(face.Normalf.X * 0.02f, 0, face.Normalf.Z * 0.02f));
            if (Api.World.Side == EnumAppSide.Client && animUtil != null)
            {
                animUtil.StopAnimation("process");
            }
        }

        protected void FetchMaterial()
        {
            internalQuantity = Math.Min(internalQuantity, inputQuantity); //this shouldn't be necessary
            Item rm = Api.World.GetItem(new AssetLocation(ingredient));
            if (rm == null)
            {
                deviceState = enDeviceState.ERROR;
            }
            foreach (BlockFacing bf in rmInputFaces)
            {
                BlockPos bp = Pos.Copy().Offset(bf);
                BlockEntity checkblock = Api.World.BlockAccessor.GetBlockEntity(bp);
                var inputContainer = checkblock as BlockEntityContainer;
                if (inputContainer == null) { continue; }
                if (inputContainer.Inventory.Empty) { continue; }
                for (int c = 0; c < inputContainer.Inventory.Count; c++)
                {
                    ItemSlot checkslot = inputContainer.Inventory[c];
                    if (checkslot == null) { continue; }
                    if (checkslot.StackSize == 0) { continue; }
                    bool match = false;
                    if (checkslot.Itemstack.Item!=null && checkslot.Itemstack.Item.FirstCodePart() == rm.FirstCodePart()) { match = true; }
                    else if (checkslot.Itemstack.Block!=null && checkslot.Itemstack.Block.FirstCodePart() == rm.FirstCodePart()) { match = true; }
                    if (match)
                    {
                        int reqQty = Math.Min(checkslot.StackSize, inputQuantity - internalQuantity);
                        checkslot.TakeOut(reqQty);
                        internalQuantity += reqQty;
                        checkslot.MarkDirty();

                    }
                }
                
            }
            return;
        }
        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            internalQuantity = tree.GetInt("internalQuantity");
            
        }
        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetInt("internalQuantity", internalQuantity);
            
        }
        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);
            
            dsc.AppendLine("RM   :" + internalQuantity.ToString() + "/" + inputQuantity.ToString());
        }
    }
}
