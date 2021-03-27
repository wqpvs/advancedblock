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
    class BEEGenerator:BEElectric
    {
        //how many power packets we can generate - will see if every more than one
        protected int genAmps = 1;
        Vec3d posvec;
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            if (Block.Attributes != null)
            {
                genAmps = Block.Attributes["genAmps"].AsInt(genAmps);
            }
            posvec = new Vec3d(Pos.X + 0.5, Pos.Y, Pos.Z + 0.5);

            if (api.World.Side == EnumAppSide.Client&&animUtil!=null)
            {
                float rotY = Block.Shape.rotateY;
                animUtil.InitializeAnimator("run", new Vec3f(0, rotY, 0));
                animUtil.StartAnimation(new AnimationMetaData() { Animation = "run", Code = "run", AnimationSpeed = 1, EaseInSpeed = 1, EaseOutSpeed = 1, Weight = 1, BlendMode = EnumAnimationBlendMode.Average });

            }
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
        //generators don't receive power
        public override int ReceivePacketOffer(BEElectric from,int volt, int amp)
        {
            return 0;
        }
        BlockEntityAnimationUtil animUtil
        {
            get {
                BEBehaviorAnimatable bea = GetBehavior<BEBehaviorAnimatable>();
                if (bea == null) { return null; }
                return GetBehavior<BEBehaviorAnimatable>().animUtil;
            }
        }
        //TODO need turn on and turn off functions
        public override void TogglePower()
        {
            isOn = !isOn;
            if (Api.World.Side == EnumAppSide.Client&&animUtil!=null)
            {
                if (isOn)
                {
                    float rotY = Block.Shape.rotateY;
                    animUtil.InitializeAnimator("run", new Vec3f(0, rotY, 0));
                    animUtil.StartAnimation(new AnimationMetaData() { Animation = "run", Code = "run", AnimationSpeed = 1, EaseInSpeed = 1, EaseOutSpeed = 1, Weight = 1, BlendMode = EnumAnimationBlendMode.Average });
                }
                else
                {
                    animUtil.StopAnimation("run");
                }

            }
        }
        public override int NeedPower()
        {
            return 0;
        }
    }
}
