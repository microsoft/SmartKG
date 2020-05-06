using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SmartKG.Common.Utils
{
    public class PathUtility
    {
        public static string CompletePath(string rootPath)
        {
            if (!rootPath.EndsWith(Path.DirectorySeparatorChar))
            {
                string newRootPath = rootPath + Path.DirectorySeparatorChar;
                return newRootPath;
            }
            else
            {
                return rootPath;
            }
        }
    }
}
