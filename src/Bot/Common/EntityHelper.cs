using BotLib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbEntity;

namespace Bot.Common
{
    public static class EntityHelper
    {
        public static T Create<T>(string account = "") where T : EntityBase
        {
            return (T)((object)EntityHelper.Create(typeof(T), account));
        }
        public static EntityBase Create(Type type, string account = "")
        {
            var et = Activator.CreateInstance(type) as EntityBase;
            et.DbAccount = account;
            et.EntityId = StringEx.xGenGuidB64Str();
            et.ModifyTick =DateTime.Now.Ticks;
            return et;
        }
        public static void SetModifyTick(this EntityBase et)
        {
            et.ModifyTick = DateTime.Now.Ticks;
        }
    }
}
