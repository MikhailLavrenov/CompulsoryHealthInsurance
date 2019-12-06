using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Services.MedicalExaminations
{
    public class ExaminationStep : IEqualityComparer<ExaminationStep>
    {
        public ExaminationStepKind ExaminationStepKind { get; set; }
        public ExaminationKind Type { get; set; }
        public int Year { get; set; }
        public DateTime Date { get; set; }
        public ExaminationHealthGroup HealthGroup { get; set; }
        public ExaminationReferral Referral { get; set; }

        public bool Equals(ExaminationStep x, ExaminationStep y)
        {
            if (x.Equals(y))
                return true;

            if (x == null && y != null) 
                return false;

            if (x != null && y == null)
                return false;

            return x.ExaminationStepKind.Equals(y.ExaminationStepKind)
                && x.Type.Equals(y.Type)
                && x.Year.Equals(y.Year)
                && x.Date.Equals(y.Date)
                && x.HealthGroup.Equals(y.HealthGroup)
                && x.Referral.Equals(y.Referral);
        }

        public int GetHashCode(ExaminationStep obj)
        {
            unchecked
            {
                // Choose large primes to avoid hashing collisions
                const int HashingBase = (int)2166136261;
                const int HashingMultiplier = 16777619;

                int hash = HashingBase;
                hash = (hash * HashingMultiplier) ^ (!ReferenceEquals(null, ExaminationStepKind) ? ExaminationStepKind.GetHashCode() : 0);
                hash = (hash * HashingMultiplier) ^ (!ReferenceEquals(null, Type) ? Type.GetHashCode() : 0);
                hash = (hash * HashingMultiplier) ^ (!ReferenceEquals(null, Year) ? Year.GetHashCode() : 0);
                hash = (hash * HashingMultiplier) ^ (!ReferenceEquals(null, Date) ? Date.GetHashCode() : 0);
                hash = (hash * HashingMultiplier) ^ (!ReferenceEquals(null, HealthGroup) ? HealthGroup.GetHashCode() : 0);
                hash = (hash * HashingMultiplier) ^ (!ReferenceEquals(null, Referral) ? Referral.GetHashCode() : 0);

                return hash;
            }
        }
    }
}
