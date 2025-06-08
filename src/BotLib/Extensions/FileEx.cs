using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Extensions
{
    public static class FileEx
    {
        private static void AddSecurityControll2File(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var accessControl = fileInfo.GetAccessControl();
            accessControl.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Allow));
            accessControl.AddAccessRule(new FileSystemAccessRule("Users", FileSystemRights.FullControl, AccessControlType.Allow));
            fileInfo.SetAccessControl(accessControl);
        }

        public static long GetFileLength(string fn)
        {
            return new FileInfo(fn).Length;
        }

        public static bool IsFileLengthMoreKB(string fn, int maxkb)
        {
            return FileEx.GetFileLength(fn) > (long)(maxkb * 1024);
        }

        public static bool Exist(string fn)
        {
            return new FileInfo(fn).Exists;
        }

        public static void DeleteWithoutException(string fn, string mark = null)
        {
            try
            {
                File.Delete(fn);
            }
            catch (Exception e)
            {
                if (mark != null)
                {
                    Log.Error(mark);
                }
                Log.Exception(e);
            }
        }

        public static void TryDelete(List<string> fnlist)
        {
            foreach (string fn in fnlist)
            {
                FileEx.DeleteWithoutException(fn, null);
            }
        }

        public static void ReadToEnd(this FileStream fs, out byte[] arr)
        {
            arr = new byte[fs.Length];
            fs.Read(arr, 0, (int)fs.Length);
        }

        public static string ReadAllText(string fileName)
        {
            string txt = null;
            if (File.Exists(fileName))
            {
                txt = File.ReadAllText(fileName);
            }
            return txt;
        }

        public static void ReName(string oldFn, string newfn)
        {
            if (File.Exists(newfn))
            {
                File.Delete(newfn);
            }
            FileInfo fileInfo = new FileInfo(oldFn);
            fileInfo.MoveTo(newfn);
        }

        public static void Serialize(object obj, string filename)
        {
            var fs = File.OpenWrite(filename);
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(fs, obj);
            fs.Close();
        }

        public static object Deserialize(string filename)
        {
            var binaryFormatter = new BinaryFormatter();
            var fs = File.OpenRead(filename);
            object result = binaryFormatter.Deserialize(fs);
            fs.Close();
            return result;
        }

        public static void SaveToFile(string fn, byte[] fileData)
        {
            File.WriteAllBytes(fn, fileData);
        }

        public static void SaveToFile(string fn, string txt)
        {
            var writer = new StreamWriter(fn);
            writer.Write(txt);
            writer.Close();
        }

        public static void SaveToFile(string fn, Stream input)
        {
            using (var fs = new FileStream(fn, FileMode.Create))
            {
                int count = 4096;
                byte[] buffer = new byte[count];
                bool done;
                do
                {
                    int len = input.Read(buffer, 0, count);
                    fs.Write(buffer, 0, len);
                    done = (len != count);
                }
                while (!done);
            }
        }

        public static string TryReadFile(string fn)
        {
            return FileEx.TryReadFile(fn, Util.EncodingGb2312);
        }

        public static string TryReadFile(string fn, Encoding encoding)
        {
            try
            {
                using (var reader = new StreamReader(fn, encoding))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return string.Empty;
        }

        public static void ShowTextFileWithNotePad(string fn)
        {
            Process.Start(fn);
        }

        public static void ShowStringWithNotePad(string txt)
        {
            var tempTxtFileName = PathEx.TempTxtFileName;
            FileEx.SaveToFile(tempTxtFileName, txt);
            Process.Start(tempTxtFileName);
        }

        public static bool IsTwoFileEqual(string fn1, string fn2)
        {
            var f1 = new FileInfo(fn1);
            var f2 = new FileInfo(fn2);
            int seed = 8;
            if (f1.Length != f2.Length) return false;

            int len = (int)Math.Ceiling((double)f1.Length / (double)seed);
            using (var fs1 = f1.OpenRead())
            {
                using (var fs2 = f2.OpenRead())
                {
                    byte[] arr1 = new byte[seed];
                    byte[] arr2 = new byte[seed];
                    for (int i = 0; i < len; i++)
                    {
                        fs1.Read(arr1, 0, seed);
                        fs2.Read(arr2, 0, seed);
                        if (BitConverter.ToInt64(arr1, 0) != BitConverter.ToInt64(arr2, 0))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public static bool FilesAreEqual_Hash(FileInfo first, FileInfo second)
        {
            var firstHashCode = MD5.Create().ComputeHash(first.OpenRead());
            var secondHashCode = MD5.Create().ComputeHash(second.OpenRead());
            for (int i = 0; i < firstHashCode.Length; i++)
            {
                if (firstHashCode[i] != secondHashCode[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
