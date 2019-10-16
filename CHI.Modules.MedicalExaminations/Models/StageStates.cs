namespace CHI.Modules.MedicalExaminations.Models
{
    public static class StageStates
    {
        public static ExaminationStage[,] States { get; }

        static StageStates()
        {
            States = new ExaminationStage[,] {
               { ExaminationStage.None,             ExaminationStage.FirstBegin },
               { ExaminationStage.FirstBegin,       ExaminationStage.FirstEnd },
               { ExaminationStage.FirstEnd,         ExaminationStage.FirstResult },
               { ExaminationStage.FirstEnd,         ExaminationStage.SecondReferral },

               { ExaminationStage.SecondReferral,   ExaminationStage.SecondBegin },
               { ExaminationStage.SecondBegin,      ExaminationStage.SecondEnd },
               { ExaminationStage.SecondEnd,        ExaminationStage.SecondResult },

               { ExaminationStage.None,             ExaminationStage.Rejection },
               { ExaminationStage.FirstBegin,       ExaminationStage.Rejection },
               { ExaminationStage.FirstEnd,         ExaminationStage.Rejection },

               { ExaminationStage.SecondReferral,   ExaminationStage.Rejection },
               { ExaminationStage.SecondBegin,      ExaminationStage.Rejection },
               { ExaminationStage.SecondEnd,        ExaminationStage.Rejection },
               { ExaminationStage.SecondResult,     ExaminationStage.Rejection }
            };
        }
    }
}