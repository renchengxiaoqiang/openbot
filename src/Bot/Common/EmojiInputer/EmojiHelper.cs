using System;
using System.Collections.Generic;
using System.Windows;

namespace Bot.Common.EmojiInputer
{
	public class EmojiHelper
    {
        private static List<EmojiInfo> _emojis;

        static EmojiHelper()
        {
            _emojis = new List<EmojiInfo>
			{
				new EmojiInfo("微笑", "/:^_^"),
				new EmojiInfo("害羞", "/:^$^"),
				new EmojiInfo("吐舌头", "/:Q"),
				new EmojiInfo("偷笑", "/:815"),
				new EmojiInfo("爱慕", "/:809"),
				new EmojiInfo("大笑", "/:^O^"),
				new EmojiInfo("跳舞", "/:081"),
				new EmojiInfo("飞吻", "/:087"),
				new EmojiInfo("安慰", "/:086"),
				new EmojiInfo("抱抱", "/:H"),
				new EmojiInfo("加油", "/:012"),
				new EmojiInfo("胜利", "/:806"),
				new EmojiInfo("强", "/:b"),
				new EmojiInfo("亲亲", "/:^x^"),
				new EmojiInfo("花痴", "/:814"),
				new EmojiInfo("露齿笑", "/:^W^"),
				new EmojiInfo("查找", "/:080"),
				new EmojiInfo("呼叫", "/:066"),
				new EmojiInfo("算账", "/:807"),
				new EmojiInfo("财迷", "/:805"),
				new EmojiInfo("好主意", "/:071"),
				new EmojiInfo("鬼脸", "/:072"),
				new EmojiInfo("天使", "/:065"),
				new EmojiInfo("再见", "/:804"),
				new EmojiInfo("流口水", "/:813"),
				new EmojiInfo("享受", "/:818"),
				new EmojiInfo("色情狂", "/:015"),
				new EmojiInfo("呆若木鸡", "/:084"),
				new EmojiInfo("思考", "/:801"),
				new EmojiInfo("迷惑", "/:811"),
				new EmojiInfo("疑问", "/:?"),
				new EmojiInfo("没钱了", "/:077"),
				new EmojiInfo("无聊", "/:083"),
				new EmojiInfo("怀疑", "/:817"),
				new EmojiInfo("嘘", "/:!"),
				new EmojiInfo("小样", "/:068"),
				new EmojiInfo("摇头", "/:079"),
				new EmojiInfo("感冒", "/:028"),
				new EmojiInfo("尴尬", "/:026"),
				new EmojiInfo("傻笑", "/:007"),
				new EmojiInfo("不会吧", "/:816"),
				new EmojiInfo("无奈", "/:'\"\""),
				new EmojiInfo("流汗", "/:802"),
				new EmojiInfo("凄凉", "/:027"),
				new EmojiInfo("困了", "/:(Zz...)"),
				new EmojiInfo("晕", "/:*&*"),
				new EmojiInfo("忧伤", "/:810"),
				new EmojiInfo("委屈", "/:>_<"),
				new EmojiInfo("悲泣", "/:018"),
				new EmojiInfo("大哭", "/:>O<"),
				new EmojiInfo("痛哭", "/:020"),
				new EmojiInfo("I服了U", "/:044"),
				new EmojiInfo("对不起", "/:819"),
				new EmojiInfo("再见", "/:085"),
				new EmojiInfo("皱眉", "/:812"),
				new EmojiInfo("好累", "/:\""),
				new EmojiInfo("吐", "/:>@<"),
				new EmojiInfo("背", "/:076"),
				new EmojiInfo("惊讶", "/:069"),
				new EmojiInfo("惊愕", "/:O"),
				new EmojiInfo("闭嘴", "/:067"),
				new EmojiInfo("欠扁", "/:043"),
				new EmojiInfo("鄙视你", "/:P"),
				new EmojiInfo("大怒", "/:808"),
				new EmojiInfo("生气", "/:>W<"),
				new EmojiInfo("财神", "/:073"),
				new EmojiInfo("学习雷锋", "/:008"),
				new EmojiInfo("恭喜发财", "/:803"),
				new EmojiInfo("小二", "/:074"),
				new EmojiInfo("老大", "/:O=O"),
				new EmojiInfo("邪恶", "/:036"),
				new EmojiInfo("单挑", "/:039"),
				new EmojiInfo("CS", "/:045"),
				new EmojiInfo("隐形人", "/:046"),
				new EmojiInfo("炸弹", "/:048"),
				new EmojiInfo("惊声尖叫", "/:047"),
				new EmojiInfo("漂亮MM", "/:girl"),
				new EmojiInfo("帅哥", "/:man"),
				new EmojiInfo("招财猫", "/:052"),
				new EmojiInfo("成交", "/:(OK)"),
				new EmojiInfo("鼓掌", "/:8*8 "),
				new EmojiInfo("握手", "/:)-("),
				new EmojiInfo("红唇", "/:lip"),
				new EmojiInfo("玫瑰", "/:-F"),
				new EmojiInfo("残花", "/:-W"),
				new EmojiInfo("爱心", "/:Y"),
				new EmojiInfo("心碎", "/:qp"),
				new EmojiInfo("钱", "/:$"),
				new EmojiInfo("购物", "/:%"),
				new EmojiInfo("礼物", "/:(&)"),
				new EmojiInfo("收邮件", "/:@"),
				new EmojiInfo("电话", "/:~B"),
				new EmojiInfo("举杯庆祝", "/:U*U"),
				new EmojiInfo("时钟", "/:clock"),
				new EmojiInfo("等待", "/:R"),
				new EmojiInfo("很晚了", "/:C"),
				new EmojiInfo("飞机", "/:plane"),
				new EmojiInfo("支付宝", "/:075")
			};
        }

        public static Int32Rect FindEmojisRect(string etxt)
		{
            Int32Rect rect = Int32Rect.Empty;
            int idx = FindEmojisIndex(etxt);
			if (idx >= 0 && idx < 98)
			{
                double x = (double)idx % 13 * 42.5;
                double y = (double)idx / 13 * 42.5;
				rect = new Int32Rect
				{
					X = (int)x,
					Y = (int)y,
					Width = 43,
					Height = 43
				};
			}
			return rect;
		}

        public static int FindEmojisIndex(string etxt)
		{
			int idx = -1;
			if (!string.IsNullOrEmpty(etxt))
			{
				idx = _emojis.FindIndex(k=>k.Text.Trim() == etxt.Trim());
			}
			return idx;
		}

		public static string GetDesc(int idx)
		{
			return _emojis[idx].Desc + " " + _emojis[idx].Text;
		}

		public static string GetText(int idx)
		{
			return _emojis[idx].Text;
		}
	}
}
