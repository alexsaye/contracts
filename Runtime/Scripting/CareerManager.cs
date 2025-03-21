using System;
using System.Collections.Generic;
using UnityEngine;

namespace Contracts.Scripting
{
    public class CareerManager : MonoBehaviour
    {
        private List<ICareer> careers = new();

        public event EventHandler<IssuedEventArgs> Issued;

        public void Update()
        {
            foreach (var career in careers)
            {
                career.IssueContracts();
            }
        }

        public void Hire(ICareerBuilder builder)
        {
            var career = builder.Build();
            careers.Add(career);
            career.Issued += ForwardIssued;
        }

        private void ForwardIssued(object sender, IssuedEventArgs issued)
        {
            Issued?.Invoke(this, issued);
        }
    }
}