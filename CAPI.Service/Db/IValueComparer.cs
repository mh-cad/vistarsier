using VisTarsier.Common;
using System;

namespace VisTarsier.Service.Db
{
    public interface IValueComparer
    {
        bool CompareStrings(string val1, string val2, StringOperand operand, string delimiter = " ");
        bool CompareDates(string criterionStudyDate, DateTime studyStudyDate, DateOperand criterionStudyDateOperand);
    }
}