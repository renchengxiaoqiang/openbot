using BotLib.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.FileSerializer
{
    public class SafetyFile
    {
        public static void Save(Stream stm, string fn)
        {
            if (File.Exists(fn))
            {
                var newName = GetNewName(fn);
                WriteStreamToFile(stm, newName);
                DeleteFilesNameStartWith(newName + "_");
                var fi = new FileInfo(newName);
                var newSplitFn = newName + "_" + fi.Length;
                FileEx.ReName(newName, newSplitFn);
                fi = new FileInfo(fn);
                string oldName = GetOldName(fn) + "_";
                DeleteFilesNameStartWith(oldName);
                string newfn = oldName + fi.Length;
                if (IsFileStreamWithName(stm, fn))
                {
                    stm.Close();
                }
                FileEx.ReName(fn, newfn);
                AssertFileSize(newSplitFn);
                FileEx.ReName(newSplitFn, fn);
            }
            else
            {
                int idx = fn.LastIndexOf('\\');
                if (idx > 0)
                {
                    Directory.CreateDirectory(fn.Substring(0, idx));
                }
                WriteStreamToFile(stm, fn);
                if (IsFileStreamWithName(stm, fn))
                {
                    stm.Close();
                }
            }
        }

        public static string ReadAll(string fn)
        {
            var fs = Open(fn);
            if (fs == null) return null;

            using (var read = new StreamReader(fs, Util.EncodingGb2312))
            {
                return read.ReadToEnd();
            }
        }

        public static FileStream Open(string fn)
        {
            if (!File.Exists(fn))
            {
                TryRestoreFile(fn);
            }
            return File.Exists(fn) ? new FileStream(fn, FileMode.Open) : null;
        }

        private static string GetOldName(string fn)
        {
            return fn + "_old";
        }

        private static string GetNewName(string fn)
        {
            return fn + "_new";
        }

        private static bool IsFileStreamWithName(Stream stm, string fn)
        {
            var fs = stm as FileStream;
            return fs != null ? (fs.Name == fn) : false;
        }

        private static void DeleteFilesNameStartWith(string fn)
        {
            var directoryName = Path.GetDirectoryName(fn);
            var files = Directory.GetFiles(directoryName);
            foreach (var f in files)
            {
                if (f.StartsWith(fn))
                {
                    File.Delete(f);
                }
            }
        }

        private static bool TryRestoreFile(string fn)
        {
            if (RestoreFromFileNewSize(fn)) return true;
            if (RestoreFromFileOldSize(fn)) return true;
            return false;
        }

        private static bool RestoreFromFileOldSize(string fn)
        {
            return RestoreFromFileWithSize(fn, "_old_");
        }

        private static bool RestoreFromFileNewSize(string fn)
        {
            return RestoreFromFileWithSize(fn, "_new_");
        }

        private static bool RestoreFromFileWithSize(string fn, string init)
        {
            try
            {
                string startWithFn = GetFilenameStartWith(fn + init);
                if (string.IsNullOrEmpty(startWithFn))
                {
                    return false;
                }
                AssertFileSize(startWithFn);
                File.Copy(startWithFn, fn);
            }
            catch
            {
                return false;
            }
            return true;
        }

        private static string GetFilenameStartWith(string fn)
        {
            var directoryName = Path.GetDirectoryName(fn);
            var files = Directory.GetFiles(directoryName);
            foreach (var f in files)
            {
                if (f.StartsWith(fn))
                {
                    return f;
                }
            }
            return null;
        }

        private static long GetFileSizeFromFilename(string fnWithSize)
        {
            string fileName = Path.GetFileName(fnWithSize);
            string[] splitNameArr = fileName.Split('_');
            if (splitNameArr.Length < 2)
            {
                throw new Exception("AssertFileSize,找不到下划线，fn=" + fnWithSize);
            }
            return Convert.ToInt64(splitNameArr[splitNameArr.Length - 1]);
        }

        private static void AssertFileSize(string fnWithSize)
        {
            long fileSizeFromFilename = GetFileSizeFromFilename(fnWithSize);
            FileInfo fileInfo = new FileInfo(fnWithSize);
            Trace.Assert(fileSizeFromFilename == fileInfo.Length, "AssertFileSize,文件大小不等");
        }

        private static void WriteStreamToFile(Stream stm, string tmpname)
        {
            stm.Seek(0L, SeekOrigin.Begin);
            int len = 8192;
            byte[] buffer = new byte[len];
            using (FileStream fileStream = new FileStream(tmpname, FileMode.Create))
            {
                bool done;
                do
                {
                    int count = stm.Read(buffer, 0, len);
                    fileStream.Write(buffer, 0, count);
                    done = (count != len);
                }
                while (!done);
            }
        }

        public static void Save(string s, string fn)
        {
            byte[] bytes = Util.EncodingGb2312.GetBytes(s);
            using (var ms = new MemoryStream(bytes))
            {
                Save(ms, fn);
            }
        }
    }
}
