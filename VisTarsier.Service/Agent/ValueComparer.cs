using VisTarsier.Common;
using System;
using System.Linq;

namespace VisTarsier.Service
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ValueComparer : IValueComparer
    {
        public bool CompareStrings(string val1, string val2, StringOperand operand, string delimiter = " ")
        {
            switch (operand)
            {
                case StringOperand.Equals:
                    if (string.Equals(val1, val2,
                        StringComparison.OrdinalIgnoreCase))
                        return true;
                    break;
                case StringOperand.Contains:
                    if (val1.ToLower().Contains(val2.ToLower()))
                        return true;
                    break;
                case StringOperand.StartsWith:
                    if (val1.ToLower().StartsWith(val2.ToLower()))
                        return true;
                    break;
                case StringOperand.EndsWith:
                    if (val1.ToLower().EndsWith(val2.ToLower()))
                        return true;
                    break;
                case StringOperand.OccursIn:
                    if (val2.Split(delimiter[0])
                        .Any(str => string.Equals(str, val1,
                            StringComparison.OrdinalIgnoreCase)))
                        return true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operand), operand, null);
            }
            return false;
        }

        public bool CompareDates(string criterionStudyDate, DateTime studyStudyDate, DateOperand criterionStudyDateOperand)
        {
            switch (criterionStudyDateOperand)
            {
                case DateOperand.Equals:

                    break;
                case DateOperand.Before:
                    break;
                case DateOperand.After:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(criterionStudyDateOperand), criterionStudyDateOperand, null);
            }
            return false;
        }
    }
}