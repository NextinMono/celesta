using System.ComponentModel;

namespace Celesta.BuilderNodes
{
    public class Pan3D
    {
        public Pan3D(short pan3dVolumeOffset, short pan3dVolumeGain, short pan3dAngleOffset, short pan3dAngleGain, short pan3dDistanceOffset, short pan3dDistanceGain)
        {
            VolumeOffset = pan3dVolumeOffset;
            VolumeGain = pan3dVolumeGain;
            AngleOffset = pan3dAngleOffset;
            AngleGain = pan3dAngleGain;
            DistanceOffset = pan3dDistanceOffset;
            DistanceGain = pan3dDistanceGain;
        }

        [Category("Pan 3D"), DisplayName("Pan 3D Volume Offset")]
        public short VolumeOffset { get; set; }

        [Category("Pan 3D"), DisplayName("Pan 3D Volume Gain")]
        public short VolumeGain { get; set; }

        [Category("Pan 3D"), DisplayName("Pan 3D Angle Offset")]
        public short AngleOffset { get; set; }

        [Category("Pan 3D"), DisplayName("Pan 3D Angle Gain")]
        public short AngleGain { get; set; }

        [Category("Pan 3D"), DisplayName("Pan 3D Distance Offset")]
        public short DistanceOffset { get; set; }

        [Category("Pan 3D"), DisplayName("Pan 3D Distance Gain")]
        public short DistanceGain { get; set; }


    }
}
