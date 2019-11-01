namespace CHI.Modules.MedicalExaminations.Models
{
    public static class StageStates
    {
        public static ExaminationStep[,] States { get; }

        static StageStates()
        {
            States = new ExaminationStep[,] {
               { ExaminationStep.None,             ExaminationStep.FirstBegin },
               { ExaminationStep.FirstBegin,       ExaminationStep.FirstEnd },
               { ExaminationStep.FirstEnd,         ExaminationStep.FirstResult },

               { ExaminationStep.FirstEnd,         ExaminationStep.SecondTransition },
               { ExaminationStep.SecondTransition, ExaminationStep.SecondBegin },
               { ExaminationStep.SecondBegin,      ExaminationStep.SecondEnd },
               { ExaminationStep.SecondEnd,        ExaminationStep.SecondResult },

               { ExaminationStep.None,             ExaminationStep.Refuse },
               { ExaminationStep.FirstBegin,       ExaminationStep.Refuse },
               { ExaminationStep.FirstEnd,         ExaminationStep.Refuse },

               { ExaminationStep.SecondTransition, ExaminationStep.Refuse },
               { ExaminationStep.SecondBegin,      ExaminationStep.Refuse },
               { ExaminationStep.SecondEnd,        ExaminationStep.Refuse },
               { ExaminationStep.SecondResult,     ExaminationStep.Refuse }
            };
        }
    }
}