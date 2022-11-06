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
    /// System.Type 变量类。
    /// </summary>
    public sealed class VarType : Variable<Type>
    {
        /// <summary>
        /// 初始化 System.Type 变量类的新实例。
        /// </summary>
        public VarType()
        {
        }

        /// <summary>
        /// 从 System.Type 到 System.Type 变量类的隐式转换。
        /// </summary>
        /// <param name="value">值。</param>
        public static implicit operator VarType(Type value)
        {
            VarType varValue = ReferencePool.Acquire<VarType>();
            varValue.Value = value;
            return varValue;
        }

        /// <summary>
        /// 从 System.Type 变量类到 System.Type 的隐式转换。
        /// </summary>
        /// <param name="value">值。</param>
        public static implicit operator Type(VarType value)
        {
            return value.Value;
        }
    }
}
