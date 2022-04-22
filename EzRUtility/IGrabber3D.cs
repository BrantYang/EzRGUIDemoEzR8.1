using Sick.GenIStream;
using Sick.EasyRanger;
using Sick.EasyRanger.Base;
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

        bool SetExposure(int Exp);

        bool SetScanHeight(int ScanHeight);

        int GetExposure();

        int GetScanHeight();

        bool Connect();

        bool Start();

        Sick.EasyRanger.Base.IFrame Grab(bool Online = true);

        bool Stop();

        bool Disconnect();

    }
}
