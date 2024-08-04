using System.Linq;
using System.Text.RegularExpressions;

namespace Rougamo.Fody.Models
{
    internal class Config(bool enabled, bool compositeAccessibility, int moArrayThreshold, bool recordingIteratorReturns, bool reverseCallNonEntry, string[] exceptTypePatterns)
    {
        /// <summary>
        /// 是否启用肉夹馍
        /// </summary>
        public bool Enabled { get; } = enabled;

        /// <summary>
        /// 方法是否使用 类+方法 组合可访问性
        /// </summary>
        public bool CompositeAccessibility { get; } = compositeAccessibility;

        /// <summary>
        /// 当方法上IMo的数量超过该值时将使用数组保存所有IMo
        /// </summary>
        public int MoArrayThreshold { get; set; } = moArrayThreshold;

        /// <summary>
        /// 是否将iterator的所有返回值暂存并在方法完成时存入MethodContext.ReturnValue中
        /// </summary>
        public bool RecordingIteratorReturns { get; } = recordingIteratorReturns;

        /// <summary>
        /// OnEntry以外的方法是否以相反的顺序调用
        /// </summary>
        public bool ReverseCallNonEntry { get; } = reverseCallNonEntry;

        /// <summary>
        /// 类型的全名称与正则表达式匹配的类型将忽略所有IMo
        /// </summary>
        public Regex[] ExceptTypePatterns { get; } = exceptTypePatterns.Select(x => new Regex(x)).ToArray();
    }
}
