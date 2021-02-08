// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.IO;

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
