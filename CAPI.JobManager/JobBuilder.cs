﻿using CAPI.Dicom.Abstraction;
using CAPI.JobManager.Abstraction;
using System.Collections.Generic;
using System.Linq;

namespace CAPI.JobManager
{
    public class JobBuilder : IJobBuilder
    {
        public IJob Build(IRecipe recipe, IDicomServices dicomServices, IJobManagerFactory jobManagerFactory)
        {
            return jobManagerFactory.CreateJob(
                dicomServices.GetStudyForAccession(recipe.NewStudyCriteria
                    .FirstOrDefault(c => c.AccessionNumber != string.Empty)?.AccessionNumber),
                FindStudyToCompareToForAccession(recipe.PriorStudyCriteria),
                recipe.IntegratedProcesses,
                recipe.Destinations
            );
        }

        private IStudy FindStudyToCompareToForAccession(IList<IStudySelectionCriteria> recipePriorStudyCriteria)
        {
            throw new System.NotImplementedException();
        }
    }
}
