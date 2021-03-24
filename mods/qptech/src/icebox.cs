using System;
using System.Collections.Generic;
using System.IO;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
namespace qptech.src
{
    public class IceBox : ModSystem
    {
        float chilltick = 0;
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            
        }
        public class BEIceBox : BlockEntityGenericTypedContainer
        {
            
            bool isChilled = false;
            public bool useIce = true;
            public float preserveBonus = 0.5f;
            public int useIceCounter = 10000;
            float chilltick = 0;
            public override float GetPerishRate()
            {
                float prate = base.GetPerishRate();
                if (isChilled) { prate = prate * preserveBonus; }
                return prate;
            }

            public override void Initialize(ICoreAPI api)
            {
                base.Initialize(api);
                preserveBonus = Block.Attributes["preserveBonus"].AsFloat(preserveBonus);
                useIce = Block.Attributes["useIce"].AsBool(useIce);
                useIceCounter = Block.Attributes["useIceCounter"].AsInt(useIceCounter);

            }

            protected override void OnTick(float dt)
            {
                base.OnTick(dt);
                if (!useIce) { isChilled = true;return; }
                isChilled = false;
                ItemSlot chillslot = null;
                foreach (ItemSlot slot in Inventory)
                {
                   if (slot.Itemstack == null) { continue; }
                   if (slot.Itemstack.Block == null) { continue; }
                   if (slot.Itemstack.Block.BlockMaterial.ToString() == "Ice")
                    {
                        isChilled = true;
                        chillslot = slot;
                        chilltick+=dt;
                        continue;
                    }

                }
                if (!isChilled) { chilltick = 0; return; }
                if (chilltick>=useIceCounter)
                {
                    chillslot.TakeOut(1);
                }
            }


        }

    }
}