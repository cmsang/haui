using Haui.ShapesDetector.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Haui.ShapesDetector.Services
{
    public class RobotService
    {
        public string GetRobotDest(string PointType)
        {
            return clsFileIO.ReadValue(PointType);
        }

        public void UpdatePosition(string key, string value)
        {
            clsFileIO.UpdateValue(key, value);
        }

        public string GetPosition(string key)
        {
            return clsFileIO.ReadValue(key);
        }
    }
}
