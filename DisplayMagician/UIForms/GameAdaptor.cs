using DisplayMagician.Contracts;
using Manina.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;

namespace DisplayMagician.UIForms
{
    #region Game Adaptor
    /// <summary>
    /// A custom item adaptor for ImageListView that reads from a Profile.
    /// </summary>
    class GameAdaptor : ImageListView.ImageListViewItemAdaptor
    {

        private bool disposed;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="GameAdaptor"/> class.
        /// </summary>
        public GameAdaptor()
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
                GameView game = (GameView)key;

                Image.GetThumbnailImageAbort myCallback = new Image.GetThumbnailImageAbort(() => { return false; });
                List<ShortcutBitmap> images = ImageUtils.GetMeAllBitmapsFromFile(game.IconPath);
                if (images.Count == 0 && !string.Equals(game.IconPath, game.ExecutablePath, StringComparison.OrdinalIgnoreCase))
                {
                    images = ImageUtils.GetMeAllBitmapsFromFile(game.ExecutablePath);
                }

                return images.Count == 0 || images[0].Image == null
                    ? Properties.Resources.exe.GetThumbnailImage(256, 256, myCallback, IntPtr.Zero)
                    : images[0].Image.GetThumbnailImage(256, 256, myCallback, IntPtr.Zero);

            }
            catch (Exception ex)
            {
                logger.Warn(ex, "GameAdapter/GetThumbnail: Exception caused while trying to get the Game Bitmap.");

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
                GameView game = (GameView)key;

                return game.Name;
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "GameAdapter/GetUniqueIdentifier: Exception caused while trying to get the Game Unique Identifier.");
                // If we have a problem with converting the submitted key to a Shortcut
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
                GameView game = (GameView)key;
                return game.Name;
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "GameAdaptor/GetSourceImage : Exception caused while trying to get the Game source name.");

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
                GameView game = (GameView)key;

                // Get file info
                {
                    Size mySize = new Size(256, 256);
                    SizeF mySizeF = new SizeF(256, 256);

                    string name = game.Name;
                    //string filepath = Path.GetDirectoryName(shortcut.SavedShortcutIconCacheFilename);
                    //string filename = Path.GetFileName(shortcut.SavedShortcutIconCacheFilename);
                    DateTime now = DateTime.Now;
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.DateCreated, string.Empty, now));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.DateAccessed, string.Empty, now));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.DateModified, string.Empty, now));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.FileSize, string.Empty, (long)0));
                    //details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.FilePath, string.Empty, filepath ?? ""));
                    //details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.FolderName, string.Empty, filepath ?? ""));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.FilePath, string.Empty, ""));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.FolderName, string.Empty, ""));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.Dimensions, string.Empty, mySize));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.Resolution, string.Empty, mySizeF));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.ImageDescription, string.Empty, name ?? ""));
                    details.Add(new Utility.Tuple<ColumnType, string, object>(ColumnType.EquipmentModel, string.Empty, ""));
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
                logger.Warn(ex, "GameAdapter/Utility.Tuple: Exception caused while trying to add details to the Game ImageListViewItem.");
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
