﻿using DisplayMagicianShared;
using DisplayMagicianShared.NVIDIA;
using DisplayMagicianShared.AMD;
using DisplayMagicianShared.Windows;
using Manina.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayMagician.UIForms
{

    #region Profile Adaptor
    /// <summary>
    /// A custom item adaptor for ImageListView that reads from a Profile.
    /// </summary>
    class ProfileAdaptor : ImageListView.ImageListViewItemAdaptor
    {

        private bool disposed;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemAdaptor"/> class.
        /// </summary>
        public ProfileAdaptor()
        {
            disposed = false;
        }

        /// <summary>
        /// Returns the thumbnail image for the given item.
        /// </summary>
        /// <param name="key">Item key.</param>
        /// <param name="size">Requested image size.</param>
        /// <param name="useEmbeddedThumbnails">Embedded thumbnail usage.</param>
        /// <param name="useExifOrientation">true to automatically rotate images based on Exif orientation; otherwise false.</param>
        /// <returns>The thumbnail image from the given item or null if an error occurs.</returns>
        public override Image GetThumbnail(object key, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, bool useExifOrientation)
        {
            if (disposed)
                return null;

            try
            {
                string profileName = key.ToString();

                ProfileItem profileToUse = null;

                foreach (ProfileItem profileToTest in ProfileRepository.AllProfiles)
                {
                    if (profileToTest.Name == profileName)
                    {
                        profileToUse = profileToTest;
                    }

                }

                if (profileToUse == null)
                {
                    profileToUse = ProfileRepository.CurrentProfile;
                }

                // Stop the callback doing anything
                Image.GetThumbnailImageAbort myCallback = new Image.GetThumbnailImageAbort(() => { return false; });

                Image imageToUse;
                imageToUse = profileToUse.ProfileBitmap.GetThumbnailImage(size.Width, size.Height, myCallback, IntPtr.Zero);                
                return imageToUse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProfileAdaptor/GetThumbnail exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                // If we have a problem with converting the submitted key to a profile
                // Then we return null
                return null;
            }

        }

        /// <summary>
        /// Returns a unique identifier for this thumbnail to be used in persistent
        /// caching.
        /// </summary>
        /// <param name="key">Item key.</param>
        /// <param name="size">Requested image size.</param>
        /// <param name="useEmbeddedThumbnails">Embedded thumbnail usage.</param>
        /// <param name="useExifOrientation">true to automatically rotate images based on Exif orientation; otherwise false.</param>
        /// <returns>A unique identifier string for the thumnail.</returns>
        public override string GetUniqueIdentifier(object key, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, bool useExifOrientation)
        {
            if (disposed)
                return null;

            try
            {
                ProfileItem profileToUse = (ProfileItem)key;

                if (profileToUse == null)
                {
                    profileToUse = ProfileRepository.CurrentProfile;
                }

                return profileToUse.UUID;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProfileAdaptor/GertUniqueIdentifier exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                // If we have a problem with converting the submitted key to a profile
                // Then we return null
                return null;
            }
        }

        /// <summary>
        /// Returns the path to the source image for use in drag operations.
        /// </summary>
        /// <param name="key">Item key.</param>
        /// <returns>The path to the source image.</returns>
        public override string GetSourceImage(object key)
        {
            if (disposed)
                return null;

            try
            {
                ProfileItem profile = (ProfileItem)key;
                return profile.Name;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProfileAdaptor/GetSourceImage exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                // If we have a problem with converting the submitted key to a profile
                // Then we return null
                return null;
            }
        }

        /// <summary>
        /// Returns the details for the given item.
        /// </summary>
        /// <param name="key">Item key.</param>
        /// <returns>An array of tuples containing item details or null if an error occurs.</returns>
        public override Utility.Tuple<ColumnType, string, object>[] GetDetails(object key)
        {
            if (disposed)
                return null;

            List<Utility.Tuple<ColumnType, string, object>> details = new List<Utility.Tuple<ColumnType, string, object>>();

            try
            {
                ProfileItem profileToUse = (ProfileItem)key;

                if (profileToUse == null)
                {
                    profileToUse = ProfileRepository.CurrentProfile;
                }

                // Get file info
                if (profileToUse.ProfileBitmap is Bitmap)
                {
                    // Have to do some gymnastics to get rid of the 
                    // System.Drawing.Image exception created while accessing the Size
                    bool gotSize = false;
                    Size mySize = new Size(256, 256);
                    while (!gotSize)
                    {
                        try
                        {
                            mySize = profileToUse.ProfileBitmap.Size;
                            gotSize = true;
                        }
                        catch (Exception ex)
                        {
                            // catch the System.Drawing.Image exception created while accessing the Size
                            logger.Debug(ex, "ProfileAdaptor/GetDetails: System.Drawing.Image exception caused while trying to get the ProfileBitmap Size as an Integer.");
                        }
                    }

                    // Have to do some gymnastics to get rid of the 
                    // System.Drawing.Image exception created while accessing the SizeF
                    bool gotSizeF = false;
                    SizeF mySizeF = new SizeF(256, 256);
                    while (!gotSizeF)
                    {
                        try
                        {
                            mySizeF = profileToUse.ProfileBitmap.PhysicalDimension;
                            gotSizeF = true;
                        }
                        catch (Exception ex)
                        {
                            // catch the System.Drawing.Image exception created while accessing the Size
                            logger.Debug(ex, "ProfileAdaptor/GetDetails: System.Drawing.Image exception caused while trying to get the ProfileBitmap Size as a Float.");
                        }
                    }
                    string name = profileToUse.Name;
                    string filepath = Path.GetDirectoryName(profileToUse.SavedProfileIconCacheFilename);
                    string filename = Path.GetFileName(profileToUse.SavedProfileIconCacheFilename);
                    DateTime now = DateTime.Now;
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.DateCreated, string.Empty, now));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.DateAccessed, string.Empty, now));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.DateModified, string.Empty, now));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.FileSize, string.Empty, (long)0));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.FilePath, string.Empty, filepath ?? ""));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.FolderName, string.Empty, filepath ?? ""));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.Dimensions, string.Empty, mySize));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.Resolution, string.Empty, mySizeF));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.ImageDescription, string.Empty, name ?? ""));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.EquipmentModel, string.Empty, profileToUse.UUID));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.DateTaken, string.Empty, now));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.Artist, string.Empty, ""));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.Copyright, string.Empty, ""));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.ExposureTime, string.Empty, (float)0));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.FNumber, string.Empty, (float)0));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.ISOSpeed, string.Empty, (ushort)0));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.UserComment, string.Empty, ""));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.Rating, string.Empty, (ushort)0));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.Software, string.Empty, ""));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.FocalLength, string.Empty, (float)0));
                }
            
                return details.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProfileAdaptor/Utility.Tuple exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                // If we have a problem with converting the submitted key to a profile
                // Then we return null
                return null;
            }
        }

        public override void Dispose()
        {
            disposed = true;
        }
    }
    #endregion
    
}