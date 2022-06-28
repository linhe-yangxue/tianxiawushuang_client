using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace Assets.Editor
{
    class ProjectSetting
    {
        public static BuildTarget GetBundleBuildTarget()
        {
            return EditorUserBuildSettings.activeBuildTarget;
            //return BuildTarget.Android;
        }
    }
}
