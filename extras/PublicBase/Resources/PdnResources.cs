/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;

namespace PaintDotNet
{
    public static class PdnResources
    {
        private static ResourceManager resourceManager;
        private const string ourNamespace = "PaintDotNet";
        private static Assembly ourAssembly;
        private static string[] localeDirs;
        private static CultureInfo pdnCulture;
        private static string resourcesDir;

        public static string ResourcesDir
        {
            get
            {
                if (resourcesDir == null)
                {
                    resourcesDir = Path.GetDirectoryName(typeof(PdnResources).Assembly.Location);
                }

                return resourcesDir;
            }

            set
            {
                resourcesDir = value;
                Initialize();
            }
        }

        public static CultureInfo Culture
        {
            get
            {
                return pdnCulture;
            }

            set
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = value;
                Initialize();
            }
        }

        private static void Initialize()
        {
            resourceManager = CreateResourceManager();
            ourAssembly = Assembly.GetExecutingAssembly();
            pdnCulture = CultureInfo.CurrentUICulture;
            localeDirs = GetLocaleDirs();
        }

        static PdnResources()
        {
            Initialize();
        }

        public static void SetNewCulture(string newLocaleName)
        {
            // TODO, HACK: post-3.0 we must refactor and have an actual user data manager that can handle all this renaming
            string oldUserDataPath = PdnInfo.UserDataPath;
            string oldPaletteDirName = PdnResources.GetString("ColorPalettes.UserDataSubDirName");
            // END HACK

            CultureInfo newCI = new CultureInfo(newLocaleName);
            Settings.CurrentUser.SetString("LanguageName", newLocaleName);
            Culture = newCI;

            // TODO, HACK: finish up renaming
            string newUserDataPath = PdnInfo.UserDataPath;
            string newPaletteDirName = PdnResources.GetString("ColorPalettes.UserDataSubDirName");

            // 1. rename user data dir from old localized name to new localized name
            if (oldUserDataPath != newUserDataPath)
            {
                try
                {
                    Directory.Move(oldUserDataPath, newUserDataPath);
                }

                catch (Exception)
                {
                }
            }

            // 2. rename palette dir from old localized name (in new localized user data path) to new localized name
            string oldPalettePath = Path.Combine(newUserDataPath, oldPaletteDirName);
            string newPalettePath = Path.Combine(newUserDataPath, newPaletteDirName);

            if (oldPalettePath != newPalettePath)
            {
                try
                {
                    Directory.Move(oldPalettePath, newPalettePath);
                }

                catch (Exception)
                {
                }
            }
            // END HACK
        }
        
        public static string[] GetInstalledLocales()
        {
            const string left = "PaintDotNet.Strings.3";
            const string right = ".resources";
            string ourDir = ResourcesDir;
            string fileSpec = left + "*" + right;
            string[] pathNames = Directory.GetFiles(ourDir, fileSpec);
            List<String> locales = new List<string>();

            for (int i = 0; i < pathNames.Length; ++i)
            {
                string pathName = pathNames[i];
                string dirName = Path.GetDirectoryName(pathName);
                string fileName = Path.GetFileName(pathName);
                string sansRight = fileName.Substring(0, fileName.Length - right.Length);
                string sansLeft = sansRight.Substring(left.Length);

                string locale;

                if (sansLeft.Length > 0 && sansLeft[0] == '.')
                {
                    locale = sansLeft.Substring(1);
                }
                else if (sansLeft.Length == 0)
                {
                    locale = "en-US";
                }
                else
                {
                    locale = sansLeft;
                }

                try
                {
                    // Ensure this locale can create a valid CultureInfo object.
                    CultureInfo ci = new CultureInfo(locale);
                }

                catch (Exception)
                {
                    // Skip past invalid locales -- don't let them crash us
                    continue;
                }

                locales.Add(locale);
            }

            return locales.ToArray();
        }

        public static string[] GetLocaleNameChain()
        {
            List<string> names = new List<string>();
            CultureInfo ci = pdnCulture;

            while (ci.Name != string.Empty)
            {
                names.Add(ci.Name);
                ci = ci.Parent;
            }

            return names.ToArray();
        }

        private static string[] GetLocaleDirs()
        {
            const string rootDirName = "Resources";
            string appDir = ResourcesDir;
            string rootDir = Path.Combine(appDir, rootDirName);
            List<string> dirs = new List<string>();

            CultureInfo ci = pdnCulture;

            while (ci.Name != string.Empty)
            {
                string localeDir = Path.Combine(rootDir, ci.Name);

                if (Directory.Exists(localeDir))
                {
                    dirs.Add(localeDir);
                }

                ci = ci.Parent;
            }

            return dirs.ToArray();
        }

