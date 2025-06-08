using BotLib.Cypto;
using BotLib.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace BotLib.FileSerializer
{
    public class Serializer
    {
        public static void ShowXml<T>(T obj, Type[] extraTypes = null)
        {
            string tempTxtFileName = PathEx.TempTxtFileName;
            try
            {
                var xmlSerializer = new XmlSerializer(obj.GetType());
                using (var writer = XmlWriter.Create(tempTxtFileName, new XmlWriterSettings
                {
                    Indent = true
                }))
                {
                    xmlSerializer.Serialize(writer, obj);
                }
                FileEx.ShowTextFileWithNotePad(tempTxtFileName);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        public static void SerializeToFileWithFileName(object obj, string fn, bool isEncrypt = true)
        {
            if (obj != null)
            {
                string text = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented, _settings);
                if (isEncrypt)
                {
                    text = Crypto.DesEncrypt(text,string.Empty);
                }
                SafetyFile.Save(text, fn);
            }
        }

        public static void SerializeToFile(object obj, string name = null, bool isEncrypt = true)
        {
            if (obj == null) return;
            if (name == null)
            {
                Util.Assert(!(obj is IEnumerable));
            }
            name = (name ?? obj.GetType().FullName);
            if (!name.Contains("\\"))
            {
                name = PathEx.GetFilenameUnderAppDataDir(name);
            }
            SerializeToFileWithFileName(obj, name, isEncrypt);
        }

        public static T DeserializeFromFileWithFilename<T>(string fn, bool isEncrypted = true)
        {
            T t = default(T);
            try
            {
                var text = SafetyFile.ReadAll(fn);
                if (!string.IsNullOrEmpty(text))
                {
                    if (isEncrypted)
                    {
                        text = Crypto.DesDecrypt(text,string.Empty);
                    }
                    t = JsonConvert.DeserializeObject<T>(text, _settings);
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return t;
        }

        public static T DeserializeFromFile<T>(string name = null, bool isEncrypted = true)
        {
            string filenameUnderAppDataDir = PathEx.GetFilenameUnderAppDataDir(name ?? typeof(T).FullName);
            return DeserializeFromFileWithFilename<T>(filenameUnderAppDataDir, isEncrypted);
        }

        private static JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };
    }
}
