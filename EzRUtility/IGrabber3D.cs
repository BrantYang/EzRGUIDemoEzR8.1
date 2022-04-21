using Sick.GenIStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzRUtility
{
    interface IGrabber3D
    {
        RangerConfig Config { get; set; }

        Frame FrameData { get; set; }

        bool Init();

        bool SetParams(RangerConfig _config);

        bool LoadParams(RangerConfig _config);

        bool Connect();

        bool Start();

        IFrame Grab(double yresolution, int timeout, bool Online = true);

        bool Stop();

        bool Disconnect();

    }
}