        private static ResourceManager CreateResourceManager()
        {
            const string stringsFileName = "PaintDotNet.Strings.3";
            ResourceManager rm = ResourceManager.CreateFileBasedResourceManager(stringsFileName, ResourcesDir, null);
            return rm;
        }

        public static ResourceManager Strings
        {
            get
            {
                return resourceManager;
            }
        }

        public static string GetString(string stringName)
        {
            string theString = resourceManager.GetString(stringName, pdnCulture);

            if (theString == null)
            {
                Debug.WriteLine(stringName + " not found");
                #region 调试中文界面
                //if (stringName == "Menu.View..Text") theString = "-";
                //if (stringName == "Menu.View.Pixels.Text") theString = "像素";
                //if (stringName == "Menu.View.Inches.Text") theString = "英寸";                
                //if (stringName == "Menu.Image..Text") theString = "-";                              

                //if (stringName == "LineCap2.Flat") theString = "线帽2-水平的";
                //if (stringName == "LineCap2.Arrow") theString = "线帽2-箭头";
                //if (stringName == "LineCap2.ArrowFilled") theString = "线帽2-填充后的箭头";
                //if (stringName == "LineCap2.Rounded") theString = "线帽2-圆形的";

                //if (stringName == "DashStyle.Solid") theString = "虚线样式-实心";
                //if (stringName == "DashStyle.Dash") theString = "虚线样式-短线/破折号";
                //if (stringName == "DashStyle.Dot") theString = "虚线样式-点";
                //if (stringName == "DashStyle.DashDot") theString = "虚线样式-点划线";
                //if (stringName == "DashStyle.DashDotDot") theString = "虚线样式-点点划线";
                //if (stringName == "DashStyle.Custom") theString = "虚线样式-自定义";
                //if (stringName == "ToolConfigStrip.PenStyleLabel.Text") theString = "钢笔样式标签";
                //if (stringName == "ToolConfigStrip.PenStartCapSplitButton.ToolTipText") theString = "钢笔开始盖拆分按钮";
                //if (stringName == "ToolConfigStrip.PenDashStyleSplitButton.ToolTipText") theString = "笔划样式拆分按钮";
                //if (stringName == "ToolConfigStrip.PenEndCapSplitButton.ToolTipText") theString = "钢笔结束盖拆分按钮";
                //if (stringName == "ToolConfigStrip.SelectionCombineModeLabel.Text") theString = "选择组合标签";
                //if (stringName == "ToolConfigStrip.FloodModeLabel.Text") theString = "洪水模型标签";
                //if (stringName == "ToolConfigStrip.SelectionDrawModeLabel.Text") theString = "选择绘图模型标签";
                //if (stringName == "ToolConfigStrip.SelectionDrawModeWidthLabel.Text") theString = "选择绘图模式宽度标签";
                //if (stringName == "ToolConfigStrip.SelectionDrawModeHeightLabel.Text") theString = "选择绘图模式高度标签";
                //if (stringName == "ToolConfigStrip.SelectionDrawModeSplitButton.Normal.Text") theString = "选择绘制模式拆分按钮-正常模式";
                //if (stringName == "BlockedPluginException.UnstablePlugin") theString = "不稳定的插件";
                //if (stringName == "BlockedPluginException.PluginIsNowBuiltIn") theString = "插件现在是内置的";
                //if (stringName == "Effects.Noise.Submenu.Name") theString = "噪声";
                //if (stringName == "Effects.Stylize.Submenu.Name") theString = "样式化/风格化";
                //if (stringName == "FragmentEffect.Name") theString = "碎片效应";
                //if (stringName == "Effects.Photo.Submenu.Name") theString = "照片";
                //if (stringName == "InkSketchEffect.Name") theString = "水墨素描效果";
                //if (stringName == "Effects.Artistic.Submenu.Name") theString = "艺术的";
                //if (stringName == "JuliaFractalEffect.Name") theString = "Julia分形效应";
                //if (stringName == "MandelbrotFractalEffect.Name") theString = "Mandelbrot分形效应";
                //if (stringName == "PolarInversion.Name") theString = "极性反转";
                //if (stringName == "PosterizeAdjustment.Name") theString = "色调分离调整";
                //if (stringName == "ReduceNoiseEffect.Name") theString = "减少噪声效果";
                //if (stringName == "PencilSketchEffect.Name") theString = "铅笔素描效果";
                //if (stringName == "SoftenPortraitEffect.Name") theString = "柔化肖像效果";
                #endregion
            }

            return theString;
        }

