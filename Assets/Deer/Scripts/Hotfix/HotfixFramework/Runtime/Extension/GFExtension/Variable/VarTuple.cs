//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using System;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// System.Tuple 变量类。
    /// </summary>
    public sealed class VarTuple : Variable<(string, Type)>
    {
        /// <summary>
        /// 初始化 System.Tuple 变量类的新实例。
        /// </summary>
        public VarTuple()
        {
        }

        /// <summary>
        /// 从 System.Tuple 到 System.Tuple 变量类的隐式转换。
        /// </summary>
        /// <param name="value">值。</param>
        public static implicit operator VarTuple((string, Type) value)
        {
            VarTuple varValue = ReferencePool.Acquire<VarTuple>();
            varValue.Value = value;
            return varValue;
        }

        /// <summary>
        /// 从 System.Tuple 变量类到 System.Tuple 的隐式转换。
        /// </summary>
        /// <param name="value">值。</param>
        public static implicit operator (string, Type)(VarTuple value)
        {
            return value.Value;
        }
    }
}
