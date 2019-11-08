namespace CHI.Modules.MedicalExaminations.Models
{
    public static class StageStates
    {
        public static ExaminationStepKind[,] States { get; }

        static StageStates()
        {
            States = new ExaminationStepKind[,] {
               { ExaminationStepKind.None,             ExaminationStepKind.FirstBegin },
               { ExaminationStepKind.FirstBegin,       ExaminationStepKind.FirstEnd },
               { ExaminationStepKind.FirstEnd,         ExaminationStepKind.FirstResult },

               { ExaminationStepKind.FirstEnd,         ExaminationStepKind.TransferSecond },
               { ExaminationStepKind.TransferSecond, ExaminationStepKind.SecondBegin },
               { ExaminationStepKind.SecondBegin,      ExaminationStepKind.SecondEnd },
               { ExaminationStepKind.SecondEnd,        ExaminationStepKind.SecondResult },

               { ExaminationStepKind.None,             ExaminationStepKind.Refuse },
               { ExaminationStepKind.FirstBegin,       ExaminationStepKind.Refuse },
               { ExaminationStepKind.FirstEnd,         ExaminationStepKind.Refuse },

               { ExaminationStepKind.TransferSecond, ExaminationStepKind.Refuse },
               { ExaminationStepKind.SecondBegin,      ExaminationStepKind.Refuse },
               { ExaminationStepKind.SecondEnd,        ExaminationStepKind.Refuse },
               { ExaminationStepKind.SecondResult,     ExaminationStepKind.Refuse }
            };
        }
    }
}