        public static Stream GetResourceStream(string fileName)
        {
            Stream stream = null;

            for (int i = 0; i < localeDirs.Length; ++i)
            {
                string filePath = Path.Combine(localeDirs[i], fileName);

                if (File.Exists(filePath))
                {
                    stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    break;
                }
            }

            if (stream == null)
            {
                string fullName = ourNamespace + "." + fileName;
                stream = ourAssembly.GetManifestResourceStream(fullName);
            }

            return stream;
        }

        public static Image GetImageBmpOrPng(string fileNameNoExt)
        {
            // using Path.ChangeExtension is not what we want; quite often filenames are "Icons.BlahBlahBlah"
            string fileNameBmp = fileNameNoExt + ".bmp";
            Image image = GetImage(fileNameBmp);

            if (image == null)
            {
                string fileNamePng = fileNameNoExt + ".png";
                image = GetImage(fileNamePng);
            }

            return image;
        }

        public static Image GetImage(string fileName)
        {
            Stream stream = GetResourceStream(fileName);

            Image image = null;
            if (stream != null)
            {
                image = LoadImage(stream);
            }

            return image;
        }

        private sealed class PdnImageResource
            : ImageResource
        {
            private string name;
            private static Dictionary<string, ImageResource> images;

            protected override Image Load()
            {
                return PdnResources.GetImage(this.name);
            }

            public static ImageResource Get(string name)
            {
                ImageResource ir;

                if (!images.TryGetValue(name, out ir))
                {
                    ir = new PdnImageResource(name);
                    images.Add(name, ir);
                }

                return ir;
            }

            static PdnImageResource()
            {
                images = new Dictionary<string, ImageResource>();
            }

            private PdnImageResource(string name)
                : base()
            {
                this.name = name;
            }

            private PdnImageResource(Image image)
                : base(image)
            {
                this.name = null;
            }                
        }

        public static ImageResource GetImageResource(string fileName)
        {
            return PdnImageResource.Get(fileName);
        }

        public static Icon GetIcon(string fileName)
        {
            Stream stream = GetResourceStream(fileName);
            Icon icon = null;

            if (stream != null)
            {
                icon = new Icon(stream);
            }

            return icon;
        }

        public static Icon GetIconFromImage(string fileName)
        {
            Stream stream = GetResourceStream(fileName);

            Icon icon = null;

            if (stream != null)
            {
                Image image = LoadImage(stream);
                icon = Icon.FromHandle(((Bitmap)image).GetHicon());
                image.Dispose();
                stream.Close();
            }

            return icon;
        }

        private static bool CheckForSignature(Stream input, byte[] signature)
        {
            long oldPos = input.Position;
            byte[] inputSig = new byte[signature.Length];
            int amountRead = input.Read(inputSig, 0, inputSig.Length);

            bool foundSig = false;
            if (amountRead == signature.Length)
            {
                foundSig = true;

                for (int i = 0; i < signature.Length; ++i)
                {
                    foundSig &= (signature[i] == inputSig[i]);
                }
            }

            input.Position = oldPos;
            return foundSig;
        }

        public static bool IsGdiPlusImageAllowed(Stream input)
        {
            byte[] wmfSig = new byte[] { 0xd7, 0xcd, 0xc6, 0x9a };
            byte[] emfSig = new byte[] { 0x01, 0x00, 0x00, 0x00 };

            // Check for and explicitely block WMF and EMF images
            return !(CheckForSignature(input, emfSig) || CheckForSignature(input, wmfSig));
        }

        public static Image LoadImage(string fileName)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return LoadImage(stream);
            }
        }

        /// <summary>
        /// Loads an image from the given stream. The stream must be seekable.
        /// </summary>
        /// <param name="input">The Stream to load the image from.</param>
        public static Image LoadImage(Stream input)
        {
            /*
            if (!IsGdiPlusImageAllowed(input))
            {
                throw new IOException("File format is not supported");
            }
            */

            Image image = Image.FromStream(input);

            if (image.RawFormat == ImageFormat.Wmf || image.RawFormat == ImageFormat.Emf)
            {
                image.Dispose();
                throw new IOException("File format isn't supported");
            }

            return image;
        }
    }
}
