using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using SevenZipExtractor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Misc
{
    public class Zip
    {
        public static void Un7ZipFile(string archiveFilenameIn, string outFolder)
        {
            try
            {
                using (ArchiveFile archiveFile = new ArchiveFile(archiveFilenameIn))
                {
                    archiveFile.Extract(outFolder);
                }
            }
            finally
            {
            }
        }

        public static void UnZipFile(string archiveFilenameIn, string outFolder, string password = "")
        {
            ZipFile zf = null;
            try
            {
                var fs = File.OpenRead(archiveFilenameIn);
                zf = new ZipFile(fs);
                if (!string.IsNullOrEmpty(password))
                {
                    zf.Password = password;
                }
                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;
                    }
                    var entryFileName = zipEntry.Name;
                    byte[] buffer = new byte[4096];
                    var zipStream = zf.GetInputStream(zipEntry);
                    var fullZipToPath = Path.Combine(outFolder, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);
                    using (var streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true;
                    zf.Close();
                }
            }
        }

        public static void ZipDir(string dirToZip, string zipedFileName, int compressionLevel = 9)
        {
            if (Path.GetExtension(zipedFileName) != ".zip")
            {
                zipedFileName = zipedFileName + ".zip";
            }
            using (var zipoutputstream = new ZipOutputStream(File.Create(zipedFileName)))
            {
                zipoutputstream.SetLevel(compressionLevel);
                Crc32 crc = new Crc32();
                Hashtable fileList = GetAllFies(dirToZip);
                foreach (DictionaryEntry item in fileList)
                {
                    FileStream fs = new FileStream(item.Key.ToString(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    // ZipEntry entry = new ZipEntry(item.Key.ToString().Substring(dirToZip.Length + 1));
                    ZipEntry entry = new ZipEntry(Path.GetFileName(item.Key.ToString()))
                    {
                        DateTime = (DateTime)item.Value,
                        Size = fs.Length
                    };
                    fs.Close();
                    crc.Reset();
                    crc.Update(buffer);
                    entry.Crc = crc.Value;
                    zipoutputstream.PutNextEntry(entry);
                    zipoutputstream.Write(buffer, 0, buffer.Length);
                }
            }
        }
 
        /// <summary>  
        /// 获取所有文件  
        /// </summary>  
        /// <returns></returns>  
        private static Hashtable GetAllFies(string dir)
        {
            Hashtable filesList = new Hashtable();
            DirectoryInfo fileDire = new DirectoryInfo(dir);
            if (!fileDire.Exists)
            {
                throw new FileNotFoundException("目录:" + fileDire.FullName + "没有找到!");
            }
 
            GetAllDirFiles(fileDire, filesList);
            GetAllDirsFiles(fileDire.GetDirectories(), filesList);
            return filesList;
        }
 
        /// <summary>  
        /// 获取一个文件夹下的所有文件夹里的文件  
        /// </summary>  
        /// <param name="dirs"></param>  
        /// <param name="filesList"></param>  
        private static void GetAllDirsFiles(IEnumerable<DirectoryInfo> dirs, Hashtable filesList)
        {
            foreach (DirectoryInfo dir in dirs)
            {
                foreach (FileInfo file in dir.GetFiles("*.*"))
                {
                    filesList.Add(file.FullName, file.LastWriteTime);
                }
                GetAllDirsFiles(dir.GetDirectories(), filesList);
            }
        }                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                
 
        /// <summary>  
        /// 获取一个文件夹下的文件  
        /// </summary>  
        /// <param name="dir">目录名称</param>
        /// <param name="filesList">文件列表HastTable</param>  
        public static void GetAllDirFiles(DirectoryInfo dir, Hashtable filesList)
        {
            foreach (FileInfo file in dir.GetFiles("*.*"))
            {
                filesList.Add(file.FullName, file.LastWriteTime);
            }
        }
    }
}
