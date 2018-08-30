// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace Project2015To2017.Reading.Conditionals
{
    /// <summary>
    /// Performs logical NOT on left child
    /// Does not update conditioned properties table
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal sealed class NotExpressionNode : OperatorExpressionNode
    {
        /// <summary>
        /// Evaluate as boolean
        /// </summary>
        internal override bool BoolEvaluate(IConditionEvaluationState state)
        {
            return !this.LeftChild.BoolEvaluate(state);
        }

        internal override bool CanBoolEvaluate(IConditionEvaluationState state)
        {
            return this.LeftChild.CanBoolEvaluate(state);
        }

        /// <summary>
        /// Returns unexpanded value with '!' prepended. Useful for error messages.
        /// </summary>
        internal override string GetUnexpandedValue(IConditionEvaluationState state)
        {
            return "!" + this.LeftChild.GetUnexpandedValue(state);
        }

        /// <summary>
        /// Returns expanded value with '!' prepended. Useful for error messages.
        /// </summary>
        internal override string GetExpandedValue(IConditionEvaluationState state)
        {
            return "!" + this.LeftChild.GetExpandedValue(state);
        }

        internal override string DebuggerDisplay => $"(not {this.LeftChild.DebuggerDisplay})";
    }
}
