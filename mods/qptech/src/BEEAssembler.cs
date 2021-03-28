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
    //Finds items in a container and using power, assembles into new items
    class BEEAssembler:BEEDevice
    {
        protected string recipe = "game:bowl-raw";
        protected int outputQuantiy = 1;
        protected string ingredient = "game:clay-blue";
        protected int inputQuantity = 4;
        protected override void DoDeviceStart()
        {
            //TODO
            //Check for power
            //Check for supplies
            //If ok - begin process, use up supplies
            
            if (capacitor >= requiredAmps)
            {

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
    }
}
