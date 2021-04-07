﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace qptech.src
{
    public class BlockEntitySingleSink : BlockEntityContainer
    {
        public int CapacityLitres { get; set; } = 50;

        GuiDialogSingleSink invDialog;

        // Slot 0: Input/Item slot
        // Slot 1: Liquid slot

        internal InventoryGeneric inventory;

        public override InventoryBase Inventory => inventory;
        public override string InventoryClassName => "singlesink";

        //MeshData currentMesh;
        BlockSingleSink ownBlock;
        public float MeshAngle;

        Vec3f rendererRot = new Vec3f();

        private BlockEntityAnimationUtil animUtil => ((BEBehaviorAnimatable)GetBehavior<BEBehaviorAnimatable>())?.animUtil;

        public BlockEntitySingleSink()
        {
            inventory = new InventoryGeneric(9, null, null, (id, self) =>
            {
                if (id == 0 || id == 2 || id == 3 || id == 4 || id == 5 || id == 6 || id == 7 || id == 8) return new ItemSlotBarrelInput(self);
                else return new ItemSlotLiquidOnly(self, 50);
            });
            inventory.BaseWeight = 9;
            inventory.OnGetSuitability = (sourceSlot, targetSlot, isMerge) => (isMerge ? (inventory.BaseWeight + 9) : (inventory.BaseWeight + 9)) + (sourceSlot.Inventory is InventoryBasePlayer ? 9 : 0);


            inventory.SlotModified += Inventory_SlotModified;
        }


        protected override float Inventory_OnAcquireTransitionSpeed(EnumTransitionType transType, ItemStack stack, float baseMul)
        {
            if (transType == EnumTransitionType.Dry) return 0;
            if (Api == null) return 0;

            if (transType == EnumTransitionType.Perish || transType == EnumTransitionType.Ripen)
            {
                float perishRate = GetPerishRate();
                if (transType == EnumTransitionType.Ripen)
                {
                    return GameMath.Clamp(((1 - perishRate) - 0.5f) * 3, 0, 1);
                }

                return baseMul * perishRate;
            }

            return 1;

        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            ownBlock = Block as BlockSingleSink;

            if (ownBlock?.Attributes?["capacityLitres"].Exists == true)
            {
                CapacityLitres = ownBlock.Attributes["capacityLitres"].AsInt(50);
                (inventory[1] as ItemSlotLiquidOnly).CapacityLitres = CapacityLitres;
            }

            if (api.World.Side == EnumAppSide.Client)
            {
                float rotY = Block.Shape.rotateY;
                animUtil.InitializeAnimator("lidopen", new Vec3f(0, rotY, 0));
            }
        }

        bool ignoreChange = false;

        private void Inventory_SlotModified(int slotId)
        {
            if (ignoreChange) return;

            if (slotId == 0 || slotId == 1 || slotId == 2 || slotId == 3 || slotId == 4 || slotId == 5 || slotId == 7 || slotId == 8)
            {
                invDialog?.UpdateContents();
                if (Api?.Side == EnumAppSide.Client)
                {
                    //currentMesh = GenMesh();
                    MarkDirty(true);
                }
            }

        }

        public override void OnBlockBroken()
        {
            base.OnBlockBroken();

            invDialog?.TryClose();
            invDialog = null;
        }


        public override void OnReceivedClientPacket(IPlayer fromPlayer, int packetid, byte[] data)
        {
            if (packetid <= 1000)
            {
                inventory.InvNetworkUtil.HandleClientPacket(fromPlayer, packetid, data);
            }

            if (packetid == (int)EnumBlockEntityPacketId.Close)
            {
                if (fromPlayer.InventoryManager != null)
                {
                    fromPlayer.InventoryManager.CloseInventory(Inventory);
                }
            }
        }

        public override void OnReceivedServerPacket(int packetid, byte[] data)
        {
            base.OnReceivedServerPacket(packetid, data);

            if (packetid == (int)EnumBlockEntityPacketId.Close)
            {
                (Api.World as IClientWorldAccessor).Player.InventoryManager.CloseInventory(Inventory);
                invDialog?.TryClose();
                invDialog?.Dispose();
                invDialog = null;
            }
        }

        public void OnBlockInteract(IPlayer byPlayer)
        {
            if (Api.World.Side == EnumAppSide.Client)
            {
                if (invDialog == null)
                {
                    invDialog = new GuiDialogSingleSink("Cabinet Sink", Inventory, Pos, Api as ICoreClientAPI);
                    invDialog.OnClosed += () =>
                    {
                        invDialog = null;
                        (Api as ICoreClientAPI).Network.SendBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, (int)EnumBlockEntityPacketId.Close, null);
                        byPlayer.InventoryManager.CloseInventory(inventory);
                        animUtil.StopAnimation("lidopen");
                        Api.World.PlaySoundAt(new AssetLocation("game:sounds/block/largechestclose"), Pos.X, Pos.Y, Pos.Z);
                    };
                }

                if (Api.Side == EnumAppSide.Client)
                {
                    animUtil.StartAnimation(new AnimationMetaData()
                    {
                        Animation = "lidopen",
                        Code = "lidopen",
                        AnimationSpeed = 1.8f,
                        EaseOutSpeed = 6,
                        EaseInSpeed = 15
                    });

                    Api.World.PlaySoundAt(new AssetLocation("game:sounds/block/largechestopen"), Pos.X, Pos.Y, Pos.Z);
                }


                invDialog.TryOpen();


                (Api as ICoreClientAPI).Network.SendPacketClient(inventory.Open(byPlayer));
            }
            else
            {
                byPlayer.InventoryManager.OpenInventory(inventory);
            }
        }

        protected void TranslateMesh(MeshData mesh, int index)
        {
            if (mesh == null) return;
            if (index > 3)
            {
                mesh.Clear();
                return;
            }
            float x = (index % 2 == 0) ? 5 / 16f : 11 / 16f;
            float y = 0.56f;
            float z = (index > 1) ? 11 / 16f : 5 / 16f;

            if (!Inventory[index].Empty)
            {
                if (Inventory[index].Itemstack.Class == EnumItemClass.Block)
                    mesh.Scale(new Vec3f(0.5f, 0, 0.5f), 0.35f, 0.35f, 0.35f);
                else
                    mesh.Scale(new Vec3f(0.5f, 0, 0.5f), 0.75f, 0.75f, 0.75f);
            }

            mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, 45 * GameMath.DEG2RAD, 0);
            mesh.Translate(x - 0.5f, y, z - 0.5f);
            int orientationRotate = 0;
            if (Block.Variant["horizontalorientation"] == "east") orientationRotate = 270;
            if (Block.Variant["horizontalorientation"] == "south") orientationRotate = 180;
            if (Block.Variant["horizontalorientation"] == "west") orientationRotate = 90;
            mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, orientationRotate * GameMath.DEG2RAD, 0);
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {

            bool parentSkip = base.OnTesselation(mesher, tessThreadTesselator);
            if (animUtil.activeAnimationsByAnimCode.Count > 0 || parentSkip || (animUtil.animator != null && animUtil.animator.ActiveAnimationCount > 0))
            {
                return true;
            }

            return false;
        }


        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            MeshAngle = tree.GetFloat("meshAngle", MeshAngle);

            if (Api?.Side == EnumAppSide.Client)
            {
                MarkDirty(true);
            }
        }


        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            if (Api?.Side == EnumAppSide.Client)
            {
                MarkDirty(true);
                invDialog?.UpdateContents();
            }

            tree.SetFloat("meshAngle", MeshAngle);
        }

        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();

            invDialog?.Dispose();
        }


        public override float GetPerishRate()
        {
            float initial = base.GetPerishRate();

            return initial / 3.2f;
        }
    }
}