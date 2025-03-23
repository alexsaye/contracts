using SimpleGraph;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Contracts.Scripting
{
    [Serializable]
    public class StartNode : SimpleGraphNode, ICareerBuilder
    {
        public IEnumerable<IContractBuilderProgression> NextOnHired { get; set; }

        public virtual ICareer Build()
        {
            Debug.Log($"Building {this}...");
            return new Career(NextOnHired);
        }
    }
}
