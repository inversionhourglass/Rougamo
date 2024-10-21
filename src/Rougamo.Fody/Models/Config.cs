using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rougamo.Fody.Models
{
    internal class Config(bool enabled, bool compositeAccessibility, int moArrayThreshold, bool recordingIteratorReturns, bool reverseCallNonEntry, bool pureStackTrace, string[] exceptTypePatterns)
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
        public int MoArrayThreshold { get; private set; } = moArrayThreshold;

        /// <summary>
        /// 是否将iterator的所有返回值暂存并在方法完成时存入MethodContext.ReturnValue中
        /// </summary>
        public bool RecordingIteratorReturns { get; } = recordingIteratorReturns;

        /// <summary>
        /// OnEntry以外的方法是否以相反的顺序调用
        /// </summary>
        public bool ReverseCallNonEntry { get; } = reverseCallNonEntry;

        /// <summary>
        /// 是否使用纯净的StackTrace，默认true表示使用
        /// </summary>
        /// <remarks>
        /// 肉夹馍使用的是代理模式，原始方法Xxx会迁移到$Rougamo_Xxx中，然后再Xxx方法中执行AOP并调用$Rougamo_Xxx。
        /// 这样的设计会导致调用堆栈增加，在抛出异常时这些调用堆栈会显得冗余臃肿，所以默认使用纯净的StackTrace。
        /// </remarks>
        public bool PureStackTrace { get; } = pureStackTrace;

        /// <summary>
        /// 类型的全名称与正则表达式匹配的类型将忽略所有IMo
        /// </summary>
        public Regex[] ExceptTypePatterns { get; } = exceptTypePatterns.Select(x => new Regex(x)).ToArray();

        /// <summary>
        /// 临时修改MoArrayThreshold的值，在dispose时修改回来
        /// </summary>
        public IDisposable ChangeMoArrayThresholdTemporarily(int value) => new MoArrayThresholdTempScope(this, value);

        class MoArrayThresholdTempScope : IDisposable
        {
            private readonly int _moArrayThreshold;
            private readonly Config _config;

            public MoArrayThresholdTempScope(Config config, int value)
            {
                _config = config;
                _moArrayThreshold = config.MoArrayThreshold;
                config.MoArrayThreshold = value;
            }

            public void Dispose()
            {
                _config.MoArrayThreshold = _moArrayThreshold;
            }
        }
    }
}
